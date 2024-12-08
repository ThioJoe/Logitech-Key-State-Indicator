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

namespace G915X_KeyState_Indicator
{
    using DWORD = System.UInt32;        // 4 Bytes, aka uint, uint32

    public partial class MainForm : Form
    {
        // Keyboard hook constants
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int VK_NUMLOCK = 0x90;
        private const int VK_CAPSLOCK = 0x14;
        private const int VM_SYSKEYDOWN = 0x104;
        private const int VM_SYSKEYUP = 0x105;

        List<int> keysToMonitor = new List<int> { 
            VK_NUMLOCK, 
            VK_CAPSLOCK 
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

        // Track previous state
        private bool _previousNumLockState;
        private Label _statusLabel;

        // Track current desired colors
        private int defaultRed = 0;
        private int defaultGreen = 0;
        private int defaultBlue = 255;

        private int activatedRed = 255;
        private int activatedGreen = 0;
        private int activatedBlue = 0;

        public MainForm()
        {
            // Add exe current directory to PATH
            string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);

            LogitechGSDK.LogiLedInit(); // If this gives an error about module not found, try using x86 instead of x64. The x64 dll might be broken.
            
            InitializeComponent();
            SetupUI();

            // Set up keyboard hook
            _proc = HookCallback;
            _hookID = SetHook(_proc);

            foreach (int key in keysToMonitor)
            {
                UpdateNumLockStatus(key);
            }
        }

        private void SetupUI()
        {
            this.Text = "G915X Key State Monitor";
            this.Size = new Size(300, 150);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            _statusLabel = new Label
            {
                AutoSize = true,
                Location = new Point(12, 20),
                Font = new Font("Segoe UI", 12F),
                Text = "Initializing..."
            };

            this.Controls.Add(_statusLabel);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
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
                    this.BeginInvoke(new Action(() => UpdateNumLockStatus(vkCode)));
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void UpdateNumLockStatus(int vkCode)
        {
            bool isKeyOn = IsKeyStateOn((int)vkCode);

            // Update UI
            _statusLabel.Text = $"Key is currently: {(isKeyOn ? "ON" : "OFF")}";
            _statusLabel.ForeColor = isKeyOn ? Color.Green : Color.Red;

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

            // First set the base lighting for all keys (e.g., white)
            LogitechGSDK.LogiLedSetLighting(redPercentage: defaultRed, greenPercentage: defaultGreen, bluePercentage: defaultBlue); // This sets all keys to white

            keyboardNames keyToUpdate;
            if (vkCode == VK_NUMLOCK)
            {
                keyToUpdate = keyboardNames.NUM_LOCK;
            }
            else if (vkCode == VK_CAPSLOCK)
            {
                keyToUpdate = keyboardNames.CAPS_LOCK;
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
                    redPercentage: defaultRed,
                    greenPercentage: defaultGreen,
                    bluePercentage: defaultBlue
                   );
            }
            else
            {
                LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(
                    keyCode: keyToUpdate,
                    redPercentage: activatedRed,
                    greenPercentage: activatedGreen,
                    bluePercentage: activatedBlue);
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
    }
}