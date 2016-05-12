// ReSharper disable InconsistentNaming
using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.Hardware;

namespace IA.NetBrew
{
    public static class Mappings
    {
        //One-Wire ROM Addresses.  These are 8 byte and used to address the one-wire devices on the bus
        public static readonly byte[] RomRIMSHotLiqourTun = {0x28, 0x14, 0xd3, 0x7e, 0x03, 0x00, 0x00, 0xd0};
        public static readonly byte[] RomHotLiqourTun =     {0x28, 0x62, 0xD7, 0x7E, 0x03, 0x00, 0x00, 0xE5};
        public static readonly byte[] RomRIMSMashTun =      {0x28, 0x9d, 0xe1, 0x7e, 0x03, 0x00, 0x00, 0xab};  
        public static readonly byte[] RomMashTun =          {0x28, 0xe8, 0xb7, 0x7e, 0x03, 0x00, 0x00, 0xd1};

        //Emergency Shut off values.  If any RIMS tube exceeds this temperature (in degress F) we will 
        //Sound the alarm and turn off the heating element
        public static readonly double RIMSShutoffTemp = 200.0;
        
        //Two-Wire IC2 Addresses
        public static readonly byte BTPDMashTun = 0x20;                 //BTDP handles loss of bit; use actual address displayed
        public static readonly byte BTPDHotLiqourTun = 0x21;            //BTDP handles loss of bit; use actual address displayed

        //Serial LCD I2C Addresses
        public static readonly ushort LCDMain = 40;
        public static readonly ushort LCDSecondary = 41;

        //IO Assignments.  
        public static readonly Cpu.Pin PinOW1 = (Cpu.Pin) (FEZ_Pin.Digital.IO19);       //ANY Digital IO Port
        public static readonly PWM.Pin PinRIMSMLT = PWM.Pin.PWM0;                       //Power Modulation Port
        public static readonly PWM.Pin PinRIMSHLT = PWM.Pin.PWM1;                       //Power Modulation Port
        
        public static OutputPort PumpRight = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.IO18,false);
        public static OutputPort PumpLeft = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.IO17,false);
        public static OutputPort Alarm = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.IO16, true);

        public static OutputPort Gas1 = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.IO23, true);
        public static OutputPort Gas2 = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.IO22, true);
        public static OutputPort Gas3 = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.IO21, true);
        //20 is last wired Relay

        //PWM Modulation Times in nanoseconds
        public static uint RIMSMLTPulseLength = 300000000;
    }
}
