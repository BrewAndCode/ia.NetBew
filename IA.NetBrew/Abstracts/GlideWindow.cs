using System;
using GHIElectronics.NETMF.Glide;
using GHIElectronics.NETMF.Glide.Display;

namespace IA.NetBrew.Abstracts
{
    public abstract class GlideWindow
    {
        protected static readonly object SyncRoot = new Object();
        public Window Window { get; protected set; }
        public Window ParentWin { get; set; }
        protected abstract void PauseThread();
        protected abstract void ResumeThread();
        
        public void Show(Window ParentWindow)
        {
            ParentWin = ParentWindow;
            Tween.SlideWindow(ParentWindow,Window,Direction.Up);
            ResumeThread();
        }

        public void GoBack()
        {
            if (ParentWin == null) return;
            PauseThread();
            Tween.SlideWindow(Window, ParentWin, Direction.Down);
        }

       
    }
}
