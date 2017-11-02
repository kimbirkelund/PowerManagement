using System;

namespace Kimb.PowerManagement
{
    public class DurationPowerSettingValueParserSelector : IPowerSettingValueParserSelector
    {
        private static readonly IPowerSettingValueParser _fromIndexAsMilliseconds = DelegatingPowerSettingValueParser.FromValueIndex(valueIndex => TimeSpan.FromMilliseconds(valueIndex));
        private static readonly IPowerSettingValueParser _fromIndexAsSeconds = DelegatingPowerSettingValueParser.FromValueIndex(valueIndex => TimeSpan.FromSeconds(valueIndex));

        public IPowerSettingValueParser Select(Guid powerSchemeId, Guid powerSettingsGroupId, Guid powerSettingId, string powerSettingValueUnitSpecifier)
        {
            if (powerSettingValueUnitSpecifier == null)
                return null;

            switch (powerSettingValueUnitSpecifier.ToLowerInvariant())
            {
                case "seconds":
                case "second":
                    return _fromIndexAsSeconds;
                case "millisecond":
                case "milliseconds":
                    return _fromIndexAsMilliseconds;
                default:
                    return null;
            }
        }
    }
}