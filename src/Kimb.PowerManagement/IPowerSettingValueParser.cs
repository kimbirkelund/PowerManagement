using System;

namespace Kimb.PowerManagement
{
    public interface IPowerSettingValueParser
    {
        object Parse(Func<object> valueReader, Func<uint> valueIndexReader, Func<uint> valueMinReader, Func<uint> valueMaxReader, Func<uint> valueIncrementReader);
    }
}