using System;
using Microsoft.SPOT;
using System.Collections;

namespace IA.NetBrew.hardware
{
    public class Alarm
    {
        protected class Indicators
        {
            public Guid ID;
            public string Text;
        }

        private static Alarm _instance;
        private static readonly object SyncRoot = new Object();
        protected ArrayList indicators = new ArrayList();


        public static Alarm Instance
        {
            get
            {
                if(_instance==null)
                    lock(SyncRoot)
                    {
                        if(_instance==null)
                            _instance = new Alarm();
                    }
                return _instance;
            }
        }
    }
}
