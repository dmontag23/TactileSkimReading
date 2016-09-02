using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;


namespace HyperBraille.HBBrailleDis
{
    [DeviceType(1, "1")]
    public class DeviceBrailleDis : Device
    {
        public override void ModifyInitData(byte[/*8*/] byteArray)
        {
            if (Parent.empfindlich) byteArray[1] = 1;
        }
        public override void DoBeforeInitialization()
        {
            Thread.Sleep(500);
        }
        public override void DoAfterInitialization()
        {
            Thread.Sleep(600);

            /* set and lower one pin for termination of algorithm */
            bool[] pins = new bool[10];
            pins[0] = true;
            Parent.SetModulePins(0, 0, pins, true);

            Thread.Sleep(100);

            pins[0] = false;
            Parent.SetModulePins(0, 0, pins, true);
            Thread.Sleep(100);
        }
    }

    [DeviceType(2, "2H")] //HV520
    public class DeviceHyperBrailleHV550 : DeviceHyperBraille
    {
        //public override int Glaetten(int gespeicherterWert, int jetzigerWert)
        //{
        //    return (gespeicherterWert + jetzigerWert) / 2;
        //}

        public override void Filter(int[,] diffCapacities, int threshold)
        {
            int normalThreshold = threshold;
            int singleTouchThreshold = normalThreshold * 2;
            int rowDim = diffCapacities.GetLength(0);
            int colDim = diffCapacities.GetLength(1);

            for (int row = 0; row < rowDim; row++)
            {
                for (int column = 0; column < colDim; column++)
                {
                    int currentValue = diffCapacities[row, column];
                    if (currentValue == 0) continue;
                    if (currentValue >= singleTouchThreshold) continue;
                    if (column > 0 && diffCapacities[row, column - 1] >= normalThreshold) continue;
                    if (row > 0 && diffCapacities[row - 1, column] >= normalThreshold) continue;
                    if (column < colDim - 1 && diffCapacities[row, column + 1] >= normalThreshold) continue;
                    if (row < rowDim - 1 && diffCapacities[row + 1, column] >= normalThreshold) continue;
                    diffCapacities[row, column] = 0;
                }
            }
        }

    }

    [DeviceType(2, "2")]
    public class DeviceHyperBraille : Device
    {
        public override void ModifyInitData(byte[/*8*/] byteArray)
        {
            //Empfindlichkeit gibt's hier nicht.
        }

        public override void DoBeforeInitialization()
        {
            Thread.Sleep(400);
            {
                byte[] dataHV = new byte[8];
                dataHV[0] = 0x04;
                dataHV[1] = 0x00;
                dataHV[2] = 0x08;
                bool cmessResult = BrailleDisNet.BrdCommand(
                this.DeviceNumber, // device-number
                dataHV); // flags
                Thread.Sleep(100);
            }
        }
        public override bool Initialize()
        {
            var result = base.Initialize();

            Thread.Sleep(10000);

            return result;
        }
        /// <summary>
        /// called after output is enabled
        /// </summary>
        public override void DoAfterInitialization()
        {
            //Laaaaange wartezeit.

        }

        public override void Recalibrate(BrailleDisNet brailleDis)
        {
            Thread.Sleep(500);
            brailleDis.ReleaseAllPins();
            /*bool[,] matrix = new bool[brailleDis.NumberOfPinRows, brailleDis.NumberOfPinColumns];
            for (int c = 0; c < brailleDis.NumberOfPinColumns; c++)
            {
                for (int r = 0; r < brailleDis.NumberOfPinRows; r++)
                {
                    matrix[r, c] =
                        (r + 1) % brailleDis.NumberOfModuleRows == 0;
                }
            }
            brailleDis.SetCompleteMatrix(matrix);*/

            Thread.Sleep(500);

            base.Recalibrate(brailleDis);
            Thread.Sleep(300);
        }
    }

    public abstract class Device
    {
        public Device()
        {
            DeviceNumber = -1;
        }

        public virtual bool StartHochVolt()
        {
            byte[] dataHV = new byte[8];
            dataHV[0] = 0x02;
            dataHV[1] = 0x01;
            bool initResultHV = BrailleDisNet.BrdCommand(
            this.DeviceNumber, // device-number
            dataHV); // flags
            return initResultHV;
        }

        public virtual int Glaetten(int gespeicherterWert, int jetzigerWert)
        {
            return jetzigerWert;
        }
        public virtual void Filter(int[,] diffCapacities, int threashold)
        {
        }


        public DeviceInformation_T DeviceInformation { get; protected internal set; }
        public int DeviceNumber { get; internal set; }
        public BrailleDisNet Parent { get; set; }

        public virtual bool OpenDevice()
        {
            try
            {
                var criteria = new byte[2];
                criteria[0] = 32;
                criteria[1] = 0;
                int devType = 0;
                this.DeviceNumber = BrailleDisNet.BrdInitDevice(criteria, ref devType);
                if (this.DeviceNumber == -1)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                this.DeviceNumber = -1;
                return false;
            }
            return true;
        }

        public virtual bool Initialize()
        {
            bool opened = this.OpenDevice();
            if (!opened) return false;

            this.StartHochVolt();

            this.DoBeforeInitialization();

            byte[] data = new byte[8];
            data[0] = 0x01;
            // equalize sensor results
            //0xc0 alle Refsoll Werte werden gleich initialisiert. Bei Ref = Messkapazität wird 75% Messbereich ausgegeben
            //0x80 alle Refsoll Werte werden gleich initialisiert. Bei Ref = Messkapazität wird 50% Messbereich ausgegeben
            //0xFF individueller Abgleich. Alle Module werden gemessen und dann wird versucht jedes einzelne auf 50% Messbereich
            // einzustellen
            data[2] = 0xFF;

            ModifyInitData(data);

            bool initResult = BrailleDisNet.BrdCommand(
                this.DeviceNumber, // device-number
                data); // flags
            return initResult;

        }

        public virtual void ModifyInitData(byte[/*8*/] byteArray)
        {
        }
        public virtual void DoBeforeInitialization()
        {
        }
        public virtual void DoAfterInitialization()
        {
        }
        public virtual void Recalibrate(BrailleDisNet brailleDis)
        {
            brailleDis.m_touch_input = new int[BrailleDisConsts.NUMBER_OF_SENSOR_ROWS, BrailleDisConsts.NUMBER_OF_MODULE_COLUMNS];
            brailleDis.m_reference_touch_input = new int[BrailleDisConsts.NUMBER_OF_SENSOR_ROWS,
                                                BrailleDisConsts.NUMBER_OF_MODULE_COLUMNS];
            brailleDis.m_reference_touch_input.Initialize();

            byte[] readBuffer = new byte[BrailleDisConsts.TOTAL_INPUT_LENGTH];
            int readResult = 0;
            try
            {
                //System.Threading.Thread.BeginCriticalRegion();
                readResult = BrailleDisNet.BrdReadData(DeviceNumber, readBuffer.Length, readBuffer);

                if (readResult < BrailleDisConsts.TOTAL_INPUT_LENGTH)
                {
                    return;
                }
            }
            finally
            {
                //System.Threading.Thread.EndCriticalRegion();
            }

            if (readBuffer[BrailleDisConsts.INPUT_TYPE] == BrailleDisConsts.OLD_BRAILLEDIS)
            { // first generation of BrailleDis with 720 sensors
                // todo: if you wish to treat the old BrailleDis withh 1440 sensors
                // then set m_deviceGeneration to 2 instead of 1
                brailleDis.m_deviceGeneration = 1;
                foreach (var damagedTouches in brailleDis.m_damagedTouches)
                { // duplicate because of duplication of sensors
                    int pos = damagedTouches.X + damagedTouches.Y * BrailleDisConsts.NUMBER_OF_MODULE_COLUMNS * 2;
                    if (pos >= BrailleDisConsts.TOUCH_ARRAY_SIZE) continue;
                    readBuffer[BrailleDisConsts.FIRST_TOCH_BYTE + pos] = 0;
                    pos += 60; // second occurence of sensor
                    if (pos >= BrailleDisConsts.TOUCH_ARRAY_SIZE) continue;
                    readBuffer[BrailleDisConsts.FIRST_TOCH_BYTE + pos] = 0;
                }
            }
            else
            {
                // todo: if you wish to treat the new BrailleDis withh 720 sensors
                // then set m_deviceGeneration to 1 instead of 2
                brailleDis.m_deviceGeneration = 2;
                foreach (var damagedTouches in brailleDis.m_damagedTouches)
                {
                    int pos = damagedTouches.X + damagedTouches.Y * BrailleDisConsts.NUMBER_OF_MODULE_COLUMNS;
                    if (pos >= BrailleDisConsts.TOUCH_ARRAY_SIZE) continue;
                    readBuffer[BrailleDisConsts.FIRST_TOCH_BYTE + pos] = 0;
                }
            }

            for (int i = 0; i < BrailleDisConsts.TOUCH_ARRAY_SIZE; i++)
            {
                int row = i / BrailleDisConsts.NUMBER_OF_MODULE_COLUMNS;
                int column = i - row * BrailleDisConsts.NUMBER_OF_MODULE_COLUMNS;
                // for the old BrailleDis you should only treat 720 sensors
                if (brailleDis.m_deviceGeneration == 1)
                {
                    if ((row & 1) == 1) // second sensor on a module
                    {
                        i += 59; // do not process this sensor line
                        continue;
                    }
                    row /= 2; // correct number
                }
                brailleDis.m_reference_touch_input[row, column] = readBuffer[BrailleDisConsts.FIRST_TOCH_BYTE + i];
            }
        }

        public class DeviceTypeInfoCollection : IDictionary<string, DeviceTypeInformation>
        {
            static IDictionary<string, DeviceTypeInformation> m_devicetypes = initializeDeviceTypeCollection();
            private static IDictionary<string, DeviceTypeInformation> initializeDeviceTypeCollection()
            {
                var devicetypes = new Dictionary<string, DeviceTypeInformation>();

                var types = Assembly.GetExecutingAssembly().GetTypes();
                foreach (var type in types)
                {
                    var attribs = type.GetCustomAttributes(typeof(DeviceTypeAttribute), false);
                    if (attribs.Length <= 0) continue;
                    foreach (DeviceTypeAttribute att in attribs)
                    {
                        devicetypes.Add(att.DeviceType, new DeviceTypeInformation(att.Generation, att.DeviceType));
                    }
                }
                return devicetypes;
            }

            #region IDictionary<string,DeviceTypeInformation> Member

            public void Add(string key, DeviceTypeInformation value)
            {
                throw new NotImplementedException();
            }

            public bool ContainsKey(string key)
            {
                return m_devicetypes.ContainsKey(key);
            }

            public ICollection<string> Keys
            {
                get { return m_devicetypes.Keys; }
            }

            public bool Remove(string key)
            {
                throw new NotImplementedException();
            }

            public bool TryGetValue(string key, out DeviceTypeInformation value)
            {
                return m_devicetypes.TryGetValue(key, out value);
            }

            public ICollection<DeviceTypeInformation> Values
            {
                get { return m_devicetypes.Values; }
            }

            public DeviceTypeInformation this[string key]
            {
                get
                {
                    return m_devicetypes[key];
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            #endregion

            #region ICollection<KeyValuePair<string,DeviceTypeInformation>> Member

            public void Add(KeyValuePair<string, DeviceTypeInformation> item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<string, DeviceTypeInformation> item)
            {
                return m_devicetypes.Contains(item);
            }

            public void CopyTo(KeyValuePair<string, DeviceTypeInformation>[] array, int arrayIndex)
            {
                m_devicetypes.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return m_devicetypes.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public bool Remove(KeyValuePair<string, DeviceTypeInformation> item)
            {
                throw new NotImplementedException();
            }

            #endregion

            #region IEnumerable<KeyValuePair<string,DeviceTypeInformation>> Member

            public IEnumerator<KeyValuePair<string, DeviceTypeInformation>> GetEnumerator()
            {
                return m_devicetypes.GetEnumerator();
            }

            #endregion

            #region IEnumerable Member

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return m_devicetypes.GetEnumerator();
            }

            #endregion
        }
        public static readonly DeviceTypeInfoCollection DeviceTypes = new DeviceTypeInfoCollection();

        public DeviceTypeInformation _DeviceTypeInformation = new DeviceTypeInformation(1, "1");
        public DeviceTypeInformation DeviceTypeInformation { get { return _DeviceTypeInformation; } internal set { _DeviceTypeInformation = value; } }
    }

    [System.AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DeviceTypeAttribute : Attribute
    {
        public DeviceTypeAttribute(int generation, string deviceType)
        {
            this.DeviceType = deviceType;
            this.Generation = generation;
        }
        public string DeviceType;
        public int Generation;
    }

    internal static class DeviceChooser
    {
        public static Device CreateDeviceObject(DeviceInformation_T di, BrailleDisNet parent)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var attribs = type.GetCustomAttributes(typeof(DeviceTypeAttribute), false);
                if (attribs.Length <= 0) continue;
                foreach (DeviceTypeAttribute att in attribs)
                {
                    if (att.DeviceType == di.DeviceType)
                    {
                        var constructor = type.GetConstructor(new Type[] { });
                        var newDevice = constructor.Invoke(new object[] { }) as Device;
                        newDevice.DeviceInformation = di;
                        newDevice.Parent = parent;
                        newDevice.DeviceTypeInformation = Device.DeviceTypes[att.DeviceType];
                        return newDevice;
                    }
                }
            }

            return new DeviceBrailleDis();
        }
    }

}
