namespace GameServerCore.Content
{
    public class Vector<T> where T : struct
    {
        public T X;
        public T Y;
        public T Z;

        public void ForceSize(int size)
        {
            if (size <= 2)
            {
                Z = default(T);
            }
            if (size <= 1)
            {
                Y = default(T);
            }
            if (size == 0)
            {
                X = default(T);
            }
        }
    }
}