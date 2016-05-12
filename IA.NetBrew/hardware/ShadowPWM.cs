using System;
using GHIElectronics.NETMF.Hardware.LowLevel;
using Microsoft.SPOT;

namespace IA.NetBrew.hardware
{
    /// <summary>
    /// PWM implementation that uses the shadow registers for a clean transition between changing duty.
    /// This is how it should be implemented in the GHI firmware :)
    /// 
    /// Programmed by Huysentruit Wouter
    /// Fastload-Media.be
    /// </summary>
    public class ShadowPWM : IDisposable
    {
        #region Consts

        const uint PINSEL4 = 0xE002C000 + 0x10;
        const uint PINSEL7 = 0xE002C000 + 0x1C;

        const uint PWM0_BASE_ADDR = 0xE0014000;
        const uint PWM0TCR = PWM0_BASE_ADDR + 0x04;
        const uint PWM0PR = PWM0_BASE_ADDR + 0x0C;
        const uint PWM0MCR = PWM0_BASE_ADDR + 0x14;
        const uint PWM0MR0 = PWM0_BASE_ADDR + 0x18;
        const uint PWM0MR1 = PWM0_BASE_ADDR + 0x1C;
        const uint PWM0MR2 = PWM0_BASE_ADDR + 0x20;
        const uint PWM0MR3 = PWM0_BASE_ADDR + 0x24;
        const uint PWM0MR4 = PWM0_BASE_ADDR + 0x40;
        const uint PWM0MR5 = PWM0_BASE_ADDR + 0x44;
        const uint PWM0MR6 = PWM0_BASE_ADDR + 0x48;
        const uint PWM0PCR = PWM0_BASE_ADDR + 0x4C;
        const uint PWM0LER = PWM0_BASE_ADDR + 0x50;

        const uint PWM1_BASE_ADDR = 0xE0018000;
        const uint PWM1TCR = PWM1_BASE_ADDR + 0x04;
        const uint PWM1PR = PWM1_BASE_ADDR + 0x0C;
        const uint PWM1MCR = PWM1_BASE_ADDR + 0x14;
        const uint PWM1MR0 = PWM1_BASE_ADDR + 0x18;
        const uint PWM1MR1 = PWM1_BASE_ADDR + 0x1C;
        const uint PWM1MR2 = PWM1_BASE_ADDR + 0x20;
        const uint PWM1MR3 = PWM1_BASE_ADDR + 0x24;
        const uint PWM1MR4 = PWM1_BASE_ADDR + 0x40;
        const uint PWM1MR5 = PWM1_BASE_ADDR + 0x44;
        const uint PWM1MR6 = PWM1_BASE_ADDR + 0x48;
        const uint PWM1PCR = PWM1_BASE_ADDR + 0x4C;
        const uint PWM1LER = PWM1_BASE_ADDR + 0x50;

        #endregion

        #region Enums

        internal enum Device
        {
            EMX
        }

        #endregion

        #region Pins

        public class Pin
        {
            internal Device Device { get; private set; }
            internal byte Modulator { get; private set; }
            internal byte Channel { get; private set; }

            private Pin(Device device, byte modulator, byte channel)
            {
                Device = device;
                Modulator = modulator;
                Channel = channel;
            }

            public static readonly Pin EmxPWM0 = new Pin(Device.EMX, 0, 1);   // P3.16 / PWM0[1]
            public static readonly Pin EmxPWM1 = new Pin(Device.EMX, 1, 1);   // P3.24 / PWM1[1]
            public static readonly Pin EmxPWM2 = new Pin(Device.EMX, 0, 2);   // P3.17 / PWM0[2]
            public static readonly Pin EmxPWM3 = new Pin(Device.EMX, 1, 3);   // P3.26 / PWM1[3]
            public static readonly Pin EmxPWM4 = new Pin(Device.EMX, 1, 4);   // P3.27 / PWM1[4]
            public static readonly Pin EmxPWM5 = new Pin(Device.EMX, 1, 6);   // P3.29 / PWM1[6]
        }

        #endregion

        #region Declarations

        // Registers for modulator 0
        private static Register ler0 = new Register(PWM0LER);
        private static Register tcr0 = new Register(PWM0TCR);
        private static Register mr00 = new Register(PWM0MR0);
        private static Register pcr0 = new Register(PWM0PCR);
        private static Register mcr0 = new Register(PWM0MCR);

        // Registers for modulator 1
        private static Register ler1 = new Register(PWM1LER);
        private static Register tcr1 = new Register(PWM1TCR);
        private static Register mr10 = new Register(PWM1MR0);
        private static Register pcr1 = new Register(PWM1PCR);
        private static Register mcr1 = new Register(PWM1MCR);

        // Declarations for pin
        private Pin pin;
        private Register ler = null;
        private Register mr0 = null;
        private Register mr = null;

        private static object syncLock = new object();

        #endregion

        #region Construction / destruction

        static ShadowPWM()
        {
            Register reg;
            reg = new Register(PWM0PR); reg.Write(0);
            reg = new Register(PWM1PR); reg.Write(0);

            // reset
            tcr0.Write(0x02);   // Counter reset
            tcr1.Write(0x02);   // Counter reset

            pcr0.Write(0x00);   // Disable all PWM outputs
            pcr1.Write(0x00);   // Disable all PWM outputs

            mcr0.Write(0x02);   // Reset on PWMMR0
            mcr1.Write(0x02);   // Reset on PWMMR0
        }

        public ShadowPWM(Pin pin)
        {
            this.pin = pin;

            Init();
        }

        public void Dispose()
        {
            Deinit();
        }

        #endregion

        #region Private methods

        private void Init()
        {
            lock (syncLock)
            {
                if (pin.Modulator == 0)
                {
                    // Enable PWM function on pin
                    switch (pin.Device)
                    {
                        case Device.EMX:
                            Register pinsel = new Register(PINSEL7);
                            int offset = (pin.Channel - 1) << 1;
                            pinsel.ClearBits(0x03U << offset);
                            pinsel.SetBits(0x02U << offset);
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    ler = ler0;
                    mr0 = mr00;
                    tcr0.Write(0x09); // Enable counter and PWM

                    switch (pin.Channel)
                    {
                        case 1: mr = new Register(PWM0MR1); break;
                        case 2: mr = new Register(PWM0MR2); break;
                        case 3: mr = new Register(PWM0MR3); break;
                        case 4: mr = new Register(PWM0MR4); break;
                        case 5: mr = new Register(PWM0MR5); break;
                        case 6: mr = new Register(PWM0MR6); break;
                    }

                    // Enable PWM output
                    pcr0.SetBits(1U << (8 + pin.Channel));
                }
                else
                {
                    // Enable PWM function on pin
                    switch (pin.Device)
                    {
                        case Device.EMX:
                            Register pinsel = new Register(PINSEL7);
                            int offset = ((pin.Channel - 1) << 1) + 16;
                            uint temp = pinsel.Read();
                            pinsel.ClearBits(0x03U << offset);
                            pinsel.SetBits(0x03U << offset);
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    ler = ler1;
                    mr0 = mr10;
                    tcr1.Write(0x09); // Enable counter and PWM

                    switch (pin.Channel)
                    {
                        case 1: mr = new Register(PWM1MR1); break;
                        case 2: mr = new Register(PWM1MR2); break;
                        case 3: mr = new Register(PWM1MR3); break;
                        case 4: mr = new Register(PWM1MR4); break;
                        case 5: mr = new Register(PWM1MR5); break;
                        case 6: mr = new Register(PWM1MR6); break;
                    }

                    // Enable PWM output
                    pcr1.SetBits(1U << (8 + pin.Channel));
                }

                mr.Write(0);
                ler.Write(1U << pin.Channel);
            }
        }

        private void Deinit()
        {
            lock (syncLock)
            {
                if (pin == null)
                    throw new ObjectDisposedException();

                if (pin.Modulator == 0)
                {
                    // Disable PWM output
                    pcr0.ClearBits(1U << (8 + pin.Channel));

                    // Disable PWM function on pin (select normal GPIO)
                    switch (pin.Device)
                    {
                        case Device.EMX:
                            Register pinsel = new Register(PINSEL7);
                            int offset = (pin.Channel - 1) << 1;
                            pinsel.ClearBits(0x03U << offset);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                else
                {
                    // Disable PWM output
                    pcr1.SetBits(1U << (8 + pin.Channel));

                    // Disable PWM function on pin (select normal GPIO)
                    switch (pin.Device)
                    {
                        case Device.EMX:
                            Register pinsel = new Register(PINSEL7);
                            int offset = ((pin.Channel - 1) << 1) + 16;
                            uint temp = pinsel.Read();
                            pinsel.ClearBits(0x03U << offset);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }

                ler = null;
                mr = null;
                pin = null;
            }
        }

        #endregion

        #region Public methods

        public void SetPulse(uint period_nanosecond, uint highTime_nanosecond)
        {
            // Adjust for 72 MHz / 4 clock
            period_nanosecond *= 9;
            period_nanosecond /= 500;
            highTime_nanosecond *= 9;
            highTime_nanosecond /= 500;

            lock (syncLock)
            {
                if (pin == null)
                    throw new ObjectDisposedException();

                mr0.Write(period_nanosecond);           // Set the clock
                mr.Write(highTime_nanosecond);          // Set duty
                ler.SetBits(1U | (1U << pin.Channel));  // Set latch enable bits for clock and duty
            }
        }

        #endregion
    }
}
