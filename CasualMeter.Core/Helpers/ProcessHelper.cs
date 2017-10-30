﻿using System;
using CasualMeter.Core.Conductors;
using Lunyx.Common;

namespace CasualMeter.Core.Helpers
{
    public sealed class ProcessHelper
    {
        private static readonly Lazy<ProcessHelper> Lazy = new Lazy<ProcessHelper>(() => new ProcessHelper());

        public static ProcessHelper Instance => Lazy.Value;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ProcessInfo.WinEventDelegate _dele;//leave this here to prevent garbage collection

        private ProcessHelper() 
        {
            //listen to window focus changed event
            _dele = OnFocusedWindowChanged;
            ProcessInfo.RegisterWindowFocusEvent(_dele);
        }

        public void Initialize()
        {
            //empty method to ensure initialization
        }

        public void ForceVisibilityRefresh(bool toggle=false)
        {
            CasualMessenger.Instance.RefreshVisibility(IsTeraActive, toggle);
        }

        public void UpdateHotKeys()
        {
            var isActive = IsTeraActive;
            if (!isActive.HasValue)
                return;
            if (isActive.Value)
                HotkeyHelper.Instance.Activate();
            else
                HotkeyHelper.Instance.Deactivate();
        }

        private void OnFocusedWindowChanged(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            ForceVisibilityRefresh();
            UpdateHotKeys();
        }

        public bool SendString(string s)
        {
            if (TeraWindow == IntPtr.Zero)
                return false;
            try
            {
                ProcessInfo.SendString(TeraWindow, s);
                return true;
            }
            catch (Exception)
            {
                //eat this
            }
            return false;
        }

        public bool? IsTeraActive
        {
            get
            {
                try
                {
                    return ProcessInfo.GetActiveProcessName()?.Equals("Tera", StringComparison.OrdinalIgnoreCase);
                } catch (Exception)
                {	//seems like there are multiple exceptions that can be thrown here, so just catch all of them.
                    return false;
                }
            }
        }

        public IntPtr TeraWindow => ProcessInfo.FindWindow("LaunchUnrealUWindowsClient", "TERA");
    }
}
