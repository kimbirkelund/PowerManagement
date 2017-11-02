using System;
using System.Collections.Immutable;

namespace Kimb.PowerManagement
{
    public interface IPowerSettingsGroup : IEquatable<IPowerSettingsGroup>
    {
        Guid Id { get; }
        string Name { get; }
        Guid SchemeId { get; }
        IImmutableList<IPowerSetting> Settings { get; }
    }
}