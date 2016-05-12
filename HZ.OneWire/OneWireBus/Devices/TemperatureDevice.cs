using System;

namespace HZ.OneWireBus.Devices
{
	/// <summary>
	/// A generic temperature device
	/// </summary>
	public abstract class TemperatureDevice : OneWireDevice
	{
		/// <summary>
		/// Creates a TemperatureDevice
		/// </summary>
		/// <param name="oneWireBus">The one wire bus which this device belongs to</param>
		/// <param name="deviceAddress">The address of this device</param>
		protected TemperatureDevice(OneWireBus oneWireBus, DeviceAddress deviceAddress)
			: base(oneWireBus, deviceAddress)
		{
		}

		/// <summary>
		/// Queries the current temperature of the device
		/// </summary>
		/// <param name="asyncCall">If <c>false</c>, the one wire bus is locked while the query is performed, optional</param>
		/// <returns>The temperature in degrees C</returns>
		public abstract double GetTemperature(bool asyncCall = false);

		/// <summary>
		/// Starts an asynchronous temperature query
		/// </summary>
		/// <param name="asyncCallback">The callback to be invoked upon completion, optional</param>
		/// <param name="asyncState">The state object to be stored against the TemperatureAsyncResult, optional</param>
		/// <returns>The TemperatureAsyncResult</returns>
		public TemperatureAsyncResult BeginGetTemperature(AsyncCallback asyncCallback = null, object asyncState = null)
		{
			return new TemperatureAsyncResult(this, asyncCallback, asyncState);
		}

		/// <summary>
		/// Finishes the asynchronous temperature query
		/// <remarks>Blocks until the asynchronous query has completed</remarks>
		/// </summary>
		/// <param name="asyncResult">The TemperatureAsyncResult to finish</param>
		/// <returns>The temperature in degrees C</returns>
		public static double EndGetTemperature(TemperatureAsyncResult asyncResult)
		{
			return asyncResult.End();
		}
	}
}
