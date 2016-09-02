NODEBUG=1
APPVER= 5.01
# Nmake macros for building Windows 32-Bit apps

!include <win32.mak>

all: $(OUTDIR) $(OUTDIR)\MetecBD.dll

#----- If OUTDIR does not exist, then create directory
$(OUTDIR) :
    if not exist "$(OUTDIR)/$(NULL)" mkdir $(OUTDIR)

# Update the object files if necessary

$(OUTDIR)\MetecBD.obj: MetecBD.cpp cmnhdr.h MetecBD.h mitsuusb.h usbio_i.h
    $(cc) $(cflags) $(cvarsmt) $(cdebug) /WX /Fo"$(OUTDIR)\\" /Fd"$(OUTDIR)\\" MetecBD.cpp

# Update the resources if necessary

# Update the dynamic link library

$(OUTDIR)\MetecBD.res: MetecBD.rc
    $(rc) $(rcflags) $(rcvars) /fo$(OUTDIR)\MetecBD.res  MetecBD.rc

$(OUTDIR)\MetecBD.dll: $(OUTDIR)\MetecBD.obj MetecBD.def $(OUTDIR)\MetecBD.res
    $(link) \
    $(ldebug) $(dlllflags)   \
    $(OUTDIR)\MetecBD.res   -out:$(OUTDIR)\MetecBD.dll /DEF:MetecBD.def $(OUTDIR)\MetecBD.obj \
    $(guilibsmt) setupapi.lib /manifest
    mt -nologo -manifest $(OUTDIR)\MetecBD.dll.manifest -outputresource:$(OUTDIR)\MetecBD.dll;2



#--------------------- Clean Rule --------------------------------------------------------
# Rules for cleaning out those old files
clean:
	$(CLEANUP)
