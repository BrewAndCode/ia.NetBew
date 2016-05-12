using System;
using HZ.OneWireBus.Devices;

namespace HZ.OneWireBus
{
	/// <summary>
	/// An asynchronous result object returning a temperature
	/// </summary>
	public class TemperatureAsyncResult : AsyncResult
	{
		/// <summary>
		/// The temperature result in degrees C
		/// <remarks>Returns <c>double.MinValue</c> if no result has been set</remarks>
		/// </summary>
		public double Result = double.MinValue;

		/// <summary>
		/// Creates an AsyncResult
		/// </summary>
		/// <param name="owner">The device which this AsyncResult belongs to</param>
		/// <param name="asyncCallback">The callback to be invoked upon completion, optional</param>
		/// <param name="asyncState">The state object to be stored against this AsyncResult, optional</param>
		public TemperatureAsyncResult(TemperatureDevice owner, AsyncCallback asyncCallback = null, object asyncState = null)
			: base(owner, asyncCallback, asyncState)
		{
		}

		/// <summary>
		/// Finishes the asynchronous processing and throws an exception if one was generated
		/// <remarks>Blocks until the asynchronous processing has completed</remarks>
		/// </summary>
		/// <returns>Returns the temperature result in degrees C or <c>double.MinValue</c> if no result has been set</returns>
		public new double End()
		{
			base.End();

			return Result;
		}

		/// <summary>
		/// The method used to perform the asynchronous processing
		/// </summary>
		public override void Process()
		{
			Exception caughtException = null;

			try
			{
				TemperatureDevice temperatureDevice = Owner as TemperatureDevice;
				if (temperatureDevice != null)
				{
					Result = temperatureDevice.GetTemperature(true);
				}
			}
			catch (Exception exception)
			{
				caughtException = exception;
			}

			Complete(caughtException);
		}
	}
}
