using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace FEZ3.core
{
    class RelayControl : OutputPort
    {
        public RelayControl(Cpu.Pin portId, bool initialState) : base(portId, initialState)
        {
        }

        protected RelayControl(Cpu.Pin portId, bool initialState, bool glitchFilter, ResistorMode resistor) : base(portId, initialState, glitchFilter, resistor)
        {
        }
    }
}
