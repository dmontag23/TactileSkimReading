NODEBUG=1
APPVER= 5.01
CPU=AMD64
# Nmake macros for building Windows 32-Bit apps

!include <win32.mak>

all: $(OUTDIR) $(OUTDIR)\MetecBD_Test.exe

#----- If OUTDIR does not exist, then create directory
$(OUTDIR) :
    if not exist "$(OUTDIR)/$(NULL)" mkdir $(OUTDIR)

# Update the object files if necessary

$(OUTDIR)\MetecBD_Test.obj: MetecBD_Test.cpp
    $(cc) $(cflags) $(cvars) $(cdebug) /WX /Fo"$(OUTDIR)\\" /Fd"$(OUTDIR)\\" MetecBD_Test.cpp

# Update the resources if necessary

$(OUTDIR)\MetecBD_Test.res: MetecBD_Test.rc
    $(rc) $(rcflags) $(rcvars) /fo$(OUTDIR)\MetecBD_Test.res  MetecBD_Test.rc

$(OUTDIR)\MetecBD_Test.exe: $(OUTDIR)\MetecBD_Test.obj $(OUTDIR)\MetecBD.dll $(OUTDIR)\MetecBD_Test.res
    $(link) -nologo \
    $(linkdebug) -out:$(OUTDIR)\MetecBD_Test.exe $(OUTDIR)\MetecBD_Test.obj \
 $(OUTDIR)\MetecBD.lib $(OUTDIR)\MetecBD_Test.res $(guilibs) /manifest
    mt -nologo -manifest $(OUTDIR)\MetecBD_Test.exe.manifest -outputresource:$(OUTDIR)\MetecBD_Test.exe;1

#--------------------- Clean Rule --------------------------------------------------------
# Rules for cleaning out those old files
clean:
	$(CLEANUP)
