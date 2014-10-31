using System;

namespace Sekhmet.PowerManagement
{
    internal class DefaultPowerSetting : IPowerSetting
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public Guid SchemeId { get; private set; }
        public Guid SettingsGroupId { get; private set; }

        public IPowerSettingValue Value
        {
            get { return PowerApi.ReadPowerSettingValue(SchemeId, SettingsGroupId, Id); }
        }

        public DefaultPowerSetting(Guid schemeId, Guid settingsGroupId, Guid id, string name)
        {
            SchemeId = schemeId;
            SettingsGroupId = settingsGroupId;
            Id = id;
            Name = name;
        }

        public bool Equals(IPowerSetting other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return SchemeId.Equals(other.SchemeId) && SettingsGroupId.Equals(other.SettingsGroupId) && Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IPowerSetting);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SchemeId.GetHashCode();
                hashCode = (hashCode * 397) ^ SettingsGroupId.GetHashCode();
                hashCode = (hashCode * 397) ^ Id.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return new
                   {
                       SchemeId,
                       SettingsGroupId,
                       Id,
                       Name
                   }.ToString();
        }

        public static bool operator ==(DefaultPowerSetting left, DefaultPowerSetting right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DefaultPowerSetting left, DefaultPowerSetting right)
        {
            return !Equals(left, right);
        }
    }
}