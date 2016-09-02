// MetecBD.cpp

// Controller-DLL for METEC's MetecBD

// Copyright by METEC AG, Stuttgart

// Author F. Lüthi / 29. 01. 2005

#ifdef NOT_BRD_EXTERN // zum Einbinden in ein Programm
#define BRDISDECLSPEC
#else
#define BRDISDECLSPEC __declspec(dllexport)
#endif

#include "cmnhdr.h"
#include <windows.h>
#include <stdio.h>
#include <string.h>
#include "MetecBD.h"
#include <stdlib.h>
#include "mitsuusb.h"
#include <basetyps.h>
#include <wtypes.h>
#include <initguid.h>
#include <conio.h>
#include <setupapi.h>
#include <winioctl.h>
#include "usbio_i.h"

#define SUBR // serves as label for functions
#define EXTERNC extern "C"
#define DEV_ID_LEN 128 // length of the device string
#define MAXPIPES 10 // max no of pipes

// data shared between different processes
// precaution: all shared data-items must be explicitelly initiated
#pragma data_seg("._METECBD")

GUID g_UsbioID = USBIO_IID;

int g_proc_nr = 0; // no of least attached process
char version[] = "0.1";
HINSTANCE ghDllInst = 0;   // Handle to the DLL's instance.
char testfilename [256] = "-"; // "-" = standard output
int testsw = 0; // no test output
enum {
    DRIVERTYP_UNKNOWN,
    DRIVERTYP_MITSUUSB,
    GRIVERTYP_USBIO
} drivertyp = DRIVERTYP_UNKNOWN;

struct _devtab { // table of the open devices
    int proc_nr; // no of the owner process
    int lastPaket;
    HANDLE hDriver;
    DEVICEINFO DeviceInfo;
    USBIO_CONFIGURATION_INFO UsbioConfigurationInfo;
    unsigned char UsbioActualPipe; // actual bounded pipe
    unsigned char UsbioUnplugged;
    unsigned char DeviceIdString[DEV_ID_LEN];
    unsigned char vers[6]; // version or id-string
    char drvname[128];
    PIPEINFO PipeInfo[MAXPIPES]; //Pipe Info of the usb Device
    int devtype;
    volatile long lock;
    int eof;
} devtab[BRD_MAX_NO_OF_DEVICES] = { 0 };

#pragma data_seg() // end of shared data

// separate data for eacf brocess
int proc_nr = 0; // no of the process

SUBR BOOL BindPipe(
    int devnr,
    ULONG ulPipe
    );

SUBR void TestLog( // append data to the testoutputfile
    LPCSTR str, // text to be written
    int nr= 0, // an integer to be written
    int code = 0) // 0 = write no only if != 0; 1 write always
		  // | 2 = write ' ' at end instead of '\n'
		  // | 4 = use %x instead %d
    // the function writes the data only if testsw != 0
    // each output will be appended to the file, then the file is closed
{
    FILE *fp = 0;

    if (*testfilename && testsw)
    {
	if (*testfilename == '-') fp = stdout;
	else if (fopen_s(&fp,testfilename,"a"))
	{
	    fp = 0;
	}
    };

    if (fp)
    {
	if (nr || (code & 1))
	    fprintf(fp,code & 4 ? "%s %x" : "%s %d",
		str,nr);
	else fprintf(fp,"%s",str);
	if (code & 2) putc(' ',fp);
	else putc('\n',fp);
       if (fp != stdout) fclose(fp);
    };
}

SUBR void TestLogVrd(
    VENDOR_REQUEST_DATA *pvRD) // data to be written
    // function writes vendor_requst_data only if appropriate testbit is set.
{
    int i, j, l;
    char scr[80];

    if (!(testsw & BRD_TEST_LOG_VENDOR_REQUEST)) return;
    TestLog("\nVendor Request Data");
    TestLog("bmRequestType",pvRD->bmRequestType,1);
    TestLog("bRequest",pvRD->bRequest,1);
    TestLog("wValue",pvRD->wValue,1);
    TestLog("wIndex",pvRD->wIndex,1);
    TestLog("wLength",pvRD->wLength,1);
    TestLog("pBuff",!!pvRD->pBuff,1);
    if (pvRD->pBuff)
    {
	l = pvRD->wLength;
	if (l > 30) i = 30;
	for (i = j = 0; i < l; i++)
	{
	    sprintf_s(scr+j,sizeof(scr)-j,"%02x",pvRD->pBuff[i]);
	    j += 2;
	    if (i && !(i % 4)) scr[j++] = ' ';
	};
	scr[j] = 0;
	TestLog(scr);
    };
}

SUBR EXTERNC BOOL WINAPI DllMain (HANDLE hModule, DWORD dwFunction,
    LPVOID lpNot)
	// start and end of the dll
{

    ghDllInst = (HINSTANCE) hModule;
    switch (dwFunction)
    {
	case DLL_PROCESS_ATTACH:
	    g_proc_nr++; // increment global proc-nr
	    proc_nr = g_proc_nr; // assign this no to actual process
	    TestLog("\nprocess attach, p:",proc_nr,1); // erst ab 2. da vorher 0
		// no output for first process because testsw == 0!
	    break;
	case DLL_PROCESS_DETACH:
	    {
		TestLog("\nprocess detach, p:",proc_nr,1);
		// close open devices for this process
		for (int i = 0; i < BRD_MAX_NO_OF_DEVICES; i++)
		{
		    if (devtab[i].proc_nr == proc_nr && devtab[i].hDriver)
		    {
			TestLog("\nDevice not closed",i,1);
			BrdCloseDevice(i);
		    }
		}
	    };
	    break;
	case DLL_THREAD_ATTACH:
	    break;
	case DLL_THREAD_DETACH:
	    break;
	default:
	    break;
    }
    return TRUE;
}

SUBR BOOL CheckDevnr(int devnr)
    // checks the correctness of  a divnr
{

    if (devnr >= 0 && devnr < BRD_MAX_NO_OF_DEVICES
	&& proc_nr == devtab[devnr].proc_nr)
    {
	return TRUE;
    }
    else
    {
	SetLastError( BRD_ERR_ILLEGAL_DEVICE_NUMBER);
	TestLog("\nillegal Device-Number",devnr,1);
	return FALSE;
    }
}

SUBR EXTERNC int BRDISDECLSPEC BrdSetTestData(const char *name, int *flags)
    // for all Brd...() please see the description in MetecBD.h
{

    SetLastError(0);

    if (flags) testsw = *flags; // set only if flags != 0

    if (name && *name) // change only if value is present
    {
	if (strlen(name) >= sizeof(testfilename))
	{
	    SetLastError(BRD_ERR_TOO_LONG_TESTLOG_FILENAME);
	    return testsw;
	};
	strcpy_s(testfilename,sizeof(testfilename),name);
	TestLog("\nMetecBD.DLL from",0,2);
	TestLog(version);
    }

    if (testsw) TestLog("Test-Switch ",testsw,1);
    return testsw;
}

SUBR EXTERNC void BRDISDECLSPEC BrdTestOutput(int devnr, int code)
    // the function works also if the device is used by another process
{

    if (devnr < 0 || devnr >= BRD_MAX_NO_OF_DEVICES)
    {
	TestLog("illegal Device-Number, d:",devnr,1);
	return;
    };

    switch (code)
    {
	case 'd': // Driverinfo
	    TestLog("\nDeviceInfo, d:",devnr,1);
	    TestLog("drvname",0,2);
	    TestLog( devtab[devnr].drvname);
	    TestLog("nNumConfigurations",
		devtab[devnr].DeviceInfo.nNumConfigurations,1);
	    TestLog("wVendorID",devtab[devnr].DeviceInfo.wVendorID,1);
	    TestLog("wProductID",devtab[devnr].DeviceInfo.wProductID,1);
	    TestLog("bNumPipes",devtab[devnr].DeviceInfo.bNumPipes,1);
	    TestLog("bNumInterfaces",devtab[devnr].DeviceInfo.bNumInterfaces,1);
	    TestLog("bSurpriseRemoval",
		devtab[devnr].DeviceInfo.bSurpriseRemoval,1);
	    break;
	case 'p': // Pipe-Info
	    if (drivertyp == DRIVERTYP_MITSUUSB)
	    {
		TestLog("\nPipeInfo, d:",devnr,1);
		TestLog("hPipeHandle",
		!!devtab[devnr].PipeInfo[0].hPipeHandle,1);
		TestLog("PipeType",
		    (int) devtab[devnr].PipeInfo[0].PipeType,1);
		TestLog("bEndpointAddress",
		    (int) devtab[devnr].PipeInfo[0].bEndpointAddress,1);
		TestLog("Gap",!!devtab[devnr].PipeInfo[0].Gap,1);
	    }
	    else
	    {
		int p;
		for (p = 0; p < 5; p++)
		{
		    TestLog("\nPipeInfo, d:",devnr,3); TestLog("nr",p,1);
		    TestLog("PipeType",
			devtab[devnr].UsbioConfigurationInfo.PipeInfo[p].
			PipeType,1);
		    TestLog("MaximumTransferSize",
			devtab[devnr].UsbioConfigurationInfo.PipeInfo[p].
			MaximumTransferSize,1);
		    TestLog("MaximumPacketSize",
			devtab[devnr].UsbioConfigurationInfo.PipeInfo[p].
			MaximumPacketSize,1);
		    TestLog("EndpointAddress",
			devtab[devnr].UsbioConfigurationInfo.PipeInfo[p].
			EndpointAddress,1);
		    TestLog("Interval",
			devtab[devnr].UsbioConfigurationInfo.PipeInfo[p].
			Interval,1);
		    TestLog("InterfaceNumber",
			devtab[devnr].UsbioConfigurationInfo.PipeInfo[p].
			InterfaceNumber,1);
		}
	    }
	    break;
	case 'i': // device-id-string
	    TestLog("\nDeviceIdString, d:",devnr,1);
	    TestLog("DeviceType:",devtab[devnr].devtype,1);
	    TestLog((LPCSTR) devtab[devnr].DeviceIdString);
	    break;
	default:
	    TestLog("illegal Code for TestOutput:",code,1);
	    break;
    };
}

SUBR void TestHexa(unsigned char *buf, int pos, int len)
{
    int b;
    char scr[80];

    if (len > 30) len = 30;
    sprintf_s(scr,sizeof(scr),"%4d: ",pos);
	for  (b = 0; b < len; b++)
	{
	    sprintf_s(scr+strlen(scr ),sizeof(scr)-strlen(scr),
		"%02x",buf[b]&0xff);
	}
	TestLog(scr);
}

SUBR  BOOL lock(int devnr)
{
    int loop;

    if (drivertyp != GRIVERTYP_USBIO) return TRUE;
    for (loop = 0; loop < 5; loop++)
    {
	if (!InterlockedCompareExchange(&devtab[devnr].lock,1,0)) return TRUE;
	Sleep(1);
    }
    return false;
}

SUBR  BOOL unlock(int devnr)
{

    InterlockedExchange(&devtab[devnr].lock,0);
    return TRUE;
}

SUBR DWORD UsbioIoctlSync(
  HANDLE FileHandle,
  DWORD IoctlCode,
  const void *InBuffer,
  DWORD InBufferSize,
  void *OutBuffer,
  DWORD OutBufferSize,
  DWORD *BytesReturned
  )
{
  DWORD Status;
  DWORD BytesRet = 0;
  BOOL succ;
  OVERLAPPED Overlapped;

  ZeroMemory(&Overlapped,sizeof(Overlapped));
  Overlapped.hEvent = CreateEvent(NULL,FALSE,FALSE,NULL);
  if ( Overlapped.hEvent==NULL ) {
    return USBIO_ERR_NO_MEMORY;
  }

  // call the device driver
  succ = DeviceIoControl(
	    FileHandle,         // driver handle
	    IoctlCode,          // IOCTL code
	    (void*)InBuffer,    // input buffer
	    InBufferSize,       // input buffer size
	    OutBuffer,          // output buffer
	    OutBufferSize,      // output buffer size
	    &BytesRet,          // number of bytes returned
	    &Overlapped         // OVERLAPPED structure
	    );
  if ( succ ) {
    // ioctl completed successfully
    Status = USBIO_ERR_SUCCESS;
  } else {
    Status = GetLastError();
    if ( Status == ERROR_IO_PENDING ) {
      // the operation is pending, wait for completion
      succ = GetOverlappedResult(
		FileHandle,
		&Overlapped,
		&BytesRet,  // number of bytes successfully transferred
		TRUE        // wait for completion
		);
      if ( succ ) {
	// completed successfully
	Status = USBIO_ERR_SUCCESS;
      } else {
	// completed with error
	Status = GetLastError();
      }
    }
  }

  if ( BytesReturned != NULL ) {
    *BytesReturned = BytesRet;
  }

  CloseHandle(Overlapped.hEvent);

  return Status;

}

SUBR BOOL UsbioSetConfiguration(
    HANDLE DevHandle // device handle
    )
{
    // check the following Device- parameters and adapt them to the
    // actual USB device
    // size of the I/O buffer in bytes
    // The size should be a multiple of the endpoint's FIFO size.
    // The buffer will be returned if
    // 1. a short or zero packet is received
    // 2. the buffer is completely filled.
    #define BUFFER_SIZE 2048

    // configuration
    //
    // Index to be used for "GetConfigurationDescriptor"
    // This parameter is greater than 0 for multiconfiguration devices only.
    #define CONFIG_INDEX 0
    // number of interfaces, all possible interfaces must be activated
    // The number of interfaces can be changed, if the multi-interface or
    // composite device driver from Microsoft is installed.
    #define CONFIG_NB_OF_INTERFACES 1
    // settings per interface
    // This application uses one interface only.
    #define CONFIG_INTERFACE    0
    #define CONFIG_ALT_SETTING  0
    #define CONFIG_TRAN_SIZE    4096

    // configuration struct
    USBIO_SET_CONFIGURATION         Config;
    // other variables
    DWORD                           Status;

    // configure the USB device
    ZeroMemory(&Config,sizeof(Config));
    Config.ConfigurationIndex = CONFIG_INDEX;
    Config.NbOfInterfaces = CONFIG_NB_OF_INTERFACES;
    Config.InterfaceList[0].InterfaceIndex = CONFIG_INTERFACE;
    Config.InterfaceList[0].AlternateSettingIndex = CONFIG_ALT_SETTING;
    Config.InterfaceList[0].MaximumTransferSize = CONFIG_TRAN_SIZE;
    // send the configuration request
    Status = UsbioIoctlSync(
    DevHandle,
    IOCTL_USBIO_SET_CONFIGURATION,
    &Config,
    sizeof(USBIO_SET_CONFIGURATION),
    NULL,
    0,
    NULL
    );
    if ( Status != USBIO_ERR_SUCCESS )
    {
	TestLog("SET_CONFIGURATION error, status= 0x",Status,5);
	return FALSE;
    }

    return TRUE;
}

SUBR int UsbioReadPipe(
    int devnr,
    LPVOID lpBuffer,
    int BufferSize)
{
    HANDLE DevHandle = devtab[devnr].hDriver;
    // OVERLAPPED struct, needed for asynchronous I/O operations
    OVERLAPPED Overlapped;

    // other variables
    DWORD Status;
    DWORD BytesTransferred;
    int ReturnCode = -1;
    BOOL succ;

    // initialize overlapped struct, create an event object
    ZeroMemory(&Overlapped,sizeof(Overlapped));
    Overlapped.hEvent = CreateEvent(NULL,FALSE,FALSE,NULL);
    if ( Overlapped.hEvent==NULL )
    {
	TestLog("Unable to create Event Object");
	unlock(devnr);
	return -1;
    }

    // read data into the buffer
    // the ReadFile function does not block, it returns immediately
    succ = ReadFile(
	    DevHandle,          // handle of file to read
	    lpBuffer,             // pointer to buffer that receives data
	    BufferSize,        // number of bytes to read
	    &BytesTransferred,  // number of bytes read
	    &Overlapped         // pointer to OVERLAPPED struct
	    );
	    unlock(devnr);
    if ( succ )
    {
      // read operation completed immediately and successfully
      Status = USBIO_ERR_SUCCESS;
      ReturnCode = (int)BytesTransferred;
    }
    else
    {
      // read operation is pending or an error is ocurred
      Status = GetLastError();
      if ( Status==ERROR_IO_PENDING )
      {
	// read operation pending wait for completion with a timeout of 1 sec
	if ( WaitForSingleObject(Overlapped.hEvent,1000) == WAIT_TIMEOUT )
	{
	    goto Exit;
	}
	// operation completed, get final status
	succ = GetOverlappedResult(
		  DevHandle,
		  &Overlapped,
		  &BytesTransferred,  // number of bytes read
		  FALSE               // do not wait for completion
		  );
	if ( succ )
	{
	  // success
	  Status = USBIO_ERR_SUCCESS;
	  ReturnCode = (int)BytesTransferred;
	}
	else
	{
	  // read operation completed with error
	  Status = GetLastError();
	  TestLog("Read request returned with error code 0x",Status,5);
	}
      }
      else
      {
	// read operation completed with error
	TestLog("Read request returned with error code0x",Status,5);
      }
    }

    Exit:
    // close handles
    CloseHandle(Overlapped.hEvent);

    return ReturnCode;

}

SUBR int UsbioWritePipe(
    int devnr,
    LPVOID lpBuffer,
    int BufferSize)
{
    HANDLE DevHandle = devtab[devnr].hDriver;
    // OVERLAPPED struct, needed for asynchronous I/O operations
    OVERLAPPED Overlapped;

    // other variables
    DWORD Status;
    DWORD BytesTransferred;
    int ReturnCode = -1;
    BOOL succ;

    // initialize overlapped struct, create an event object
    ZeroMemory(&Overlapped,sizeof(Overlapped));
    Overlapped.hEvent = CreateEvent(NULL,FALSE,FALSE,NULL);
    if ( Overlapped.hEvent==NULL )
    {
	TestLog("Unable to create Event Object");
	unlock(devnr);
	return -1;
    }

    // wrinte data into the buffer
    // the WriteFile function does not block, it returns immediately
    succ = WriteFile(
	    DevHandle,          // handle of file to wrinte
	    lpBuffer,             // pointer to buffer that receives data
	    BufferSize,        // number of bytes to wrinte
	    &BytesTransferred,  // number of bytes wrinte
	    &Overlapped         // pointer to OVERLAPPED struct
	    );
    unlock(devnr);
    if ( succ )
    {
      // wrinte operation completed immediately and successfully
      Status = USBIO_ERR_SUCCESS;
      ReturnCode = (int)BytesTransferred;
    }
    else
    {
      // wrinte operation is pending or an error is ocurred
      Status = GetLastError();
      if ( Status==ERROR_IO_PENDING )
      {
	// wrinte operation pending wait for completion with a timeout of 1 sec
	if ( WaitForSingleObject(Overlapped.hEvent,1000) == WAIT_TIMEOUT )
	{
	    goto Exit;
	}
	// operation completed, get final status
	succ = GetOverlappedResult(
		  DevHandle,
		  &Overlapped,
		  &BytesTransferred,  // number of bytes wrinte
		  FALSE               // do not wait for completion
		  );
	if ( succ )
	{
	  // success
	  Status = USBIO_ERR_SUCCESS;
	  ReturnCode = (int)BytesTransferred;
	}
	else
	{
	  // wrinte operation completed with error
	  Status = GetLastError();
	  TestLog("Write request returned with error code 0x",Status,5);
	}
      }
      else
      {
	// wrinte operation completed with error
	TestLog("Write request returned with error code0x",Status,5);
      }
    }

    Exit:
    // close handles
    CloseHandle(Overlapped.hEvent);

    return ReturnCode;

}

SUBR BOOL BrdDeviceIoControl(
    int devnr,
    DWORD dwIoControlCode,
    LPVOID lpInBuffer,
    DWORD nInBufferSize,
    LPVOID lpOutBuffer,
    DWORD nOutBufferSize,
    DWORD *lpBytesReturned,
    LPOVERLAPPED lpOverlapped
    )
{
    BOOL rc = TRUE;

    if (drivertyp == DRIVERTYP_MITSUUSB)
    {
	return DeviceIoControl(
	    devtab[devnr].hDriver, // handle to driver
	    dwIoControlCode,
	    lpInBuffer,
	    nInBufferSize,
	    lpOutBuffer,
	    nOutBufferSize,
	    lpBytesReturned,
	    lpOverlapped);
    }

    switch (dwIoControlCode)
    {
	case IOCTL_MITSU_BULK_READ:
	    {
		int nrOfBytes = UsbioReadPipe(
		    devnr,
		    lpOutBuffer,
		    nOutBufferSize);
		if (nrOfBytes >= 0)
		{
		    *lpBytesReturned = (DWORD) nrOfBytes;
		}
		else
		{
		    *lpBytesReturned = 0;
		    rc = FALSE;
		}
	    }
	    break;

	case IOCTL_MITSU_BULK_WRITE:
	    {
		int nrOfBytes = UsbioWritePipe(
		    devnr,
		    lpOutBuffer,
		    nOutBufferSize);
		if (nrOfBytes >= 0)
		{
		    *lpBytesReturned = (DWORD) nrOfBytes;
		}
		else
		{
		    *lpBytesReturned = 0;
		    rc = FALSE;
		}
	    }
	    break;

	case IOCTL_MITSU_GET_DEVICE_INFO:
	    {
		DWORD status;

		rc = UsbioSetConfiguration( devtab[devnr].hDriver);
		if (!rc) break;
		status = UsbioIoctlSync(
		devtab[devnr].hDriver, // handle to driver
		IOCTL_USBIO_GET_CONFIGURATION_INFO,
		NULL,
		0,
		&devtab[devnr].UsbioConfigurationInfo,
		sizeof(USBIO_CONFIGURATION_INFO),
		lpBytesReturned);
		if (status)
		{
		    rc = FALSE;
		    TestLog("get_configuration_info error 0x",status,5);
		}
		else
		{
		    devtab[devnr].DeviceInfo.nNumConfigurations =  1;
		    devtab[devnr].DeviceInfo.wVendorID = 0;
		    devtab[devnr].DeviceInfo.wProductID  = 0;
		    devtab[devnr].DeviceInfo.bNumPipes = (UCHAR)
			devtab[devnr].UsbioConfigurationInfo.NbOfPipes;
		    devtab[devnr].DeviceInfo.bNumInterfaces = (UCHAR)
			devtab[devnr].UsbioConfigurationInfo.NbOfInterfaces;
		    devtab[devnr].DeviceInfo.bSurpriseRemoval = 0;
		}
	    }
	    break;

	case IOCTL_MITSU_GET_PIPE_INFO:
	    // Pnothing to du
	    break;

	case IOCTL_MITSU_ISO_SETUP:
	    break;

	case IOCTL_MITSU_SEND_VENDOR_REQUEST:
	    {
		USBIO_CLASS_OR_VENDOR_REQUEST vrd;
	      DWORD status;
	      VENDOR_REQUEST_DATA *pIn = (VENDOR_REQUEST_DATA *) lpInBuffer;

	      vrd.Flags = 0;
	      vrd.Type = RequestTypeVendor;
	      vrd.Recipient = RecipientDevice;
	      vrd.RequestTypeReservedBits = pIn->bmRequestType;
	      vrd.Request = pIn->bRequest;
	      vrd.Value = pIn->wValue;
	      vrd.Index = pIn->wIndex;

	      if ( pIn->bmRequestType == 0xc0)
	      { // input from device
		    status = UsbioIoctlSync(
		  devtab[devnr].hDriver, // handle to driver
		  IOCTL_USBIO_CLASS_OR_VENDOR_IN_REQUEST,
		  &vrd,
		  sizeof(vrd),
		  lpOutBuffer,
		  nOutBufferSize,
		  lpBytesReturned);
	      }
	      else
	      { // output to device
		    status = UsbioIoctlSync(
		  devtab[devnr].hDriver, // handle to driver
		  IOCTL_USBIO_CLASS_OR_VENDOR_OUT_REQUEST,
		  &vrd,
		  sizeof(vrd),
		  pIn->pBuff,
		      pIn->wLength,
		      lpBytesReturned);
	      }
	      if (status)
	      {
		  rc = FALSE;
		  TestLog("vendor_request error 0x",status,5);
		  if (status == USBIO_ERR_DEV_NOT_RESPONDING)
		    devtab[devnr].eof = 1;
	      }
	    }
	    break;

	case IOCTL_SET_CURRENT_PIPE:
	    {
		ULONG ulPipe = *((ULONG*) lpInBuffer);
		UCHAR EndpointAddress =
		    devtab[devnr].UsbioConfigurationInfo.PipeInfo[ulPipe].
		    EndpointAddress;
		USBIO_BIND_PIPE BindPipe;
		DWORD Status;

		if (EndpointAddress == devtab[devnr].UsbioActualPipe)
		{
		    break;
		}

		// unbind if necessary
		if (devtab[devnr].UsbioActualPipe)
		{
		    Status = UsbioIoctlSync(
		    devtab[devnr].hDriver,
		    IOCTL_USBIO_UNBIND_PIPE,
		    NULL,
		    0,
		    NULL,
		    0,
		    NULL
		    );
		    devtab[devnr].UsbioActualPipe = 0;
		    if ( Status != USBIO_ERR_SUCCESS )
		    {
			TestLog("Unable to unbind pipe, status=. 0x", Status,5);
		    }
		}

		// bind the handle to a pipe
		// Note: a handle can be bound to one pipe only
		if (EndpointAddress)
		{
		    ZeroMemory(&BindPipe,sizeof(BindPipe));
		    BindPipe.EndpointAddress = EndpointAddress;
		    // send bind request to USBIO
		    Status = UsbioIoctlSync(
		    devtab[devnr].hDriver,
		    IOCTL_USBIO_BIND_PIPE,
		    &BindPipe,
		    sizeof(USBIO_BIND_PIPE),
		    NULL,
		    0,
		    NULL
		    );
		    if ( Status != USBIO_ERR_SUCCESS )
		    {
			TestLog("Unable to bind pipe, status=. 0x", Status,5);
			rc = FALSE;
			break;
		    }
		    else
		    {
			devtab[devnr].UsbioActualPipe = EndpointAddress;
		    }
		}
	    }
	    break;
    }

    return rc;
}

SUBR BOOL BindPipe(
    int devnr,
    ULONG ulPipe
    )
{
    BOOL bRtn;
    DWORD dwBytesReturned = 0;

    bRtn = BrdDeviceIoControl(devnr,
    IOCTL_SET_CURRENT_PIPE,
    &ulPipe, sizeof( ULONG ), NULL, 0, &dwBytesReturned, NULL );
    if ( !bRtn )
    {
	TestLog("\nerror for IOCTL_SET_CURRENT_PIPE",ulPipe,1);
    }
    return bRtn;
}

SUBR int UsbioEnumDevices()
{
    HDEVINFO                            hardwareDeviceInfo;
    SP_DEVICE_INTERFACE_DATA            deviceInterfaceData;
    PSP_DEVICE_INTERFACE_DETAIL_DATA    deviceInterfaceDetailData = NULL;
    ULONG                               predictedLength = 0;
    ULONG                               requiredLength = 0;
    ULONG                               i =0;

    // Open a handle to the device interface information set of all
    // present METEC-Devices
    hardwareDeviceInfo = SetupDiGetClassDevs (
	(LPGUID)&g_UsbioID,
	NULL, // Define no enumerator (global)
	NULL, // Define no
	(DIGCF_PRESENT | // Only Devices present
	DIGCF_DEVICEINTERFACE)); // Function class devices.
    if (INVALID_HANDLE_VALUE == hardwareDeviceInfo)
    {
	TestLog("SetupDiGetClassDevs error: 0x", GetLastError(),5);
	return 0;
    }

    // Enumerate devices of MetecUsb-class
    deviceInterfaceData.cbSize = sizeof (SP_DEVICE_INTERFACE_DATA);
    i = 0;
    int enumFlags[BRD_MAX_NO_OF_DEVICES] = { 0 };
    for (;;)
    {
	if (SetupDiEnumDeviceInterfaces (hardwareDeviceInfo,
	    0, // No care about specific PDOs
	    (LPGUID)&g_UsbioID,
	    i, //
	    &deviceInterfaceData))
	{

	    if (deviceInterfaceDetailData)
	    {
		free (deviceInterfaceDetailData);
		deviceInterfaceDetailData = NULL;
	    }

	    // Allocate a function class device data structure to
	    // receive the information about this particular device.
	    // First find out required length of the buffer
	    if (!SetupDiGetDeviceInterfaceDetail (
		hardwareDeviceInfo,
		&deviceInterfaceData,
		NULL, // probing so no output buffer yet
		0, // probing so output buffer length of zero
		&requiredLength,
		NULL))
	    { // not interested in the specific dev-node
		if (ERROR_INSUFFICIENT_BUFFER != GetLastError())
		{
		    TestLog(
		    "SetupDiGetDeviceInterfaceDetail error 0x",
		    GetLastError(),5);
		    SetupDiDestroyDeviceInfoList (hardwareDeviceInfo);
		    return FALSE;
		}

	    }

	    predictedLength = requiredLength;

	    deviceInterfaceDetailData =
		(PSP_DEVICE_INTERFACE_DETAIL_DATA) malloc (predictedLength);

	    if (deviceInterfaceDetailData)
	    {
		deviceInterfaceDetailData->cbSize =
		    sizeof (SP_DEVICE_INTERFACE_DETAIL_DATA);
	    }
	    else
	    {
		TestLog(
		"Couldn't allocate%d bytes for device interface details.:",
		predictedLength,1);
		SetupDiDestroyDeviceInfoList (hardwareDeviceInfo);
		return FALSE;
	    }

	    if (! SetupDiGetDeviceInterfaceDetail (
		hardwareDeviceInfo,
		&deviceInterfaceData,
		deviceInterfaceDetailData,
		predictedLength,
		&requiredLength,
		NULL))
	    {
		TestLog("Error in SetupDiGetDeviceInterfaceDetail");
		SetupDiDestroyDeviceInfoList (hardwareDeviceInfo);
		free (deviceInterfaceDetailData);
		return FALSE;
	    }
	    if (testsw & BRD_TEST_LOG_DRIVER_INFO)
	    {
		TestLog("devicepath",i,3);
		TestLog(deviceInterfaceDetailData->DevicePath);
	    }
	    int d;
	    drivertyp = GRIVERTYP_USBIO; // neuer Treiber
	    // existiert device bereits?
	    for (d = 0; d < BRD_MAX_NO_OF_DEVICES; d++)
	    {
		if  (!strcmp(devtab[d].drvname,
		    deviceInterfaceDetailData->DevicePath))
		{
		    enumFlags[d] = 1;
		    d = -1;
		    break;
		}
	    }
	    if (d >= 0) // not found in devtab
	    {
		for (d = 0; d < BRD_MAX_NO_OF_DEVICES; d++)
		{
		    if (!*devtab[d].drvname)
		    {
			strcpy_s(devtab[d].drvname,sizeof(devtab[0].drvname),
			    deviceInterfaceDetailData->DevicePath);
			enumFlags[d] = 1;
			break;
		    }
		}
	    }
	    i++;
	}
	else if (ERROR_NO_MORE_ITEMS != GetLastError())
	{
	    free (deviceInterfaceDetailData);
	    deviceInterfaceDetailData = NULL;
	    continue;
	}
	else
	{
	    break;
	}

    }

    // clear unused devices
    int d;
    for (d = 0; d < BRD_MAX_NO_OF_DEVICES; d++)
    {
	if (!devtab[d].hDriver)
	{
	    if (!enumFlags[d] ) *devtab[d].drvname = 0;
	    *devtab[d].DeviceIdString = 0;
	}
    }

    SetupDiDestroyDeviceInfoList (hardwareDeviceInfo);

    return TRUE;
}

SUBR EXTERNC int BRDISDECLSPEC BrdEnumDevice (LPSTR lpszDev, int len)
{
    int devnr, i, l, ll, a, ts;

    if (testsw & BRD_TEST_LOG_CALLS)
    {
	TestLog("\nEnumDevice, p:",proc_nr,3);
	TestLog("len: ",len,1);
    };

    if (len <= 0 || !lpszDev)
    {
	SetLastError(BRD_ERR_ILLEGAL_LENGTH);
	TestLog("length <= 0 or illegal buffer");
	return 0;
    };

    // try to open all unopened devices
    ts = testsw;
    testsw = 0;
    BrdInitDevice("E", 0);
    testsw = ts;

    for (devnr = a = l = 0; devnr < BRD_MAX_NO_OF_DEVICES; devnr++)
    {
	if (!*devtab[devnr].DeviceIdString) continue;
	for (i = 0; devtab[devnr].DeviceIdString[i] ; i++) ;
	if (i < 9) continue; // before '<'
	if (l + i > len) break; // lpszDev to short
	ll = l;
	if (devtab[devnr].hDriver)
	    sprintf_s(lpszDev+l,len-l,"*%d=",devnr);
	else sprintf_s(lpszDev+l,len-l,"%d=",devnr);
	while (lpszDev[l]) l++;
	strncpy_s(lpszDev+l,len-l, (LPCSTR) devtab[devnr].DeviceIdString+8,i-8);
	lpszDev[l+i-8] = 0;
	if (testsw & BRD_TEST_LOG_DATA)
	{
	    TestLog(lpszDev+ll);
	};
	l += i-7;
	a++;
    };
    lpszDev[l++] = 0;

    if (testsw & BRD_TEST_LOG_DATA)
    {
	TestLog("Total DeVICES:",a,1);
    };
    return a;
}

SUBR EXTERNC int BRDISDECLSPEC BrdInitDevice (LPCSTR lpszCriteria,
											  int *pdevtype)
{
	int i, b = 0, e = BRD_MAX_NO_OF_DEVICES;
	int enumeration = lpszCriteria && !strcmp(lpszCriteria,"E");
	VENDOR_REQUEST_DATA vRD;
	DWORD dwBytesReturned = 0;
	BOOL bRtn;
	UCHAR bOutBuff[8]={0,0,0,0,0,0,0,0};
	int error=0;
	int devnr = 0;
	char *pdrvname, drvname[64];

	// fuer neuen Wreiber geraete enumerieren
	if (drivertyp != DRIVERTYP_MITSUUSB)
	{
		UsbioEnumDevices();
	}

	//Start Driver
	SetLastError(0);
	if (pdevtype) *pdevtype = 0;
	if (!lpszCriteria) lpszCriteria = "";
	if (testsw & BRD_TEST_LOG_CALLS)
	{
		TestLog("\nInitDevice, p:",proc_nr,3);
		TestLog(lpszCriteria);
	};

	// change b and e if lpszCriteria begins with a digit
	if (isdigit(*lpszCriteria))
	{
		for (i = 0; isdigit(lpszCriteria[i]); i++)
		{
			b = 10 * b + lpszCriteria[i] - '0';
		};
		e = b + 1;
	};

	for (devnr = b; devnr < e; devnr++)
	{
		if (devtab[devnr].hDriver) continue;
		if (drivertyp == GRIVERTYP_USBIO && !*devtab[devnr].drvname) continue;
		*devtab[devnr].DeviceIdString = 0;
		error= 0;
		if (drivertyp != GRIVERTYP_USBIO)
		{
			sprintf_s(drvname, sizeof(drvname),
				"\\\\.\\MITSUUSB%d",devnr); // Driver Name
			pdrvname = drvname;
		}
		else
		{
			pdrvname = devtab[devnr].drvname;
		}
		devtab[devnr].hDriver = CreateFile(
			pdrvname, // Driver Name
			GENERIC_READ | GENERIC_WRITE, // Access
			0,  // FILE_SHARE_READ | FILE_SHARE_WRITE, // Share mode
			NULL, // Security
			OPEN_EXISTING, // Open mode
			drivertyp == GRIVERTYP_USBIO ? FILE_FLAG_OVERLAPPED
			: FILE_FLAG_DELETE_ON_CLOSE,
			NULL );
		if (devtab[devnr].hDriver == INVALID_HANDLE_VALUE)
		{
			devtab[devnr].hDriver = 0;
			*devtab[devnr].drvname =  0;
			continue;
		}
		devtab[devnr].proc_nr = proc_nr;
		devtab[devnr].UsbioActualPipe = 0;
		devtab[devnr].UsbioUnplugged = 0;

		if (drivertyp == DRIVERTYP_UNKNOWN)
		{
			drivertyp = DRIVERTYP_MITSUUSB;
		}

		//Get Device Info
		bRtn = BrdDeviceIoControl(devnr,
			IOCTL_MITSU_GET_DEVICE_INFO, // Function
			NULL, 0, // No input buffer
			&devtab[devnr].DeviceInfo, sizeof( DEVICEINFO ), // Output buffer
			&dwBytesReturned, // # of bytes returned
			NULL ); // No overlapped object
		if ( !bRtn )
		{
			BrdCloseDevice(devnr);
			*devtab[devnr].drvname =  0;
			if (enumeration) continue;
			SetLastError(BRD_ERR_CAN_NOT_GET_DEVICE_INFO);
			TestLog(TEXT("Can't get device info!"));
			return -1;
		}
		if (testsw & BRD_TEST_LOG_DRIVER_INFO) BrdTestOutput(devnr, 'd');

		// Get Pipe information for this device
		PPIPEINFO pPipeInfo = (PPIPEINFO) &devtab[devnr].PipeInfo;
		bRtn = BrdDeviceIoControl(devnr,
			IOCTL_MITSU_GET_PIPE_INFO,
			NULL, 0, pPipeInfo, MAXPIPES*sizeof(PIPEINFO), &dwBytesReturned, NULL );
		if ( !bRtn )
		{
			error = 1;
			TestLog("\nerror for IOCTL_MITSU_GET_PIPE_INFO");
		}
		else if (testsw & BRD_TEST_LOG_DRIVER_INFO)
			BrdTestOutput(devnr, 'p'); // Pipeinfo

		//Set Current Pipe =0
		Sleep(30); // Notfall+Wait fuer korrektes Oeffnen der USB-Zeile
		bRtn = BindPipe(devnr, 0);
		if ( !bRtn )
		{
			error = 2;
		}

		// wegen  allfaelligen Leseproblemen  bei Fehler mehrfach  ausfuehren
		int loop, error2 = 0;
		for (loop = 0; loop < 3; loop++)
		{
			error2 = error;

			// Set up a vendor request: Start Bulk Read (Device ID String)
			vRD.bmRequestType = 0x40; // Vendor Request, write
			vRD.bRequest = 4; // Start Bulk read
			vRD.wIndex = 0;
			vRD.wValue = 0;
			vRD.wLength = 1;
			vRD.pBuff = bOutBuff;
			bOutBuff[0]=0; //read ID String
			bRtn = BrdDeviceIoControl(devnr,
				IOCTL_MITSU_SEND_VENDOR_REQUEST, // Function
				&vRD, // Input Data
				sizeof(VENDOR_REQUEST_DATA) , // Size of Input Data
				NULL,0, // Output buffer not needed
				&dwBytesReturned, // # bytes read
				NULL ); // No overlapped obj
			if ( !bRtn )
			{
				error2 = 3;
				TestLog(
					"\nerror for IOCTL_MITSU_SEND_VENDOR_REQUEST (start bulk read");
				TestLogVrd(&vRD);
			}

			char buf[1000];
			memset(buf,0,1000);
			// Sit and wait for BULK in packet DEVICE ID String
			bRtn = BrdDeviceIoControl(devnr,
				IOCTL_MITSU_BULK_READ,
				NULL, 0, // Not needed
				//devtab[devnr].DeviceIdString, DEV_ID_LEN,// Buffer to recv data&size
				buf, 1000,
				&dwBytesReturned, NULL ); // BULK bytes received
			if ( !bRtn)
			{
				error2 = 4;
				TestLog("\nerror for IOCTL_MITSU_BULK_READ DEVICE ID String");
				TestLogVrd(&vRD);
				devtab[devnr].DeviceIdString[0] = 0;
				TestLog("new loop:",loop+1,1);
				Sleep(30);
				continue;
			}
			//*****************************
			//TODO Dies hier ist nur eine Fehlersymptom behandlung
			//buf enthält manchmal den DeviceID-String mehrmals! Nur der
			//letzte Eintrag des Devicestrings ist korrekt!
			//*****************************
			char* found;
			while ( (found = strstr(buf+1,"Device: <"))!=NULL) {
				strncpy(buf,found,1000);
			}
			//*****************************

			strcpy((char*)devtab[devnr].DeviceIdString,buf);
			
			if (strlen((char*)devtab[devnr].DeviceIdString) < 80 // zu kurz
				|| strncmp((char*)devtab[devnr].DeviceIdString, "Device: <",9)
				|| strstr ((char*)devtab[devnr].DeviceIdString, "::vice:") != NULL
				)
			{
				error2 = 4;
				TestLog("illegal device-string",0,2);
				TestLog((char*)devtab[devnr].DeviceIdString);
				devtab[devnr].DeviceIdString[0] = 0;
				TestLog("new loop:",loop+1,1);
				Sleep(30);
				continue;
			}
			else
			{
				if (!strncmp((char*)devtab[devnr].DeviceIdString,
					"Device: <BrailleDis",19))
					devtab[devnr].devtype = BRD_TYPE_BRAILLEDIS;
				else devtab[devnr].devtype = BRD_TYPE_USB_LINE;
				int v;
				memset(devtab[devnr].vers,0,sizeof(devtab[devnr].vers));
				for (v = 0; v < DEV_ID_LEN; v++)
				{
					if (!devtab[devnr].DeviceIdString[v]) break;
					if (devtab[devnr].DeviceIdString[v] == 'V'
						&& !strncmp((char*)(devtab[devnr].DeviceIdString+v),
						"Vers:<",6))
					{
						strncpy_s((char*)devtab[devnr].vers,
							sizeof(devtab[devnr].vers),
							(char*)(devtab[devnr].DeviceIdString+v+6),5);
						break;
					}
				}
				if (testsw & BRD_TEST_LOG_DRIVER_INFO)
					BrdTestOutput(devnr, 'i'); // device id
				break;
			}

			// ende loop fuer device-string
		}
		error = error2;

		// select a specific device "<..."
		// or close the device while enumerating them
		if ((*lpszCriteria == '<'
			&& strncmp(lpszCriteria,
			((LPCSTR)  devtab[devnr].DeviceIdString)+8,
			strlen(lpszCriteria)))
			|| enumeration)
		{ // a wrong device or enumeration only
			int ts = testsw;
			testsw = 0; // no testoutput in close
			BrdCloseDevice(devnr);
			testsw = ts;
			continue;
		};

		// vendor Request Switch on high Voltage
		if (devtab[devnr].devtype == BRD_TYPE_USB_LINE)
		{
			vRD.bmRequestType = 0x40; // Vendor Request, Write
			vRD.bRequest = 0x01;
			vRD.wIndex = 0;
			vRD.wValue = 0;
			vRD.wLength = 0x01;
			vRD.pBuff = bOutBuff; // Pointer to data buffer
			bOutBuff[0]=0xef;
			bRtn = BrdDeviceIoControl(devnr,
				IOCTL_MITSU_SEND_VENDOR_REQUEST, // Function
				&vRD, // Input data
				sizeof(VENDOR_REQUEST_DATA) , // Input data size
				NULL, 0, // Not needed
				&dwBytesReturned, // # of bytes returned
				NULL ); // No overlapped obj
			if ( !bRtn )
			{
				error = 5;
				TestLog(
					"\nerror for IOCTL_MITSU_SEND_VENDOR_REQUEST high voltage");
				TestLogVrd(&vRD);
			}
		}

		if (error)
		{
			BrdCloseDevice(devnr);
			*devtab[devnr].drvname =  0;
			SetLastError(BRD_ERR_ERROR_WHILE_INITIALIZING);
			TestLog(TEXT("Error while Initializing!"));
			return -1;
		}

		// Change the number of bytes requested for each packet to be
		// 8 (because we know the device will give us 8-byte packets every
		// time an ISO packet is returned)
		if (devtab[devnr].devtype != BRD_TYPE_USB_LINE)
		{
			ISO_SETUP_DATA iso_setup_data;
			iso_setup_data.ulIsoReadPacketSize = 8;
			iso_setup_data.ulIsoWritePacketSize = 64;
			bRtn = BrdDeviceIoControl(devnr,
				IOCTL_MITSU_ISO_SETUP,
				&iso_setup_data, sizeof( ISO_SETUP_DATA ),
				NULL, 0, &dwBytesReturned, NULL );
			if (!bRtn)
			{
				BrdCloseDevice(devnr);
				*devtab[devnr].drvname =  0;
				SetLastError(BRD_ERR_ISO_SETUP);
				TestLog(TEXT("Error for ISO-SETUP!"));
				return -1;
			}
		};

		if (pdevtype) *pdevtype = devtab[devnr].devtype;
		if (testsw & BRD_TEST_LOG_CALLS)
		{
			TestLog("device-nr: ",devnr,3);
			TestLog("type: ",devtab[devnr].devtype,3);
			TestLog("vers:",0,2);
			TestLog((char*)devtab[devnr].vers);
		};
		devtab[devnr].lastPaket = 256;
		return devnr;
	};

	TestLog("No unused device found");
	SetLastError(BRD_NO_UNUSED_DEVICE_FOUND);
	return -1;
}

SUBR EXTERNC BOOL BRDISDECLSPEC BrdCloseDevice(int devnr)
{
    BOOL bRtn;

    SetLastError(0);
    if (testsw & BRD_TEST_LOG_CALLS)
    {
	TestLog("\nCloseDevice, p:",proc_nr,3);
	TestLog("d:",devnr,1);
    }
    if (!CheckDevnr(devnr)) return FALSE;
    if (!devtab[devnr].hDriver)
    {
	SetLastError(BRD_ERR_DEVICE_NOT_OPENED);
	TestLog("Device not opened");
	return FALSE;
    };

    VENDOR_REQUEST_DATA vRD;
    unsigned long dwBytesReturned;

		// unbind if necessary
		if (devtab[devnr].UsbioActualPipe)
		{
		    DWORD Status = UsbioIoctlSync(
		    devtab[devnr].hDriver,
		    IOCTL_USBIO_UNBIND_PIPE,
		    NULL,
		    0,
		    NULL,
		    0,
		    NULL
		    );
		    devtab[devnr].UsbioActualPipe = 0;
		    if ( Status != USBIO_ERR_SUCCESS )
		    {
			TestLog("Unable to unbind pipe, status=. 0x", Status,5);
		    }
		}

    // Data to send to device
    UCHAR bBuff[ 7 ] = { 0x0, 0xff, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

    // Set up a vendor request: low voltage
	if (devtab[devnr].devtype == BRD_TYPE_USB_LINE)
	{
	vRD.bmRequestType = 0x40; // Vendor Request, Write
	vRD.bRequest = 0x01; // CHECK_ID command
	vRD.wIndex = 0;
	vRD.wValue = 0;
	vRD.wLength = 0x01; // Data length (must match buffer size)
	vRD.pBuff = bBuff; // Pointer to data buffer
	bRtn = BrdDeviceIoControl(devnr,
	IOCTL_MITSU_SEND_VENDOR_REQUEST, // Function
	&vRD, // Input data
	sizeof(VENDOR_REQUEST_DATA) , // Input data size
	NULL, 0, // Not needed
	&dwBytesReturned, // # of bytes returned
	NULL ); // No overlapped obj
    }
    else
    {
	unsigned char data[8];
	data[0] = 2;
	memset(data+1,0,7);
	vRD.bmRequestType = 0x40; // Vendor Request, Write
	vRD.bRequest = 0x03;
	vRD.wIndex = 0;
	vRD.wValue = 0;
	vRD.wLength = 8;
	vRD.pBuff = (PUCHAR )(data); // Pointer to data buffer
	bRtn = BrdDeviceIoControl(devnr,
	IOCTL_MITSU_SEND_VENDOR_REQUEST, // Function
	&vRD,sizeof(VENDOR_REQUEST_DATA), NULL, 0,&dwBytesReturned,NULL );
    }
    if (!bRtn)
    {
	TestLog(
	    "\nerror for IOCTL_MITSU_SEND_VENDOR_REQUEST cancel device");
	TestLogVrd(&vRD);
    };

    if (drivertyp == GRIVERTYP_USBIO)
    {
	UsbioIoctlSync(
	    devtab[devnr].hDriver,
	IOCTL_USBIO_UNCONFIGURE_DEVICE,
	NULL,
	0,
	NULL,
	0,
	NULL
	);
    }

    CloseHandle(devtab[devnr].hDriver);
    devtab[devnr].hDriver = 0;
    devtab[devnr].devtype = 0;
    devtab[devnr].proc_nr = 0;
    return TRUE;
}

SUBR EXTERNC int BRDISDECLSPEC BrdReadData(
    int devnr,
    int len, // buffer lengnth
    unsigned char *buf)
{
	BOOL bRtn;
    VENDOR_REQUEST_DATA vRD;
    ULONG dwBytesReturned = 0;
    unsigned char *pBuf = buf;
    int len2 = len;

    SetLastError(0);
    memset(buf,0,len);
    if (testsw & BRD_TEST_LOG_CALLS)
    {
	TestLog("\nReadData, p:",proc_nr,3);
	TestLog("d:",devnr,3);
	TestLog("len:",len,1);
    }
    if (!CheckDevnr(devnr)) return -1;
    if (!devtab[devnr].hDriver)
    {
	SetLastError(BRD_ERR_DEVICE_NOT_OPENED);
	TestLog("read-error: Device not opened");
	return -1;
    };

      //read Cursorposition of METEC USB line
      if (devtab[devnr].devtype == BRD_TYPE_USB_LINE)
      {
	if (len < 8)
	{
	    SetLastError(BRD_ERR_ILLEGAL_LENGTH);
	    TestLog("length < 8; only:",len,1);
	    return -1;
	};
	  memset(buf,0,8);
	vRD.bmRequestType = 0xC0; // Vendor Request, read
	vRD.bRequest = 0x80; // MEM_READ
	vRD.wIndex = 0; // Start Address
	vRD.wValue = 0;
	vRD.wLength = 0;
	vRD.pBuff = NULL;
	bRtn = BrdDeviceIoControl(devnr,
	IOCTL_MITSU_SEND_VENDOR_REQUEST, // Function
	&vRD, // Input Data
	sizeof(VENDOR_REQUEST_DATA) , // Size of Input Data
	buf, // inplt buffer
	8, // Size of data to read
	&dwBytesReturned, // # bytes read
	NULL ); // No overlapped obj
	if ( !bRtn )
	{
	    TestLog("\nerror for IOCTL_MITSU_SEND_VENDOR_REQUEST (read data)");
	    TestLogVrd(&vRD);
	    return-1;
	}
    }

    // read BRAILLEDIS
    else
    {
	if (len < 733) // minimum for sensors
	{
	    SetLastError(BRD_ERR_ILLEGAL_LENGTH);
	    TestLog("read-error: length < 733; only:",len,1);
	    return len;
	};
	UCHAR bOutBuff[8]={0,0,0,0,0,0,0,0};
	//rec Bulk Data
	// Set up a vendor request: Start Bulk Read ( Touch Data)
	vRD.bmRequestType = 0x40; // Vendor Request, write
	vRD.bRequest = 10;        // Start Bulk read Touch Data
	vRD.wIndex = 0;
	vRD.wValue = 0;
	vRD.wLength = 1;
	vRD.pBuff = bOutBuff;
	bOutBuff[0]=1;          // 0 = old, 1 = new protocol
	if (!lock(devnr))
	{
	    TestLog("error read lock failed");
	    return 0;
	}
	bRtn = BrdDeviceIoControl(devnr,
	IOCTL_MITSU_SEND_VENDOR_REQUEST, // Function
	&vRD, // Input Data
	sizeof(VENDOR_REQUEST_DATA) , // Size of Input Data
	NULL,0, // Output buffer not needed
	&dwBytesReturned, // # bytes read
	NULL ); // No overlapped obj
	if ( !bRtn )
	{
	    unlock(devnr);
	    TestLog(
		"\nread-error for IOCTL_MITSU_SEND_VENDOR_REQUEST (read data)");
	    TestLogVrd(&vRD);
	    if (devtab[devnr].eof) return -1;
	    return 0;
	}
	bRtn = BindPipe(devnr, 0);
	if ( !bRtn )
	{
	    unlock(devnr);
	    TestLog("read-error for Bulk Read");
	    return 0;
	}
	// convert input from 1440 to 720 sensors?
	int convert = 0;
	if (!strncmp((char*)devtab[devnr].vers,"02",2) && len < 1488)
	{
	    convert = 1;
	    len2 = 1488;
	    pBuf = new unsigned char[1488];
	}
	if (strncmp((char*)devtab[devnr].vers,"02",2) && len >= 1488)
	{
	    convert = 2;
	    len2 = 733;
	    pBuf = new unsigned char[733];
	}
	// Sit and wait for BULK in packet
	bRtn = BrdDeviceIoControl(devnr,
	IOCTL_MITSU_BULK_READ,
	NULL, 0, // Not needed
	pBuf, len2,             // Buffer to recv data & size
	&dwBytesReturned, NULL );       // BULK bytes received
	// the lock was unlocked in the   ReadFile
	if ( !bRtn )
	{
	    TestLog(
		"\nread-error for IOCTL_MITSU_SEND_VENDOR_REQUEST (read data)");
	    if (convert) delete [] pBuf;
	    return 0;
	}
	// convert 1440 to 720 sensors
	if (convert == 1)
	{
	    if (dwBytesReturned != 1488)
	    {
		delete [] pBuf;
		char scr[60];
		sprintf_s(scr,sizeof(scr),"r=%d 1488 bytes required",
		    dwBytesReturned);
		 TestLog(scr);
		return dwBytesReturned;
	    }
	    int l, b;
	    unsigned char *pb = pBuf+8;
	    /* test for dummyy-devices
	    memset(pBuf+8,0,1440);
	    pBuf[8] = 0xffu; pBuf[68] = 0xffu;
	    pBuf[9] = 0xf0u; pBuf[69] = 0xf0u;
	    pBuf[10] = 0x8fu; pBuf[70] = 0x8fu;
	    pBuf[11] = 0x70u; pBuf[71] = 0x70u;
	    pBuf[12] = 0x80u; pBuf[72] = 0x08u;
	    pBuf[1387] = 0x80u; pBuf[1447] = 0x08u;
	    pBuf[1448] = 0x01u; pBuf[1449] = 0x02u;
	    pBuf[1456] = 0x03u; pBuf[1457] = 0x04u;
	    // ende test */
	    for (l = 0; l < 720; l += 60)
	    {
		pb = pBuf + 2 * l + 8;
		for (b = 0; b < 60; b++, pb++)
		{
		    buf[l+b] = (pb[0] + pb[60]) / 2; // durchschnitt
		}
	    }
	    memcpy(buf+720,pBuf+1,3);
	    pb = pBuf + 1448;
	    for (b = 723; b < 733; b += 2, pb += 8)
	    {
		buf[b] = pb[0];
		buf[b+1] = pb[1];
	    }
	    delete [] pBuf;
	    dwBytesReturned = 733;
	}
	// convert 720 to 1440 sensors
	if (convert == 2)
	{
	    if (dwBytesReturned != 733)
	    {
		delete [] pBuf;
		char scr[60];
		sprintf_s(scr,sizeof(scr),"r=%d 733 bytes required",
		    dwBytesReturned);
		TestLog(scr);
		return dwBytesReturned;
	    }
	    int l, b;
	    unsigned char *pb;
	    /* test for dummyy-devices
	    memset(pBuf,0,720);
	    for (b = 0; b < 256; b++)
	    {
		pBuf[b] = (unsigned char) b;
		pBuf[660] = 0x01;
		pBuf[689] = 0x02;
		pBuf[690] = 0x03;
		pBuf[719] = 0x04;
	    }
	    // pBuf[720..722] are unchanged
	    memcpy(pBuf+723,"1234567890",10);
	    // ende test */
	    buf[0] = 0; // structure-nr
	    memcpy(buf+1,pBuf+720,3); // paket-nr + timer
	    *((USHORT*) (buf+4)) = 1488; // length
	    memset(buf+6,0,2); // reserve
	    for (l = 0; l < 720; l += 60)
	    {
		pb = buf + 2 * l + 8;
		for (b = 0; b < 60; b++, pb++)
		{
		    pb[0] = pb[60] = pBuf[l+b];
		}
	    }
	    memset(buf+1448,0,40); // keyboard
	    pb = buf + 1448;
	    for (b = 723; b < 733; b += 2, pb += 8)
	    {
		pb[0] = pBuf[b];
		pb[1] = pBuf[b+1];
		/* ausser betrieb
		pb[1] = pBuf[b+1] & 0xcf; // change bit 4 & 5
		if (pBuf[b+1] & 0x10) pb[1]  |= 0x20;
		if (pBuf[b+1] & 0x20) pb[1]  |= 0x10;
		ausser betrieb */
	    }
	    delete [] pBuf;
	    dwBytesReturned = 1488;
	}
    }

    if (testsw)
    {
	char scr[84];
	int t = GetTickCount();
	int newPaket;
	if (dwBytesReturned == 733) newPaket = buf[720];
	else if (dwBytesReturned == 1488) newPaket = buf[1];
	else newPaket = 256;
	if (devtab[devnr].lastPaket == 256) ;
	else if (newPaket == 256) ;
	else if (devtab[devnr].lastPaket == newPaket)
	{
	    sprintf_s(scr,sizeof(scr),"equal packet: %02x tick=%d ms",
	    newPaket,t % 100000);
	    TestLog(scr);
	}
	else if (devtab[devnr].lastPaket == 255 && newPaket == 0) ;
	else if (devtab[devnr].lastPaket + 1 == newPaket) ;
	else
	{
	    sprintf_s(scr,sizeof(scr),"missing packet: %02x %02x tick=%d ms",
	    devtab[devnr].lastPaket,newPaket,t % 100000);
	    TestLog(scr);
	}
	devtab[devnr].lastPaket = newPaket;
	if (testsw & BRD_TEST_LOG_DRIVER_INFO)
	{
	    sprintf_s(scr,sizeof(scr),"r=%d tick=%d",dwBytesReturned,t % 1000);
	    TestLog(scr);
	    if (dwBytesReturned < 30)
	    {
		TestHexa(buf,0,dwBytesReturned);
	    }
	    else if (dwBytesReturned < 1488)
	    {
		TestHexa(buf,0,30);
		TestHexa(buf+30,30,30);
		TestHexa(buf+60,60,30);
		TestHexa(buf+690,690,30);
		TestHexa(buf+720,720,13);
	    }
	    else
	    {
		TestHexa(buf,0,8);
		TestHexa(buf+8,8,30);
		TestHexa(buf+38,38,30);
		TestHexa(buf+68,68,30);
		TestHexa(buf+1418,1418,30);
		TestHexa(buf+1448,1448,24);
		TestHexa(buf+1472,1472,16);
	    }
	};
    };

    return (int) dwBytesReturned;
}

SUBR EXTERNC BOOL BRDISDECLSPEC BrdWriteData(
    int devnr,
    int len, // length in bytes
    unsigned char *data)
{
    int i, l;
    BOOL bRtn;
    VENDOR_REQUEST_DATA vRD;
    DWORD dwBytesReturned = 0;

    SetLastError(0);
    if (testsw & BRD_TEST_LOG_CALLS)
    {
	TestLog("\nWriteData, p:",proc_nr,3);
	TestLog("d:",devnr,3);
	TestLog("len:",len,1);
    }
    if (!CheckDevnr(devnr)) return FALSE;
    if (!devtab[devnr].hDriver)
    {
	SetLastError(BRD_ERR_DEVICE_NOT_OPENED);
	TestLog("write-error: Device not opened");
	return FALSE;
    };

    if ((testsw & BRD_TEST_LOG_DATA) && len)
    {
	int s = 0;
	char scr[84];
	for  (i = 0; i < len && s < 64; i++)
	{
	    sprintf_s(scr+s,sizeof(scr)-s,"%02x",data[i] & 0xff);
	    s  += 2;
	}
	TestLog(scr);
    };

    // write to METEC-USB-LINE
    if (devtab[devnr].devtype == BRD_TYPE_USB_LINE)
    {
	for ( i = 0, l = 8; i < len; i += 8)
	{
	    if (i + l > len) l = len - i;
	    vRD.bmRequestType = 0x40; // Vendor Request, Write
	    vRD.bRequest = (unsigned char)(i/8+10); // send braille pattern
	    vRD.wIndex = 0;
	    vRD.wValue = 0;
	    vRD.wLength = (CHAR)l; // Data length (must match buffer size)
	    vRD.pBuff = (PUCHAR )(data+i); // Pointer to data buffer
	    bRtn = BrdDeviceIoControl(devnr,
	    IOCTL_MITSU_SEND_VENDOR_REQUEST, // Function
	    &vRD,sizeof(VENDOR_REQUEST_DATA), NULL, 0,&dwBytesReturned,NULL );
	    if (!bRtn)
	    {
		SetLastError( BRD_ERR_VENDOR_REQUEST_FAILED);
		TestLog("Vendor Request error at byte:",i,1);
		TestLogVrd(&vRD);
		return FALSE;
	    };
	}
    }

    // write to BRAILLEDIS
    else
    {
	UCHAR bOutBuff[8]={0,0,0,0,0,0,0,0};
	// Vendor Req 0x80 Get Status; sets Wr Index to 0
	vRD.bmRequestType = 0xC0; // Vendor Request, read
	vRD.bRequest = 0x80; // MEM_READ
	vRD.wIndex = 0; // Start Address
	vRD.wValue = 0;
	vRD.wLength = 0;
	vRD.pBuff = NULL;
	if (!lock(devnr))
	{
	    TestLog("error write lock failed");
	    return 0;
	}
	bRtn = BrdDeviceIoControl(devnr,
	IOCTL_MITSU_SEND_VENDOR_REQUEST, // Function
	&vRD, // Input Data
	sizeof(VENDOR_REQUEST_DATA) , // Size of Input Data
	bOutBuff, // Output buffer
	8, // Size of data to read
	&dwBytesReturned, // # bytes read
	NULL ); // No overlapped obj
	if (!bRtn )
	  {
	    unlock(devnr);
	    TestLog("error  in vendor-request for write");
	    return FALSE;
	  }
	// Send the data out the bulk out pipe
	bRtn = BindPipe(devnr, 1);
	if (!bRtn )
	  {
	    unlock(devnr);
	    TestLog("Error for Set Pipe in write");
	    return FALSE;
	  }
	bRtn = BrdDeviceIoControl(devnr,
	IOCTL_MITSU_BULK_WRITE,NULL, 0,
	data, len, // Buffer with data & size
	&dwBytesReturned, NULL ); //bytes written
	// the lock was unlocked immediately after the WriteFile
	if (!bRtn)
	{
	    SetLastError( BRD_ERR_VENDOR_REQUEST_FAILED);
	    TestLog("Vendor Request 2 error in write");
	    TestLogVrd(&vRD);
	    return FALSE;
	};
    };

    return TRUE;
}

SUBR EXTERNC BOOL BRDISDECLSPEC BrdSetNoOfModules(
	int devnr, unsigned char number)
//Set New Value for the number of Modules
{
    unsigned char Buff=number;
    VENDOR_REQUEST_DATA vRD;
    DWORD dwBytesReturned = 0;

    SetLastError(0);
    if (testsw & BRD_TEST_LOG_CALLS)
    {
	TestLog("\nSetNoOfModules, p:",proc_nr,3);
	TestLog("d:",devnr,3);
	TestLog("number",number,1);
    }
    if (!CheckDevnr(devnr)) return FALSE;
    if (!devtab[devnr].hDriver)
    {
	SetLastError(BRD_ERR_DEVICE_NOT_OPENED);
	TestLog("Device not opened");
	return FALSE;
    };
    vRD.bmRequestType = 0x40; // Vendor Request, Write
    vRD.bRequest = 2; // CHECK_ID command
    vRD.wIndex = 0;
    vRD.wValue = 0;
    vRD.wLength = 1; // Data length (must match buffer size)
    vRD.pBuff = &Buff; // Pointer to data buffer
    BOOL bRtn = BrdDeviceIoControl(devnr,
    IOCTL_MITSU_SEND_VENDOR_REQUEST, // Function
    &vRD, sizeof(VENDOR_REQUEST_DATA),NULL, 0,&dwBytesReturned,NULL );
    if (!bRtn)
    {
	SetLastError( BRD_ERR_VENDOR_REQUEST_FAILED);
	TestLog("Vendor Request error");
	TestLogVrd(&vRD);
    };
    return bRtn;
}

SUBR EXTERNC BOOL BRDISDECLSPEC BrdCommand(
    int devnr, // device-number
    unsigned char data[8]) // data to be sent (allways 8 bytes!)
// return-code: true or false
// call GetLastError() for error reasoon
{
    int i;
    BOOL bRtn;
    VENDOR_REQUEST_DATA vRD;
    DWORD dwBytesReturned = 0;

    SetLastError(0);
    if (testsw & BRD_TEST_LOG_CALLS)
    {
	TestLog("\nCommand, p:",proc_nr,3);
	TestLog("d:",devnr,1);
    }
    if (!CheckDevnr(devnr)) return FALSE;
    if (!devtab[devnr].hDriver)
    {
	SetLastError(BRD_ERR_DEVICE_NOT_OPENED);
	TestLog("Device not opened");
	return FALSE;
    };

    if (testsw & BRD_TEST_LOG_DATA)
    {
	int s = 0;
	char scr[84];
	for  (i = 0; i < 8; i++)
	{
	    sprintf_s(scr+s,sizeof(scr)-s,"%02x",data[i] & 0xff);
	    s  += 2;
	}
	TestLog(scr);
    };

    // METEC-USB-LINE not allowed
    if (devtab[devnr].devtype == BRD_TYPE_USB_LINE)
    {
	SetLastError(BRD_ERR_NOT_ALLOWED_FOR_USB_LINE);
	TestLog("not allowed for USB-line");
	return FALSE;
    }

    // brailledis
    else
    {
	vRD.bmRequestType = 0x40; // Vendor Request, Write
	vRD.bRequest = 0x03;
	vRD.wIndex = 0;
	vRD.wValue = 0;
	vRD.wLength = 8;
	vRD.pBuff = (PUCHAR )(data); // Pointer to data buffer
	bRtn = BrdDeviceIoControl(devnr,
	IOCTL_MITSU_SEND_VENDOR_REQUEST, // Function
	&vRD,sizeof(VENDOR_REQUEST_DATA), NULL, 0,&dwBytesReturned,NULL );
	if (!bRtn)
	{
	    SetLastError( BRD_ERR_VENDOR_REQUEST_FAILED);
	    TestLog("Vendor Request error:");
	    TestLogVrd(&vRD);
	    return FALSE;
	};
    }

    return TRUE;
}

SUBR EXTERNC BOOL BRDISDECLSPEC BrdHighVoltage(int devnr, int state)
    // state 0 = power off, 1 = power on
{
    BOOL bRtn;

    SetLastError(0);
    if (testsw & BRD_TEST_LOG_CALLS)
    {
	TestLog("\nPower, p:",proc_nr,3);
	TestLog("d:",devnr,3);
	TestLog("state",state,1);
    }
    if (!CheckDevnr(devnr)) return FALSE;
    if (!devtab[devnr].hDriver)
    {
	SetLastError(BRD_ERR_DEVICE_NOT_OPENED);
	TestLog("Device not opened");
	return FALSE;
    };

    VENDOR_REQUEST_DATA vRD;
    unsigned long dwBytesReturned;

    // Data to send to device
    UCHAR bBuff[ 7 ] = { 0x0, 0xff, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

    // Set up a vendor request: high or low voltage
    if (devtab[devnr].devtype == BRD_TYPE_USB_LINE)
    {
	    if (state) bBuff[0] = 0xef; // high voltage
	vRD.bmRequestType = 0x40; // Vendor Request, Write
	vRD.bRequest = 0x01; // CHECK_ID command
	vRD.wIndex = 0;
	vRD.wValue = 0;
	vRD.wLength = 0x01; // Data length (must match buffer size)
	vRD.pBuff = bBuff; // Pointer to data buffer
	bRtn = BrdDeviceIoControl(devnr,
	IOCTL_MITSU_SEND_VENDOR_REQUEST, // Function
	&vRD, // Input data
	sizeof(VENDOR_REQUEST_DATA) , // Input data size
	NULL, 0, // Not needed
	&dwBytesReturned, // # of bytes returned
	NULL ); // No overlapped obj
    }
    else
    {
	unsigned char data[8];
	data[0] = 2;
	data[1] = state  ? 1 : 0;
	memset(data+2,0,6);
	vRD.bmRequestType = 0x40; // Vendor Request, Write
	vRD.bRequest = 0x03;
	vRD.wIndex = 0;
	vRD.wValue = 0;
	vRD.wLength = 8;
	vRD.pBuff = (PUCHAR )(data); // Pointer to data buffer
	bRtn = BrdDeviceIoControl(devnr,
	IOCTL_MITSU_SEND_VENDOR_REQUEST, // Function
	&vRD,sizeof(VENDOR_REQUEST_DATA), NULL, 0,&dwBytesReturned,NULL );
    }
    if (!bRtn)
    {
	TestLog(
	    "\nerror for IOCTL_MITSU_SEND_VENDOR_REQUEST cancel device");
	TestLogVrd(&vRD);
    };

    return TRUE;
}

