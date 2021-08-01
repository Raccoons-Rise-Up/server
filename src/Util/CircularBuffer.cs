namespace GameServer.Util
{
    public class CircularBuffer<T> {
        private T[] data;
        private int head;
        private int count;

        public int Size => count > Capacity ? Capacity : count;
        public readonly int Capacity;

        public T this[int i] {
             get => data[(head + i) % Capacity];
             set => data[(head + i) % Capacity] = value;
        }

        public CircularBuffer(int capacity) {
            Capacity = capacity;
            data = new T[capacity];
        }

        public void Push(T item) {
            data[count % Capacity] = item;
            
            if (++count > Capacity)
                head = count % Capacity;
        }
    }
}