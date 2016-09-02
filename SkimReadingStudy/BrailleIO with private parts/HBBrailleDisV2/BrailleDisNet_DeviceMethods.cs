using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Security.Cryptography;


namespace HyperBraille.HBBrailleDis
{

    /// <summary>
    /// Wrapperclass to the MetecBD.dll. Generates
    /// an object to use the Stiftplatte.
    /// </summary>
    public partial class BrailleDisNet
    {
        #region Dll Imports

        [DllImportAttribute(@"MetecBD.dll", EntryPoint = "BrdEnumDevice", CallingConvention = CallingConvention.Cdecl)]
        //private static extern int BrdEnumDevice([MarshalAs(UnmanagedType.LPStr)] String lpszDev, int len);
        internal static extern int BrdEnumDevice(byte[] lpszDev, int len);

        [DllImportAttribute(@"MetecBD.dll", EntryPoint = "BrdInitDevice", CallingConvention = CallingConvention.Cdecl)]
        //private static extern int BrdInitDevice([MarshalAs(UnmanagedType.LPStr)] String lpszCriteria, ref int pdevtype);
        internal static extern int BrdInitDevice(byte[] lpszCriteria, ref int pdevtype);

        [DllImportAttribute(@"MetecBD.dll", EntryPoint = "BrdWriteData", CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool BrdWriteData(int devnr, int len, byte[] data);

        [DllImportAttribute(@"MetecBD.dll", EntryPoint = "BrdCommand", CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool BrdCommand(int devnr, byte[] data);

        [DllImportAttribute(@"MetecBD.dll", EntryPoint = "BrdCloseDevice", CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool BrdCloseDevice(int devnr);

        [DllImportAttribute(@"MetecBD.dll", EntryPoint = "BrdReadData", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int BrdReadData(int devnr, int len, byte[] buf);

        #endregion

        #region DeviceInformation

        /// <summary>
        /// Parses the device string, the device returns
        /// </summary>
        /// <param name="DeviceString">the string the device has returned</param>
        /// <returns>the parsed information</returns>
        public DeviceInformation_T ParseDeviceString(string DeviceString)
        {
            var parts = DeviceString.Split('<');
            if (parts.Length > 5)
            {
                try
                {
                    return new DeviceInformation_T()
                    {
                        DeviceName = parts[1].Substring(0, Math.Max(0, parts[1].IndexOf('>'))),
                        DeviceType = parts[2].Substring(0, Math.Max(0, parts[2].IndexOf('>'))),
                        USBinterfaceNo = parts[3].Substring(0, Math.Max(0, parts[3].IndexOf('>'))),
                        FirmwareBuild = parts[4].Substring(0, Math.Max(0, parts[4].IndexOf('>'))),
                        SerialNo = parts[5].Substring(0, Math.Max(0, parts[5].IndexOf('>')))
                    };
                }
                catch
                {
                    fireErrorOccurredEvent(ErrorType.DEVICE_STRING_DEFECTIVE);
                }
            }
            return new DeviceInformation_T()
            {
                DeviceName = parts[1].Substring(0, parts[1].IndexOf('>')),
                DeviceType = "Unknown",
                USBinterfaceNo = "Unknown",
                FirmwareBuild = "Unknown",
                SerialNo = "0000"
            };
        }

        private DeviceInformation_T LoadFirstDevice()
        {
            string dummy;
            return LoadFirstDevice(out dummy);
        }

        private DeviceInformation_T LoadFirstDevice(out string devicestring1)
        {
            devicestring1 = "";
            DeviceInformation_T di = new DeviceInformation_T() { DeviceName = null };
            try
            {
                // enumerate devices
                char[] line = new char[1000];
                String strline = new String(line);
                var ba = new byte[1000];
                int devices = 0;
                lock (synchLock)
                {
                    devices = BrdEnumDevice(ba, strline.Length);
                }
                if (devices > 0)
                {
                    int position = 0;
                    var devicestring = new string[devices];
                    for (int i = 0; i < devices; i++)
                    {
                        int newposition = Array.IndexOf<Byte>(ba, 0, position);
                        devicestring[i] = Encoding.ASCII.GetString(ba, position, newposition - position);
                        position = newposition + 2;
                    }
                    var deviceInfo = devicestring.Select(s => this.ParseDeviceString(s));
                    di = deviceInfo.First(x => true);
                    devicestring1 = devicestring[0];
                    di.DeviceUsbString = devicestring1;
                }
            }
            catch (Exception ex) { }
            return di;
        }
        #endregion

        public void CheckForDevice()
        {
            lock (synchLock)
            {
                //Wir arbeiten nur mit einem Device, erstmal dieses ausstecken!
                if (this.deviceIsInitialized) return;

                string deviceString;
                DeviceInformation_T now = LoadFirstDevice(out deviceString);

                if (now.DeviceName != null)
                {
                    this.Initialize();

                    this.fireErrorOccurredEvent(ErrorType.NEW_DEVICE);
                }
            }
        }
        /// <summary>
        /// Closes connection to braille display device and releases resources.
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            deviceIsInitialized = false;
            bool result = false;
            try
            {
                int num = UsedDevice.DeviceNumber;
                //Close current device!
                result = BrdCloseDevice(num);
            }
            catch (System.Exception ex) { }
            this.UsedDevice = new DeviceBrailleDis();
            m_deviceGeneration = 0;
            return result;
        }

        #region tactile image methods
        // set and release all pins
        /// <summary>
        /// Releases all pins.
        /// </summary>
        /// <returns>True, if operation succeeds, false otherwise</returns>
        public bool ReleaseAllPins()
        {
            if (!this.deviceIsInitialized) return false;
            lock (synchLock)
            {
                bool result = false;
                int loop;
                for (loop = 0; loop < 5; loop++)
                {
                    result = BrdWriteData(
                    this.UsedDevice.DeviceNumber,
                    2 * BrailleDisConsts.MODULE_COUNT,
                    this.m_matrix_lower_all);
                    if (result) return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Sets all pins.
        /// </summary>
        /// <returns>True, if operation succeeds, false otherwise</returns>
        public bool SetAllPins()
        {
            if (!this.deviceIsInitialized) return false;
            lock (synchLock)
            {
                bool result = false;
                int loop;
                for (loop = 0; loop < 5; loop++)
                {
                    result = BrdWriteData(
                    this.UsedDevice.DeviceNumber,
                    2 * BrailleDisConsts.MODULE_COUNT,
                    this.m_matrix_set_all);
                    if (result) return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Refreshs the complete display with data given in matrix.
        /// </summary>
        /// <param name="_matrix">Boolean matrix, containing values to set.</param>
        /// <returns>True, if operation succeeds, false otherwise</returns>
        public bool SetCompleteMatrix(bool[,] _matrix)
        {
            return SetCompleteMatrix(_matrix, true);
        }

        /// <summary>
        /// Sends complete boolean matrix to display.
        /// </summary>
        /// <param name="_matrix">Boolean matrix, containing values to set.</param>
        /// <param name="doUpdate">if set to <c>true</c> [do update].</param>
        /// <returns>True, if operation succeeds, false otherwise</returns>
        public bool SetCompleteMatrix(bool[,] _matrix, bool doUpdate)
        {
            if (_matrix == null) return false;
            lock (synchLock)
            {
                // calculation per module!
                for (int moduleRow = 0; moduleRow < BrailleDisConsts.NUMBER_OF_MODULE_ROWS; moduleRow++)
                {
                    int pinRowIndex = moduleRow * 5;
                    if (_matrix.GetLength(0) <= pinRowIndex) break;
                    for (int moduleColumn = 0; moduleColumn < BrailleDisConsts.NUMBER_OF_MODULE_COLUMNS; moduleColumn++)
                    {
                        int pinColumnIndex = moduleColumn * 2;
                        if (_matrix.GetLength(1) <= pinColumnIndex) break;

                        // lower two pins
                        byte lowerByte = 0;
                        // upper eight pins
                        byte upperByte = 0;



                        if (_matrix[pinRowIndex, pinColumnIndex] == true)
                        {
                            upperByte = (byte)(upperByte | 1);
                        }
                        //module pin (0,1)
                        if (_matrix.GetLength(1) > (pinColumnIndex + 1) && _matrix[pinRowIndex, pinColumnIndex + 1] == true)
                        {
                            upperByte = (byte)(upperByte | 2);
                        }

                        if (_matrix.GetLength(0) > (pinRowIndex + 1))
                        {
                            //module pin (1,0)
                            if (_matrix[pinRowIndex + 1, pinColumnIndex] == true)
                            {
                                upperByte = (byte)(upperByte | 4);
                            }
                            //module pin (1,1)
                            if (_matrix.GetLength(1) > (pinColumnIndex + 1) && _matrix[pinRowIndex + 1, pinColumnIndex + 1] == true)
                            {
                                upperByte = (byte)(upperByte | 8);
                            }
                        }

                        if (_matrix.GetLength(0) > (pinRowIndex + 2))
                        {
                            //module pin (2,0)
                            if (_matrix[pinRowIndex + 2, pinColumnIndex] == true)
                            {
                                upperByte = (byte)(upperByte | 16);
                            }
                            //module pin (2,1)
                            if (_matrix.GetLength(1) > (pinColumnIndex + 1) && _matrix[pinRowIndex + 2, pinColumnIndex + 1] == true)
                            {
                                upperByte = (byte)(upperByte | 32);
                            }
                        }

                        if (_matrix.GetLength(0) > (pinRowIndex + 3))
                        {
                            //module pin (3,0)
                            if (_matrix[pinRowIndex + 3, pinColumnIndex] == true)
                            {
                                upperByte = (byte)(upperByte | 64);
                            }
                            //module pin (3,1)
                            if (_matrix.GetLength(1) > (pinColumnIndex + 1) && _matrix[pinRowIndex + 3, pinColumnIndex + 1] == true)
                            {
                                upperByte = (byte)(upperByte | 128);
                            }
                        }

                        if (_matrix.GetLength(0) > (pinRowIndex + 4))
                        {
                            //module pin (4,0)
                            if (_matrix[pinRowIndex + 4, pinColumnIndex] == true)
                            {
                                lowerByte = (byte)(lowerByte | 1);
                            }
                            //module pin (4,1)
                            if (_matrix.GetLength(1) > (pinColumnIndex + 1) && _matrix[pinRowIndex + 4, pinColumnIndex + 1] == true)
                            {
                                lowerByte = (byte)(lowerByte | 2);
                            }
                        }



                        // set module bytes
                        int currModuleIndex = 2 * (moduleRow * BrailleDisConsts.NUMBER_OF_MODULE_COLUMNS + moduleColumn);
                        this.m_matrix[currModuleIndex] = lowerByte;
                        this.m_matrix[currModuleIndex + 1] = upperByte;
                    }
                }

                m_tempBoolMatrix = _matrix;

                if (doUpdate)
                {
                    // write data
                    return SendMatrix();
                }
                return false;
            }
        }
        /// <summary>
        /// Set the pins of a single module
        /// </summary>
        /// <param name="_moduleRow">module row number</param>
        /// <param name="_moduleColumn">module columnnumber</param>
        /// <param name="_pins">the pins. should be bool[<see cref="BrailleDisConsts.NUMBER_OF_PINS_PER_MODULE"/>]</param>
        /// <returns>true on success</returns>
        [Obsolete("Use: SetModulePins(int _moduleRow, int _moduleColumn, bool[] _pins, bool doUpdate)")]
        public bool SetModulePins(int _moduleRow, int _moduleColumn, bool[] _pins)
        {
            return SetModulePins(_moduleRow, _moduleColumn, _pins, true);
        }

        /// <summary>
        /// Set the pins of a single module
        /// </summary>
        /// <param name="_moduleRow">module row number</param>
        /// <param name="_moduleColumn">module columnnumber</param>
        /// <param name="_pins">the pins. should be bool[<see cref="BrailleDisConsts.NUMBER_OF_PINS_PER_MODULE"/>]</param>
        /// <param name="doUpdate">true, if the changed sould be directly send to stiftplatte. If you like to call
        /// the method many times, then better set it to false and call <see cref="SendMatrix"/>. Sending data to stiftplatte
        /// is time consuming!</param>
        /// <returns>true on success</returns>
        public bool SetModulePins(int _moduleRow, int _moduleColumn, bool[] _pins, bool doUpdate)
        {

            /* check validity of array */
            if (_pins.GetLength(0) < BrailleDisConsts.NUMBER_OF_PINS_PER_MODULE)
            {
                return false;
            }

            // lower two pins
            byte lowerByte = 0;
            // upper eight pins
            byte upperByte = 0;

            //module pin (0,0)
            if (_pins[0] == true)
            {
                upperByte = (byte)(upperByte | 1);
            }
            //module pin (0,1)
            if (_pins[1] == true)
            {
                upperByte = (byte)(upperByte | 2);
            }
            //module pin (1,0)
            if (_pins[2] == true)
            {
                upperByte = (byte)(upperByte | 4);
            }
            //module pin (1,1)
            if (_pins[3] == true)
            {
                upperByte = (byte)(upperByte | 8);
            }
            //module pin (2,0)
            if (_pins[4] == true)
            {
                upperByte = (byte)(upperByte | 16);
            }
            //module pin (2,1)
            if (_pins[5] == true)
            {
                upperByte = (byte)(upperByte | 32);
            }
            //module pin (3,0)
            if (_pins[6] == true)
            {
                upperByte = (byte)(upperByte | 64);
            }
            //module pin (3,1)
            if (_pins[7] == true)
            {
                upperByte = (byte)(upperByte | 128);
            }
            //module pin (4,0)
            if (_pins[8] == true)
            {
                lowerByte = (byte)(lowerByte | 1);
            }
            //module pin (4,1)
            if (_pins[9] == true)
            {
                lowerByte = (byte)(lowerByte | 2);
            }

            /* calculate index of first module byte */
            int index = 2 * (_moduleRow * BrailleDisConsts.NUMBER_OF_MODULE_COLUMNS + _moduleColumn);

            lock (synchLock)
            {
                this.m_matrix[index] = lowerByte;
                this.m_matrix[++index] = upperByte;
                if (doUpdate)
                {
                    /* write data */
                    return SendMatrix();
                }
                return false;
            }
        }

        #endregion

        #region Sendmatrix
        struct SendMatrixTimerStarter : IDisposable
        {
            public Stopwatch SendMatrixTimer;
            #region IDisposable Member
            public void Dispose()
            {
                SendMatrixTimer.Start();
            }
            #endregion
        }

        Stopwatch SendMatrixTimer = new Stopwatch();
        /// <summary>
        /// Sends the matrix.
        /// </summary>
        /// <returns></returns>
        public bool SendMatrix()
        {
            lock (synchLock)
            {
                UpdateOnPinStateChanges(m_boolMatrix, m_tempBoolMatrix);
                if (!this.deviceIsInitialized) return false;
                bool result = false;
                int loop;
                for (loop = 0; loop < 5; loop++)
                {
                    using (var SendMatrixTimerStart = new SendMatrixTimerStarter() { SendMatrixTimer = this.SendMatrixTimer })
                    {
                        if (SendMatrixTimer.IsRunning)
                        {
                            while (SendMatrixTimer.ElapsedMilliseconds < 2)
                                Thread.Sleep(1);
                        }
                        SendMatrixTimer.Stop();
#if DEBUG
                        this.UsedDevice.StartHochVolt();
                        Thread.Sleep(1); //VHA: Wir müssen schlafen, da ein zu häufiges Aufrufen von Devicefunktionen
                        //Zum absturz des Gerät führen kann...!
#endif
                        result = BrdWriteData(
                            this.UsedDevice.DeviceNumber,
                            2 * BrailleDisConsts.MODULE_COUNT,
                            this.m_matrix);
                        if (result) return true;
                    }
                }
                return false;

            }
        }
        #endregion

    }
}
