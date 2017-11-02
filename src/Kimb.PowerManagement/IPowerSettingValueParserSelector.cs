using System;

namespace Kimb.PowerManagement
{
    public interface IPowerSettingValueParserSelector
    {
        IPowerSettingValueParser Select(Guid powerSchemeId, Guid powerSettingsGroupId, Guid powerSettingId, string powerSettingValueUnitSpecifier);
    }
}