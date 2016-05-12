using System;
using Microsoft.SPOT.Hardware;
namespace IA.NetBrew.hardware
{
        public class IOPort : IDisposable
        {
            private readonly TristatePort _tristatePort;

            public IOPort(Cpu.Pin portPin, bool initialState, bool glitchFiler, Port.ResistorMode resistor)
            {
                _tristatePort = new TristatePort(portPin, initialState, glitchFiler, resistor);
            }

            public void Write(bool state)
            {
                if (!_tristatePort.Active)
                {
                    _tristatePort.Active = true;
                }

                _tristatePort.Write(state);
            }

            public bool Read()
            {
                if (_tristatePort.Active)
                {
                    _tristatePort.Active = false;
                }

                return _tristatePort.Read();
            }

            public void Dispose()
            {
                _tristatePort.Dispose();
            }
        }
    }


