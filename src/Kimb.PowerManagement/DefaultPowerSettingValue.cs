namespace Kimb.PowerManagement
{
    internal class DefaultPowerSettingValue : IPowerSettingValue
    {
        public object ValueOnBattery { get; private set; }
        public object ValuePluggedIn { get; private set; }

        public DefaultPowerSettingValue(object valuePluggedIn, object valueOnBattery)
        {
            ValuePluggedIn = valuePluggedIn;
            ValueOnBattery = valueOnBattery;
        }

        public override string ToString()
        {
            return new
                   {
                       ValuePluggedIn,
                       ValueOnBattery
                   }.ToString();
        }
    }
}