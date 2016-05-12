using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using GHIElectronics.NETMF.Hardware;
using HZ.OneWireBus.Devices;
using Microsoft.SPOT.Hardware;

namespace HZ.OneWireBus
{
	/// <summary>
	/// The main class representing a one wire bus
	/// <remarks>Disposable</remarks>
	/// </summary>
	public class OneWireBus : IDisposable
	{
		private bool _disposed;
		private readonly OneWire _oneWire;

		private readonly Hashtable _deviceFactories;
		private readonly Hashtable _devices;

		private readonly object _lock = new object();
		private bool _externalLock;

		private readonly Queue _asyncTaskQueue = new Queue();
		private readonly Thread _asyncTaskQueueThread;

		/// <summary>
		/// Gets an array of the devices which are currently known about
		/// </summary>
		public OneWireDevice[] Devices
		{
			get
			{
				ICollection devices = _devices.Values;
				OneWireDevice[] deviceList = new OneWireDevice[devices.Count];
				devices.CopyTo(deviceList, 0);
				return deviceList;
			}
		}

		/// <summary>
		/// Creates a OneWireBus and finds all devices connected to it
		/// </summary>
		/// <param name="pin">The pin which is connected to the one wire bus data line</param>
		public OneWireBus(Cpu.Pin pin)
		{
			_oneWire = new OneWire(pin);
			_devices = new Hashtable();
			_deviceFactories = new Hashtable();

			var assembly = Assembly.GetExecutingAssembly();
			var types = assembly.GetTypes();
			var baseDeviceType = typeof (OneWireDevice);
			var constructorParameterTypes = new []
			{
				typeof (OneWireBus),
				typeof (DeviceAddress)
			};
			foreach (Type type in types)
			{
				if (type.IsSubclassOf(baseDeviceType))
				{
					var familyCodeField = type.GetField("FamilyCode", BindingFlags.GetField | BindingFlags.Static | BindingFlags.Public);

                    var familyCode = (byte)familyCodeField.GetValue(null);
					if (familyCode == 0) { continue; }

					var constructor = type.GetConstructor(constructorParameterTypes);
					if (constructor == null) { continue; }

					_deviceFactories.Add(familyCode, constructor);
				}
			}
			FindDevices();
			_asyncTaskQueueThread = new Thread(AsyncTaskQueueThread);
			_asyncTaskQueueThread.Start();
		}

		/// <summary>
		/// Finalises the object
		/// </summary>
		~OneWireBus()
		{
			Dispose(false);
		}

		/// <summary>
		/// Disposed of all resources
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Disposed of all resources
		/// </summary>
		/// <param name="disposing"><c>True</c> if managed resources should be released</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				// Release managed resources
				try
				{
					Grab();

					_asyncTaskQueueThread.Abort();
					_asyncTaskQueueThread.Join();
					_oneWire.Dispose();
				}
				finally
				{
					Release();
				}
			}

			_disposed = true;
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
		/// <summary>
		/// Finds all devices on the one wire bus, existing devices which have been found are cleared
		/// <remarks>The devices can be retrieved using the Devices property</remarks>
		/// </summary>
		public void FindDevices()
		{
			try
			{
				Grab();

				_devices.Clear();

				_oneWire.Search_Restart();

				byte[] addressBytes = new byte[8];
				while (_oneWire.Search_GetNextDevice(addressBytes))
				{
					DeviceAddress deviceAddress = new DeviceAddress(addressBytes);
					Microsoft.SPOT.Debug.Print("Found device: " + deviceAddress + "Hex: " + ToHex(deviceAddress.Address));

					byte familyCode = deviceAddress.FamilyCode;
					if (!_deviceFactories.Contains(familyCode))
					{
						continue;
					}

					ConstructorInfo constructor = (ConstructorInfo)_deviceFactories[familyCode];
					OneWireDevice device = (OneWireDevice)constructor.Invoke(new object[] { this, deviceAddress });

					_devices.Add(device.DeviceAddress, device);
				}
			}
			finally
			{
				Release();
			}
		}

		/// <summary>
		/// Retrieves a specific device based upon its device address
		/// <remarks>Only retrieves devices which have already been found</remarks>
		/// </summary>
		/// <param name="deviceAddress">The device address to search for</param>
		/// <returns>The OneWireDevice</returns>
		public OneWireDevice GetDevice(DeviceAddress deviceAddress)
		{
			return !_devices.Contains(deviceAddress) ? null : (OneWireDevice)_devices[deviceAddress];
		}

        /// <summary>
        /// Queries known device Addresses.  Quicker check than the DeviceIsValid since we don't have to query 
        /// The OneWire Bus.  All we care about is whether the device was present when the system started up
        /// </summary>
        /// <param name="deviceAddress">The Device to search for</param>
        /// <returns>returns <c>true</c> if the device is known and was found at startup.</returns>
        public bool DeviceIsKnown(DeviceAddress deviceAddress)
        {
            return _devices.Contains(deviceAddress);
        }
		/// <summary>
		/// Queries if a device is known about and is still connected to the one wire bus
		/// <remarks>Can be used to identify a specific device on the bus by disabling it and checking each unknown device</remarks>
		/// </summary>
		/// <param name="device">The device to search for</param>
		/// <returns>Returns <c>true</c> if the device is known about and is connected to the one wire bus</returns>
		public bool DeviceIsValid(OneWireDevice device)
		{
			try
			{
				Grab();

				return _devices.Contains(device.DeviceAddress) && _oneWire.Search_IsDevicePresent(device.DeviceAddress.Address);
			}
			finally
			{
				Release();
			}
		}

		/// <summary>
		/// Queries if a device is known about and is still connected to the one wire bus
		/// <remarks>Can be used to identify a specific device on the bus by disabling it and checking each unknown device</remarks>
		/// </summary>
		/// <param name="deviceAddress">The device address to search for</param>
		/// <returns>Returns <c>true</c> if the device is known about and is connected to the one wire bus</returns>
		public bool DeviceIsValid(DeviceAddress deviceAddress)
		{
			try
			{
				Grab();

				return _devices.Contains(deviceAddress) && _oneWire.Search_IsDevicePresent(deviceAddress.Address);
			}
			finally
			{
				Release();
			}
		}

		/// <summary>
		/// Sends the reset command across the one wire bus
		/// </summary>
		/// <returns>Returns <c>true</c> if there are devices on the one wire bus</returns>
		public bool Reset()
		{
			return _oneWire.Reset();
		}

		/// <summary>
		/// Writes data to the one wire bus
		/// </summary>
		/// <param name="data">The data to write</param>
		public void Write(byte data)
		{
			_oneWire.WriteByte(data);
		}

		/// <summary>
		/// Writes data to the one wire bus
		/// </summary>
		/// <param name="data">The data to write</param>
		/// <param name="offset">The offset in data to start writing from, optional, defaults to 0</param>
		/// <param name="count">The number of bytes to write or <c>int.MinValue</c> for all, optional, defaults to all</param>
		public void Write(byte[] data, int offset = 0, int count = int.MinValue)
		{
			if (data == null)
			{
				return;
			}

			_oneWire.Write(data, offset, count == int.MinValue ? data.Length - offset : count);
		}

		/// <summary>
		/// Writes data to the one wire bus
		/// </summary>
		/// <param name="data">The data to write</param>
		public void Write(bool data)
		{
			_oneWire.WriteBit(data ? (byte)1 : (byte)0);
		}

		/// <summary>
		/// Reads data from the one wire bus
		/// </summary>
		/// <returns>The data read from the bus</returns>
		public byte ReadByte()
		{
			return _oneWire.ReadByte();
		}

		/// <summary>
		/// Reads data from the one wire bus
		/// </summary>
		/// <param name="data">A buffer to hold the data read from the bus</param>
		/// <param name="offset">The offset in data to start writing to, optional, defaults to 0</param>
		/// <param name="count">The number of bytes to read or <c>int.MinValue</c> for the size of the buffer, optional, defaults to the size of the buffer</param>
		public void ReadBytes(ref byte[] data, int offset = 0, int count = int.MinValue)
		{
			if (data == null)
			{
				return;
			}

			_oneWire.Read(data, offset, count == int.MinValue ? data.Length - offset : count);
		}

		/// <summary>
		/// Reads data from the one wire bus
		/// </summary>
		/// <returns>The data read from the bus</returns>
		public bool ReadBit()
		{
			return (_oneWire.ReadBit() & 0x01) == 0x01;
		}

		/// <summary>
		/// Waits for the current one wire bus transaction to finish then locks the bus
		/// <remarks>Blocks until the lock has been asserted, used to ensure that asynchronous transactions do not contaminate synchronous transactions</remarks>
		/// </summary>
		public void Grab()
		{
			while (true)
			{
				lock (_lock)
				{
					if (!_externalLock)
					{
						_externalLock = true;
						return;
					}
				}

				Thread.Sleep(10);
			}
		}

		/// <summary>
		/// Releases the lock on the bus
		/// <remarks>Used to ensure that asynchronous transactions do not contaminate synchronous transactions</remarks>
		/// </summary>
		public void Release()
		{
			lock (_lock)
			{
				_externalLock = false;
			}
		}

		/// <summary>
		/// The thread used to process asynchronous queries
		/// </summary>
		private void AsyncTaskQueueThread()
		{
			while (true)
			{
				if (_asyncTaskQueue.Count > 0 && !_externalLock)
				{
					lock (_lock)
					{
						if (!_externalLock)
						{
							AsyncResult asyncResult = (AsyncResult)_asyncTaskQueue.Dequeue();

							asyncResult.Process();

							continue;
						}
					}
				}

				Thread.Sleep(10);
			}
		}

		/// <summary>
		/// Adds an AsyncResult to the asynchronous task queue
		/// </summary>
		/// <param name="asyncResult">the AsyncResult to add</param>
		public void AddAsyncTask(AsyncResult asyncResult)
		{
			lock (_lock)
			{
				_asyncTaskQueue.Enqueue(asyncResult);
			}
		}
	}
}
