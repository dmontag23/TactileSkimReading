// MetecBD.H

// Header-file for MetecBD.h

// Copyright by METEC AG, Stuttgart

// Author F. Lüthi / 26. 04. 2007

#ifndef _METECBD_H
#define _METECBD_H

#ifndef BRDISDECLSPEC
#define BRDISDECLSPEC __declspec(dllimport)
#endif

#ifdef __cplusplus
extern "C"
{                               /* Assume C declarations for C++ */
#endif                          /* __cplusplus */

// maximal number of devices
#define BRD_MAX_NO_OF_DEVICES 10

/*
The driver-dll can serve more than 1 devices in 1 or more processes. But a
specific device only in 1 process

if you plugin a first device, this device gets the "filename" \\.\MITSUUSB0. The
next the \\.\MITSUUSB1, And so on.

If you open a device with this driver-dll, then unplug the device and re-plugin
it, the filename changes! So you have to close the "old" device and to open the
"new" device to use it again.

This driver-dll is able to serve at most BRD_MAX_NO_OF_DEVICES different devices
/ "filenames". If you unplug and replug devices to much  without closing devices
you will run out of the capabilities of this driver. This problem is an open
item for a redisigned driver-dll which looks in the registry and does not only
open \\.\MITSUUSB0 \\.\MITSUUSB1  ...

To open a device, the driver-dll tries first to open \\.\MITSUUSB0 ... until it
can open a device which has not been opened before. Then it returns the nr of
the filename of this device. So if you have more than 1 hardware-device and you
 don't change the places of this devices the first opened device will be the
 same every time you starts your computer and open your devices.

 The BrdEnumDevice function enumerates all devices and returns a table of zero
 terminated strings "nr=devicestring\0nr=devicestring\0\0". The nr is the number
 of the device and "devicestring" is the Identification-Sstring of the Device.

 An Asteric before the "nr" denotes that this Device is allready opened (by this
 or an other program.

 If you wish to open a specific device yu can use a first part of the
 "devicestring" e.g "<Pra" as Parameters of the BrdInitDev  function (or only
 the nr!).

 Known problems in Windows XP:
    - if you start the computer while 2 devices are plugged in, this DLL don't
      find them!
    - If you have plugged in 1 device and you try to insert a second, the
      computer don't recognize them everytime. Perhaps you have to unplug and
      replug it until the the computer recognizes it correct.
    - If you use two different (copies f the) driver-dll both can write on
      same device. They don't lock the devices for other DLL (copies).
*/


// Device-Types
#define BRD_TYPE_UNKNOWN 0
#define BRD_TYPE_USB_LINE 1 // METEC USB  LINE
#define BRD_TYPE_BRAILLEDIS 2 // BRAILLEDIS

// errorcodes returned via SetLastError:
#define BRD_ERR_TOO_LONG_TESTLOG_FILENAME 1001
#define BRD_ERR_USB_DRIVER_NOT_FOUND 1002
#define BRD_ERR_CAN_NOT_GET_DEVICE_INFO 1003
#define BRD_ERR_ERROR_WHILE_INITIALIZING 1004
#define BRD_ERR_DEVICE_ALLREADY_OPENED 1005
#define BRD_ERR_DEVICE_NOT_OPENED 1006
#define BRD_ERR_ILLEGAL_DEVICE_NUMBER 1007
#define BRD_ERR_VENDOR_REQUEST_FAILED 1008
#define BRD_ERR_ISO_SETUP 1009
#define BRD_ERR_ILLEGAL_LENGTH 1010
#define BRD_NO_UNUSED_DEVICE_FOUND 1011
#define BRD_ERR_NOT_ALLOWED_FOR_USB_LINE 1012

// Test-Codes for BrdSetTestData
#define BRD_TEST_LOG_CALLS 1
#define BRD_TEST_LOG_DRIVER_INFO 2
#define BRD_TEST_LOG_DATA 4
#define BRD_TEST_LOG_VENDOR_REQUEST 8

// function definitions

int BRDISDECLSPEC BrdSetTestData( // sets Data for testoutput
    const char *name, // name of the Testfile
		// "-" for standard output
		// 0 lets file-name unchanged
    int *flags); // see BRD_TEST_...
		 // 0 lets the testflags unchanged
// return-code: active flags
// note: this function writes only on the testfile if at least 1 testbit is set
// note: the Testoutput will always be appended on file "name"
//      please clear the file before startinn your test
// note: BRDSetTestData(0, 0); changes nothing, returns actice flags

void BRDISDECLSPEC BrdTestOutput( // writes testoubput on testfile
    int devnr, // device-number
    int code); // type of output:
	       // 'd' = device info
	       // 'i' = device-id-string
	       // 'p' = pipe-info

int BRDISDECLSPEC BrdEnumDevice( // enumerates all devices
    LPSTR lpszDev, // returns device-infos in the form
		   // 0=devicestring\01=devicestring\0\0
		   // *0=devicestring\0... where '*' denotes an open device
		   // The length of lpszDev should be aproximately 1300 (for a
		   // maximum of 10 devices
    int len); // length of lpszDev
// returns the number of found devices
// sets LastError to BRD_ERR_ILLEGAL_LENGTH for parameter errors

int BRDISDECLSPEC BrdInitDevice( // initiates a device
    LPCSTR lpszCriteria, // search criteria (or "" or 0)
			 // search-Criteria:
			 // a number e.g. "3".
			 // or a device-name <Name> or a beginning string
			 // "E" for enumeration only (only used in DLL)
    int *pdevtype); // returns BRD_TYPE_... (if pdevnr != 0)
// returncode: 0 .. = device-number, -1 g error
// call GetLastError() for error reasoon

int BRDISDECLSPEC BrdReadData( // reads data from device
    int devnr, // divice-number
    int len, // buffer-len
	     // METEC-USB-LINE at least 8 bytes
	     // BRAILLEDIS returns 0 (error), 733 or 1488 Bytes:
	     // depending on param "le":
	     // for the old interface: len = 733 to 1487 bytes returns 733 bytes
	     // for the new interface: len at least 1488 bytes returns 1488 b.
    unsigned char *buf); // databuffer at least len bytes
// here are the data returned
// Metec--Usb-Line: 8 bytes
// BraileeDis old interface: 733 bytes:
// [0..719] 720 bytes for the touch-sensors
// for the BrailleDis with 1440 sensors: they are reduce on 720 sensors
// building the mean value of both sensors of a module
// [720] paket-number
// [721] time in ms since the last touch-scan
// [722] time in ms until the next touch-scan
// [723-724] = last keyboard-state of  the paket
// [725-726] = keybordstate "end of paket" - 10 ms
// [727-728] = keybordstate "end of paket" - 20 ms
// [729-730] = keybordstate "end of paket" - 30 ms
// [731-732] = keybordstate "end of paket" - 40 ms
// BraileeDis new interface: 1488 bytes
// [0] = Layout-ID (0 = old, 1 = new BrailleDis
// [1] paket-number
// [2] time in ms since the last touch-scan
// [3] time in ms until the next touch-scan
// [8..1447] 1440 bytes for the touch-sensors
// for a old BrailleDis with 720 sensors: the sensors of a module-row
// are stored twice (first row: 0..59 + 60..119)
// [1448-1455] = last keyboard-state of  the paket
// [1456-1463] = keybordstate "end of paket" - 10 ms
// [1464-1471] = keybordstate "end of paket" - 20 ms
// [1472-1479] = keybordstate "end of paket" - 30 ms
// [1480-1487] = keybordstate "end of paket" - 40 ms
// returncode: no of bytes read or -1 for error
// call GetLastError() for error reasoon
// the old BraileeDis (Version 01#..) returns immediatly. So you have to wait
// until the "wait time iss ellapsed. If you read too early, you get twice the
// same packet
// For the new BrailleDis (Version 02#..) BrdReadData returns immediatly only if
// the actual packet was not sent yet. Otherwise it waits until a new packet was
// generated by the firmware of the BrailleDis. So you have not to wait with a
// Sleep in your Read-Thread because BrdReadData waits in the Read-Command.
// In case of error, please read tfe data again immediatly.

BOOL BRDISDECLSPEC BrdWriteData(
    int devnr, // device-number
    int len, // length  of data in bytes
	     // METEC-USB-LINE up to 42
	     // BRAILLEDIS up to 1440
    unsigned char *data); // data to be written
	     // METEC-USB-LINE up to 42
	     // braille point 1 = 0x80 ... braille point 8 = 0x01
	     // BRAILLEDIS up to 1440
	     // 1 Word (16 bits) per cell
	     // line 0 = cell 0 .e. 59
	     // line 1 = cell 60 ... 119
	     // bits per cell in PC-Wrds (low-Order-Byte first)
	     // 0x0100 0x0200
	     // 0x0400 0x0800
	     // 0x1000 0x2000
	     // 0x4000 008000
	     // 0x0001 0x0002
// return-code: true or false
// for BrailleDis: if the result is false, you should retry it 2 - 3 times
// because it could be a collision to the read-command
// call GetLastError() for error reasoon

BOOL BRDISDECLSPEC BrdCommand(
    int devnr, // device-number
    unsigned char data[8]); // data to be sent (allways 8 bytes!)
	// byte 0:
	    // 0x00 NOOP
	    // 0x01 = start the algorithm to equalize the Sensors
	// parameters for command 0x01
	    // byte 1 range of measurement (not defined yet)
	    // byte 2
		// values 0 - 0xf0 use this value as reference
		// value 0xff: equalize all sensors on 50 %
// return-code: true or false
// call GetLastError() for error reasoon

BOOL BRDISDECLSPEC BrdHighVoltage( // set high voltage on or off
    int devnr, // device-number
    int state);
	// state = 0 sets high voltage off
	// state = 1 sets high voltage on

// only for metec usb line
BOOL BRDISDECLSPEC BrdSetNoOfModules( // sets the numer of modules for line-disp
    int devnr, // device-number
    unsigned char number); // no of modules
// return-code: true or false
// call GetLastError() for error reasoon

BOOL BRDISDECLSPEC BrdCloseDevice( // closes a device
    int devnr); // device-number
// return-code: true or false
// call GetLastError() for error reasoon
// note: the device is also closed if the using process exits

// function prototypes

typedef int (*LPBrdSetTestData)(
    const char *name,
    int *flags);

typedef void (*LPBrdTestOutput)(
    int devnr,
    int code);

typedef int (*LPBrdEnumDevice)(
    LPSTR lpszDev,
    int len);

typedef int (*LPBrdInitDevice)(
    LPCSTR lpszCriteria,
    int *devtype);

typedef int (*LPBrdReadData)(
    int devnr,
    int len,
    unsigned char *buf);

typedef BOOL (*LPBrdWriteData)(
    int devnr,
    int len,
    unsigned char *data);

    typedef BOOL (*LPBrdCommand)(
    int devnr,
    unsigned char *data);

typedef BOOL (*LPBrdSetNoOfModules)(
    int devnr,
    unsigned char number);

typedef BOOL (*LPBrdCloseDevice)(
    int devnr);

typedef BOOL (*LPBrdHighVoltage)(
    int devnr, int state);


#ifdef __cplusplus
}                               /* End of extern "C" { */

#endif                          /* __cplusplus */

#endif  // _BRDIS_H
