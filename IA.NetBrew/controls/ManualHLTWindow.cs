using System;
using System.Threading;
using GHIElectronics.NETMF.Glide;
using GHIElectronics.NETMF.Glide.UI;
using IA.NetBrew.Abstracts;
using IA.NetBrew.Composites;
using Microsoft.SPOT.Presentation.Media;

namespace IA.NetBrew.controls
{
    public class ManualHLTWindow : GlideWindow
    {
        private  TextBlock _txtCurrentTemp;
        private  TextBox _txtSetTemp;
        private  Button _btnBack;
        private  Button _btnRunStop;
        private  Button _btnUp;
        private  Button _btnDown;
        private  Thread _display;
        private  static ManualHLTWindow _instance;


        //Singleton Implementation
        public static ManualHLTWindow Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new ManualHLTWindow();
                        }
                    }
                }
                return _instance;
            }
        }

        private ManualHLTWindow()
        {
            Initalize();
        }
        
        private void Initalize() 
        {
            Window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.WinMLT));
            _txtCurrentTemp = (TextBlock)Window.GetChildByName("txtCurrentTemp");
            _txtSetTemp = (TextBox)Window.GetChildByName("txtSetTemp");
            _btnBack = (Button)Window.GetChildByName("btnBack");
            _btnDown = (Button)Window.GetChildByName("btnDown");
            _btnUp = (Button)Window.GetChildByName("btnUp");
            _btnDown = (Button)Window.GetChildByName("btnDown");
            _btnRunStop = (Button) Window.GetChildByName("btnRunStop");

            _btnBack.TapEvent += BtnBackTapEvent;
            _btnUp.TapEvent += BtnUpTapEvent;
            _btnDown.TapEvent += BtnDownTapEvent;
            _btnRunStop.TapEvent += BtnRunStopTapEvent;
            _txtSetTemp.TapEvent += EnterTempValue;
            _txtSetTemp.ValueChangedEvent += TxtSetTempValueChangedEvent;
            _display = new Thread(UpdateDisplay) {Priority = ThreadPriority.Lowest};
        }

        void EnterTempValue(object sender)
        {
            _display.Suspend();
            Glide.OpenKeyboard(sender);
        }

        void TxtSetTempValueChangedEvent(object sender)
        {
            _display.Resume();
            var textBox = (TextBox)Window.GetChildByName("txtSetTemp");
            var newTemp = Convert.ToDouble(textBox.Text);
            HotLiqourTank.Instance.TempSet = newTemp;
        }

        void BtnRunStopTapEvent(object sender)
        {
            if(HotLiqourTank.Instance.IsHeating)
            {
                //Stop and Reset button
                HotLiqourTank.Instance.IsHeating = false;
                _btnRunStop.Text = "Run";
                Window.FillRect(_btnRunStop.Rect);
                _btnRunStop.Invalidate();
            }
            else
            {
                HotLiqourTank.Instance.IsHeating = true;
                _btnRunStop.Text = "STOP";
                Window.FillRect(_btnRunStop.Rect);
                _btnRunStop.Invalidate();
            }
        }

        static void BtnDownTapEvent(object sender)
        {
            if (HotLiqourTank.Instance.TempSet >= 71)
                HotLiqourTank.Instance.TempSet--;
        }

        static void BtnUpTapEvent(object sender)
        {
            if (HotLiqourTank.Instance.TempSet <= 200)
                HotLiqourTank.Instance.TempSet++;
        }

        private void BtnBackTapEvent(object sender)
        {
            _display.Suspend();
            GoBack();
        }

        private void UpdateDisplay()
        {
            while (true)
            {
                if (Window.Visible)
                {
                    var curTempDisplay = HotLiqourTank.Instance.TempActualRims.ToString("N1");
                    var delta = HotLiqourTank.Instance.TempActualRims - HotLiqourTank.Instance.TempSet;
                    if (delta < 0) delta = delta*-1.00;
                    if (delta < 1.0)
                        _txtCurrentTemp.FontColor = ColorUtility.ColorFromRGB(8, 199, 53);
                    else if (delta < 2)
                        _txtCurrentTemp.FontColor = ColorUtility.ColorFromRGB(238, 245, 32);
                    else
                        _txtCurrentTemp.FontColor = ColorUtility.ColorFromRGB(245, 10, 10);
                    _txtCurrentTemp.Text = curTempDisplay;
                    Window.FillRect(_txtCurrentTemp.Rect);
                    _txtCurrentTemp.Invalidate();

                    _txtSetTemp.Text = HotLiqourTank.Instance.TempSet.ToString("N1");
                    Window.FillRect(_txtSetTemp.Rect);
                    _txtSetTemp.Invalidate();
                }
                Thread.Sleep(1000);
            }
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