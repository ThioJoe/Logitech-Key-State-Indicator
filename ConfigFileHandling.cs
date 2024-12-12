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
                    errors.Add($"Failed to load {settingName}: {ex.Message}");
                    return defaultValue;
                }
            }

            // Load all color, specifying the default value for each to use if there's an error
            default_Color = LoadColor("default_Color", _default_Color);
            capsLock_On_Color = LoadColor("capsLock_On_Color", _capsLock_On_Color);
            scrollLock_On_Color = LoadColor("scrollLock_On_Color", _scrollLock_On_Color);
            numLock_On_Color = LoadColor("numLock_On_Color", _numLock_On_Color);
            capsLock_Off_Color = LoadColor("capsLock_Off_Color", _capsLock_Off_Color);
            scrollLock_Off_Color = LoadColor("scrollLock_Off_Color", _scrollLock_Off_Color);
            numLock_Off_Color = LoadColor("numLock_Off_Color", _numLock_Off_Color);

            // Load debug mode
            try
            {
                DEBUGMODE = ReadConfigIni(sectionName, "debugMode").Trim().ToLower() == "true";
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to load debugMode setting: {ex.Message}");
                DEBUGMODE = false;
            }

            // Show errors if any occurred
            if (errors.Count > 0)
            {
                MessageBox.Show(
                    $"Some settings failed to load (default values will be used for these settings):\n\n{string.Join("\n", errors)}" +
                    $"\n\nNote: If it says \"config file not found\" it probably means that individual setting wasn't found, not the entire file.\n(It's a Windows API thing)",
                    "Configuration Warnings",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void UseDefaults()
        {
            default_Color = _default_Color;
            capsLock_On_Color = _capsLock_On_Color;
            scrollLock_On_Color = _scrollLock_On_Color;
            numLock_On_Color = _numLock_On_Color;
            capsLock_Off_Color = _capsLock_Off_Color;
            scrollLock_Off_Color = _scrollLock_Off_Color;
            numLock_Off_Color = _numLock_Off_Color;
            DEBUGMODE = false;
        }

        private void WriteTemplateConfig()
        {
            string defaultTemplateString = """
# Set the colors. They must be in comma separated format of 3 numbers from 0 to 255 (R,G,B)
    # For example, a pure green key at max brightness would be    whateverSetting=0,255,0
# Or, you can use the special keyword "default" to make it equal to the default color
    # For example  whateverSetting=default

[Settings]
    # The color of the rest of the key son the keyboard
default_Color=0,0,255

    # The color of each key while its status is ON
capsLock_On_Color=255,0,0
scrollLock_On_Color=255,0,0
numLock_On_Color=255,0,0

    # The color of each key while its status is OFF. Can be either 
capsLock_Off_Color=default
scrollLock_Off_Color=default
numLock_Off_Color=default

debugMode=false

""";

            File.WriteAllText(ConfigFileName, defaultTemplateString);
        }


    }
}
