using System;
using System.Collections.Immutable;

namespace Kimb.PowerManagement
{
    internal sealed class DefaultPowerScheme : IPowerScheme
    {
        private readonly Lazy<IImmutableList<IPowerSettingsGroup>> _lazyPowerSettingsGroups;

        public Guid Id { get; private set; }
        public string Name { get; private set; }

        public IImmutableList<IPowerSettingsGroup> SettingsGroups
        {
            get { return _lazyPowerSettingsGroups.Value; }
        }

        public DefaultPowerScheme(Guid id, string name)
        {
            Id = id;
            Name = name;

            _lazyPowerSettingsGroups = new Lazy<IImmutableList<IPowerSettingsGroup>>(() => PowerApi.GetPowerSettingsGroups(Id));
        }

        public bool Equals(IPowerScheme other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IPowerScheme);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return new
                   {
                       Id,
                       Name
                   }.ToString();
        }

        public static bool operator ==(DefaultPowerScheme left, DefaultPowerScheme right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DefaultPowerScheme left, DefaultPowerScheme right)
        {
            return !Equals(left, right);
        }
    }
}