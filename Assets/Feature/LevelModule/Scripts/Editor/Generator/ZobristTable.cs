using System;

namespace Feature.LevelModule.Scripts.Editor.Generator {
    public class ZobristTable {
        private readonly ulong[,,] _table;
        private readonly int _ringCount;
        private readonly int _sectorCount;
        private readonly int _maxColors;

        public ZobristTable(int ringCount, int sectorCount, int maxColors) {
            _ringCount = ringCount;
            _sectorCount = sectorCount;
            _maxColors = maxColors;
            _table = new ulong[ringCount, sectorCount, maxColors];

            var rng = new Random(12345);
            byte[] buffer = new byte[8];
            for (int r = 0; r < ringCount; r++)
                for (int s = 0; s < sectorCount; s++)
                    for (int c = 0; c < maxColors; c++) {
                        rng.NextBytes(buffer);
                        _table[r, s, c] = BitConverter.ToUInt64(buffer, 0);
                    }
        }

        public ulong ComputeHash(byte[,] colors) {
            ulong hash = 0;
            for (int r = 0; r < _ringCount; r++)
                for (int s = 0; s < _sectorCount; s++) {
                    int colorIndex = colors[r, s];
                    if (colorIndex > 0 && colorIndex < _maxColors)
                        hash ^= _table[r, s, colorIndex];
                }
            return hash;
        }

        public ulong RotateHash(ulong currentHash, int ringIndex, int offset, int sectorCount) {
            return currentHash;
        }

        public ulong SlideHash(ulong currentHash, int sectorIndex, int startRing, int endRing, int offset, byte[,] colors) {
            return currentHash;
        }
    }
}
