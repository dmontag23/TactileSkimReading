/* Disable ridiculous warnings so that the code */
/* compiles cleanly using warning level 4.      */

/* nonstandard extension 'single line comment' was used */

// nonstandard extension used : nameless struct/union
#pragma warning(disable: 4201)

// nonstandard extension used : bit field types other than int
#pragma warning(disable: 4214)

// Note: Creating precompiled header
#pragma warning(disable: 4699)

// unreferenced inline function has been removed
#pragma warning(disable: 4514)

// unreferenced formal parameter
#pragma warning(disable: 4100)

// indirection to slightly different base types
#pragma warning(disable: 4057)

// named type definition in parentheses
#pragma warning(disable: 4115)

// nonstandard extension used : benign typedef redefinition
#pragma warning(disable: 4209)

/* nonstandard extension 'zero-sized array' in struct/union */
#pragma warning(disable: 4200)

#ifdef  needed
// Windows Version Build Option //

// #define WINVER 0x0501

// Application Build Option //

// Force all EXEs/DLLs to be built for Windows 4.0. Comment out the following
// #pragma line to create applications that run under Windows NT 3.1 or Win32s.
// NOTE: Windows NT 3.5 and 3.51 run Win32 programs marked as 4.0.
// #pragma comment(linker, "-subsystem:Windows,4.0")

// STRICT Build Option //

// Force all EXEs/DLLs to use STRICT type checking.
#define STRICT

// CPU Portability Macros //

// If no CPU platform was specified, default to the current platform.
#if !defined(_PPC_) && !defined(_ALPHA_) && !defined(_MIPS_) && !defined(_X86_)
   #if defined(_M_IX86)
      #define _X86_
   #endif
   #if defined(_M_MRX000)
      #define _MIPS_
   #endif
   #if defined(_M_ALPHA)
      #define _ALPHA_
   #endif
   #if defined(_M_PPC)
      #define _PPC_
   #endif
#endif

#if defined(_DEBUG)
   #define adgLIBBUILDTYPE "Dbg_"
#else
   #define adgLIBBUILDTYPE "Rel_"
#endif

#if defined(_X86_)
   #define adgLIBCPUTYPE "x86"
#elif defined(_MIPS_)
   #define adgLIBCPUTYPE "MIPS"
#elif defined(_ALPHA_)
   #define adgLIBCPUTYPE "Alph"
#elif defined(_PPC_)
   #define adgLIBCPUTYPE "PPC"
#else
   #error CmnHdr.h : Unknown CPU platform.
#endif

// Compile all CONTEXT structures to use 32-bit members
// instead of 16-bit members.  Currently, the only sample
// application that requires this is TInjLib.16 in order
// for it to work correctly on the DEC Alpha AXP.
#define _PORTABLE_32BIT_CONTEXT

// Unicode Build Option //

// If we are not compiling for an x86 CPU, we always compile using Unicode.

// To compile using Unicode on the x86 CPU, uncomment the line below.

// When using Unicode Win32 functions, use Unicode C-Runtime functions too.

// dimof Macro //

// This macro evaluates to the number of elements in an array.
#define dimof(Array) (sizeof(Array) / sizeof(Array[0]))

// chINRANGE Macro //

// This macro evaluates to TRUE if val is between lo and hi inclusive.
#define chINRANGE(lo, val, hi) (((lo) <= (val)) && ((val) <= (hi)))

// chBEGINTHREADEX Macro //

// Create a chBeginThreadEx macro function that calls the C
// run-time's _beginthreadex function. The C run-time library
// doesn't want to have any reliance on Win32 data types such
// as HANDLE. This means that a Win32 programmer needs to cast
// the return value to a HANDLE. This is terribly inconvenient,
// so I have created this macro to perform the casting.
typedef unsigned (__stdcall *PTHREAD_START) (void *);

#define chBeginThreadEx(lpsa, cbStack, lpStartAddr, \
   lpvThreadParm, fdwCreate, lpIDThread)            \
      ((HANDLE)_beginthreadex(                      \
	 (void *) (lpsa),                           \
	 (unsigned) (cbStack),                      \
	 (PTHREAD_START) (lpStartAddr),             \
	 (void *) (lpvThreadParm),                  \
	 (unsigned) (fdwCreate),                    \
	 (unsigned *) (lpIDThread)))

// Assert/Verify Macros //

#define chFAIL(szMSG) {                                                   \
      MessageBox(GetActiveWindow(), szMSG,                                \
	 __TEXT("Assertion Failed"), MB_OK | MB_ICONERROR);                  \
      DebugBreak();                                                       \
   }

// Put up an assertion failure message box
#define chASSERTFAIL(file,line,expr) {                                    \
      TCHAR sz[128];                                                      \
      wsprintf(sz, __TEXT("File %hs, line %d : %hs"), file, line, expr);     \
      chFAIL(sz);                                                         \
   }

// Put up a message box if an assertion fails in a debug build
#ifdef _DEBUG
#define chASSERT(x) if (!(x)) chASSERTFAIL(__FILE__, __LINE__, #x)
#else
#define chASSERT(x)
#endif

// Assert in debug builds, but don't remove the code in retail builds
#ifdef _DEBUG
#define chVERIFY(x) chASSERT(x)
#else
#define chVERIFY(x) (x)
#endif

// chHANDLE_DLGMSG Macro //

// The normal HANDLE_MSG macro in WINDOWSX.H does not work properly for dialog
// boxes because DlgProc's return a BOOL instead of an LRESULT (like
// WndProcs). This chHANDLE_DLGMSG macro corrects the problem:
#define chHANDLE_DLGMSG(hwnd, message, fn)                           \
   case (message): return (SetDlgMsgResult(hwnd, uMsg,               \
      HANDLE_##message((hwnd), (wParam), (lParam), (fn))))

// Window Extra Byte Macros //

// Macros to compute the size and offset of structure members
#define chMEMBEROFFSET(structure, member) (int) (&(((structure *)0)->member))

// Macros to compute offsets and get/set window values based on the layout of
// a structure.
#define chSETWINDOWWORD(hwnd, structure, member, value) \
   SetWindowWord(hwnd, chMEMBEROFFSET(structure, member), (WORD) (value))
#define chSETWINDOWLONG(hwnd, structure, member, value) \
   SetWindowLong(hwnd, chMEMBEROFFSET(structure, member), (LONG) (value))
#define chGETWINDOWWORD(hwnd, structure, member) \
   GetWindowWord(hwnd, chMEMBEROFFSET(structure, member))
#define chGETWINDOWLONG(hwnd, structure, member) \
   GetWindowLong(hwnd, chMEMBEROFFSET(structure, member))

// Quick MessageBox Macro //

#define chMB(s) {                                                    \
      TCHAR szTMP[128];                                              \
      GetModuleFileName(NULL, szTMP, dimof(szTMP));                  \
      MessageBox(GetActiveWindow(), s, szTMP, MB_OK);                \
   }

// Zero Variable Macro //

// Zero out a structure. If fInitSize is TRUE then initialize the first int to
// the size of the structure. Many structures like WNDCLASSEX and STARTUPINFO
// require that their first member be set to the size of the structure itself.
#define chINITSTRUCT(structure, fInitSize)                           \
   (ZeroMemory(&(structure), sizeof(structure)),                     \
   fInitSize ? (*(int*) &(structure) = sizeof(structure)) : 0)

// Dialog Box Icon Setting Macro //

// The call to SetClassLong is for Windows NT 3.51 or less.  The WM_SETICON
// messages are for Windows 95 and future versions of NT.
#define chSETDLGICONS(hwnd, idiLarge, idiSmall)                               \
   {                                                                          \
      OSVERSIONINFO VerInfo;                                                  \
      chINITSTRUCT(VerInfo, TRUE);                                           \
      GetVersionEx(&VerInfo);                                                 \
      if ((VerInfo.dwPlatformId == VER_PLATFORM_WIN32_NT) &&                  \
	  (VerInfo.dwMajorVersion <= 3 && VerInfo.dwMinorVersion <= 51)) {    \
	 SetClassLong(hwnd, GCL_HICON, (LONG)                                 \
	    LoadIcon(GetWindowInstance(hwnd), MAKEINTRESOURCE(idiLarge)));    \
      } else {                                                                \
	 SendMessage(hwnd, WM_SETICON, TRUE,  (LPARAM)                        \
	    LoadIcon(GetWindowInstance(hwnd), MAKEINTRESOURCE(idiLarge)));    \
	 SendMessage(hwnd, WM_SETICON, FALSE, (LPARAM)                        \
	    LoadIcon(GetWindowInstance(hwnd), MAKEINTRESOURCE(idiSmall)));    \
      }                                                                       \
   }

#define chWARNIFUNICODEUNDERWIN95()

// WM_CAPTURECHANGED Message Cracker Macros //

// I have defined message cracker macros for the WM_CAPTURECHANGED message

/* void Cls_OnCaptureChanged(HWND hwnd, HWND hwndNewCapture) */
#define HANDLE_WM_CAPTURECHANGED(hwnd, wParam, lParam, fn) \
    ((fn)((hwnd), (HWND)(lParam)), 0L)
#define FORWARD_WM_CAPTURECHANGED(hwnd, hwndNewCapture, fn) \
    (void)(fn)((hwnd), WM_CAPTURECHANGED, (WPARAM)(HWND)(hwndNewCapture), 0L)

//

// JMR: Comment
#define chStringize(str)  #str
#define chStringize2(str)       Stringize(str)
#define chMSG(desc) message(__FILE__ "(" chStringize2(__LINE__) "):" #desc)

#define trace(x,y) trace4(x,y,0,0)
#define trace3(x,y,z) trace4(x,y,z,1)
#define trace4(x,y,a,b) \
    { \
	static int nr = x; \
	if (nr-- > 0) Fehler(y,a,b); \
    }
#endif needed
