using System;

namespace Sekhmet.PowerManagement
{
    public class DelegatingPowerSettingValueParser : IPowerSettingValueParser
    {
        private readonly Func<Func<object>, Func<uint>, Func<uint>, Func<uint>, Func<uint>, object> _parser;

        private DelegatingPowerSettingValueParser(Func<Func<object>, Func<uint>, Func<uint>, Func<uint>, Func<uint>, object> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            _parser = parser;
        }

        public object Parse(Func<object> valueReader, Func<uint> valueIndexReader, Func<uint> valueMinReader, Func<uint> valueMaxReader, Func<uint> valueIncrementReader)
        {
            return _parser(valueReader,
                           valueIndexReader,
                           valueMinReader,
                           valueMaxReader,
                           valueIncrementReader);
        }

        public static IPowerSettingValueParser FromValueIndex(Func<uint, object> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            return new DelegatingPowerSettingValueParser((p1, valueIndexReader, p2, p3, p4) => parser(valueIndexReader()));
        }
    }
}