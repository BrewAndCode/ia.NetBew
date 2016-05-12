using System.Text;
using IA.NetBrew.Helpers;
using Microsoft.SPOT.Hardware;

namespace IA.NetBrew.hardware
{
    /// <summary>
    /// Brewtroller PID (BTPD) Display I2C Hardware wrapping class
    /// </summary>
    public class BTPD
    {
        public byte Address { get; private set; }
        public const byte ClockRate = 100; //Khz
        private const int TransactionTimeout = 1000; //ms
        private readonly I2CDevice.Configuration _slaveConfig;
        private double _tempActual;
        private double _tempSet;
        private bool _isChanged;



        /// <summary>
        /// Constructor class, pass the ACTUAL BTPD address displayed on the hardware when 
        /// you press the set address button.  Do not shift this register; the BTDP harware
        /// takes that into account for you.
        /// </summary>
        /// <param name="address">byte value of the hardware I2C bus address</param>
        public BTPD(byte address)
        {
            _isChanged = false;
            Address = address;
            _slaveConfig = new I2CDevice.Configuration(address, ClockRate);
        }

        /// <summary>
        /// Public Temperature Set, double value that is displayed on the second 
        /// line of the sensor.
        /// </summary>
        public double TempSet
        {
            get { return _tempSet; }
            set
            {
                if (value != _tempSet)
                    _isChanged = true;
                _tempSet = value;
            }
        }
        /// <summary>
        /// Public Temperature Actual, double value that is displayed on the 
        /// first line of the sensor
        /// </summary>
        public double TempActual
        {
            get { return _tempActual; }
            set
            {
                if (value != _tempActual)
                    _isChanged = true;
                _tempActual = value;
            }
        }

        

        /// <summary>
        /// Updates the sensor temperature values and writes to sensor if values have changed.
        /// Resolution is hard coded to 1 decimal place.  
        /// </summary>
       public void Update()
       {
           if (!_isChanged) return;
           var stringActual = StringHelper.PadLeft(_tempActual.ToString("N1"),4);
           var stringSet = StringHelper.PadLeft(_tempSet.ToString("N1"), 4);
           I2CBus.GetInstance().Write(_slaveConfig,Encoding.UTF8.GetBytes(stringActual+stringSet),TransactionTimeout);
       }

        public void ShowErr(string err)
        {
            I2CBus.GetInstance().Write(_slaveConfig,Encoding.UTF8.GetBytes("err " + err),1000);
        }
        public void ShowInit()
        {
            I2CBus.GetInstance().Write(_slaveConfig, Encoding.UTF8.GetBytes("8.8:8.88.8:8.8"), 1000);
        }

    }
}
