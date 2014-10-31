namespace Sekhmet.PowerManagement
{
    public class RawValueWithDescription
    {
        public object Description { get; private set; }
        public object Value { get; private set; }
        public uint ValueIndex { get; private set; }

        public RawValueWithDescription(object value, uint valueIndex, object description)
        {
            Value = value;
            ValueIndex = valueIndex;
            Description = description;
        }

        public override string ToString()
        {
            return new
                   {
                       Value,
                       ValueIndex,
                       Description
                   }.ToString();
        }
    }
}