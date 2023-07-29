namespace MigraDoc.DocumentObjectModel.Tests
{
    public class SomethingCompletelyDifferent
    {
        public T? GetValue<T>(ref T field)
        {
            return default;
        }

        public void SetValue<T>(T value, ref T field)
        { }

        public int SomeValue
        {
            get => GetValue(ref _someValue);
            set => SetValue(value, ref _someValue);
        }
        int _someValue;
    }
}
