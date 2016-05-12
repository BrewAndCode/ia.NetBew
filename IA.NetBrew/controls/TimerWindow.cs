using System;
using System.Threading;
using GHIElectronics.NETMF.Glide;
using GHIElectronics.NETMF.Glide.UI;
using IA.NetBrew.Abstracts;
using Microsoft.SPOT;

namespace IA.NetBrew.controls
{
    public class TimerWindow : GlideWindow
    {
        private static TimerWindow _instance;
        private Button _btnStart;
        private Button _btnBack;
        private Button _btn60;
        private Button _btn75;
        private Button _btn90;
        private TextBlock _txtTime;
        private Thread _display;

        private bool _isRunning = false;
        private DateTime _startTime;
        private TimeSpan _timer= new TimeSpan(0,1,0,0);
        private TimeSpan counter;

        public static TimerWindow Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new TimerWindow();
                        }
                    }
                }
                return _instance;
            }            
        }
        private TimerWindow()
        {
            Initialize();
        }


        private void Initialize()
        {
            Window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.WinTimer));
            _btnStart = (Button) Window.GetChildByName("btnStartStop");
            _btnBack = (Button) Window.GetChildByName("btnBack");
            _btn60 = (Button) Window.GetChildByName("btn60");
            _btn75 = (Button) Window.GetChildByName("btn75");
            _btn90 = (Button) Window.GetChildByName("btn90");
            _txtTime = (TextBlock) Window.GetChildByName("txtTime");

            _btnBack.TapEvent += BtnBackTapEvent;
            _btnStart.TapEvent += BtnStartTapEvent;
            _btn60.TapEvent += Btn60TapEvent;
            _btn75.TapEvent += Btn75TapEvent;
            _btn90.TapEvent += Btn90TapEvent;


            _txtTime.Text = ToTimeString(_timer);

            _display = new Thread(UpdateDisplay) { Priority = ThreadPriority.Lowest };
        }

        void Btn90TapEvent(object sender)
        {
            if (_isRunning) return;
            _timer = new TimeSpan(0, 0, 0,20);
            ShowTime();
        }

        void Btn75TapEvent(object sender)
        {
            if (_isRunning) return;
            _timer = new TimeSpan(0,0,75,0);
            ShowTime();
        }

        void Btn60TapEvent(object sender)
        {
            if (_isRunning) return;
            _timer = new TimeSpan(0,0,60,0);
            ShowTime();
        }

        private static String ToTimeString(TimeSpan t)
        {
            var hours = t.Hours.ToString();
            var minutes = t.Minutes.ToString();
            var seconds = t.Seconds.ToString();

            if (hours.Length==1) hours = "0" + hours;
            if (minutes.Length==1) minutes = "0" + minutes;
            if (seconds.Length==1) seconds = "0" + seconds;

            return hours + ":" + minutes + ":" + seconds;
        }
        private void ShowTime()
        {
            _txtTime.Text = ToTimeString(_timer);
            Window.FillRect(_txtTime.Rect);
            _txtTime.Invalidate();
        }

        void BtnStartTapEvent(object sender)
        {
            if (_isRunning)
            {
                _btnStart.Text = "Start";
                Window.FillRect(_txtTime.Rect);
                _btnStart.Invalidate();
                _isRunning = false;
                if ((_display.ThreadState & ThreadState.Running) == ThreadState.Running)
                     _display.Suspend();

            }
            else
            {
                _startTime = DateTime.Now;
                counter = _timer;
                _btnStart.Text = "Stop";
                Window.FillRect(_btnStart.Rect);
                _btnStart.Invalidate();
                if ( (_display.ThreadState & ThreadState.Suspended) == ThreadState.Suspended)
                    _display.Start();
                _isRunning = true;
            }
        }

        void BtnBackTapEvent(object sender)
        {
            _display.Suspend();
            GoBack();
        }



        protected void UpdateDisplay()
        {
            while (true)
            {
                if (_isRunning)
                {
                    var delta = DateTime.Now.Subtract(_startTime);// _startTime.Subtract(DateTime.Now);
                    _timer = counter.Subtract(delta);
                    if (_timer.Ticks < 0)
                    {
                        _txtTime.FontColor = Platform.Constants.TintRed;
                        Window.FillRect(_txtTime.Rect);
                        _txtTime.Invalidate();
                        BtnStartTapEvent(null);
                        Alarm();
                    }
                    
                    ShowTime();
                }
                Thread.Sleep(1000);
            }
        }

        protected void Alarm()
        {
            //SOund Alarm
        }

        protected override void PauseThread()
        {
            _display.Suspend();
        }
        protected override void ResumeThread()
        {
            switch (_display.ThreadState)
            {
                case ThreadState.Unstarted:
                    _display.Start();
                    break;
                default:
                    _display.Resume();
                    break;
            }
        }
    }
}
