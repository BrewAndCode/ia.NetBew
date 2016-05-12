using System;
using System.Collections;
using GHIElectronics.NETMF.Hardware;
using Microsoft.SPOT;

namespace IA.NetBrew.hardware
{
    public  class DS1820
    {
        //ROM Identifiers for specific DS1820s.  This is hardware-unique and must be modified to match
        public static readonly byte[] RomMashTun = { 0x28, 0x44, 0xCC, 0x7E, 0x03, 0x00, 0x00, 0xED };
        public static readonly byte[] RomHotLiqourTun = { 0x28, 0x62, 0xD7, 0x7E, 0x03, 0x00, 0x00, 0xE5 };

        public const byte ConvertT = 0x44;
        public const byte CopyScratchpad = 0x48;
        public const byte WriteScratchpad = 0x4E;
        public const byte ReadPowerSupply = 0xB4;
        public const byte RecallE2 = 0xB8;
        public const byte ReadScratchpad = 0xBE;
        public const byte FamilyCode = 0x10;
        public const byte SkipRom = 0xCC;
        public const byte AddressRom = 0x55;
        public const byte SearchRom = 0xF0;
        public const int ScratchPadSize = 9;
        public const int CRCByte = 8;

        public enum Resolution
        {
            NineBit=0,
            TenBit,
            ElevenBit,
            TwelveBit
        }
        public static readonly int[] ConversionTime = {94, 188, 375, 750};


        public static ArrayList GetDeviceROMs(OneWire ow)
        {
            var retval = new ArrayList();
            ow.Reset();
            ow.WriteByte(SearchRom);
            ow.Search_Restart();
            var newrom= new byte[8];
            while(ow.Search_GetNextDevice(newrom))
            {
                Debug.Print(ToHex(newrom));
                retval.Add(newrom);
            }
            return retval;
        }
        public static string ToHex(byte[] bytes)
        {
            var c = new char[bytes.Length * 2];
            for (int bx = 0, cx = 0; bx < bytes.Length; ++bx, ++cx)
            {
                var b = ((byte)(bytes[bx] >> 4));
                c[cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
                b = ((byte)(bytes[bx] & 0x0F));
                c[++cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
            }
            return new string(c);
        }
        public static void SendCommand(byte[] addr, byte command, OneWire ow)
        {
            ow.Reset();
            ow.WriteByte(0x55);
            ow.Write(addr, 0, 8);
            ow.WriteByte(command);
        }
        public static double GetTemperature(OneWire ow, byte[] addr)
        {
            SendCommand(addr, ConvertT, ow);
            while (ow.ReadByte() == 0)
            {
                //Do Nothing but Wait For BUS
                //TODO Add timeout waiting and error report
            }
            SendCommand(addr, ReadScratchpad, ow);
            ushort temp = ow.ReadByte();
            temp |= (ushort)(ow.ReadByte() << 8);
            var tempf = (1.80 * (temp / 16.00)) + 32.00;
            return tempf;
        }
    }
}
