namespace HZ.OneWireBus.Devices
{
	/// <summary>
	/// A basic one wire device
	/// </summary>
	public abstract class OneWireDevice
	{
		/// <summary>
		/// The expected family code for this device
		/// </summary>
		public static byte FamilyCode;

		/// <summary>
		/// The ROM command to start a search for devices on the bus
		/// <remarks>Followed by the one wire search algorithm</remarks>
		/// </summary>
		public const byte SearchROM = 0xF0;

		/// <summary>
		/// The ROM command to read the address of the one device on the bus
		/// <remarks>Can only be used when there is one device on the bus</remarks>
		/// </summary>
		public const byte ReadROM = 0x33;

		/// <summary>
		/// The ROM command to address a specific device by address
		/// <remarks>Followed by the device address</remarks>
		/// </summary>
		public const byte MatchROM = 0x55;

		/// <summary>
		/// The ROM command to address the one device on the bus
		/// <remarks>Can only be used when there is one device on the bus</remarks>
		/// </summary>
		public const byte SkipROM = 0xCC;

		/// <summary>
		/// The one wire bus which this device belongs to
		/// </summary>
		public readonly OneWireBus OneWireBus;

		/// <summary>
		/// The address of this device
		/// </summary>
		public readonly DeviceAddress DeviceAddress;

		/// <summary>
		/// Creates a OneWireDevice
		/// </summary>
		/// <param name="oneWireBus">The one wire bus which this device belongs to</param>
		/// <param name="deviceAddress">The address of this device</param>
		protected OneWireDevice(OneWireBus oneWireBus, DeviceAddress deviceAddress)
		{
			OneWireBus = oneWireBus;
			DeviceAddress = deviceAddress;
		}

		/// <summary>
		/// Sends the select command across the one wire bus
		/// <remarks>Automatically uses skip if there is only one device on the bus</remarks>
		/// </summary>
		/// <returns>Returns <c>true</c> if there are devices on the one wire bus</returns>
		protected bool Select()
		{
			if (!OneWireBus.Reset())
			{
				return false;
			}

			if (OneWireBus.Devices.Length > 1)
			{
				OneWireBus.Write(MatchROM);
				OneWireBus.Write(DeviceAddress.Address);
			}
			else
			{
				OneWireBus.Write(SkipROM);
			}

			return true;
		}
	}
}
