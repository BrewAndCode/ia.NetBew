using System;
using Microsoft.SPOT.Hardware;

namespace IA.NetBrew.hardware
{
    public class GasSolenoid : IDisposable

    {
        private bool _state;
        public OutputPort Gas { get; set; }

        public GasSolenoid(OutputPort gas)
        {
            Gas = gas;
            Off();
        }
        public bool State
        {
            get { return _state; }
            set 
            { 
                _state = value;
                Gas.Write(!value);
            }
        }
        public void On()
        {
           Gas.Write(false);
            _state = true;
        }
        public void Off()
        {
            Gas.Write(true);
            _state = false;
        }

        public void Dispose()
        {
            Off();
        }
    }
}
