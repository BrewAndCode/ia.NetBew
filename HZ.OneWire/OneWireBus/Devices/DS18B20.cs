namespace HZ.OneWireBus.Devices
{
	/// <summary>
	/// A Maxim DS18B20 device
	/// </summary>
	public class DS18B20 : TemperatureDevice
	{
		/// <summary>
		/// The expected family code for this device
		/// </summary>
		public new static byte FamilyCode = 0x28;

		/// <summary>
		/// The ROM command to start a search for devices on the bus which have an alarm condition
		/// <remarks>Followed by the one wire search algorithm</remarks>
		/// </summary>
		public const byte AlarmSearch = 0xEC;

		/// <summary>
		/// The function command to start a temperature conversion
		/// <remarks>The device will hold the bus at 0 while the conversion takes place and will release the bus when it is finished</remarks>
		/// </summary>
		public const byte StartTemperatureConversion = 0x44;

		/// <summary>
		/// The function command to read from the scratch pad
		/// </summary>
		public const byte ReadScratchPad = 0xBE;

		/// <summary>
		/// The function command to write to the scratch pad
		/// <remarks>Followed by the data to write</remarks>
		/// </summary>
		public const byte WriteScratchPad = 0x4E;

		/// <summary>
		/// The function command to save the scratch pad to the EEPROM
		/// <remarks>If the device is in parasite power mode, the bus must be pulled high with a strong pull-up for 10ms</remarks>
		/// </summary>
		public const byte CopySratchPad = 0x48;

		/// <summary>
		/// The function command to recall the EEPROM data to the scratch pad
		/// </summary>
		public const byte RecallEEPROM = 0xB8;

		/// <summary>
		/// The function command to see if any devices on the bus are in parasite power mode
		/// <remarks>Any devices in parasite power mode will pull the bus low</remarks> 
		/// </summary>
		public const byte ReadPowerSupply = 0xB4;

		/// <summary>
		/// Creates a DS18B20
		/// </summary>
		/// <param name="oneWireBus">The one wire bus which this device belongs to</param>
		/// <param name="deviceAddress">The address of this device</param>
		public DS18B20(OneWireBus oneWireBus, DeviceAddress deviceAddress)
			: base(oneWireBus, deviceAddress)
		{
		}

		/// <summary>
		/// Queries the current temperature of the device
		/// </summary>
		/// <param name="asyncCall">If <c>false</c>, the one wire bus is locked while the query is performed, optional</param>
		/// <returns>The temperature in degrees C</returns>
		public override double GetTemperature(bool asyncCall = false)
		{
			try
			{
				if (!asyncCall)
				{
					OneWireBus.Grab();
				}

				if (Select())
				{
					OneWireBus.Write(StartTemperatureConversion);
					while (!OneWireBus.ReadBit())
					{
					}

					if (Select())
					{
						OneWireBus.Write(ReadScratchPad);

						ushort temperature = OneWireBus.ReadByte();
						temperature |= (ushort)(OneWireBus.ReadByte() << 8);

						if (!asyncCall)
						{
							OneWireBus.Release();
						}

						return temperature / 16.0;
					}
				}

				return double.MinValue;
			}
			finally
			{
				if (!asyncCall)
				{
					OneWireBus.Release();
				}
			}
		}
	}
}
