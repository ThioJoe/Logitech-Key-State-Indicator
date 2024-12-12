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
using static G915X_KeyState_Indicator.MainForm;
using System.Diagnostics.Eventing.Reader;

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
        private const string logiDllName = "LogitechLedEnginesWrapper.dll";
        private const string ProgramName = "Key State Indicator For Logitech";

        List<int> keysToMonitor = new List<int> { 
            VK_NUMLOCK, 
            VK_CAPSLOCK,
            VK_SCROLL
        };

        Dictionary<int, string> keyNames = new Dictionary<int, string>
        {
            { VK_NUMLOCK, "Num Lock" },
            { VK_CAPSLOCK, "Caps Lock" },
            { VK_SCROLL, "Scroll Lock" },
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

        // Set up colors for each key. These will properties will be updated from the config file
        private RGBTuple default_Color =        (R:0, G:0, B:255);
        // On
        private RGBTuple capsLock_On_Color =    (R: 255, G: 0, B: 0);
        private RGBTuple scrollLock_On_Color =  (R: 0, G: 0, B: 255);
        private RGBTuple numLock_On_Color =     (R: 255, G: 0, B: 0);
        // Off
        private RGBTuple capsLock_Off_Color =   (R: 0, G: 0, B: 255);
        private RGBTuple scrollLock_Off_Color = (R: 255, G: 0, B: 0);
        private RGBTuple numLock_Off_Color =    (R: 0, G: 0, B: 255);


        // Readonly default versions - Will be used if the config file is not found or a value is missing
        private static readonly RGBTuple _default_Color =           (R: 0, G: 0, B: 255);
        // On
        private static readonly RGBTuple _capsLock_On_Color =       (R: 255, G: 0, B: 0);
        private static readonly RGBTuple _scrollLock_On_Color =     (R: 255, G: 0, B: 0);
        private static readonly RGBTuple _numLock_On_Color =        (R: 255, G: 0, B: 0);
        // Off
        private static readonly RGBTuple _capsLock_Off_Color =      (R: 0, G: 0, B: 255);
        private static readonly RGBTuple _scrollLock_Off_Color =    (R: 0, G: 0, B: 255);
        private static readonly RGBTuple _numLock_Off_Color =       (R: 0, G: 0, B: 255);

        // Debugging variables
        private static bool DEBUGMODE = false;
        private static int debugCounter = 0;

        // Other Defaults
        private static bool startMinimizedToTray = false;

        // ---------------------------------------------------------------------------------

        public MainForm()
        {
            #if DEBUG
            DEBUGMODE = true;
            #endif

            // Add exe current directory to PATH
            string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            InitStatus initStatus = CheckLogitechDLL(); // Checks if the Logitech DLL is present, and then tries to initialize the engine

            // Read the config file and load the colors
            LoadConfig();
            CreateTrayIcon(startMinimized:startMinimizedToTray); // Create the icon before the rest of the initialization or else it will give errors
            InitializeComponent();

            SetupUI(initStatus: initStatus);

            // First set the base lighting for all keys
            // Set target device to per-key RGB keyboards
            LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_PERKEY_RGB);

            LogitechGSDK.LogiLedSetLighting(redPercentage: default_Color.R, greenPercentage: default_Color.G, bluePercentage: default_Color.B);
            UpdateColorLabel(labelColorDefault, default_Color, "Default");

            // Set up keyboard hook
            _proc = HookCallback;
            _hookID = SetHook(_proc);

            // Update the status of each key on startup
            foreach (int key in keysToMonitor)
            {
                UpdateKeyStatus(key);
            }

            // Apparently need to do this both here AND in the Load event handler or else it doesn't work
            if (startMinimizedToTray)
            {
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
            }
        }

        // ---------------------------------------------------------------------------------

        private void SetupUI(InitStatus initStatus)
        {
            this.Text = ProgramName + " - Statuses";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            if (initStatus == InitStatus.SUCCESS)
            {
                labelLogitechStatus.Text = "Logitech Engine Status: Initialized";
                labelLogitechStatus.ForeColor = Color.Green;
            }
            else if (initStatus == InitStatus.FAIL)
            {
                labelLogitechStatus.Text = "Logitech Engine Status: Failed to Initialize";
                labelLogitechStatus.ForeColor = Color.Red;
            }
            else if (initStatus == InitStatus.NO_DLL)
            {
                labelLogitechStatus.Text = "Logitech Engine Status: DLL Not Found";
                labelLogitechStatus.ForeColor = Color.Red;
            }

            if (DEBUGMODE)
                labelDebug.Visible = true;
            else
                labelDebug.Visible = false;
        }

        public enum InitStatus
        {
            SUCCESS,
            NO_DLL,
            FAIL
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

                // Checks if a monitored key was the one pressed, and only care about key up event
                // While the key is held down, the event is triggered repeatedly, but only once when released, so we can go based just on key up
                if (keysToMonitor.Contains((int)vkCode) && flags.HasFlag(LowLevelKeyboardHookFlags.KeyUp))
                {
                    if (DEBUGMODE)
                    {
                        debugCounter++;
                        labelDebug.Text = $"Key: {vkCode}, Flags: {flags}\nCount: {debugCounter}";
                    }

                    this.BeginInvoke(new Action(() => UpdateKeyStatus(vkCode)));
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private InitStatus CheckLogitechDLL()
        {
            // Look for the dll first, LogitechLedEnginesWrapper.dll
            if (!File.Exists(logiDllName))
            {
                // If the dll is not found, show a message box and exit the application
                MessageBox.Show("LogitechLedEnginesWrapper.dll not found.",
                    "Logitech DLL Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return InitStatus.NO_DLL;
            }

            // Initialize the Logitech engine
            bool initSuccess = LogitechGSDK.LogiLedInit();
            if (!initSuccess)
            {
                MessageBox.Show("Failed to load Logitech engine. Make sure the Logitech GHUB software is installed. " +
                    $"Also if you used the 64 bit version of {logiDllName}, use the 32 bit version. I've found the 64 bit version didn't work for me.",
                    "Logitech DLL Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return InitStatus.FAIL;
            }
            else
            {
                return InitStatus.SUCCESS;
            }
        }

        private void ShowHelp()
        {
            MessageBox.Show("" +
                "",


                "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }


        private void UpdateColorLabel(Label label, RGBTuple color, string keyName)
        {
            //label.Text = "◼";
            System.Drawing.Color colorObj = System.Drawing.Color.FromArgb(255, color.R, color.G, color.B);
            label.ForeColor = colorObj;
            // Get the tooltip of the label and display the RGB values
            string tooltipText = $"Currently Displayed {keyName} Color:\nR:  {color.R}\nG:  {color.G}\nB:  {color.B}";
            toolTip1.SetToolTip(label, tooltipText);
        }

        private void UpdateKeyStatus(int vkCode)
        {
            // Local function to update the label for whichever key changed
            void UpdateLabel(Label label, bool isOn, string keyName)
            {
                label.Text = $"{keyName} is currently: {(isOn ? "ON" : "OFF")}";
                //label.ForeColor = isOn ? Color.Green : Color.Red;
            }

            bool isKeyOn = IsKeyStateOn((int)vkCode);

            // Switch case for each key
            switch (vkCode)
            {
                case VK_NUMLOCK:
                    UpdateLabel(labelNumLock, isKeyOn, keyNames[VK_NUMLOCK]);
                    break;
                case VK_CAPSLOCK:
                    UpdateLabel(labelCapsLock, isKeyOn, keyNames[VK_CAPSLOCK]);
                    break;
                case VK_SCROLL:
                    UpdateLabel(labelScrollLock, isKeyOn, keyNames[VK_SCROLL]);
                    break;
                default:
                    break;
            }

            UpdateLogitechKeyLight(isKeyOn, vkCode);
        }

        private void ShutDown_HooksAndLogi()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
            }
            LogitechGSDK.LogiLedShutdown();
        }

        //protected override void OnFormClosing(FormClosingEventArgs e)
        //{
        //    if (_hookID != IntPtr.Zero)
        //    {
        //        UnhookWindowsHookEx(_hookID);
        //    }
        //    LogitechGSDK.LogiLedShutdown();
        //    base.OnFormClosing(e);
        //}

        private static bool IsKeyStateOn(int keyEnum)
        {
            return (GetKeyState(keyEnum) & 1) == 1;
        }

        private void UpdateLogitechKeyLight(bool isOn, int vkCode)
        {
            // Set key colors for each
            RGBTuple onColor;
            RGBTuple offColor;
            Label GUILabelForKey;

            keyboardNames keyToUpdate;
            if (vkCode == VK_NUMLOCK)
            {
                keyToUpdate = keyboardNames.NUM_LOCK; // Enum from Logitech SDK for key wScan codes (not vk)
                onColor = numLock_On_Color;
                offColor = numLock_Off_Color;
                GUILabelForKey = labelColorNumLock;
            }
            else if (vkCode == VK_CAPSLOCK)
            {
                keyToUpdate = keyboardNames.CAPS_LOCK; // Enum from Logitech SDK for key wScan codes (not vk)
                onColor = capsLock_On_Color;
                offColor = capsLock_Off_Color;
                GUILabelForKey = labelColorCapsLock;
            }
            else if (vkCode == VK_SCROLL)
            {
                keyToUpdate = keyboardNames.SCROLL_LOCK; // Enum from Logitech SDK for key wScan codes (not vk)
                onColor = scrollLock_On_Color;
                offColor = scrollLock_Off_Color;
                GUILabelForKey = labelColorScrollLock;
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
                UpdateColorLabel(GUILabelForKey, onColor, keyNames[vkCode]);
            }
            else
            {
                LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(
                    keyCode: keyToUpdate,
                    redPercentage: offColor.R,
                    greenPercentage: offColor.G,
                    bluePercentage: offColor.B
                );
                UpdateColorLabel(GUILabelForKey, offColor, keyNames[vkCode]);
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

        private void buttonOpenConfigFile_Click(object sender, EventArgs e)
        {
            // Set working directory to the exe directory
            string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Ensure the file exists. It should be on first startup but maybe the user deleted it
            if (!File.Exists(ConfigFileName))
            {
                MessageBox.Show($"Config file not found. A template config file has been created called {ConfigFileName}. " +
                    $"\n\nIn the mean time default color will be used. You can customize the colors in the config then restart the app to use them.",
                    "Config File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                WriteTemplateConfig();
                return;
            }
            // Open the config file in the default text editor
            System.Diagnostics.Process.Start(ConfigFileName);

        }

        private void buttonOpenDirectory_Click(object sender, EventArgs e)
        {
            // Open the directory of the current exe
            string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            System.Diagnostics.Process.Start(exeDir);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Apparently need to do this both here AND after initalizing the form or else it doesn't work
            if (startMinimizedToTray)
            {
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
            }
        }

        // --------------------------------------------------------------------------------

    } // End of MainForm

} // End of namespace