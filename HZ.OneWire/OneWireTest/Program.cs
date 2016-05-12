using GHIElectronics.NETMF.Hardware;
using HZ.OneWireBus;
using HZ.OneWireBus.Devices;
using Microsoft.SPOT;

namespace HZ.OneWireTest
{
	/// <summary>
	/// Test program to test the one wire library
	/// <remarks>Assumes that an EMX device is connected and that pin EMX.Pin.IO16 (Socket #8, pin #6 on the Spider) is used for the one wire bus - Multiple devices are allowed on the bus</remarks>
	/// </summary>
	public class Program
	{
		/// <summary>
		/// The main entry point
		/// </summary>
		public static void Main()
		{
			Debug.Print("Starting ...");

			// Spider: EMX.Pin.IO16 = Socket #8, Pin #6
			using (OneWireBus.OneWireBus oneWireBus = new OneWireBus.OneWireBus(EMX.Pin.IO16))
			{
				OneWireDevice[] oneWireDevices = oneWireBus.Devices;
				Debug.Print("Found " + oneWireDevices.Length + " devices");
				Debug.Print(string.Empty);

				while (true)
				{
					foreach (OneWireDevice oneWireDevice in oneWireDevices)
					{
						Debug.Print("Device: " + oneWireDevice.DeviceAddress);

						// We only handle temperature devices
						TemperatureDevice temperatureDevice = oneWireDevice as TemperatureDevice;
						if (temperatureDevice == null)
						{
							continue;
						}

						Debug.Print("Synchronous temperature: " + temperatureDevice.GetTemperature() + "C");

						TemperatureAsyncResult temperatureAsyncResult = temperatureDevice.BeginGetTemperature();
						Debug.Print("Asynchronous temperature: " + temperatureAsyncResult.End() + "C");

						Debug.Print(string.Empty);
					}
				}
			}
		}
	}
}
