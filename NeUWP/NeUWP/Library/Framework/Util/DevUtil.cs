using System;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;

namespace NeUWP.Utilities
{
    public enum DeviceFamilyType : int
    {
        Unknown,
        Desktop,
        Mobile
    }

    public static class DevUtil
    {
        private static EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();

        private static string _deviceManufacturer;
        public static string DeviceManufacturer
        {
            get
            {
                if (string.IsNullOrEmpty(_deviceManufacturer))
                {

                    _deviceManufacturer = deviceInfo.SystemManufacturer;
                }
                return _deviceManufacturer;
            }
        }

        private static string _deviceName;
        public static string DeviceName
        {
            get
            {
                if (string.IsNullOrEmpty(_deviceName))
                {
                    _deviceName = deviceInfo.SystemProductName;
                }
                return _deviceName;
            }
        }

        private static string _deviceUniqueID;
        public static string DeviceUniqueID
        {
            get
            {
                if (string.IsNullOrEmpty(_deviceUniqueID))
                {
                    try
                    {
                        var token = HardwareIdentification.GetPackageSpecificToken(null);
                        if (token != null)
                        {
                            _deviceUniqueID = "";// Md5Util.GetString(token.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to get DeviceUniqueID: " + ex.Message);
                    }
                }
                return _deviceUniqueID;
            }
        }

        private static string _os;
        public static string OS
        {
            get
            {
                if (string.IsNullOrEmpty(_os))
                {
                    _os = deviceInfo.OperatingSystem;
                }
                return _os;
            }
        }

        private static Version _osVersion;
        public static Version OSVersion
        {
            get
            {
                if (_osVersion == null)
                {
                    // see: https://www.suchan.cz/category/uwp/
                    var version = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
                    var v = ulong.Parse(version);
                    var v1 = (v & 0xFFFF000000000000L) >> 48;
                    var v2 = (v & 0x0000FFFF00000000L) >> 32;
                    var v3 = (v & 0x00000000FFFF0000L) >> 16;
                    var v4 = (v & 0x000000000000FFFFL);

                    _osVersion = new Version((int)v1, (int)v2, (int)v3, (int)v4);
                }
                return _osVersion;
            }
        }

        private static Version _appVersion;
        public static Version AppVersion
        {
            get
            {
                if (_appVersion == null)
                {
                    var package = Package.Current;
                    if (package != null)
                    {
                        var packageId = package.Id;
                        if (packageId != null)
                        {
                            _appVersion = new Version(packageId.Version.Major, packageId.Version.Minor, packageId.Version.Build, packageId.Version.Revision);
                        }
                    }
                }
                return _appVersion;
            }
        }

        private static DeviceFamilyType? deviceFamily;
        public static DeviceFamilyType DeviceFamily
        {
            get
            {
                if (deviceFamily == null)
                {
                    try
                    {
                        switch (AnalyticsInfo.VersionInfo.DeviceFamily)
                        {
                            case "Windows.Desktop":
                                deviceFamily = DeviceFamilyType.Desktop;
                                break;
                            case "Windows.Mobile":
                                deviceFamily = DeviceFamilyType.Mobile;
                                break;
                            default:
                                deviceFamily = DeviceFamilyType.Unknown;
                                break;
                        }
                    }
                    catch
                    {
                        deviceFamily = DeviceFamilyType.Unknown;
                    }
                }
                return deviceFamily.Value;
            }
        }

        public static void Initialize()
        {
            try
            {
                var displayInfo = DisplayInformation.GetForCurrentView();
                if (displayInfo != null)
                {
                    Scale = displayInfo.RawPixelsPerViewPixel;
                }
            }
            catch
            {
                Scale = 1.0;
            }
        }

        private static double scale = 1.0;
        public static double Scale
        {
            get { return scale; }
            private set
            {
                if (scale != value)
                {
                    scale = value;
                }
            }
        }

        public static void SetAutoRotation(DisplayOrientations orientation)
        {
            try
            {
                DisplayInformation.AutoRotationPreferences = orientation;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}
