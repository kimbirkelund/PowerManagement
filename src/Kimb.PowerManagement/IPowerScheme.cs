using System;
using System.Collections.Immutable;

namespace Kimb.PowerManagement
{
    public interface IPowerScheme : IEquatable<IPowerScheme>
    {
        Guid Id { get; }
        string Name { get; }
        IImmutableList<IPowerSettingsGroup> SettingsGroups { get; }
    }
}