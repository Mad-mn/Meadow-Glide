using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Feature.InputModule.Scripts;
using Feature.UndoModule.Scripts.Actions;

namespace Feature.UndoModule.Scripts
{
    public class UndoService : IUndoService
    {
        private readonly IInteractionStateService _interactionState;
        private readonly Stack<IUndoableAction> _undoStack = new();
        private bool _isUndoing;

        public bool CanUndo => _undoStack.Count > 0 && !_isUndoing;

        public UndoService(IInteractionStateService interactionState)
        {
            _interactionState = interactionState;
        }

        public void Record(IUndoableAction action)
        {
            _undoStack.Push(action);
        }

        public async UniTask Undo()
        {
            if (!CanUndo) return;

            _isUndoing = true;
            _interactionState.BlockInput();

            var action = _undoStack.Pop();
            await action.ExecuteReverse();
            action.RestoreState();

            _interactionState.UnblockInput();
            _isUndoing = false;
        }

        public void Clear()
        {
            _undoStack.Clear();
        }
    }
}
