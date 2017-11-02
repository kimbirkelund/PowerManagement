using System;

namespace Kimb.PowerManagement
{
    public interface IPowerSetting : IEquatable<IPowerSetting>
    {
        Guid Id { get; }
        string Name { get; }
        Guid SchemeId { get; }
        Guid SettingsGroupId { get; }
        IPowerSettingValue Value { get; }
    }
}