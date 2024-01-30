namespace Ford.WebApi.DTOs.Outgoing
{
    public class RetrieveArray<T>
    {
        public T[] Items { get; set; } = null!;

        public RetrieveArray()
        {
            Items = new T[0];
        }

        public RetrieveArray(T[] array)
        {
            Items = array;
        }
    }
}
