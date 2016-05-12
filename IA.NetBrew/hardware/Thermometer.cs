using System;
using System.Collections;
using GHIElectronics.NETMF.Hardware;
using Microsoft.SPOT;

namespace IA.NetBrew.hardware
{
    public class Thermometer
    {
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
        public enum TempResolution
        {
            NineBit = 0,
            TenBit,
            ElevenBit,
            TwelveBit
        }
        public static readonly int[] ConversionTime = { 94, 188, 375, 750 };


        public byte[] Address { get; set; }
        public TempResolution Resolution { get; set; }

        public OneWire OW;

        public Thermometer(byte[] inAddress, OneWire inOw)
        {
            Address = inAddress;
            OW = inOw;
            
        }

        private void ResetandAddress()
        {
            
        }
    }
}
