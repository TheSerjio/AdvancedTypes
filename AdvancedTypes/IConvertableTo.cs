namespace AdvancedTypes
{
    public interface IConvertableTo<T> : System.IComparable<T>
    {
        /// <summary>
        /// Must be constant
        /// </summary>
        public T Converted { get; }
    }
}