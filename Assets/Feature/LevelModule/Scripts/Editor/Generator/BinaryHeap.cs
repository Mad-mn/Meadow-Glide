using System;
using System.Collections.Generic;

namespace Feature.LevelModule.Scripts.Editor.Generator {
    public class BinaryHeap<T> where T : IComparable<T> {
        private readonly List<T> _data = new List<T>();

        public int Count => _data.Count;

        public void Push(T item) {
            _data.Add(item);
            SiftUp(_data.Count - 1);
        }

        public T Pop() {
            if (_data.Count == 0) throw new InvalidOperationException("Heap is empty");
            var top = _data[0];
            int last = _data.Count - 1;
            _data[0] = _data[last];
            _data.RemoveAt(last);
            if (_data.Count > 0) SiftDown(0);
            return top;
        }

        public T Peek() {
            if (_data.Count == 0) throw new InvalidOperationException("Heap is empty");
            return _data[0];
        }

        public void Clear() => _data.Clear();

        private void SiftUp(int i) {
            while (i > 0) {
                int parent = (i - 1) / 2;
                if (_data[i].CompareTo(_data[parent]) >= 0) break;
                Swap(i, parent);
                i = parent;
            }
        }

        private void SiftDown(int i) {
            int n = _data.Count;
            while (true) {
                int left = 2 * i + 1;
                int right = 2 * i + 2;
                int smallest = i;

                if (left < n && _data[left].CompareTo(_data[smallest]) < 0)
                    smallest = left;
                if (right < n && _data[right].CompareTo(_data[smallest]) < 0)
                    smallest = right;

                if (smallest == i) break;
                Swap(i, smallest);
                i = smallest;
            }
        }

        private void Swap(int a, int b) {
            var temp = _data[a];
            _data[a] = _data[b];
            _data[b] = temp;
        }
    }
}
