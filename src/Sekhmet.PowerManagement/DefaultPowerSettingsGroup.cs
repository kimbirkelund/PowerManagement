using System;
using System.Collections.Immutable;

namespace Sekhmet.PowerManagement
{
    internal class DefaultPowerSettingsGroup : IPowerSettingsGroup
    {
        private readonly Lazy<IImmutableList<IPowerSetting>> _lazyPowerSettings;

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public Guid SchemeId { get; private set; }

        public IImmutableList<IPowerSetting> Settings
        {
            get { return _lazyPowerSettings.Value; }
        }

        public DefaultPowerSettingsGroup(Guid schemeId, Guid id, string name)
        {
            SchemeId = schemeId;
            Id = id;
            Name = name;

            _lazyPowerSettings = new Lazy<IImmutableList<IPowerSetting>>(() => PowerApi.GetPowerSettings(SchemeId, Id));
        }

        public bool Equals(IPowerSettingsGroup other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return SchemeId.Equals(other.SchemeId) && Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IPowerSettingsGroup);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (SchemeId.GetHashCode() * 397) ^ Id.GetHashCode();
            }
        }

        public override string ToString()
        {
            return new
                   {
                       SchemeId,
                       Id,
                       Name
                   }.ToString();
        }

        public static bool operator ==(DefaultPowerSettingsGroup left, DefaultPowerSettingsGroup right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DefaultPowerSettingsGroup left, DefaultPowerSettingsGroup right)
        {
            return !Equals(left, right);
        }
    }
}