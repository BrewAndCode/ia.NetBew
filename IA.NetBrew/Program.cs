using System.Collections;
using System.IO;
using System.Threading;
using GHIElectronics.NETMF.IO;
using GHIElectronics.NETMF.Glide;
using GHIElectronics.NETMF.Glide.Display;
using GHIElectronics.NETMF.Glide.UI;
using IA.NetBrew.Composites;
using IA.NetBrew.Platform;
using IA.NetBrew.controls;
using IA.NetBrew.hardware;
using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using Button = GHIElectronics.NETMF.Glide.UI.Button;


namespace IA.NetBrew
{
    public static class Program
    {
        public static PersistentStorage PS;
        public static VolumeInfo Sd;
        public static MashLauterTun MashTun;
        public static HotLiqourTank HLT;
        public static GasSolenoid GasBoil = new GasSolenoid(Mappings.Gas3);

#region WinMain Objects
        private static Window _winMain;
        private static Button _btnLoad;
        private static Button _btnLoadFile;
        private static Button _btnLoadCancel;
        private static Button _btnMLT;
        private static Button _btnHLT;
        private static Button _btnBoil;
        private static Dropdown _ddlFiles;
        private static Button _btnRtPump;
        private static Button _btnLfPump;
        private static Button _btnTimer;
        private static List _list;
#endregion

        public static void Main()
        {

            //RemovableMedia.Insert += RemovableMediaInsert;
            //RemovableMedia.Eject += RemovableMediaEject;
            //new Thread(SdMountThread).Start();
            //USBHostController.DeviceConnectedEvent += DeviceConnectedEvent;
           
            GlideTouch.Initialize();
            Glide.FitToScreen = true;

            #region MainWindow
            _winMain = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.winMain));
             _btnLoad = (Button) _winMain.GetChildByName("btnLoad");
            //_btnLoad.TapEvent += BtnLoadTapEvent;
            _btnMLT = (Button) _winMain.GetChildByName("btnMLT");
            _btnMLT.TapEvent += BtnMLTTapEvent;
            _btnHLT = (Button) _winMain.GetChildByName("btnHLT");
            _btnHLT.TapEvent += BtnHLTTapEvent;
            _btnBoil = (Button) _winMain.GetChildByName("btnBoil");
            _btnBoil.TapEvent += BtnBoilTapEvent;
            _btnTimer = (Button) _winMain.GetChildByName("btnTimer");
            _btnTimer.TapEvent += new OnTap(_btnTimer_TapEvent  );
            _btnRtPump = (Button) _winMain.GetChildByName("btnPumpRight");
            _btnRtPump.TapEvent += BtnRtPumpTapEvent;
            _btnLfPump = (Button) _winMain.GetChildByName("btnPumpLeft");
            _btnLfPump.TapEvent += BtnLfPumpTapEvent;

            //Btn Load File/Cancel Dynamically Appears after file selection
            //_btnLoadFile = new Button("btnLoadFile", 100, _winMain.Width - 100, _btnMLT.Y, 100, 30) {Text = "Load", Visible = false};
            //_btnLoadFile.TapEvent += BtnLoadFileTapEvent;
            //_winMain.AddChild(_btnLoadFile);
            //_btnLoadCancel = new Button("btnLoadCancel", 100, _winMain.Width - 100, _btnHLT.Y, 100, 30)
            //                     {Text = "Cancel", Visible = false};
            //_btnLoadCancel.TapEvent += BtnLoadCancelTapEvent;
            //_winMain.AddChild(_btnLoadCancel);
            #endregion

            Glide.MainWindow = _winMain;
            MashTun = MashLauterTun.Instance;
            MashTun.Gas = new GasSolenoid(Mappings.Gas1);
            MashTun.TempSet =80;

            HLT = HotLiqourTank.Instance;
            HLT.Gas = new GasSolenoid(Mappings.Gas2);
            HLT.TempSet = 170;

            GasBoil.Off();
           Thread.Sleep(-1);
        }

        static void _btnTimer_TapEvent(object sender)
        {
            TimerWindow.Instance.Show(_winMain);
        }

        static void BtnLfPumpTapEvent(object sender)
        {
            if (Mappings.PumpLeft.Read())
            {
                //Pump is on. Turn off
                Mappings.PumpLeft.Write(false);
                _btnLfPump.Text = "Left Pump[OFF]";
                _btnLfPump.TintColor = Constants.TintRed;
                _btnLfPump.TintAmount = 80;
                _winMain.FillRect(_btnLfPump.Rect);
                _btnLfPump.Invalidate();
            }
            else
            {
                //Pump is Off. Turn On
                Mappings.PumpLeft.Write(true);
                _btnLfPump.Text = "Left Pump[ON]";
                _btnLfPump.TintColor = Constants.TintGreen;
                _btnLfPump.TintAmount = 80;
                _winMain.FillRect(_btnLfPump.Rect);
                _btnLfPump.Invalidate();
            }
        }
        static void BtnRtPumpTapEvent(object sender)
        {
            if (Mappings.PumpRight.Read())
            {
                //Pump is on. Turn off
                Mappings.PumpRight.Write(false);
                _btnRtPump.Text = "Right Pump[OFF]";
                _btnRtPump.TintColor = Constants.TintRed;
                _btnRtPump.TintAmount = 80;
                _winMain.FillRect(_btnRtPump.Rect);
                _btnRtPump.Invalidate();
            }
            else
            {
                //Pump is Off. Turn On
                Mappings.PumpRight.Write(true);
                _btnRtPump.Text = "Right Pump[ON]";
                _btnRtPump.TintColor = Constants.TintGreen;
                _btnRtPump.TintAmount = 80;
                _winMain.FillRect(_btnRtPump.Rect);
                _btnRtPump.Invalidate();
                
            }
        }

        static void BtnLoadCancelTapEvent(object sender)
        {
            _btnLoadCancel.Visible = false;
            _btnLoadFile.Visible = false;
            _ddlFiles.Visible = false;
            _winMain.FillRect(_btnLoadCancel.Rect);
            _winMain.FillRect(_btnLoadFile.Rect);
            _winMain.FillRect(_ddlFiles.Rect);
            _btnLoadCancel.Invalidate();
            _btnLoadFile.Invalidate();
            _ddlFiles.Invalidate();
            _winMain.RemoveChild(_ddlFiles);
            _winMain.Invalidate();
        }
        static void BtnLoadFileTapEvent(object sender)
        {
            
        }
        static void BtnBoilTapEvent(object sender)
        {
            if(GasBoil.State)
            {
                GasBoil.Off();
                _btnBoil.TintColor = Constants.TintRed;
                _btnBoil.TintAmount = 80;
                _winMain.FillRect(_btnBoil.Rect);
                _btnBoil.Invalidate();
            }
            else
            {
                GasBoil.On();
                _btnBoil.TintColor = Constants.TintGreen;
                _btnBoil.TintAmount = 80;
                _winMain.FillRect(_btnBoil.Rect);
                _btnBoil.Invalidate();
            }
        }

        static void BtnHLTTapEvent(object sender)
        {
            ManualHLTWindow.Instance.Show(_winMain);
        }
        static void BtnMLTTapEvent(object sender)
        {
            ManualMashWindow.Instance.Show(_winMain);
        }
        static void BtnLoadTapEvent(object sender)
        {
            _ddlFiles = new Dropdown("ddlFiles",100,_btnLoad.X+_btnLoad.Width+10,_btnLoad.Y,200,30);
            _ddlFiles.TapEvent += DdlFilesTapEvent;
            _ddlFiles.ValueChangedEvent += DdlFilesValueChangedEvent;
            _winMain.AddChild(_ddlFiles);
            _winMain.FillRect(_ddlFiles.Rect);
            _ddlFiles.Invalidate();

            //load Filse and Populate Drop Down
            if (Sd != null)
            {
                var files = Directory.GetFiles(Sd.RootDirectory);
                var options = new ArrayList();
                foreach(var file in files)
                    options.Add(new object[] {file, file});
                _list = new List(options,300);
                _list.CloseEvent += ListCloseEvent;
                DdlFilesTapEvent(_ddlFiles);
            }

        }
        static void ListCloseEvent(object sender)
        {
            Glide.CloseList();
        }
        static void DdlFilesValueChangedEvent(object sender)
        {
            var ddl = (Dropdown) sender;
            Debug.Print("Dropdown file: "+ddl.Value);
            _btnLoadFile.Visible = true;
            _btnLoadCancel.Visible = true;
            _btnLoadFile.Invalidate();
            _btnLoadCancel.Invalidate();
        }
        static void DdlFilesTapEvent(object sender)
        {
            Glide.OpenList(sender,_list);
        }

        //static void downButton_OnInterrupt(uint data1, uint data2, System.DateTime time)
        //{
        //    MashLauterTun.Instance.SetTunings(MashLauterTun.Instance.Kc, MashLauterTun.Instance.TauR, MashLauterTun.Instance.TauD-1);
        //}

        //static void upButton_OnInterrupt(uint data1, uint data2, System.DateTime time)
        //{
        //   MashLauterTun.Instance.SetTunings(MashLauterTun.Instance.Kc,MashLauterTun.Instance.TauR,MashLauterTun.Instance.TauD+1);
        //}

        //static void StartStopButton_OnInterrupt(uint data1, uint data2, System.DateTime time)
        //{
        //    if (MashLauterTun.Instance.IsHeating)
        //    {
        //        Debug.Print("Stopping PID Control of MLT RIMS");
        //        MashLauterTun.Instance.IsHeating = false;
        //    }
        //    else
        //    {
        //        Debug.Print("Starting PID Control of MLT RIMS");
        //        MashLauterTun.Instance.IsHeating = true;
        //        MashLauterTun.Instance.TempSet =152;
        //    }
        //    if (HotLiqourTun.Instance.IsHeating)
        //    {
        //        Debug.Print("Stopping PID Control of HLT RIMS");
        //        HotLiqourTun.Instance.IsHeating = false;
        //    }
        //    else
        //    {
        //        Debug.Print("Starting PID Control of HLT RIMS");
        //        HotLiqourTun.Instance.IsHeating = true;
        //        HotLiqourTun.Instance.TempSet = 170;
        //    }
        //}

        //static void WinMainShowMLTWindow(object source, string winName, EventArgs e)
        //{
        //    Tween.SlideWindow(WinMain.ThisWindow.GWindow, WinTempMLT.ThisWindow.GWindow, Direction.Left);
        //}


        //private static void RemovableMediaEject(object sender, MediaEventArgs e)
        //{
        //    Debug.Print("Storage \"" + e.Volume.RootDirectory + "\" is ejected.");

        //}
        //private static void RemovableMediaInsert(object sender, MediaEventArgs e)
        //{
        //    Debug.Print("Storage \"" + e.Volume.RootDirectory + "\" is inserted.");
        //    Debug.Print("Getting files and folders:");
        //    if (e.Volume.IsFormatted)
        //    {
        //        Sd = e.Volume;
        //        var files = Directory.GetFiles(e.Volume.RootDirectory);
        //        Debug.Print("Files available on " + e.Volume.RootDirectory + ":");

        //        for (var i = 0; i < files.Length; i++)
        //            Debug.Print(files[i]);
        //    }
        //    else
        //    {
        //        Debug.Print("Storage is not formatted. Format on PC with FAT32/FAT16 first.");
        //    }

        //}
        public static void SdMountThread()
        {
            PersistentStorage sdPS = null;
            const int pollTime = 500; // check every 500 millisecond

            while (true)
            {
                try // If SD card was removed while mounting, it may throw exceptions
                {
                    var sdExists = PersistentStorage.DetectSDCard();
 
                    // make sure it is fully inserted and stable
                    if (sdExists)
                    {
                        Thread.Sleep(50);
                        sdExists = PersistentStorage.DetectSDCard();
                    }
 
                    if (sdExists && sdPS == null)
                    {
                        sdPS = new PersistentStorage("SD");
                        sdPS.MountFileSystem();
                    }
                    else if (!sdExists && sdPS != null)
                    {
                        sdPS.UnmountFileSystem();
                        sdPS.Dispose();
                        sdPS = null;
                    }
                }
                catch
                {
                    if (sdPS != null)
                    {
                        sdPS.Dispose();
                        sdPS = null;
                    }
                }
 
                Thread.Sleep(pollTime);
            }
        }

    }
}
