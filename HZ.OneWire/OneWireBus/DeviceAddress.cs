using System;

namespace HZ.OneWireBus
{
	/// <summary>
	/// Holds the address of a one wire device
	/// </summary>
	public class DeviceAddress
	{
		/// <summary>
		/// The raw 8 byte address of the device
		/// <remarks>The address is stored in a reversed byte order (As it comes from the one wire bus)</remarks>
		/// </summary>
		public byte[] Address;

		/// <summary>
		/// The 1 byte family code
		/// </summary>
		public byte FamilyCode
		{
			get
			{
				return Address[0];
			}
		}

		/// <summary>
		/// The 6 byte serial number
		/// <remarks>The serial number is returned in a reversed byte order (As it comes from the one wire bus)</remarks>
		/// </summary>
		public byte[] SerialNumber
		{
			get
			{
				byte[] serialNumber = new byte[6];
				Array.Copy(Address, 1, serialNumber, 0, 6);
				return serialNumber;
			}
		}

		/// <summary>
		/// The 1 byte CRC
		/// </summary>
		public byte CRC
		{
			get
			{
				return Address[7];
			}
		}

		/// <summary>
		/// Creates a shallow copy of another DeviceAddress object
		/// </summary>
		/// <param name="address">The DeviceAddress to copy</param>
		public DeviceAddress(DeviceAddress address)
			: this(address.Address)
		{
		}

		/// <summary>
		/// Creates a new DeviceAddress
		/// </summary>
		/// <param name="address">The raw 8 byte address of the device, in a reversed byte order (As it comes from the one wire bus)</param>
		public DeviceAddress(params byte[] address)
		{
			Address = new byte[8];
			address.CopyTo(Address, 0);
		}

		/// <summary>
		/// Equality operator
		/// </summary>
		/// <param name="address1">The first object to compare</param>
		/// <param name="address2">The second object to compare</param>
		/// <returns>Returns <c>true</c> if the objects are equivalent</returns>
		public static bool operator ==(DeviceAddress address1, DeviceAddress address2)
		{
			int address1Length = address1.Address.Length;
			if (address1Length != address2.Address.Length)
			{
				return false;
			}

			for (int index = 0; index < address1Length; index++)
			{
				if (address1.Address[index] != address2.Address[index])
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Inequality operator
		/// </summary>
		/// <param name="address1">The first object to compare</param>
		/// <param name="address2">The second object to compare</param>
		/// <returns>Returns <c>true</c> if the objects are not equivalent</returns>
		public static bool operator !=(DeviceAddress address1, DeviceAddress address2)
		{
			return !(address1 == address2);
		}

		/// <summary>
		/// Compares this DeviceAddress with another object
		/// </summary>
		/// <param name="other">The object to compare to this DeviceAddress</param>
		/// <returns>Returns <c>true</c> if the objects are equivalent</returns>
		public override bool Equals(object other)
		{
			return other is DeviceAddress && Equals((DeviceAddress)other);
		}

		/// <summary>
		/// Compares this DeviceAddress with another DeviceAddress
		/// </summary>
		/// <param name="other">The DeviceAddress to compare to this DeviceAddress</param>
		/// <returns>Returns <c>true</c> if the objects are equivalent</returns>
		public bool Equals(DeviceAddress other)
		{
			return this == other;
		}

		/// <summary>
		/// Produces a hash code which represents this DeviceAddress
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
		{
			int hashCode = 0;
			foreach (byte addressByte in Address)
			{
				hashCode = (hashCode * 31) ^ addressByte;
			}

			return hashCode;
		}

		/// <summary>
		/// Produces a friendly string representing this DeviceAddress
		/// </summary>
		/// <returns>The friendly string</returns>
		public override string ToString()
		{
			string hex = "Family Code: " + FamilyCode + ", Serial Number: ";

			foreach (byte byteValue in SerialNumber)
			{
				hex += byteValue + ", ";
			}

			hex += "CRC: " + CRC;

			return hex;
		}
	}
}
