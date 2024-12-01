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

namespace G915X_KeyState_Indicator
{
    public partial class MainForm : Form
    {
        // Keyboard hook constants
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int VK_NUMLOCK = 0x90;

        // Win32 API imports
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        // Track previous state
        private bool _previousNumLockState;
        private Label _statusLabel;

        public MainForm()
        {
            InitializeComponent();
            SetupUI();

            // Set up keyboard hook
            _proc = HookCallback;
            _hookID = SetHook(_proc);

            // Initialize state
            _previousNumLockState = Control.IsKeyLocked(Keys.NumLock);
            UpdateNumLockStatus();
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
                int vkCode = Marshal.ReadInt32(lParam);
                if (vkCode == VK_NUMLOCK)
                {
                    // The NumLock key was just pressed/released
                    // Wait a tiny bit for the state to settle
                    Task.Delay(50).ContinueWith(t =>
                    {
                        bool currentState = Control.IsKeyLocked(Keys.NumLock);
                        if (currentState != _previousNumLockState)
                        {
                            _previousNumLockState = currentState;
                            // Invoke on UI thread since we're in a callback
                            this.BeginInvoke(new Action(() =>
                            {
                                UpdateNumLockStatus();
                                // Double-check the state after a short delay
                                Task.Delay(100).ContinueWith(t2 =>
                                {
                                    bool verifyState = Control.IsKeyLocked(Keys.NumLock);
                                    if (verifyState != currentState)
                                    {
                                        _previousNumLockState = verifyState;
                                        this.BeginInvoke(new Action(UpdateNumLockStatus));
                                    }
                                }, TaskScheduler.Default);
                            }));
                        }
                    }, TaskScheduler.Default);
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void UpdateNumLockStatus()
        {
            bool isNumLockOn = Control.IsKeyLocked(Keys.NumLock);

            // Update UI
            _statusLabel.Text = $"NumLock is currently: {(isNumLockOn ? "ON" : "OFF")}";
            _statusLabel.ForeColor = isNumLockOn ? Color.Green : Color.Red;

            // Here you could add your Logitech G915 specific code
            // for updating the keyboard LEDs based on NumLock state
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
            }
            base.OnFormClosing(e);
        }
    }
}