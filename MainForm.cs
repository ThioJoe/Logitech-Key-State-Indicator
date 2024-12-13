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
using System.Net;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.ConstrainedExecution;

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

        // App related constants
        private const string ConfigFileName = "Logi_KeyMonitor_Config.ini";
        private const string logiDllName = "LogitechLedEnginesWrapper.dll";
        private const string ProgramName = "Key State Indicator For Logitech";

        // Other constants
        private const string logitechSDKURL = "https://logitechg.com/en-us/innovation/developer-lab.html";
        private const string appGitHubURL = "https://github.com/ThioJoe/Logitech-Key-State-Indicator";
        private const string directDLLDownloadURL = "https://github.com/ThioJoe/Logitech-Key-State-Indicator/raw/refs/heads/master/Resources/LogitechLedEnginesWrapper.dll";
        private const string dllAPIURL = "https://api.github.com/repos/ThioJoe/Logitech-Key-State-Indicator/contents/Resources/LogitechLedEnginesWrapper.dll";
        // Hard coding these but can't rely on them 100% in case they get updated. Though they haven't updated them since 2018.
        private const string x64dllHash = "46A0773E5AE6EF5B24557F3051E18A62527C7B2C133360DFB21522CBFE9CBDD1";
        private const string x86dllHash = "189172A1DE545A9F8058A1BF0980FA42F8CFA203340981EE38233094F75C3FD2";

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
        private RGBTuple default_key_color =         (R: 0, G: 0, B: 255);
        private RGBTuple default_otherDevice_color = (R: 0, G: 0, B: 255);
        // On
        private RGBTuple capsLock_On_Color =    (R: 255, G: 0, B: 0);
        private RGBTuple scrollLock_On_Color =  (R: 0, G: 0, B: 255);
        private RGBTuple numLock_On_Color =     (R: 255, G: 0, B: 0);
        // Off
        private RGBTuple capsLock_Off_Color =   (R: 0, G: 0, B: 255);
        private RGBTuple scrollLock_Off_Color = (R: 255, G: 0, B: 0);
        private RGBTuple numLock_Off_Color =    (R: 0, G: 0, B: 255);


        // Readonly default versions - Will be used if the config file is not found or a value is missing
        private static readonly RGBTuple _default_key_Color =         (R: 0, G: 0, B: 255);
        private static readonly RGBTuple _default_otherDevice_Color = (R: 0, G: 0, B: 255);
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
        private static InitStatus initStatus = InitStatus.NOT_CHECKED;
        private static bool firstConfigLoad = true;

        // ---------------------------------------------------------------------------------

        public MainForm()
        {
            #if DEBUG
            DEBUGMODE = true;
            #endif

            // Add exe current directory to PATH
            string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            initStatus = CheckLogitechDLL(); // Checks if the Logitech DLL is present, and then tries to initialize the engine

            // Read the config file and load the colors
            LoadConfig();
            CreateTrayIcon(startMinimized: startMinimizedToTray); // Create the icon before the rest of the initialization or else it will give errors
            InitializeComponent();

            SetupUI(initStatus: initStatus);

            LoadAndApplyConfig();

            // Set up keyboard hook
            _proc = HookCallback;
            _hookID = SetHook(_proc);

            // Update the status of each key on startup
            foreach (int key in keysToMonitor)
            {
                UpdateKeyStatus(key);
            }

            firstConfigLoad = false;

            // Apparently need to do this both here AND in the Load event handler or else it doesn't work
            if (startMinimizedToTray)
            {
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
            }
        }

        private void LoadAndApplyConfig()
        {
            if (!firstConfigLoad)
                LoadConfig();

            if (initStatus == InitStatus.SUCCESS)
            {
                // First set default color for all devices
                LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_ALL);
                LogitechGSDK.LogiLedSetLighting(redPercentage: ToPct(default_otherDevice_color.R), greenPercentage: ToPct(default_otherDevice_color.G), bluePercentage: ToPct(default_otherDevice_color.B));
                // Wait a short time
                System.Threading.Thread.Sleep(50);

                // Then target the keyboard and set base lighting color
                LogitechGSDK.LogiLedSetTargetDevice((LogitechGSDK.LOGI_DEVICETYPE_PERKEY_RGB));
                LogitechGSDK.LogiLedSetLighting(redPercentage: ToPct(default_key_color.R), greenPercentage: ToPct(default_key_color.G), bluePercentage: ToPct(default_key_color.B));

                // This tries to set it for keyboards, but doesn't seem to work for newer ones
                //LogitechGSDK.LogiLedSetLightingForTargetZone(
                //    DeviceType.Keyboard,
                //    0, // Zone - 0 is the entire keyboard
                //    ToPct(default_key_color.R),
                //    ToPct(default_key_color.G),
                //    ToPct(default_key_color.B)
                //);
            }

            UpdateColorLabel(labelColorDefault, default_key_color, "Default");

            // Don't apply the colors yet if this is the first time for this function because we haven't loaded the keyboard hooks yet
            if (!firstConfigLoad)
            {
                foreach (int key in keysToMonitor)
                {
                    UpdateKeyStatus(key);
                }
            }
        }

        private int ToPct(int value)
        {
            return (int)Math.Round((double)value / 255 * 100);
        }

        //TESTING
        private void ShowAndWaitForMessageBox(string message)
        {
            MessageBox.Show(message);
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
            FAIL,
            NOT_CHECKED
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
            // Get full path to the DLL
            string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string logiDllName = Path.Combine(exeDir, "LogitechLedEnginesWrapper.dll");
            string logiDllPath = Path.Combine(exeDir, logiDllName);

            // Look for the dll first, LogitechLedEnginesWrapper.dll
            if (!File.Exists(logiDllPath))
            {
                // If the dll is not found, show a message box and exit the application
                MessageBox.Show("LogitechLedEnginesWrapper.dll not found.",
                    "Logitech DLL Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return InitStatus.NO_DLL;
            }

            string failMessage = "Failed to load Logitech engine. General Tips:\n 1. Make sure the Logitech GHUB software is installed (it might need to also be running). " +
                $"\n2. If you used the 64 bit version of {logiDllName}, use the 32 bit version. I've found the 64 bit version didn't work for me.";

            // Initialize the Logitech engine
            bool initSuccess;
            try
            {
                initSuccess = LogitechGSDK.LogiLedInit();
            }
            catch (Exception ex)
            {
                string messageToShow = failMessage + $"\n\nHere is the Error:\n" + ex.Message;

                if (messageToShow.Contains("incorrect format") || messageToShow.ToLower().Contains("0x8007000B"))
                {
                    if (GetSha256Hash(logiDllPath) == x64dllHash)
                        messageToShow += "\n\nSPECIAL NOTE: This error is because you are trying to use the x64 (64-bit) version of the DLL. You must use the x86 (32-bit) version.";
                    else 
                        messageToShow += "\n\nSPECIAL NOTE: This particular error is likely because you used the 64 bit version of the DLL. Use the 32 bit version instead.";
                }

                MessageBox.Show(messageToShow,
                    "Logitech Engine Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return InitStatus.FAIL;
            }

            if (!initSuccess)
            {
                string messageToShow = failMessage + "\n\nSPECIAL NOTE: ";

                // Check if the file is signed by Logitech
                var (isValid, signerName) = VerifySignature(logiDllPath);

                // First check the hard coded hash
                if (GetSha256Hash(logiDllPath) == x86dllHash)
                {
                    messageToShow += "You definitely have the correct DLL, but it still failed to initialize the Logitech engine. Make sure you have the GHUB software is installed and running.";
                }
                else if (!isValid && signerName != null)
                {
                    messageToShow += $"The DLL appears to have an invalid or untrusted signature. It might be corrupted.";
                }
                else if (!isValid)
                {
                    messageToShow += "The DLL is NOT signed by Logitech. It may be corrupted or you're using the wrong file.";
                }
                else if (signerName.StartsWith("Logitech"))
                {
                    messageToShow += "The DLL appears likely valid, but it still failed to initialize Logitech engine. Make sure the GHUB software is installed and running.";
                }
                
                MessageBox.Show(messageToShow, "Failed to attach to Logitech Engine", MessageBoxButtons.OK, MessageBoxIcon.Error);

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

            if (initStatus == InitStatus.SUCCESS)
            {
                LogitechGSDK.LogiLedShutdown();
            }
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
                UpdateColorLabel(GUILabelForKey, onColor, keyNames[vkCode]);

                if (initStatus != InitStatus.SUCCESS)
                    return;

                LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(
                    keyCode: keyToUpdate,
                    redPercentage: onColor.R,
                    greenPercentage: onColor.G,
                    bluePercentage: onColor.B
                );
            }
            else
            {
                UpdateColorLabel(GUILabelForKey, offColor, keyNames[vkCode]);

                if (initStatus != InitStatus.SUCCESS)
                    return;

                LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(
                    keyCode: keyToUpdate,
                    redPercentage: offColor.R,
                    greenPercentage: offColor.G,
                    bluePercentage: offColor.B
                );
            }
        }
        
        private string GetSha256Hash(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

        private (bool isValid, string signerName) VerifySignature(string filePath)
        {
            X509Certificate2 cert;
            string signerName;

            try
            {
                cert = new X509Certificate2(filePath);
            }
            catch
            {
                return (false, null);
            }

            try
            {
                signerName = cert.GetNameInfo(X509NameType.SimpleName, false);
            }
            catch
            {
                return (false, null);
            }

            // Check if the certificate is valid
            try
            {
                cert.Verify(); // Throws an exception if not valid

            }
            catch (CryptographicException)
            {
                if (signerName != null)
                {
                    return (false, signerName);
                }
                else
                {
                    return (false, null);
                }
            }
            catch
            {
                return (false, null);
            }

            signerName = cert.GetNameInfo(X509NameType.SimpleName, false);
            return (true, signerName);
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

        private void buttonOpenLogitechSDKPage_Click(object sender, EventArgs e)
        {
            // First show a message box asking if they want to open the page, if they hit OK then open the page
            DialogResult result = MessageBox.Show($"This will open the Logitech SDK Webpage:\n{logitechSDKURL}" +
                $"\n\nIf you prefer to download the required {logiDllName} file from there, you can find it in the \"LED Illumination SDK\" download. " +
                $"\n\nExtract the ZIP file and navigate to:" +
                $"\nLED > Lib > LogitechLedEnginesWrapper > x86" +
                $"\n\n(Note: Use the x86 version, for some reason the x64 version doesn't seem to work)" +
                $"\n\nContinue?",
                "Open Logitech SDK Website", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if (result == DialogResult.OK)
            {
                System.Diagnostics.Process.Start(logitechSDKURL);
            }
        }

        private void buttonOpenAppGitHubPage_Click(object sender, EventArgs e)
        {
            // First show a message box asking if they want to open the page, if they hit OK then open the page
            DialogResult result = MessageBox.Show($"This will open the GitHub page for this application:\n{appGitHubURL}" +
                $"\n\nHere you can find the source code, report issues, or contribute to the project." +
                $"\n\nYou'll also find the required {logiDllName} file in the \"Resources\" folder, the direct download button didn't work for some reason." +
                $"\n\nContinue?",
                "Open GitHub Page", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if (result == DialogResult.OK)
            {
                System.Diagnostics.Process.Start(appGitHubURL);
            }
        }

        private void buttonDownloadDLL_Click(object sender, EventArgs e)
        {
            // First show a message box asking if they want to download, if they click OK then proceed
            DialogResult result = MessageBox.Show($"This will download the required {logiDllName} file directly from this app's GitHub repository. It is signed by Logitech." +
                $"\n\nThis is the file that allows the application to interface with Logitech keyboards." +
                $"\nYou also need to have Logitech's GHUB software installed, and it probably needs to be running." +
                $"\n\nIf you prefer, you can also get the DLL directly from Logitech using the other button to open their SDK website." +
                $"\n\nDownload?",
                "Download DLL", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if (result == DialogResult.OK)
            {
                DownloadDLL();
            }
        }

        // From: https://stackoverflow.com/a/30201071/17312053
        private void DownloadDLL()
        {
            //GetFileInfo();
            // Get current directory
            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //Retrieve the path from the input textbox
            string filepath = currentDir + $"\\{logiDllName}";

            // If it already exists, ask if they want to overwrite it
            if (File.Exists(filepath))
            {
                DialogResult result = MessageBox.Show($"The file {logiDllName} already exists in the application directory. Do you want to overwrite it?",
                    "File Already Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                {
                    return;
                }
            }

            //Create a WebClient to use as our download proxy for the program.
            WebClient webClient = new WebClient();

            //Attach the DownloadFileCompleted event to your new AsyncCompletedEventHandler Completed
            //so when the event occurs the method is called.
            webClient.DownloadFileCompleted += (sender, e) => Completed(sender, e, filepath);

            //Attempt to actually download the file, this is where the error that you
            //won't see is probably occurring, this is because it checks the url in 
            //the blocking function internally and won't execute the download itself 
            //until this clears.
            webClient.DownloadFileAsync(new Uri(directDLLDownloadURL), filepath);
        }

        //This is your method that will pop when the AsyncCompletedEvent is fired,
        //this doesn't mean that the download was successful though which is why
        //it's misleading, it just means that the Async process completed.
        private void Completed(object sender, AsyncCompletedEventArgs e, string filepath)
        {
            // Check if the file exists
            if (File.Exists(filepath))
            {
                // First check the known good hash
                string hash = GetSha256Hash(filepath);
                if (hash == x86dllHash)
                {
                    MessageBox.Show($"Download completed!\n\n\nRestart the app for it to take effect.");
                    return;
                }

                // Check if the file is signed by Logitech
                var (isValid, signerName) = VerifySignature(filepath);
                if (isValid && signerName.StartsWith("Logitech"))
                {
                    MessageBox.Show($"Download completed!\nRestart the app for it to take effect.");
                }
                else if (!isValid)
                {
                    MessageBox.Show("Download completed it doesn't seem correct. It might have been corrupted. Try downloading it yourself.");
                }
            }
            else
            {
                MessageBox.Show("Download failed! Try downloading it yourself with one of the other buttons.");
            }
        }

        private void buttonReloadConfig_Click(object sender, EventArgs e)
        {
            LoadAndApplyConfig();
        }

        //private void GetFileInfo()
        //{
        //    string url = dllAPIURL;
        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        //    request.UserAgent = ProgramName;
        //    request.Accept = "application/vnd.github+json";
        //    //request.Headers.Add("Authorization", "Bearer <YOUR-TOKEN>");
        //    request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

        //    try
        //    {
        //        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        //        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        //        {
        //            string responseText = reader.ReadToEnd();
        //            var serializer = new JavaScriptSerializer();
        //            dynamic result = serializer.Deserialize<dynamic>(responseText);
        //            string sha = result["sha"];
        //            string base64Content = result["content"];
        //            string encoding = result["encoding"];

        //            Debug.WriteLine(responseText);
        //        }
        //    }
        //    catch (WebException ex)
        //    {
        //        Debug.WriteLine($"Error: {ex.Message}");
        //    }
        //}

        // --------------------------------------------------------------------------------

    } // End of MainForm

} // End of namespace