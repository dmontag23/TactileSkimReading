/////////////////////////////////////////////////////////////////////
//
//	mitsuusb.h - main include file
//
//	Author: Dennis Merrill
//
//	Creation Date: 1/26/2001
//
/////////////////////////////////////////////////////////////////////
#ifndef __MITSUUSB_H
#define __MITSUUSB_H

#ifndef DRIVER
#include <winioctl.h>
#endif

/////////////////////////////
// Custom IOCTL Definitions
/////////////////////////////
#define IOCTL_MITSU_BULK_READ			CTL_CODE( FILE_DEVICE_UNKNOWN, 0x800, METHOD_IN_DIRECT,	FILE_ANY_ACCESS )
#define IOCTL_MITSU_GET_DEVICE_INFO		CTL_CODE( FILE_DEVICE_UNKNOWN, 0x801, METHOD_BUFFERED,	FILE_ANY_ACCESS )
#define IOCTL_MITSU_SEND_VENDOR_REQUEST	CTL_CODE( FILE_DEVICE_UNKNOWN, 0x802, METHOD_BUFFERED,	FILE_ANY_ACCESS )
#define IOCTL_MITSU_INTERRUPT_READ		CTL_CODE( FILE_DEVICE_UNKNOWN, 0x803, METHOD_IN_DIRECT,	FILE_ANY_ACCESS )
#define IOCTL_MITSU_BULK_WRITE			CTL_CODE( FILE_DEVICE_UNKNOWN, 0x804, METHOD_OUT_DIRECT,FILE_ANY_ACCESS )
#define IOCTL_MITSU_GET_PIPE_INFO		CTL_CODE( FILE_DEVICE_UNKNOWN, 0x805, METHOD_BUFFERED,	FILE_ANY_ACCESS )
#define IOCTL_SET_CURRENT_PIPE			CTL_CODE( FILE_DEVICE_UNKNOWN, 0x806, METHOD_BUFFERED,	FILE_ANY_ACCESS )
#define IOCTL_MITSU_SET_DEVICE_INFO		CTL_CODE( FILE_DEVICE_UNKNOWN, 0x807, METHOD_BUFFERED,	FILE_ANY_ACCESS )
#define IOCTL_MITSU_ISO_WRITE			CTL_CODE( FILE_DEVICE_UNKNOWN, 0x808, METHOD_OUT_DIRECT,FILE_ANY_ACCESS )
#define IOCTL_MITSU_ISO_READ			CTL_CODE( FILE_DEVICE_UNKNOWN, 0x809, METHOD_IN_DIRECT,	FILE_ANY_ACCESS )
#define IOCTL_MITSU_ISO_SETUP			CTL_CODE( FILE_DEVICE_UNKNOWN, 0x810, METHOD_BUFFERED,	FILE_ANY_ACCESS )
#define IOCTL_MITSU_START_ISO_IN_STREAM	CTL_CODE( FILE_DEVICE_UNKNOWN, 0x811, METHOD_BUFFERED,	FILE_ANY_ACCESS )
#define IOCTL_MITSU_STOP_ISO_IN_STREAM	CTL_CODE( FILE_DEVICE_UNKNOWN, 0x812, METHOD_BUFFERED,	FILE_ANY_ACCESS )
#define IOCTL_MITSU_GET_ISO_IN_BUFFER	CTL_CODE( FILE_DEVICE_UNKNOWN, 0x812, METHOD_OUT_DIRECT,FILE_ANY_ACCESS )

#define IOCTL_MITSU_SEND_FEATURE_REQUEST		CTL_CODE( FILE_DEVICE_UNKNOWN, 0x813, METHOD_BUFFERED,	FILE_ANY_ACCESS )
#define IOCTL_MITSU_GET_STRING_DESCRIPTOR		CTL_CODE( FILE_DEVICE_UNKNOWN, 0x814, METHOD_BUFFERED,	FILE_ANY_ACCESS )
#define IOCTL_MITSU_SEND_GET_STATUS_REQUEST		CTL_CODE( FILE_DEVICE_UNKNOWN, 0x815, METHOD_BUFFERED,	FILE_ANY_ACCESS )
#define IOCTL_MITSU_GET_CONFIGURATIONS			CTL_CODE( FILE_DEVICE_UNKNOWN, 0x818, METHOD_BUFFERED,	FILE_ANY_ACCESS )
#define IOCTL_MITSU_GET_INTERFACES				CTL_CODE( FILE_DEVICE_UNKNOWN, 0x819, METHOD_BUFFERED,	FILE_ANY_ACCESS )
#define IOCTL_MITSU_SET_INTERFACE				CTL_CODE( FILE_DEVICE_UNKNOWN, 0x820, METHOD_BUFFERED,	FILE_ANY_ACCESS )

/////////////////////////////
// Shared data structures
/////////////////////////////

typedef struct _ISO_SETUP_DATA {
	ULONG ulIsoReadPacketSize;
	ULONG ulIsoWritePacketSize;
} ISO_SETUP_DATA, *PISO_SETUP_DATA;

typedef struct _DEVICEINFO {
	UCHAR	nNumConfigurations;		// # of configurations in device (Get only)
	USHORT	wVendorID;				// Vendor ID
	USHORT	wProductID;				// Product ID
	UCHAR	bNumPipes;				// Number of pipes in default interface of default config
	UCHAR	bNumInterfaces;			// Number of interfaces in default config
	BOOLEAN bSurpriseRemoval;		// If TRUE, surprise removal is OK (Win2k only)
} DEVICEINFO, *PDEVICEINFO;

typedef struct _VENDOR_REQUEST_DATA {
	UCHAR	bmRequestType;	// must be 0x40 or 0xC0 (write/read)
	UCHAR	bRequest;		// user supplied vendor code
	USHORT	wValue;			// user supplied value
	USHORT	wIndex;
	USHORT	wLength;		// Length of following buffer
	PUCHAR	pBuff;			// Buffer (length  of wLength)
} VENDOR_REQUEST_DATA, *PVENDOR_REQUEST_DATA;

// These are used for bDescriptorType field of GET_DESCRIPTOR_REQUEST_DATA
#define USB_DEVICE_DESCRIPTOR_TYPE			0x01
#define USB_CONFIGURATION_DESCRIPTOR_TYPE	0x02
#define USB_STRING_DESCRIPTOR_TYPE			0x03

typedef struct _GET_STRING_DESCRIPTOR_DATA {
	UCHAR		bIndex;
	USHORT		wLanguageId;
} GET_STRING_DESCRIPTOR_DATA, *PGET_STRING_DESCRIPTOR_DATA;

// These are used for wOp field of FEATURE_REQUEST_DATA
#define SET_FEATURE_TO_DEVICE          0x000D
#define SET_FEATURE_TO_INTERFACE       0x000E
#define SET_FEATURE_TO_ENDPOINT        0x000F
#define SET_FEATURE_TO_OTHER           0x0023
#define CLEAR_FEATURE_TO_DEVICE        0x0010
#define CLEAR_FEATURE_TO_INTERFACE     0x0011
#define CLEAR_FEATURE_TO_ENDPOINT      0x0012
#define CLEAR_FEATURE_TO_OTHER         0x0022

typedef struct _FEATURE_REQUEST_DATA {
	USHORT		wOp;
	USHORT		wFeatureSelector;
	USHORT		wIndex;
} FEATURE_REQUEST_DATA, *PFEATURE_REQUEST_DATA;

// These are used for wOp field of STATUS_REQUEST_DATA
#define GET_STATUS_FROM_DEVICE         0x0013
#define GET_STATUS_FROM_INTERFACE      0x0014
#define GET_STATUS_FROM_ENDPOINT       0x0015
#define GET_STATUS_FROM_OTHER          0x0021

typedef struct _STATUS_REQUEST_DATA {
	USHORT		wOp;
	USHORT		wIndex;
} STATUS_REQUEST_DATA, *PSTATUS_REQUEST_DATA;

typedef struct _SET_CONFIGURATION_DATA {
	// TODO: figure this one out
} SET_CONFIGURATION_DATA, *PSET_CONFIGURATION_DATA;

typedef struct _SET_INTERFACE_DATA {
	// TODO: figure this one out
} SET_INTERFACE_DATA, *PSET_INTERFACE_DATA;

typedef struct _ISOBUFFER {
	PUCHAR	pBuff;			// pointer to the begining of the memory buffer
	PUCHAR	pHead;			// Where data gets inserted
	PUCHAR	pTail;			// Where data gets read from
	PUCHAR	pEnd;			// Where linear buffer ends
	LONG	lSize;
} ISOBUFFER, *PISOBUFFER;

enum PIPETYPE {
	BULK_IN,
	BULK_OUT,
	INTERRUPT_IN,
	ISOCHRONOUS_IN,
	ISOCHRONOUS_OUT,
	CONTROL,
	UNKNOWN
};

typedef struct _PIPEINFO {
	PVOID		hPipeHandle;		// Handle used for tranasactions
	PIPETYPE	PipeType;			// Type of pipe (bulk in, interrupt in, etc)
	UCHAR		bEndpointAddress;	// Address of device endpoint for this pipe
        PVOID           Gap;    //Unknown ###
} PIPEINFO, *PPIPEINFO;

typedef struct _CONFIGURATIONINFO {
	UCHAR		bNumInterfaces;
	UCHAR		iConfiguration;
	UCHAR		bmAttributes;
	UCHAR		MaxPower;
} CONFIGURATIONINFO, *PCONFIGURATIONINFO;

typedef struct _USBINTERFACEINFO {
	UCHAR		bInterfaceNumber;
	UCHAR		bAlternateSetting;
	UCHAR		bNumEndpoints;
	UCHAR		bInterfaceClass;
	UCHAR		bInterfaceSubClass;
	UCHAR		bInterfaceProtocol;
	UCHAR		iInterface;
} USBINTERFACEINFO, *PUSBINTERFACEINFO;

typedef struct _ISOINFO {
	ULONG	ulPipeIndex;
	ULONG	ulPacketSize;
} ISOINFO, *PISOINFO;

#ifdef DRIVER

///////////
// Macros
///////////
#define USB_Print(a) KdPrint(( "MITUUSB - " BUILD_NUM " - " a ));

//////////////
// Constants
//////////////
#define BUILD_NUM "33"

#define MEUS_VID 0x0452
#define ME_VID 0x06D3

#define MAX_NUM_PIPES			32				// USB Spec says max of 16 in and 16 out = 32
#define MAX_TRANSFER_SIZE		PAGE_SIZE
// #define ISO_BUFFER_SIZE			4 * PAGE_SIZE	// 4K * 4 = 16KB (typically)
#define ISO_BUFFER_SIZE			2 * PAGE_SIZE	// 4K * 4 = 16KB (typically)
#define DEFAULT_ISO_PACKET_SIZE	10
#define ISO_NUM_URBS			3				// Number of URB's to cycle thru in buffered mode
#define MAX_CONFIGS				10

enum DevState {
	DEVICE_NOSTATE,
	DEVICE_STARTED,
	DEVICE_PENDING_STOP,
	DEVICE_STOPPED,
	DEVICE_PENDING_REMOVE,
	DEVICE_REMOVED
};

//
// A structure representing the instance information associated with
// this particular device.
//
typedef struct _DEVICE_EXTENSION 
{
	///////////////////////////
	IO_REMOVE_LOCK IoRemoveLock;

	ULONG ulNumPipes;					// Current number of available pipes

	ULONG ulCurrentPipe;				// Index to the current pipe

	PUSBD_PIPE_INFORMATION pPipeInfo;	// Dynamically allocated array of pipes

	UNICODE_STRING sDeviceLinkName;		// Symbolic link name
    
	PDEVICE_OBJECT pPhysicalDeviceObject;	// Pointer to the bus driver object

    PDEVICE_OBJECT pNextDeviceObject;	// Pointer to the device we call when submitting URB's (USB 
										// Request Blocks)

	LIST_ENTRY PendingIoctlList;		// list of asynchronous IOCTLs

	PDEVICE_OBJECT pDeviceObject;		// pointer back to this device's device object
	LONG lNumHandles;					// Number of time this driver had been opened

	PUSB_CONFIGURATION_DESCRIPTOR pConfigDescriptor;	// configuration descriptor
	USBD_CONFIGURATION_HANDLE hConfig;	// selected configuration handle

	DEVICE_CAPABILITIES DeviceCapabilities;	// Copy
	PNP_DEVICE_STATE PnpStateMask;		// used to mask out PnP events

	DEVICEINFO DeviceInfo;				// Saved select device information

	LONG lTransfersPending;				// Num of transfers pending

	HANDLE hPipeToReset;

	// Staged read/write queues,etc
	LIST_ENTRY QueueHead;
	KSPIN_LOCK kQueueLock;
	PIRP pCurrentIrp;
	LONG lTransferLeft;
	LONG lBlockSize;
	LONG lTotalTransfer;
	USBD_PIPE_HANDLE hCurrentPipe;
	USBD_PIPE_HANDLE hPipeError;
	PMDL pCurrentMdl;
	PURB pCurrentUrb;
	PUCHAR pCurrentBuff;

	DevState DeviceState;				// State of PnP state machine

	ULONG ulIsoReadPacketSize;
	ULONG ulIsoWritePacketSize;

	ISOINFO IsoInInfo;					// Isochronous transfer setup info
	ISOINFO IsoOutInfo;					// Isochronous transfer setup info
	
	ISOBUFFER IsoInBuff;				// Isochronous input buffer
	ISOBUFFER IsoOutBuff;				// Isochronous output buffer

	HANDLE hThread;		// The iso streaming handle
	KEVENT evKillThread;
	BOOLEAN bIsoThreadRunning;

	PCONFIGURATIONINFO pConfig[ MAX_CONFIGS ];

} DEVICE_EXTENSION, *PDEVICE_EXTENSION;

#endif	// #ifdef DRIVER

#endif
