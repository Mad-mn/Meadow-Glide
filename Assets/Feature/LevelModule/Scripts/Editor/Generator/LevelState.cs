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
        public int RingCount => Colors.GetLength(0);
        public int SectorCount => Colors.GetLength(1);

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

        public LevelState Rotate(int ringIndex, int direction) {
            var next = new LevelState(this);
            int sectors = SectorCount;
            for (int s = 0; s < sectors; s++) {
                int targetS = (s + direction + sectors) % sectors;
                next.Colors[ringIndex, targetS] = Colors[ringIndex, s];
            }
            return next;
        }

        public LevelState Slide(int sectorIndex, int startRing, int endRing, int direction) {
            var next = new LevelState(this);
            int count = endRing - startRing + 1;
            for (int r = 0; r < count; r++) {
                int currentR = startRing + r;
                int targetR = startRing + (r + direction + count) % count;
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
        public int Direction; // 1 or -1

        public override string ToString() => $"{Type}({Index}, {Direction})";
    }
}
