using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;


namespace HyperBraille.HBBrailleDis
{
    /// <summary>
    /// Structure to return information about modules and their values seperately from the module grid
    /// </summary>
    public struct BrailleDisModuleState
    {
	/// <summary>
	/// Simple constructor
	/// </summary>
	/// <param name="sensorRow">the row, in which the sensor is.
	/// 0 - 11 for old BrailleDis, 0 - 23 for new BrailleDis </param>
	/// <param name="moduleColumn">the column, in which the module is.</param>
	/// <param name="lastValue">the value before the change</param>
	/// <param name="currentValue">the value after the change</param>
	/// <param name="generation"> BraileDisGeneration: 1 = old BraileeDis, 2 = new BrailleDis </param>
	public BrailleDisModuleState(int sensorRow, int moduleColumn, int lastValue, int currentValue, int generation)
	{
	    this.sensorRow = sensorRow;
	    this.moduleColumn = moduleColumn;
	    this.lastValue = lastValue;
	    this.currentValue = currentValue;
	    this.generation = generation;
	}

	private int sensorRow;
	/// <summary>
	/// the row, in which the module is.
	/// 0 - 11 for old BrailleDis
	/// 0 - 23 for new BrailleDis
	/// </summary>
	public int SensorRow
	{
	    get { return sensorRow; }
	}

	/// <summary>
	/// the row, in which the module is.
	/// 0 - 11 for old & new  brailedis
	/// </summary>
	public int ModuleRow
	{
	    get { if (generation == 2) return sensorRow / 2;
		  else return sensorRow; }
	}

	private int moduleColumn;
	/// <summary>
	/// the column, in which the module is.
	/// </summary>
	public int ModuleColumn
	{
	    get { return moduleColumn; }
	}

	private int lastValue;
	/// <summary>
	/// the value before the change
	/// </summary>
	public int LastValue
	{
	    get { return lastValue; }
	}

	private int currentValue;
	/// <summary>
	/// the value after the change
	/// </summary>
	public int CurrentValue
	{
	    get { return currentValue; }
	}

	private int generation;
	/// <summary>
	///  Generation of the BrailleDis 1 = old, 2 = new BrailleDis
	/// </summary>
	public int Generation
	{
	    get { return generation; }
	}

    }


    /// <summary>
    /// Struct that contains the device information of the stiftplatte.
    /// These information are read from hardware.
    /// </summary>
    [DataContract]
    public struct DeviceInformation_T
    {
        /// <summary>
        /// The DeviceName. i.e. "BrailleDis"
        /// </summary>
        [DataMember]
        public string DeviceName;
        /// <summary>
        /// A number that defines the metec internal type. i.e. "1"
        /// </summary>
        [DataMember]
        public string DeviceType;
        /// <summary>
        /// The USB-Port or device: i.e. "01#01" or something. possibly the USB-Hub
        /// </summary>
        [DataMember]
        public string USBinterfaceNo;
        /// <summary>
        /// The build date of the firmware. i.e. "090707"
        /// </summary>
        [DataMember]
        public string FirmwareBuild;
        /// <summary>
        /// The serial number of the stiftplatte. i.e. "0009"
        /// </summary>
        [DataMember]
        public string SerialNo;
        /// <summary>
        /// The unparsed version of the DeviceSting
        /// </summary>
        [DataMember]
        public string DeviceUsbString;
    }

    [MessageContract]
    public class DeviceTypeInformation
    {
        private int m_deviceGeneration;
        private string _DeviceType;
        public DeviceTypeInformation(int generation, string deviceType)
        {
            this.m_deviceGeneration = generation;
            this._DeviceType = deviceType;
        }

        [MessageHeader]
        public string DeviceType
        {
            get { return _DeviceType; }
        }

        /// <summary>
        /// Gets the number of pin columns (height).
        /// </summary>
        /// <value>The number of pin columns.</value>
        [MessageBodyMember]
        public int NumberOfPinColumns
        {
            get { return BrailleDisConsts.NUMBER_OF_PIN_COLUMNS; }
        }

        /// <summary>
        /// Gets the number of pin rows (width).
        /// </summary>
        /// <value>The number of pin rows.</value>
        [MessageBodyMember]
        public int NumberOfPinRows
        {
            get { return BrailleDisConsts.NUMBER_OF_PIN_ROWS; }
        }

        /// <summary>
        /// Gets the number of module columns.
        /// </summary>
        /// <value>The number of module columns.</value>
        [MessageBodyMember]
        public int NumberOfModuleColumns
        {
            get { return BrailleDisConsts.NUMBER_OF_MODULE_COLUMNS; }
        }

        /// <summary>
        /// Gets the number of module rows.
        /// </summary>
        /// <value>The number of module rows.</value>
        [MessageBodyMember]
        public int NumberOfModuleRows
        {
            get { return BrailleDisConsts.NUMBER_OF_MODULE_ROWS; }
        }

        /// <summary>
        /// Gets the number of sensor rows.
        /// </summary>
        /// <value>The number of sensor rows.</value>
        [MessageBodyMember]
        public int NumberOfSensorRows
        {
            get { return BrailleDisConsts.NUMBER_OF_MODULE_ROWS * m_deviceGeneration; }
        }

        /// <summary>
        /// Gets the pin rows per module.
        /// </summary>
        /// <value>The pin rows per module.</value>
        [MessageBodyMember]
        public int PinRowsPerModule
        {
            get { return BrailleDisConsts.PIN_ROWS_PER_MODULE; }
        }

        /// <summary>
        /// Gets the pin columns per module.
        /// </summary>
        /// <value>The pin columns per module.</value>
        [MessageBodyMember]
        public int PinColumnsPerModule
        {
            get { return BrailleDisConsts.PIN_COLUMNS_PER_MODULE; }
        }

    }


    [ServiceContract(Name = "IBrailleDisWcf", Namespace = "http://www.hyperbraille.de/IBrailleDisWcf",
    CallbackContract = typeof(IBrailleDisWcf_CallBack), SessionMode = SessionMode.Required)]
    public interface IBrailleDisWcf
    {
        [OperationContract(IsOneWay = true)]
        void GetDeviceInformation();

        [OperationContract(IsOneWay = false)]
        DeviceTypeInformation GetDeviceTypeInformation();

    }

    public interface IBrailleDisWcf_CallBack
    {
        [OperationContract(IsOneWay = true)]
        void UpdateDeviceInformation(DeviceInformation_T[] devices);

    }



}
