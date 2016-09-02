using System;
namespace HyperBraille.HBBrailleDis
{
    /// <summary>
    /// These are the constants of the stiftplatte. this may change in future, because
    /// of different devices...
    /// </summary>
    public struct BrailleDisConsts
    {
	/* modules */
	/// <summary>
	/// returns the number of module rows
	/// </summary>
	public const int NUMBER_OF_MODULE_ROWS = 12;
	/// <summary>
	/// returns the number of sensor rows
	/// </summary>
	public const int NUMBER_OF_SENSOR_ROWS = 24;
	/// <summary>
	/// returns the number of module columns
	/// </summary>
	public const int NUMBER_OF_MODULE_COLUMNS = 60;
	/// <summary>
	/// return the total number of modules
	/// </summary>
	public const int MODULE_COUNT = NUMBER_OF_MODULE_ROWS * NUMBER_OF_MODULE_COLUMNS;
	/// <summary>
	/// return the number of touchvalues
	/// </summary>
	// public const int TOUCH_ARRAY_SIZE = MODULE_COUNT; old BD
	public const int TOUCH_ARRAY_SIZE = MODULE_COUNT * 2;
	/// <summary>
	/// returns the number of pin rows per module
	/// </summary>
	public const int PIN_ROWS_PER_MODULE = 5;
	/// <summary>
	/// returns the number of pin columns per module
	/// </summary>
	public const int PIN_COLUMNS_PER_MODULE = 2;

	/* pins */
	/// <summary>
	/// returns the number of pins row on the stiftplatte
	/// </summary>
	public const int NUMBER_OF_PIN_ROWS = NUMBER_OF_MODULE_ROWS * PIN_ROWS_PER_MODULE;
	/// <summary>
	/// returns the number of pin columns on the stiftplatte
	/// </summary>
	public const int NUMBER_OF_PIN_COLUMNS = NUMBER_OF_MODULE_COLUMNS * PIN_COLUMNS_PER_MODULE;
	/// <summary>
	/// returns the number of pins per module
	/// </summary>
	public const int NUMBER_OF_PINS_PER_MODULE = PIN_ROWS_PER_MODULE * PIN_COLUMNS_PER_MODULE;
	//public const int PIN_COUNT = NUMBER_OF_PIN_ROWS * NUMBER_OF_PIN_COLUMNS;

	/// <summary>
	/// defines the threshold for touchinput
	/// </summary>
	public const int INITIAL_TOUCH_THRESHOLD = 11;

	/* bytes */
	internal const int BYTE_SET_ALL_PINS = 0xff;
	internal const int BYTE_LOWER_ALL_PINS = 0x0;

	/* time needed for reading the touch input */
	/// <summary>
	/// obsolete; time to be waitet before the next data packet is read from hardware
	/// </summary>
	internal const int TIME_TOUCH_SCAN_INTERVAL = 50; // milliseconds
	/// <summary>
	/// time of keyboard scan interval. This is a hardware constant, do not use. this is a fault.
	/// </summary>
	internal const int TIME_KEYBOARD_SCAN_INTERVAL = 10; // milliseconds
	internal const int TIME_DATA_SIZE = 3;// size of all time-bytes

	// old brailedis-interface
	// internal const int TIME_PAKET_BYTE = TOUCH_ARRAY_SIZE; // paket-number old BD
	// internal const int TIME_GONE_BYTE = TOUCH_ARRAY_SIZE + 1; // time since the last touch-scan ol  BD
	// internal const int TIME_WAIT_BYTE = TOUCH_ARRAY_SIZE + 2; // time until the next toch-scan old BD

	// new brailledis-interface
	internal const int INPUT_TYPE = 0; // byte in record
	internal const int OLD_BRAILLEDIS = 0; // value of byte 0
	internal const int NEW_BRAILLEDIS = 1; // value of byte 0
	internal const int TIME_PAKET_BYTE = 1; // paket-number
	internal const int TIME_GONE_BYTE = 2; // time since the last touch-scan
	internal const int TIME_WAIT_BYTE = 3; // time until the next toch-scan
	internal const int FIRST_TOCH_BYTE = 8;
	internal const int TOTAL_INPUT_LENGTH = 1488;


	/* BrailleDis keys */
	// internal const int KEYBOARD_ARRAY_SIZE = 100; old BD
	// internal const int KEYBOARD_START = TOUCH_ARRAY_SIZE + TIME_DATA_SIZE; old BD
	internal const int KEYBOARD_ARRAY_SIZE = 40;
	internal const int KEYBOARD_START = 1448;
	internal const int KEYBOARD_LENGTH = 8; // bytes per keyboard-state
	internal const int LOWER_KEYS_BYTE = 0;
	internal const int KEYBOARD_LAST_POS = 1480;
	internal const int UPPER_KEYS_BYTE = 1;
	// todo 6 further keyboard-bytes

	// Braille keys from left to right: Bit 3,2,1,0, 4,5,6,7 (the error of the old BraileeDis is corrected!)
	internal const int LEFT_1_BIT =    0x01; // left 1 is the inner left key
	internal const int LEFT_2_BIT =    0x02;
	internal const int LEFT_3_BIT =    0x04;
	internal const int LEFT_4_BIT =    0x08; // left 4 is the outer left key

	internal const int RIGHT_1_BIT =   0x20; // right 1 is the inner right key
	internal const int RIGHT_2_BIT =   0x10;
	internal const int RIGHT_3_BIT =   0x40;
	internal const int RIGHT_4_BIT =   0x80; // right 4 is the outer right key

	internal const int LOW_LEFT_BIT =  0x02;
	internal const int LOW_RIGHT_BIT = 0x01;
	internal const int LOW_MASK = LOW_LEFT_BIT | LOW_RIGHT_BIT;

	internal const int EXTRA_KEY_LEFT =        0x08;
	internal const int EXTRA_KEY_RIGHT =       0x10;
	internal const int EXTRA_KEY_BOTTOM =      0x20;
	internal const int EXTRA_KEY_UPPER_LEFT =  0x04;
	internal const int EXTRA_KEY_UPPER_RIGHT = 0x40;
	internal const int EXTRA_KEY_SPEZIAL = 0x80;
	internal const int EXTRA_KEY_MASK = EXTRA_KEY_LEFT |
					  EXTRA_KEY_RIGHT |
					  EXTRA_KEY_BOTTOM |
					  EXTRA_KEY_UPPER_LEFT |
					  EXTRA_KEY_UPPER_RIGHT |
					  EXTRA_KEY_SPEZIAL;
    // 64-Bit-Keyboard-Constants
    #region Byte 0 - Daumentasten
    public const UInt64 KEY_THUMB = 0x0f;
	public const UInt64 KEY_THUMB_RIGHT_HAND_RIGHT = 0x01;
	public const UInt64 KEY_THUMB_LEFT_HAND_LEFT   = 0x02;
	public const UInt64 KEY_THUMB_RIGHT_HAND_LEFT  = 0x04;
	public const UInt64 KEY_THUMB_LEFT_HAND_RIGHT  = 0x08;
    #endregion

    #region Byte 1 - Brailletasten
    /// <summary>
    /// 
    /// </summary>
    public const UInt64 KEY_BRAILLE = 0xff00;
    /// <summary>
    /// identifier for braille key 1
    /// </summary>
	public const UInt64 KEY_DOT1 =    0x0100;
    /// <summary>
    /// identifier for braille key 2
    /// </summary>
    public const UInt64 KEY_DOT2 =    0x0200;
    /// <summary>
    /// identifier for braille key 3
    /// </summary>
	public const UInt64 KEY_DOT3 =    0x0400;
    /// <summary>
    /// identifier for braille key 7
    /// </summary>
	public const UInt64 KEY_DOT7 =    0x0800;
    /// <summary>
    /// identifier for braille key 4
    /// </summary>
	public const UInt64 KEY_DOT4 =    0x2000;
    /// <summary>
    /// identifier for braille key 5
    /// </summary>
	public const UInt64 KEY_DOT5 =    0x1000;
    /// <summary>
    /// identifier for braille key 6
    /// </summary>
	public const UInt64 KEY_DOT6 =    0x4000;
    /// <summary>
    /// identifier for braille key 8
    /// </summary>
	public const UInt64 KEY_DOT8 =    0x8000;
    #endregion

    #region Byte 2 - Linkes Cursorkreuz
    //                                                0x221100;
    public const UInt64 KEY_HYPERBRAILLE_KEY_LEFT   = 0x010000;
	public const UInt64 KEY_LEFT_ROCKER_SWITCH_UP   = 0x020000;
	public const UInt64 KEY_LEFT_ROCKER_SWITCH_DOWN = 0x040000;

    //                                            0x221100;
	public const UInt64 KEY_LEFT_CURSORS        = 0xf80000;
	public const UInt64 KEY_LEFT_CURSORS_UP     = 0x080000;
	public const UInt64 KEY_LEFT_CURSORS_RIGHT  = 0x100000;
	public const UInt64 KEY_LEFT_CURSORS_CENTER = 0x200000;
	public const UInt64 KEY_LEFT_CURSORS_DOWN   = 0x400000;
	public const UInt64 KEY_LEFT_CURSORS_LEFT   = 0x800000;
    #endregion

    #region Byte 3 - Navigationsleiste
    //                                    0x33221100;
    public const UInt64 KEY_NAV         = 0xFF000000;
    public const UInt64 KEY_NAV_LEFT    = 0x80000000;
    public const UInt64 KEY_NAV_LEFT_2  = 0x40000000;
    public const UInt64 KEY_NAV_RIGHT   = 0x20000000;
    public const UInt64 KEY_NAV_RIGHT_2 = 0x10000000;
    public const UInt64 KEY_NAV_UP      = 0x04000000;
    public const UInt64 KEY_NAV_UP_2    = 0x02000000;
    public const UInt64 KEY_NAV_DOWN    = 0x01000000;
    public const UInt64 KEY_NAV_DOWN_2  = 0x08000000;
    #endregion

    #region Byte 4 - rechts Cursorkreuz
    //                                                 0x4433221100;
    public const UInt64 KEY_HYPERBRAILLE_KEY_RIGHT   = 0x0100000000;
	public const UInt64 KEY_RIGHT_ROCKER_SWITCH_UP   = 0x0200000000;
	public const UInt64 KEY_RIGHT_ROCKER_SWITCH_DOWN = 0x0400000000;

	public const UInt64 KEY_RIGHT_CURSORS            = 0xf800000000;
	public const UInt64 KEY_RIGHT_CURSORS_UP         = 0x0800000000;
	public const UInt64 KEY_RIGHT_CURSORS_RIGHT      = 0x1000000000;
	public const UInt64 KEY_RIGHT_CURSORS_CENTER     = 0x2000000000;
	public const UInt64 KEY_RIGHT_CURSORS_DOWN       = 0x4000000000;
	public const UInt64 KEY_RIGHT_CURSORS_LEFT       = 0x8000000000;
    #endregion

    #region Konstanten-Kombinationen
    public const UInt64 KEY_HYPERBRAILLE_KEY = KEY_HYPERBRAILLE_KEY_LEFT | KEY_HYPERBRAILLE_KEY_RIGHT;
	public const UInt64 KEY_ROCKER_SWITCH_UP = KEY_LEFT_ROCKER_SWITCH_UP | KEY_RIGHT_ROCKER_SWITCH_UP;
	public const UInt64 KEY_ROCKER_SWITCH_DOWN = KEY_LEFT_ROCKER_SWITCH_DOWN | KEY_RIGHT_ROCKER_SWITCH_DOWN;

	public const UInt64 KEY_CURSORS = KEY_LEFT_CURSORS | KEY_RIGHT_CURSORS;
	public const UInt64 KEY_CURSORS_UP = KEY_LEFT_CURSORS_UP | KEY_RIGHT_CURSORS_UP;
	public const UInt64 KEY_CURSORS_RIGHT = KEY_LEFT_CURSORS_RIGHT | KEY_RIGHT_CURSORS_RIGHT;
	public const UInt64 KEY_CURSORS_CENTER = KEY_LEFT_CURSORS_CENTER | KEY_RIGHT_CURSORS_CENTER;
	public const UInt64 KEY_CURSORS_DOWN = KEY_LEFT_CURSORS_DOWN | KEY_RIGHT_CURSORS_DOWN;
	public const UInt64 KEY_CURSORS_LEFT = KEY_LEFT_CURSORS_LEFT | KEY_RIGHT_CURSORS_LEFT;
    #endregion
    }


    /// <summary>
    /// Structure to hold information on pins.
    /// </summary>
    public struct BrailleDisPinState
    {
	#region private fields
	private int pinRow, pinColumn;
	private bool currentValue;
	#endregion

	/// <summary>
	/// Initializes a new instance of the <see cref="BrailleDisPinState"/> struct.
	/// </summary>
	/// <param name="pinRow">The pin row.</param>
	/// <param name="pinColumn">The pin column.</param>
	/// <param name="currentValue">The current pin state.</param>
	public BrailleDisPinState(int pinRow, int pinColumn, bool currentValue)
	{
	    this.pinRow = pinRow;
	    this.pinColumn = pinColumn;
	    this.currentValue = currentValue;
	}

	#region Properties
	/// <summary>
	/// Gets the pin row.
	/// </summary>
	/// <value>The pin row.</value>
	public int PinRow
	{
	    get { return pinRow; }
	}

	/// <summary>
	/// Gets the pin column.
	/// </summary>
	/// <value>The pin column.</value>
	public int PinColumn
	{
	    get { return pinColumn; }
	}

	/// <summary>
	/// Gets a value indicating whether [current value].
	/// </summary>
	/// <value><c>true</c> if [current value]; otherwise, <c>false</c>.</value>
	public bool CurrentValue
	{
	    get { return currentValue; }
	}
	#endregion

    }


    /// <summary>
    /// structure for BrailleDis buttons
    /// </summary>
    public struct BrailleDisKeyboard
    {
	// public static BrailleDisKeyboard Empty = new BrailleDisKeyboard(0, 0);

	internal UInt64 allKeys;
	/// <summary>
	/// Sets or get all keys.
	/// </summary>
	public UInt64 AllKeys
	{
	    get { return allKeys; }
	    set { allKeys = value; }
	}

	/// <summary>
	/// Setg the Upper keys.
	/// </summary>
	public byte UpperKeys
	{
	    get { return (byte) ((AllKeys & 0xff00) >> 8); }
	}

	/// <summary>
	/// returns the mask for the lower keys on the device. row.e. Thumb-keys)
	/// </summary>
	public byte LowerKeys
	{
	    get { return (byte)(AllKeys & BrailleDisConsts.LOW_MASK); }
	}

	/// <summary>
	/// returns the Extra Keys. (row.e. the long keys around of the device. (obsoleted)
	/// </summary>
	public byte ExtraKeys
	{
	    get { return (byte)(LowerKeys & BrailleDisConsts.EXTRA_KEY_MASK); }
	}

	/// <summary>
	/// creates the struct. (obsoleted)
	/// </summary>
	/// <param name="upperKeys">the initial upperkeys. see <see cref="BrailleDisConsts"/></param>
	/// <param name="lowerKeys">the initial lowerkeys AND extrakeys. see <see cref="BrailleDisConsts"/></param>
	public BrailleDisKeyboard(byte upperKeys, byte lowerKeys)
	{
	    this.allKeys = (UInt64) ((upperKeys << 8) | lowerKeys);
	}

	/// <summary>
	/// creates the struct.
	/// </summary>
	/// <param name="allKeys">the initial allKeys</param>
	public BrailleDisKeyboard(UInt64 allKeys)
	{
	    this.allKeys = allKeys;
	}

	/// <summary>
	/// creates the struct.
	/// </summary>
	/// <param name="inputBuffer"> data read from BrailleDis </param>
	/// <param name="pos"> position of the data in inputBuffer </param>
	/// <param name="keyFilter"> mask for allowed keys </param>
	public BrailleDisKeyboard(byte[] inputBuffer, int pos, UInt64 keyFilter)
	{
	    this.allKeys = inputBuffer[pos];
	    for (int i = 1; i < 8; i++)
	    {
		UInt64 b = inputBuffer[pos+i];
		b <<= i * 8;
		this.allKeys |= b;
	    }
	    this.allKeys &= keyFilter;
	}

	/// <summary>
	/// return if any key is set.
	/// </summary>
	public bool KeysPressed
	{
	    get { return allKeys != 0; }
	}
	/// <summary>
	/// Compares with another BrailleDisKeyboard for equality
	/// </summary>
	/// <param name="obj">some object</param>
	/// <returns>true, if both objects are equal</returns>
	public override bool Equals(object obj)
	{
	    if (obj is BrailleDisKeyboard)
	    {
		return (((BrailleDisKeyboard)obj).GetHashCode() == this.GetHashCode());
	    }

	    return false;
	}

	/// <summary>
	/// return the values of the keys as int.
	/// </summary>
	/// <returns></returns>
	public override int GetHashCode()
	{
	    return (int) ((allKeys >> 32) | (allKeys & 0xffffffff));
	}

	#region Extra keys
	/// <summary>
	/// returns the state of the key extra_left
	/// </summary>
	public bool Extra_Left
	{
	    get { return ((LowerKeys & BrailleDisConsts.EXTRA_KEY_LEFT) == BrailleDisConsts.EXTRA_KEY_LEFT); }
	}
	/// <summary>
	/// returns the state of the key extra right
	/// </summary>
	public bool Extra_Right
	{
	    get { return ((LowerKeys & BrailleDisConsts.EXTRA_KEY_RIGHT) == BrailleDisConsts.EXTRA_KEY_RIGHT); }
	}
	/// <summary>
	/// returns the state of the key extra bottom
	/// </summary>
	public bool Extra_Bottom
	{
	    get { return ((LowerKeys & BrailleDisConsts.EXTRA_KEY_BOTTOM) == BrailleDisConsts.EXTRA_KEY_BOTTOM); }
	}
	/// <summary>
	/// returns the state of the key Upperleft
	/// </summary>
	public bool Extra_Upper_Left
	{
	    get { return ((LowerKeys & BrailleDisConsts.EXTRA_KEY_UPPER_LEFT) == BrailleDisConsts.EXTRA_KEY_UPPER_LEFT); }
	}
	/// <summary>
	/// returns the state of the key extra Upperright
	/// </summary>
	public bool Extra_Upper_Right
	{
	    get { return ((LowerKeys & BrailleDisConsts.EXTRA_KEY_UPPER_RIGHT) == BrailleDisConsts.EXTRA_KEY_UPPER_RIGHT); }
	}
	/*
	/// <summary>
	/// returns the state of the key extra (Spezial), (not part of hardware!; obsoleted)
	/// </summary>
	public bool Extra_Spezial
	{
	    get { return ((LowerKeys & BrailleDisConsts.EXTRA_KEY_SPEZIAL) == BrailleDisConsts.EXTRA_KEY_SPEZIAL); }
	}
	*/
	#endregion


	#region Upperleft keys
	/// <summary>
	/// returns the state of the key Upperleft 1
	/// </summary>
	public bool Left_1
	{
	    get { return ((UpperKeys & BrailleDisConsts.LEFT_1_BIT) == BrailleDisConsts.LEFT_1_BIT); }
	}
	/// <summary>
	/// returns the state of the key Upperleft 2
	/// </summary>
	public bool Left_2
	{
	    get { return ((UpperKeys & BrailleDisConsts.LEFT_2_BIT) == BrailleDisConsts.LEFT_2_BIT); }
	}
	/// <summary>
	/// returns the state of the key Upperleft 3
	/// </summary>
	public bool Left_3
	{
	    get { return ((UpperKeys & BrailleDisConsts.LEFT_3_BIT) == BrailleDisConsts.LEFT_3_BIT); }
	}
	/// <summary>
	/// returns the state of the key Upperleft 4
	/// </summary>
	public bool Left_4
	{
	    get { return ((UpperKeys & BrailleDisConsts.LEFT_4_BIT) == BrailleDisConsts.LEFT_4_BIT); }
	}

	#endregion

	#region Upperright keys
	/// <summary>
	/// returns the state of the key Upperright 1
	/// </summary>
	public bool Right_1
	{
	    get { return ((UpperKeys & BrailleDisConsts.RIGHT_1_BIT) == BrailleDisConsts.RIGHT_1_BIT); }
	}
	/// <summary>
	/// Upperright 2
	/// </summary>
	public bool Right_2
	{
	    get { return ((UpperKeys & BrailleDisConsts.RIGHT_2_BIT) == BrailleDisConsts.RIGHT_2_BIT); }
	}
	/// <summary>
	/// returns the state of the key Upperright 3
	/// </summary>
	public bool Right_3
	{
	    get { return ((UpperKeys & BrailleDisConsts.RIGHT_3_BIT) == BrailleDisConsts.RIGHT_3_BIT); }
	}
	/// <summary>
	/// returns the state of the key Upperright 4
	/// </summary>
	public bool Right_4
	{
	    get { return ((UpperKeys & BrailleDisConsts.RIGHT_4_BIT) == BrailleDisConsts.RIGHT_4_BIT); }
	}

	#endregion

	#region Lower keys
	/// <summary>
	/// returns the state of the key Lower left
	/// </summary>
	public bool Low_Left
	{
	    get { return ((LowerKeys & BrailleDisConsts.LOW_LEFT_BIT) == BrailleDisConsts.LOW_LEFT_BIT); }
	}
	/// <summary>
	/// returns the state of the key Lower right
	/// </summary>
	public bool Low_Right
	{
	    get { return ((LowerKeys & BrailleDisConsts.LOW_RIGHT_BIT) == BrailleDisConsts.LOW_RIGHT_BIT); }
	}

	#endregion
    }




}
