using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
// Import from Include/LogitechGSDK.cs which uses namespace LedCSharp
using static LedCSharp.LogitechGSDK;
using LedCSharp;
using System.Reflection;
using System.Globalization;
using System.Security.Cryptography;
using System.IO;

namespace G915X_KeyState_Indicator
{
    using DWORD = System.UInt32;        // 4 Bytes, aka uint, uint32
    using RGBTuple = (int R, int G, int B);

    public partial class MainForm : Form
    {
        // Keyboard hook constants
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int VK_NUMLOCK = 0x90;
        private const int VK_CAPSLOCK = 0x14;
        private const int VK_SCROLL = 0x91;
        private const int VM_SYSKEYDOWN = 0x104;
        private const int VM_SYSKEYUP = 0x105;

        private const string ConfigFileName = "Logi_KeyMonitor_Config.ini";

        List<int> keysToMonitor = new List<int> { 
            VK_NUMLOCK, 
            VK_CAPSLOCK,
            VK_SCROLL
        };


        // Win32 API imports
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int keyCode);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        // Set up colors for each key
        private RGBTuple default_Color = (R:0, G:0, B:255);

        private RGBTuple capsLock_On_Color = (R: 255, G: 0, B: 0);
        private RGBTuple scrollLock_On_Color = (R: 0, G: 0, B: 255);
        private RGBTuple numLock_On_Color = (R: 255, G: 0, B: 0);

        private RGBTuple capsLock_Off_Color = (R: 0, G: 0, B: 255);
        private RGBTuple scrollLock_Off_Color = (R: 255, G: 0, B: 0);
        private RGBTuple numLock_Off_Color = (R: 0, G: 0, B: 255);

        // Readonly default versions
        private static readonly RGBTuple _default_Color = (R: 0, G: 0, B: 255);
        private static readonly RGBTuple _capsLock_On_Color = (R: 255, G: 0, B: 0);
        private static readonly RGBTuple _scrollLock_On_Color = (R: 0, G: 0, B: 255);
        private static readonly RGBTuple _numLock_On_Color = (R: 255, G: 0, B: 0);
        private static readonly RGBTuple _capsLock_Off_Color = (R: 0, G: 0, B: 255);
        private static readonly RGBTuple _scrollLock_Off_Color = (R: 255, G: 0, B: 0);
        private static readonly RGBTuple _numLock_Off_Color = (R: 0, G: 0, B: 255);

        public MainForm()
        {
            // Add exe current directory to PATH
            string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);

            // Read the config file and load the colors
            LoadColors();

            LogitechGSDK.LogiLedInit(); // If this gives an error about module not found, try using x86 instead of x64. The x64 dll might be broken.
            
            InitializeComponent();
            SetupUI();

            // First set the base lighting for all keys
            LogitechGSDK.LogiLedSetLighting(redPercentage: default_Color.R, greenPercentage: default_Color.G, bluePercentage: default_Color.B);

            // Set up keyboard hook
            _proc = HookCallback;
            _hookID = SetHook(_proc);

            // Update the status of each key on startup
            foreach (int key in keysToMonitor)
            {
                UpdateKeyStatus(key);
            }
        }

        // ---------------------------------------------------------------------------------

        private void SetupUI()
        {
            this.Text = "Logitech Key State Monitor";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);

            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                // Get the data from the struct as an object
                KBDLLHOOKSTRUCT kbd = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                int vkCode = (int)kbd.vkCode;
                LowLevelKeyboardHookFlags flags = kbd.flags;

                // Checks if the numlock key was the one pressed, and only care about key up event
                if (keysToMonitor.Contains((int)vkCode) && flags.HasFlag(LowLevelKeyboardHookFlags.KeyUp))
                {
                    this.BeginInvoke(new Action(() => UpdateKeyStatus(vkCode)));
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void UpdateKeyStatus(int vkCode)
        {
            // Local function to update the label for whichever key changed
            void UpdateLabel(Label label, bool isOn, string keyName)
            {
                label.Text = $"{keyName} is currently: {(isOn ? "ON" : "OFF")}";
                label.ForeColor = isOn ? Color.Green : Color.Red;
            }

            bool isKeyOn = IsKeyStateOn((int)vkCode);

            // Switch case for each key
            switch (vkCode)
            {
                case VK_NUMLOCK:
                    UpdateLabel(labelNumLock, isKeyOn, "Num Lock");
                    break;
                case VK_CAPSLOCK:
                    UpdateLabel(labelCapsLock, isKeyOn, "Caps Lock");
                    break;
                case VK_SCROLL:
                    UpdateLabel(labelScrollLock, isKeyOn, "Scroll Lock");
                    break;
                default:
                    break;
            }

            UpdateLogitechKeyLight(isKeyOn, vkCode);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
            }
            LogitechGSDK.LogiLedShutdown();
            base.OnFormClosing(e);
        }

        private static bool IsKeyStateOn(int keyEnum)
        {
            return (GetKeyState(keyEnum) & 1) == 1;
        }

        private void UpdateLogitechKeyLight(bool isOn, int vkCode)
        {
            // Set target device to per-key RGB keyboards
            LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_PERKEY_RGB);

            

            // Set key colors for each
            RGBTuple onColor;
            RGBTuple offColor;

            keyboardNames keyToUpdate;
            if (vkCode == VK_NUMLOCK)
            {
                keyToUpdate = keyboardNames.NUM_LOCK;
                onColor = numLock_On_Color;
                offColor = numLock_Off_Color;
            }
            else if (vkCode == VK_CAPSLOCK)
            {
                keyToUpdate = keyboardNames.CAPS_LOCK;
                onColor = capsLock_On_Color;
                offColor = capsLock_Off_Color;
            }
            else if (vkCode == VK_SCROLL)
            {
                keyToUpdate = keyboardNames.SCROLL_LOCK;
                onColor = scrollLock_On_Color;
                offColor = scrollLock_Off_Color;
            }
            else
            {
                return;
            }

            // Then set the specific color for NUM_LOCK
            if (isOn)
            {
                LogitechGSDK.LogiLedSetLightingForKeyWithKeyName (
                    keyCode: keyToUpdate,
                    redPercentage: onColor.R,
                    greenPercentage: onColor.G,
                    bluePercentage: onColor.B
                );
            }
            else
            {
                LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(
                    keyCode: keyToUpdate,
                    redPercentage: offColor.R,
                    greenPercentage: offColor.G,
                    bluePercentage: offColor.B
                );
            }
        }

        // Returned as pointer in the lparam of the hook callback
        // See: https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-kbdllhookstruct
        private struct KBDLLHOOKSTRUCT
        {
            public DWORD vkCode;          // Virtual key code
            public DWORD scanCode;        
            public LowLevelKeyboardHookFlags flags;
            public DWORD time;
            public IntPtr dwExtraInfo;
        }

        [Flags]
        public enum LowLevelKeyboardHookFlags : uint
        {
            Extended = 0x01,              // Bit 0: Extended key (e.g. function key or numpad)
            LowerILInjected = 0x02,      // Bit 1: Injected from lower integrity level process
            Injected = 0x10,             // Bit 4: Injected from any process
            AltDown = 0x20,              // Bit 5: ALT key pressed
            KeyUp = 0x80                 // Bit 7: Key being released (transition state)
            // Bits 2-3, 6 are reserved
        }


    } // End of MainForm

} // End of namespace