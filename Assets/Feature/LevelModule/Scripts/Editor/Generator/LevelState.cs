using System;
using System.Collections.Generic;
using System.Linq;
using Feature.ColorServiceModule.Scripts;

namespace Feature.LevelModule.Scripts.Editor.Generator {
    /// <summary>
    /// Lightweight representation of a level state for fast mathematical calculations.
    /// </summary>
    public struct LevelState : IEquatable<LevelState> {
        public byte[,] Colors; // [ringIndex, sectorIndex]
        public int RingCount => Colors != null ? Colors.GetLength(0) : 0;
        public int SectorCount => Colors != null ? Colors.GetLength(1) : 0;

        public LevelState(int rings, int sectors) {
            Colors = new byte[rings, sectors];
        }

        public LevelState(LevelState other) {
            Colors = (byte[,])other.Colors.Clone();
        }

        public bool IsSolved() {
            for (int r = 0; r < RingCount; r++) {
                byte firstColor = Colors[r, 0];
                for (int s = 1; s < SectorCount; s++) {
                    if (Colors[r, s] != firstColor) return false;
                }
            }
            return true;
        }

        public LevelState Rotate(int ringIndex, int offset) {
            var next = new LevelState(this);
            int sectors = SectorCount;
            offset = (offset % sectors + sectors) % sectors;
            if (offset == 0) return next;

            for (int s = 0; s < sectors; s++) {
                int targetS = (s + offset) % sectors;
                next.Colors[ringIndex, targetS] = Colors[ringIndex, s];
            }
            return next;
        }

        public LevelState Slide(int sectorIndex, int startRing, int endRing, int offset) {
            var next = new LevelState(this);
            int count = endRing - startRing + 1;
            offset = (offset % count + count) % count;
            if (offset == 0) return next;

            for (int r = 0; r < count; r++) {
                int currentR = startRing + r;
                int targetR = startRing + (r + offset) % count;
                next.Colors[targetR, sectorIndex] = Colors[currentR, sectorIndex];
            }
            return next;
        }

        public bool Equals(LevelState other) {
            if (Colors == null || other.Colors == null) return false;
            if (RingCount != other.RingCount || SectorCount != other.SectorCount) return false;
            
            for (int r = 0; r < RingCount; r++) {
                for (int s = 0; s < SectorCount; s++) {
                    if (Colors[r, s] != other.Colors[r, s]) return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj) => obj is LevelState other && Equals(other);

        public override int GetHashCode() {
            unchecked {
                if (Colors == null) return 0;
                int hash = 17;
                for (int r = 0; r < RingCount; r++) {
                    for (int s = 0; s < SectorCount; s++) {
                        hash = hash * 31 + Colors[r, s];
                    }
                }
                return hash;
            }
        }
    }

    public enum MoveType { Rotate, Slide }

    public struct Move {
        public MoveType Type;
        public int Index; // ringIndex for Rotate, areaIndex for Slide
        public int Offset; // Offset value for the move

        public override string ToString() => $"{Type}({Index}, offset:{Offset})";
    }
}
