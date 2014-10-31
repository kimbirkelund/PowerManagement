using System;

namespace Sekhmet.PowerManagement
{
    public interface IPowerSettingValueParserSelector
    {
        IPowerSettingValueParser Select(Guid powerSchemeId, Guid powerSettingsGroupId, Guid powerSettingId, string powerSettingValueUnitSpecifier);
    }
}