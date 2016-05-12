using System.Text;
using System.Threading;
using Microsoft.SPOT.Hardware;
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local

namespace IA.NetBrew.hardware
{
 public class NewHavenSerialLCD
    {
        public enum Line
        { 
            Line1=0x00,
            Line2=0x40,
            Line3=0x14,
            Line4=0x54
        }

        private readonly I2CDevice.Configuration _slaveConfig;
        private const byte COMMAND          = 0xFE;
        private const byte DISPLAY_ON       = 0x41;
        private const byte DISPLAY_OFF      = 0x42;
        private const byte SET_CURSOR = 0x45;
        private const byte CURSOR_HOME = 0x46;
        private const byte UNDERLINE_ON = 0x47;
        private const byte UNDERLINE_OFF = 0x48;
        private const byte MOVE_LEFT = 0x49;
        private const byte MOVE_RIGHT = 0x4a;
        private const byte BLINK_ON = 0x4B;
        private const byte BLINK_OFF = 0x4c;
        private const byte BACKSPACE = 0x4E;
        private const byte CLEAR = 0x51;
        private const byte CONTRAST = 0x52;
        private const byte BACKLIGHT = 0x53;
        private const byte SHIFT_LEFT = 0x55;
        private const byte SHIFT_RIGHT = 0x56;
        private const byte SHOW_I2CAddress = 0x72;
        private const byte SET_I2CAddress = 0x62;
        private const byte SHOW_FIRMWARE = 0x70;

        private bool _underline;
        public bool Underline
        {
            get { return _underline; }
            set
            {
                _underline = value;
                Transmit(_underline ? new[] {COMMAND, UNDERLINE_ON} : new[] {COMMAND, UNDERLINE_OFF});
            }
        }

        private bool _display;
        public bool Display
        {
            get { return _display; }
            set { 
                _display = value;
                Transmit(_display ? new[] {COMMAND, DISPLAY_ON} : new[] {COMMAND, DISPLAY_OFF});
            }
        }


        public void ShowAddress()
        {
            Transmit(new[] {COMMAND, SHOW_I2CAddress});
        }
        public void ShowFirmware()
        {
             Transmit(new[] {COMMAND, SHOW_FIRMWARE});
        }

        public void Clear()
        {
            Transmit(new[] {COMMAND, CLEAR});
        }

        public bool Write(string writeString)
        {
            return Transmit(Encoding.UTF8.GetBytes(writeString));
        }

        private bool Transmit(byte[] commands)
        {
            return I2CBus.GetInstance().Write(_slaveConfig, commands, 1000);
        }

        public NewHavenSerialLCD(ushort Address, int ClockSpeed)
        {
            _slaveConfig = new I2CDevice.Configuration(Address, ClockSpeed);
        }

        public bool SetPosition(byte postion)
        {
            return Transmit(new[] {COMMAND, SET_CURSOR, postion});
        }

        public bool WriteLine(Line line,string writestring)
        {
            Transmit(new[] {COMMAND, SET_CURSOR, (byte)line});
            return Transmit(Encoding.UTF8.GetBytes(writestring));

        }


        public void ShiftLeft()
        {
            Transmit(new[] {COMMAND, SHIFT_LEFT});
        }
        public void ShiftRight()
        {
            Transmit(new[] {COMMAND, SHIFT_RIGHT});
        }
        public void CursorHome()
        {
            Transmit(new[] {COMMAND, CURSOR_HOME});
        }
        public void BackSpace()
        {
            Transmit(new[] {COMMAND, BACKSPACE});
        }

        public void SetContrast(byte contrast)
        {
            Transmit(new[] {COMMAND,CONTRAST, contrast});
        }

        public void SetI2CAddress(byte Address)
        {
            Transmit(new[] {COMMAND, SET_I2CAddress, Address});
            
            Thread.Sleep(1000);
        }

        public void SetBacklight(byte backlight)
        {
            Transmit(new[] {COMMAND, BACKLIGHT, backlight});
        }

       
    }
}
