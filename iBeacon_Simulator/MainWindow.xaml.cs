using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Security.Cryptography;
using System;
using System.Linq;

namespace IBeaconSimulator
{
    public sealed partial class MainWindow : Window
    {
        private BluetoothLEAdvertisementPublisher? publisher;
        private bool isBroadcasting = false;

        public MainWindow()
        {
            this.InitializeComponent();
            SetWindowSize(500, 600);
            SetWindowIcon();
        }

        private void SetWindowIcon()
        {
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.SetIcon("Assets/icon.png");
        }

        private void SetWindowSize(int width, int height)
        {
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(width, height));
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isBroadcasting)
            {
                try
                {
                    // Validate inputs
                    if (!ValidateInputs(out string errorMessage))
                    {
                        StatusText.Text = $"Error: {errorMessage}";
                        return;
                    }

                    StartBeacon();
                    StartButton.IsEnabled = false;
                    StopButton.IsEnabled = true;
                    // Disable text boxes while broadcasting
                    UuidTextBox.IsEnabled = false;
                    MajorTextBox.IsEnabled = false;
                    MinorTextBox.IsEnabled = false;
                    IdentifierTextBox.IsEnabled = false;
                    StatusText.Text = "Status: Broadcasting...";
                    isBroadcasting = true;
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"Error: {ex.Message}";
                }
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (isBroadcasting)
            {
                try
                {
                    publisher?.Stop();
                    StartButton.IsEnabled = true;
                    StopButton.IsEnabled = false;
                    // Re-enable text boxes
                    UuidTextBox.IsEnabled = true;
                    MajorTextBox.IsEnabled = true;
                    MinorTextBox.IsEnabled = true;
                    IdentifierTextBox.IsEnabled = true;
                    StatusText.Text = "Status: Stopped.";
                    isBroadcasting = false;
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"Error: {ex.Message}";
                }
            }
        }

        private bool ValidateInputs(out string errorMessage)
        {
            errorMessage = "";

            // Validate UUID
            string uuid = UuidTextBox.Text.Trim().Replace("-", "");
            if (string.IsNullOrWhiteSpace(uuid) || uuid.Length != 32)
            {
                errorMessage = "UUID must be 32 hex characters (with or without dashes)";
                return false;
            }

            if (!uuid.All(c => "0123456789ABCDEFabcdef".Contains(c)))
            {
                errorMessage = "UUID must contain only hexadecimal characters";
                return false;
            }

            // Validate Major
            if (!ushort.TryParse(MajorTextBox.Text.Trim(), out ushort major))
            {
                errorMessage = "Major must be a number between 0 and 65535";
                return false;
            }

            // Validate Minor
            if (!ushort.TryParse(MinorTextBox.Text.Trim(), out ushort minor))
            {
                errorMessage = "Minor must be a number between 0 and 65535";
                return false;
            }

            // Identifier is just informational, no validation needed
            return true;
        }

        private byte[] ParseUuid(string uuid)
        {
            // Remove dashes and convert to byte array
            string cleanUuid = uuid.Replace("-", "").ToUpper();
            byte[] uuidBytes = new byte[16];

            for (int i = 0; i < 16; i++)
            {
                uuidBytes[i] = Convert.ToByte(cleanUuid.Substring(i * 2, 2), 16);
            }

            return uuidBytes;
        }

        private void StartBeacon()
        {
            // Parse UUID from text box
            byte[] uuidBytes = ParseUuid(UuidTextBox.Text.Trim());

            // Parse Major and Minor from text boxes
            ushort major = ushort.Parse(MajorTextBox.Text.Trim());
            ushort minor = ushort.Parse(MinorTextBox.Text.Trim());

            // Create manufacturer data for iBeacon
            var manufacturerData = new BluetoothLEManufacturerData();
            // Apple's Bluetooth company ID for iBeacon
            manufacturerData.CompanyId = 0x004C;

            // Build beacon payload dynamically
            byte[] beaconData = new byte[23];
            beaconData[0] = 0x02; // iBeacon prefix
            beaconData[1] = 0x15; // iBeacon prefix

            // Copy UUID (16 bytes)
            Array.Copy(uuidBytes, 0, beaconData, 2, 16);

            // Major (2 bytes, big-endian)
            beaconData[18] = (byte)(major >> 8);
            beaconData[19] = (byte)(major & 0xFF);

            // Minor (2 bytes, big-endian)
            beaconData[20] = (byte)(minor >> 8);
            beaconData[21] = (byte)(minor & 0xFF);

            // TX Power (1 byte) - typically measured RSSI at 1 meter
            beaconData[22] = 0xC5; // -59 dBm in two's complement

            manufacturerData.Data = CryptographicBuffer.CreateFromByteArray(beaconData);

            var advertisement = new BluetoothLEAdvertisement();
            advertisement.ManufacturerData.Add(manufacturerData);

            publisher = new BluetoothLEAdvertisementPublisher(advertisement);
            publisher.Start();
        }
    }
}