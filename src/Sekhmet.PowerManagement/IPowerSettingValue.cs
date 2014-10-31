namespace Sekhmet.PowerManagement
{
    public interface IPowerSettingValue
    {
        object ValueOnBattery { get; }
        object ValuePluggedIn { get; }
    }
}