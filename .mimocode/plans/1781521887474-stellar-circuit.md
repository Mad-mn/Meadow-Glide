# Undo System Design & Implementation Plan

## Overview

Implement a Command Pattern-based undo system that records player actions and replays them in reverse with smooth animations. The system is extensible for future mechanics without requiring rewrites.

---

## Architecture Decision: Command Pattern (Option B)

### Why NOT Full Board Snapshots (Option A)

| Factor | Assessment |
|--------|------------|
| Memory | High — stores entire board state per move |
| Animation | Hard — no "from" state to animate from |
| Extensibility | Poor — adding new state requires snapshot changes |
| Performance | Degrades with board size |

### Why NOT Hybrid (Option C)

| Factor | Assessment |
|--------|------------|
| Complexity | Over-engineered for current 2-action game |
| Maintenance | Two systems to maintain |
| Consistency | Inconsistent undo behavior between action types |

### Why Command Pattern (Option B) ✅

| Factor | Assessment |
|--------|------------|
| Memory | Low — only stores changed data per action |
| Animation | Easy — has explicit "from" and "to" states |
| Extensibility | Excellent — new mechanics implement one interface |
| Performance | Constant time per action |
| Architecture | Clean separation of concerns |

---

## Current Player Actions Analysis

### 1. Stripe Rotation

**Trigger**: Horizontal drag on a strip (`StripRotationService`)

**State Changes**:
- `StripController.ScrollOffset` changes (float representing rotation position)
- `MoveTrackModel.MovesLeft` decrements (if offset actually changed)

**Animation**: 0.25s ease-out snap to nearest segment position

**Data Needed for Undo**:
- Reference to `StripController`
- Previous `ScrollOffset` value
- Whether a move was consumed

### 2. Segment Slide

**Trigger**: Vertical drag in a SlideArea (`SlideSegmentService`)

**State Changes**:
- Segments move between strips (parent changes via `ApplyShift()`)
- `StripController._spawnedSegments` lists change
- `MoveTrackModel.MovesLeft` decrements (if positions actually changed)

**Animation**: 0.2s ease-out snap to target strip positions

**Data Needed for Undo** (MUST capture BEFORE `ApplyShift()`):
- List of affected segments
- Their original strip references (before shift)
- Their original indices in those strips
- Their original radii (before `SetRadius(0f)` is called)
- Whether a move was consumed

---

## Proposed Architecture

### Module Structure

```
Assets/Feature/UndoModule/Scripts/
├── IUndoService.cs              # Public interface
├── UndoService.cs               # Implementation
├── Actions/
│   ├── IUndoableAction.cs       # Action interface
│   ├── RotationUndoAction.cs    # Rotation-specific undo
│   └── SlideUndoAction.cs       # Slide-specific undo
└── Installers/
    └── UndoModuleInstaller.cs   # Zenject bindings
```

### IUndoableAction Interface

```csharp
namespace Feature.UndoModule.Scripts.Actions {
    public interface IUndoableAction {
        UniTask ExecuteReverse();  // Animate the reverse action
        void RestoreState();       // Restore game state (after animation)
    }
}
```

**Design Rationale**:
- `ExecuteReverse()` returns `UniTask` for async animation
- `RestoreState()` is called after animation completes
- Separation allows animation to finish before state is finalized
- New mechanics implement this interface to participate in undo

### IUndoService Interface

```csharp
namespace Feature.UndoModule.Scripts {
    public interface IUndoService {
        void Record(IUndoableAction action);
        UniTask Undo();
        bool CanUndo { get; }
        void Clear();
    }
}
```

### UndoService Implementation

```csharp
namespace Feature.UndoModule.Scripts {
    public class UndoService : IUndoService {
        private readonly Stack<IUndoableAction> _undoStack = new();
        private readonly IInteractionStateService _interactionState;
        private bool _isUndoing;

        public bool CanUndo => _undoStack.Count > 0 && !_isUndoing;

        public void Record(IUndoableAction action) {
            _undoStack.Push(action);
        }

        public async UniTask Undo() {
            if (!CanUndo) return;

            _isUndoing = true;
            _interactionState.BlockInput();

            var action = _undoStack.Pop();
            await action.ExecuteReverse();
            action.RestoreState();

            _interactionState.UnblockInput();
            _isUndoing = false;
        }

        public void Clear() {
            _undoStack.Clear();
        }
    }
}
```

### RotationUndoAction

```csharp
namespace Feature.UndoModule.Scripts.Actions {
    public class RotationUndoAction : IUndoableAction {
        private readonly StripController _strip;
        private readonly float _previousOffset;
        private readonly float _currentOffset;
        private readonly bool _consumedMove;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly ISlideSegmentService _slideSegmentService;

        public RotationUndoAction(
            StripController strip,
            float previousOffset,
            float currentOffset,
            bool consumedMove,
            MoveTrackModel moveTrackModel,
            ISlideSegmentService slideSegmentService) {
            _strip = strip;
            _previousOffset = previousOffset;
            _currentOffset = currentOffset;
            _consumedMove = consumedMove;
            _moveTrackModel = moveTrackModel;
            _slideSegmentService = slideSegmentService;
        }

        public async UniTask ExecuteReverse() {
            // Animate from currentOffset to previousOffset
            // Uses same animation as StripRotationService.SnapStrip()
            float duration = 0.25f;
            float elapsed = 0f;

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = 1f - Mathf.Pow(1f - t, 3f); // Ease-out cubic
                float offset = Mathf.Lerp(_currentOffset, _previousOffset, t);
                _strip.SetScrollOffset(offset, true);
                await UniTask.Yield();
            }

            _strip.SetScrollOffset(_previousOffset, false);
            _strip.ClearWrapGhosts();
            _slideSegmentService.UpdateSegmentsInAreas();  // Match original SnapStrip behavior
        }

        public void RestoreState() {
            if (_consumedMove)
                _moveTrackModel.AddMoves(1);
        }
    }
}
```

### SlideUndoAction

```csharp
namespace Feature.UndoModule.Scripts.Actions {
    public class SlideUndoAction : IUndoableAction {
        private readonly List<SegmentRestoreData> _segmentData;
        private readonly bool _consumedMove;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly StripModel _stripModel;
        private readonly ISlideSegmentService _slideSegmentService;

        public SlideUndoAction(
            List<SegmentRestoreData> segmentData,
            bool consumedMove,
            MoveTrackModel moveTrackModel,
            StripModel stripModel,
            ISlideSegmentService slideSegmentService) {
            _segmentData = segmentData;
            _consumedMove = consumedMove;
            _moveTrackModel = moveTrackModel;
            _stripModel = stripModel;
            _slideSegmentService = slideSegmentService;
        }

        public async UniTask ExecuteReverse() {
            // Animate segments back to original strips
            float duration = 0.2f;
            float elapsed = 0f;

            // Store current positions for animation
            float[] startRadii = new float[_segmentData.Count];
            for (int i = 0; i < _segmentData.Count; i++) {
                startRadii[i] = _segmentData[i].Segment.Radius;
            }

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = 1f - Mathf.Pow(1f - t, 3f);

                for (int i = 0; i < _segmentData.Count; i++) {
                    var data = _segmentData[i];
                    float y = Mathf.Lerp(startRadii[i], data.OriginalRadius, t);
                    data.Segment.SetRadius(y);
                }
                await UniTask.Yield();
            }
        }

        public void RestoreState() {
            // Move segments back to original strips
            // NOTE: Do NOT call ChangeSlideState(false) here — it would trigger
            // MoveTrackService.CheckForSpendStepBySlide() and consume another move.
            // The MoveTrackService already consumed the move during the original slide.
            foreach (var data in _segmentData) {
                data.SourceStrip.RemoveSegment(data.Segment);
                data.OriginalStrip.AddSegment(data.Segment, data.OriginalIndex);
            }

            _stripModel.SegmentsChanged();
            _slideSegmentService.UpdateSegmentsInAreas();

            if (_consumedMove)
                _moveTrackModel.AddMoves(1);
        }
    }

    public struct SegmentRestoreData {
        public StripSegment Segment;
        public StripController OriginalStrip;  // Captured BEFORE ApplyShift
        public StripController SourceStrip;    // Current strip (after shift)
        public int OriginalIndex;              // Index in OriginalStrip BEFORE shift
        public float OriginalRadius;           // Radius BEFORE SetRadius(0f)
    }
}
```

---

## Integration Points

### 1. StripRotationService — Record Rotation

In `SnapStrip()`, after animation completes:

```csharp
// After _interactionStateService.IsRotationActive = false;
if (Mathf.Abs(targetOffset - _initialScrollOffset) > 0.01f) {
    var action = new RotationUndoAction(
        activeStrip,
        _initialScrollOffset,
        targetOffset,
        true,
        _moveTrackModel,
        _slideSegmentService  // For UpdateSegmentsInAreas()
    );
    _undoService.Record(action);
}
```

**Constructor injection** — add `IUndoService undoService` parameter:
```csharp
public StripRotationService(IInputService inputService, MoveTrackModel moveTrackModel,
    IInteractionStateService interactionStateService, IAudioService audioService,
    IVibrationService vibrationService, StripModel stripModel,
    ISlideSegmentService slideSegmentService, ICameraService cameraService,
    IUndoService undoService) {
    // ... existing assignments ...
    _undoService = undoService;
}
```

### 2. SlideSegmentService — Record Slide

In `SnapSegments()`, BEFORE `ApplyShift()`:

```csharp
// CRITICAL: Capture original state BEFORE ApplyShift() mutates it
List<SegmentRestoreData> segmentData = null;
if (shift != 0) {
    segmentData = new List<SegmentRestoreData>();
    for (int i = 0; i < _activeSegments.Count; i++) {
        StripController originalStrip = GetStripByIndex(startIdx + i);
        segmentData.Add(new SegmentRestoreData {
            Segment = _activeSegments[i],
            OriginalStrip = originalStrip,
            SourceStrip = _activeSegments[i].GetComponentInParent<StripController>(),
            OriginalIndex = i,
            OriginalRadius = _activeSegments[i].Radius  // Before SetRadius(0f)
        });
    }
}

ApplyShift(area, shift);  // This mutates strip assignments

// After snap animation completes:
if (segmentData != null) {
    var action = new SlideUndoAction(
        segmentData,
        true,
        _moveTrackModel,
        _stripModel,
        this  // ISlideSegmentService for UpdateSegmentsInAreas()
    );
    _undoService.Record(action);
}
```

**Constructor injection** — add `IUndoService undoService` parameter:
```csharp
public SlideSegmentService(IInputService inputService, IInteractionStateService interactionState,
    ICameraService cameraService, UniTask<CircleParamsConfig> circleParamsConfigTask,
    StripModel stripModel, SlideAreaModel slideAreaModel, MoveTrackModel moveTrackModel,
    LevelModel levelModel, IAudioService audioService, IVibrationService vibrationService,
    IUndoService undoService) {
    // ... existing assignments ...
    _undoService = undoService;
}
```

### 3. LevelInitializeService — Clear on Level Start

In `Initialize()`:

```csharp
_undoService.Clear();
```

### 4. UndoButton — Trigger Undo

New UI element that calls `_undoService.Undo()` when pressed.

---

## File Change Summary

| # | File | Action |
|---|------|--------|
| 1 | `Assets/Feature/UndoModule/Scripts/IUndoService.cs` | CREATE |
| 2 | `Assets/Feature/UndoModule/Scripts/UndoService.cs` | CREATE |
| 3 | `Assets/Feature/UndoModule/Scripts/Actions/IUndoableAction.cs` | CREATE |
| 4 | `Assets/Feature/UndoModule/Scripts/Actions/RotationUndoAction.cs` | CREATE |
| 5 | `Assets/Feature/UndoModule/Scripts/Actions/SlideUndoAction.cs` | CREATE |
| 6 | `Assets/Feature/UndoModule/Scripts/Installers/UndoModuleInstaller.cs` | CREATE |
| 7 | `Assets/Feature/StripRotationModule/Scripts/StripRotationService.cs` | EDIT (inject IUndoService, record undo in SnapStrip) |
| 8 | `Assets/Feature/SlideAreaModule/Scripts/SlideSegmentService.cs` | EDIT (inject IUndoService, capture state before ApplyShift, record undo) |
| 9 | `Assets/Feature/LevelInitializeModule/LevelInitializeService.cs` | EDIT (inject IUndoService, clear on level start) |
| 10 | `Assets/Feature/Bootstrap/Scripts/ProjectContextInstaller.cs` | EDIT (register UndoModuleInstaller) |

---

## How Future Mechanics Integrate

### Example: Destroyed Segments

```csharp
public class DestroyUndoAction : IUndoableAction {
    private readonly StripSegment _segment;
    private readonly StripController _strip;
    private readonly int _index;

    public async UniTask ExecuteReverse() {
        // Recreate segment
        // Animate appearing
    }

    public void RestoreState() {
        // Re-add to strip
    }
}
```

### Example: Frozen Segments

```csharp
public class FreezeUndoAction : IUndoableAction {
    private readonly StripSegment _segment;
    private readonly bool _wasFrozen;

    public async UniTask ExecuteReverse() {
        // Animate unfreeze visual
    }

    public void RestoreState() {
        _segment.SetStatus(_wasFrozen ? SegmentStatus.Frozen : SegmentStatus.Default);
    }
}
```

### Integration Pattern

1. Implement `IUndoableAction`
2. In the mechanic's service, call `_undoService.Record(action)` after the action completes
3. No other changes needed — the undo system handles the rest

---

## Visual Requirements

### Undo Animation Quality

- **Rotation**: Smooth ease-out cubic (0.25s) — matches existing snap animation
- **Slide**: Smooth ease-out cubic (0.2s) — matches existing snap animation
- **Input blocked** during undo — prevents conflicting actions
- **Sound/Vibration** — optional: play reverse sound during undo

### Player Feedback

- Undo button disabled when stack is empty
- Visual indicator of undo stack depth (optional: "3 undos remaining")
- Clear animation shows what was undone

---

## Memory Efficiency

### Per Action Memory Cost

| Action Type | Data Stored | Size |
|-------------|-------------|------|
| Rotation | StripController ref, 2 floats, 1 bool, 1 ref | ~40 bytes |
| Slide | N segments × (refs + int + float) | ~80-120 bytes |

### Stack Depth Recommendation

- Cap at 20-30 actions (configurable)
- Oldest actions automatically removed
- Memory usage: ~2-3 KB for 30 actions

---

## Verification

1. **Compile check**: Build project — no errors
2. **Rotation undo**: Rotate strip, press undo, verify smooth reverse animation
3. **Slide undo**: Slide segments, press undo, verify segments return to original strips
4. **Multiple undo**: Perform 3 actions, undo all 3, verify correct reverse order
5. **Move restore**: Verify `MovesLeft` increments after undo
6. **Input blocking**: Verify no input during undo animation
7. **Level clear**: Start new level, verify undo stack is cleared
8. **UI state**: Verify undo button enables/disables correctly
