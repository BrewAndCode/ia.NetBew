using System;
using GHIElectronics.NETMF.Glide.UI;
using Microsoft.SPOT;
using System.Threading;
using GHIElectronics.NETMF.Hardware;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Glide;
using GHIElectronics.NETMF.Glide.Display;
using Microsoft.SPOT.Hardware;
using Button = GHIElectronics.NETMF.Glide.UI.Button;


namespace FEZ3
{
    public class Program
    {
        public static Thread[] Threads = new Thread[1];
        public static TextBlock TimeBlock;

        // This will hold the windows.        
        static Window[] windows = new Window[3];
        // Indicates the current window index.        
        static int index = 0;
        private static Button _btnRelay;
       public static void Main()        
        {
            //RealTimeClock.SetTime(new DateTime(2012,3,3,03,35,0));
            // Load the windows            
            windows[0] = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.Window1));
            windows[1] = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.Window2));
            windows[2] = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.Window3));
            // Activate touch            
            GlideTouch.Initialize();
            // Initialize the windows.            
            // Since all windows are identical we can reuse the same function.           
            InitWin(windows[0]);
            InitWin(windows[1]);
            InitWin(windows[2]);
            updateButtons();
            // Assigning a window to MainWindow flushes it to the screen.            
            // This also starts event handling on the window.            
            if (Threads[0] == null)
            {

                Threads[0] = new Thread(DoTime) { Priority = ThreadPriority.Lowest };
                Threads[0].Start();
            }
            Glide.MainWindow = windows[index];  
           

            

            Thread.Sleep(-1);        
        }         
        // Initializes the back and next buttons. 
        static void DoTime()
        {
            while (true)
            {
                //var timeBlock = (TextBlock)windows[0].GetChildByName("lblTime");
                if (TimeBlock != null)
                {
                    TimeBlock.Text = RealTimeClock.GetTime().ToString("ddd MMM dd  HH:MM:ss ");
                    windows[0].FillRect(TimeBlock.Rect);
                    TimeBlock.Invalidate();
                    Thread.Sleep(100);
                }
                
            }
            return ;
        }
        static void InitWin(Window window)        
        {            
            // Find the back button within the window.            
            Button backBtn = (Button)window.GetChildByName("backBtn");            
            // Attach a tap event handler.            
            backBtn.TapEvent += new OnTap(backBtn_TapEvent);             
            Button nextBtn = (Button)window.GetChildByName("nextBtn");   
            nextBtn.TapEvent += new OnTap(nextBtn_TapEvent);        
            if (window.Name == "window1")
            {
                _btnRelay = (Button) window.GetChildByName("relayBtn");
                _btnRelay.TapEvent += BtRelayTapEvent;
                TimeBlock = (TextBlock)window.GetChildByName("lblTime");
            


            }
        }

        private static void BtRelayTapEvent(object sender)
        {
            using( var x = new OutputPort((Cpu.Pin) FEZ_Pin.Digital.IO6, true))
            {
                Thread.Sleep(1000);
                x.Write(false);
            }
        }

        // Enables/disables navigation buttons appropriately.        


        static void updateButtons()       
        {           
            Window window = windows[index];            
            // First window            
            if (index == 0)           
            {                
                Button backBtn = (Button)window.GetChildByName("backBtn");                
                backBtn.Enabled = false;                 
                // Disabling a button reduces it's alpha to 1/3 it's current value.                
                // Simply invalidating a button won't show the alpha change because                
                // it's redrawing over the current bitmap. To show this change we need                
                // to clear the region it occupies (fill it with background color).                
                window.FillRect(backBtn.Rect);               
                backBtn.Invalidate();           
            }             
            // Last window            
            if (index == windows.Length - 1)
            {
                Button nextBtn = (Button)window.GetChildByName("nextBtn");                
                nextBtn.Enabled = false;                 
                window.FillRect(nextBtn.Rect);                
                nextBtn.Invalidate();
            }       
        } 
        // Handles the next button tap event.       
        static void nextBtn_TapEvent(object sender)        
        {            if (index < windows.Length - 1)           
        {                
            int lastIndex = index;                
            index++;                 
            // Update before we slide so the button state(s) are          
            // visually correct while we slide.            
            updateButtons();               
            Tween.SlideWindow(windows[lastIndex], windows[index], Direction.Up);   
        }       
        }        
        // Handles the back button tap event.    
        static void backBtn_TapEvent(object sender)
        {
            if (index > 0)
            {
                int lastIndex = index;         
                index--;              
                updateButtons();          
                Tween.SlideWindow(windows[lastIndex], windows[index], Direction.Down);
            }
        }        
    }
}
