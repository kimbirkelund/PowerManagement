using System;
using System.Collections.Immutable;

namespace Sekhmet.PowerManagement
{
    public interface IPowerSettingsGroup : IEquatable<IPowerSettingsGroup>
    {
        Guid Id { get; }
        string Name { get; }
        Guid SchemeId { get; }
        IImmutableList<IPowerSetting> Settings { get; }
    }
}