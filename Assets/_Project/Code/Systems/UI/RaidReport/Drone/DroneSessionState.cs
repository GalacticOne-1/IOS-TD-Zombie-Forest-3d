
namespace Galactic1.Code.UI.RaidReport.Drone
{
    public class DroneSessionState
    {
        public int ChargesMax;      // из DroneConfig
        public int ChargesLeft;     // сбрасывается при возврате в лагерь

        public bool HasCharges   => ChargesLeft > 0;
        public bool IsLastCharge => ChargesLeft == 1;
        public bool IsExhausted  => ChargesLeft == 0;

        // Фабричный метод — создаётся из конфига в начале сессии
        public static DroneSessionState FromConfig(int currLimit, int maxLimit) => new()
        {
            ChargesMax = maxLimit,
            ChargesLeft = currLimit,
        };
    }
}