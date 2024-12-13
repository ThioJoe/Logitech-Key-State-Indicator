using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace G915X_KeyState_Indicator
{
    using DWORD = System.UInt32;        // 4 Bytes, aka uint, uint32
    using RGBTuple = (int R, int G, int B);

    public partial class MainForm : Form
    {

        // ------------------------ CONFIG FILE READING ------------------------

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetPrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpDefault,
            StringBuilder lpReturnedString,
            uint nSize,
            string lpFileName
        );

        private string ReadConfigIni(string section, string key)
        {
            int error = -1;
            const uint BUFFER_SIZE = 1024; // Increased buffer size
            StringBuilder sb = new StringBuilder((int)BUFFER_SIZE);

            // Make sure ConfigFileName is a full path
            string fullPath = Path.GetFullPath(ConfigFileName);

            uint result = GetPrivateProfileString(section, key, "", sb, BUFFER_SIZE, fullPath);

            if (result == 0)
            {
                // Get the error
                error = Marshal.GetLastWin32Error();
                if (error == 2) // ERROR_FILE_NOT_FOUND
                {
                    throw new FileNotFoundException($"Config file not found: {fullPath}");
                }
                throw new Win32Exception(error);
            }

            return sb.ToString().Trim();
        }

        private RGBTuple ReadColorFromConfig(string section, string key, RGBTuple defaultColor)
        {
            // Read the raw string from the INI file
            string colorValue = ReadConfigIni(section, key).Trim();

            // Check if the value is "default"
            if (string.Equals(colorValue, "default", StringComparison.OrdinalIgnoreCase))
            {
                return defaultColor;
            }

            try
            {
                // Split the string by commas and parse each value
                string[] rgbValues = colorValue.Split(',');

                if (rgbValues.Length != 3)
                {
                    throw new FormatException($"Invalid RGB format in config for {key}. Expected 3 values, got {rgbValues.Length}");
                }

                // Parse and validate each RGB component
                int r = ValidateColorComponent(int.Parse(rgbValues[0].Trim()), key, "R");
                int g = ValidateColorComponent(int.Parse(rgbValues[1].Trim()), key, "G");
                int b = ValidateColorComponent(int.Parse(rgbValues[2].Trim()), key, "B");

                return (r, g, b);
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException)
            {
                // Log error or handle it as needed
                throw new FormatException($"Error parsing RGB values for {key}: {ex.Message}");
            }
        }

        // Validates that a color component is within the valid range (0-255)
        private int ValidateColorComponent(int value, string key, string component)
        {
            if (value < 0 || value > 255)
            {
                throw new FormatException(
                    $"Invalid {component} value {value} for {key}. Must be between 0 and 255.");
            }
            return value;
        }

        private void LoadConfig()
        {
            if (!File.Exists(ConfigFileName))
            {
                WriteTemplateConfig();
                MessageBox.Show($"Config file not found. A template config file has been created called {ConfigFileName}. " +
                    $"\n\nIn the mean time default color will be used. You can customize the colors in the config then restart the app to use them.",
                    "Config File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UseDefaults();
                return;
            }

            const string sectionName = "settings";
            List<string> errors = new List<string>();

            // Simple helper function
            RGBTuple LoadColor(string settingName, RGBTuple defaultValue)
            {
                try
                {
                    return ReadColorFromConfig(sectionName, settingName, defaultValue);
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to load setting: {settingName}\n    {ex.Message}\n\n");
                    return defaultValue;
                }
            }

            // Load all color, specifying the default value for each to use if there's an error
            default_key_color = LoadColor("default_key_color", _default_key_Color);
            default_otherDevice_color = LoadColor("default_other_device_color", _default_otherDevice_Color);
            capsLock_On_Color = LoadColor("capsLock_On_color", _capsLock_On_Color);
            scrollLock_On_Color = LoadColor("scrollLock_On_color", _scrollLock_On_Color);
            numLock_On_Color = LoadColor("numLock_On_color", _numLock_On_Color);
            capsLock_Off_Color = LoadColor("capsLock_Off_color", _capsLock_Off_Color);
            scrollLock_Off_Color = LoadColor("scrollLock_Off_color", _scrollLock_Off_Color);
            numLock_Off_Color = LoadColor("numLock_Off_color", _numLock_Off_Color);

            // Load debug mode
            try
            {
                string debugModeSetting = ReadConfigIni(sectionName, "debugMode").Trim().ToLower();
                DEBUGMODE = ValidateAndParseBool(stringValue: debugModeSetting, settingName: "debugMode", defaultOnFail: false);
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to load setting: debugMode:\n    {ex.Message}");
                DEBUGMODE = false;
            }

            // Minimalize to tray
            try
            {
                string minimizedString = ReadConfigIni(sectionName, "minimize_to_tray").Trim().ToLower();
                startMinimizedToTray = ValidateAndParseBool(stringValue:minimizedString, settingName:"minimize_to_tray", defaultOnFail:false);
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to load setting: minimize_to_tray\n    {ex.Message}");
                startMinimizedToTray = false;
            }

            // Show errors if any occurred
            if (errors.Count > 0)
            {
                MessageBox.Show(
                    $"Some settings failed to load (default values will be used for these settings):\n\n{string.Join("\n", errors)}" +
                    $"\nNote: If it says \"config file not found\" it probably means that individual setting wasn't found, not the entire file.\n(It's a Windows API thing)\n" +
                    $"You can delete or rename the config file to have it generate a new one.",
                    "Configuration Warnings",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private bool ValidateAndParseBool(string stringValue, string settingName, bool defaultOnFail)
        {
            if (string.Equals(stringValue, "true", StringComparison.OrdinalIgnoreCase))
                return true;
            else if (string.Equals(stringValue, "false", StringComparison.OrdinalIgnoreCase))
                return false;
            else
            {
                MessageBox.Show($"Invalid boolean value for {settingName}. Must be 'true' or 'false.");
                return defaultOnFail;
            }
        }

        private void UseDefaults()
        {
            default_key_color = _default_key_Color;
            default_otherDevice_color = _default_otherDevice_Color;
            capsLock_On_Color = _capsLock_On_Color;
            scrollLock_On_Color = _scrollLock_On_Color;
            numLock_On_Color = _numLock_On_Color;
            capsLock_Off_Color = _capsLock_Off_Color;
            scrollLock_Off_Color = _scrollLock_Off_Color;
            numLock_Off_Color = _numLock_Off_Color;
            DEBUGMODE = false;
            startMinimizedToTray = false;
        }

        private void WriteTemplateConfig()
        {
            string defaultTemplateString = """
# Set the colors. They must be in comma separated format of 3 numbers from 0 to 255 (R,G,B)
    # For example, a pure green key at max brightness would be:    whateverSetting=0,255,0
# Or, you can use the special keyword "default" to make it equal to the default color
    # For example  whateverSetting=default

[Settings]
    # The color of the rest of the keys on the keyboard
default_key_color=0,0,255
    # The color for other logitech devices, if any. Unfortunately there doesn't seem to be a way to just keep their current color automatically. It should go back when the app closes though.
default_other_device_color=0,0,255

    # The color of each key while its status is ON
capsLock_On_color=255,0,0
scrollLock_On_color=255,0,0
numLock_On_color=255,0,0

    # The color of each key while its status is OFF.
capsLock_Off_color=default
scrollLock_Off_color=default
numLock_Off_color=default

minimize_to_tray=false
debugMode=false

""";
            try
            {
                File.WriteAllText(ConfigFileName, defaultTemplateString);
            }
            catch (Exception ex)
            {
                // If there was a permission error, put it on the desktop and tell the user to move it manually
                if (ex is UnauthorizedAccessException || ex is IOException)
                {
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string desktopConfigPath = Path.Combine(desktopPath, Path.GetFileName(ConfigFileName));
                    File.WriteAllText(desktopConfigPath, defaultTemplateString);
                    MessageBox.Show($"Failed to write template config file to the program directory. It has been placed on your desktop as {Path.GetFileName(ConfigFileName)}. " +
                        $"Please move it to the program directory to use it.", "Error Writing Config File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }
    }
}
