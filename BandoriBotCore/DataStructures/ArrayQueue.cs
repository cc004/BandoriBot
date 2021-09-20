namespace BandoriBot.DataStructures
{
    public class ArrayQueue<T>
    {
        private readonly T[] array;
        private readonly int size;
        private int head = 0;

        public ArrayQueue(int size)
        {
            this.size = size;
            array = new T[size];
        }

        public T this[int index] => array[(head - index + size) % size];
        public void Enqueue(T t)
        {
            array[(head++) % size] = t;
        }
    }
}
