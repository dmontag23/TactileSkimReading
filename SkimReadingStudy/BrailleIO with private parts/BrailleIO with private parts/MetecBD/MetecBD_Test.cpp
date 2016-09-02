// MetecBD_Test.cpp

// Test program for MetecBD.cpp

// Copyright by METEC AG, Stuttgart

// Author F. Lüthi / 29. 01. 2005

#include <windows.h>
#include "MetecBD.h"
#include <stdio.h>
#include <stdlib.h>

#define SUBR // label for all functions

int testsw = BRD_TEST_LOG_CALLS;
int devnr = -1;
int devtype = 0;

SUBR int read(FILE *fp)
    // process a command input file
{
    int i, z, len, cmdc;
    FILE *np;
    char line[1000], *lp;
    char *cmdv[10];
    unsigned char   *bp;

    for (;;)
    {
	// read input
	fgets(line,sizeof(line),fp);
	if (feof(fp)) break;
	// cut ' ', \r and \n at the end of the line
	i = (int) strlen(line);
	while (i && line[i-1] <= ' ') i--;
	line[i] = 0;
	line[i+1] = 0; // important for scanning of the line
	// net the several parameters
	for (lp = line, cmdc = 0; *lp && cmdc < 9; lp++, cmdc++)
	{
	    for ( ; *lp == ' ' || *lp == '\t'; lp++) ;
	    if (!*lp) break;
	    cmdv[cmdc] = lp;
	    for ( ; *lp && *lp != ' ' && *lp != '\t'; lp++) ;
	    *lp = 0;
	};
	cmdv[cmdc] = ""; // significant value for a further item
	if (!cmdc) continue; // blank inplt line
	// process the commands
	switch (*cmdv[0])
	{
	    case 'l': // l filename = set logfilename
		if (cmdc > 1) BrdSetTestData(cmdv[1], 0);
		break;
	    case 't': // t flags = set testswitches
		if (cmdc > 1) testsw = atoi(cmdv[1]);
		else testsw = 0xffff;
		BrdSetTestData(0, &testsw);
		break;
	    case 'e': // enumerate all devices
		z = BrdEnumDevice(line,sizeof(line));
		if (z)
		{
		    for (i = 0; line[i]; i++)
		    {
			printf("%s\n",line+i);
			while (line[i]) i++;
		    };
		};
		break;
	    case 'i': // i [ devicedata ] = initiate device
		if (cmdc > 1) devnr =  BrdInitDevice(cmdv[1], &devtype);
		else devnr =  BrdInitDevice("", &devtype);
		break;
	    case 'd': // d data   [ devnr ] = display data
		if (cmdc > 2)
		    BrdTestOutput(atoi(cmdv[2]),*cmdv[1]);
		else BrdTestOutput(devnr, *cmdv[1]);
		break;
	    case 'r': // r [ len [ devnr ] ] = read data
		{
		    unsigned char rbuf[1600];
		    int len = 1600;
		    if (cmdc > 1)
		    {
			len = atoi(cmdv[1]);
			if (len < 1 || len > 1600) len = 1600;
		    }
		    if (cmdc > 2) len = BrdReadData(atoi(cmdv[2]), len,rbuf);
		    else len = BrdReadData(devnr, len,rbuf);
		    if (cmdc > 3 && len > 0)
		    {
			FILE *fp;
			if (!fopen_s(&fp,cmdv[3],"a"))
			{
			    fwrite(rbuf,len,1,fp);
			    fclose(fp);
			}
		    }
		};
		break;
	    case 'w': // w len [ hex-data ] [ rep-count ] [ devnr ] = write data
		{
		    unsigned char wbuf[1440*3]; // * 3 to avoid overflow
		    bp = wbuf;
		    z = 0;
		    memset(wbuf,0,sizeof(wbuf));
		    if (cmdc < 2) break;
		    len = atoi(cmdv[1]);
		    if (len > 1440) len = 1440;
		    if (cmdc > 2)
		    {
			lp = cmdv[2];
			 bp = wbuf;
			 i = (int) strlen(lp);
			 if (i > sizeof(wbuf)*2) i = sizeof(wbuf)*2;
			 *bp = 0;
			 for  ( ; i > 0; i--, lp++)
			 {
			    z = 0;
			    if (isdigit(*lp)) z = *lp - '0';
			else if (*lp >= 'A' && *lp <= 'F') z = *lp - 'A' + 10;
			else if (*lp >= 'a' && *lp <= 'f') z = *lp - 'a' + 10;
			    if (i & 1)
			    {
				*bp |= (unsigned char) z;
				bp++;
			    }
			    else
			    {
				*bp = (unsigned char) (z << 4);
			    };
			 };
		    };
		    if (cmdc > 3)
		    {
			i = (int) (bp - wbuf);
			z = atoi(cmdv[3]);
			if (z > 1)
			{
			    if (z * i > sizeof(wbuf)) z = sizeof(wbuf) / i;
			    for (z--; z > 0; z--, bp += i)
			    {
				memcpy(bp,wbuf,i);
			    };
			};
		    }
		    if (cmdc > 4) BrdWriteData(atoi(cmdv[4]), len, wbuf);
		    else BrdWriteData(devnr, len, wbuf);
		}
		break;
	    case 'v': // v hex-data  [ devnr ] = send vendor request
		{
		    unsigned char vbuf[100]; // 100 to avoid overflow
		    memset(vbuf,0,sizeof(vbuf));
		    if (cmdc < 2) break;
		       lp = cmdv[1];
			bp = vbuf;
			i = (int) strlen(lp);
			if (i > sizeof(vbuf)*2) i = sizeof(vbuf)*2;
			*bp = 0;
			for  ( ; i > 0; i--, lp++)
			{
			    z = 0;
			   if (isdigit(*lp)) z = *lp - '0';
		       else if (*lp >= 'A' && *lp <= 'F') z = *lp - 'A' + 10;
		       else if (*lp >= 'a' && *lp <= 'f') z = *lp - 'a' + 10;
			   if (i & 1)
			   {
			       *bp |= (unsigned char) z;
			       bp++;
			   }
			   else
			   {
			       *bp = (unsigned char) (z << 4);
			   };
			};
		    if (cmdc > 2) BrdCommand(atoi(cmdv[2]),  vbuf);
		    else BrdCommand(devnr,  vbuf);
		}
		break;
	    case 'h': // power on/off
		{
		    int state = 1;
		    if (cmdc >= 2) state = atoi(cmdv[1]);
		    if (cmdc > 2) BrdHighVoltage(atoi(cmdv[2]),state);
		    else BrdHighVoltage(devnr,state);
		}
		break;
	    case 'm': // m anzahl [ devnr ] = set no of modules
		if (cmdc > 2) BrdSetNoOfModules(atoi(cmdv[2]),
		    (UCHAR)  atoi(cmdv[1]));
		else BrdSetNoOfModules(devnr, (UCHAR) atoi(cmdv[1]));
		break;
	    case 'p': // p ms = pause for ms milliseconds
		if (cmdc > 1) Sleep(atoi(cmdv[1]));
		break;
	    case 'f':  // f filename [ times ] = process filename as command
		// input file [ "times" times
		if (cmdc < 2) break;
		if (cmdc > 2) z = atoi(cmdv[2]);
		else z = 1;
		for ( ; z > 0; z--)
		{
		    if (fopen_s(&np, cmdv[1],"r"))
		    {
			np = 0;
		    }
		    if (np)
		    {
			read(np);
			fclose(np);
		    }
		    else
		    {
		    fprintf(stderr,"open error on '%s'\n",cmdv[1]);
		    break;
		    };
		};
		if (!z) fprintf(stderr,"\nend of '%s'\n",cmdv[1]);
		break;
	    case 'c': // c [ devnr ] = close device
		if (cmdc > 1)
		{
		    BrdCloseDevice(atoi(cmdv[1]));
		}
		else
		{
		    BrdCloseDevice(devnr);
		    devnr = -1;
		};
		break;
	    case 'x': // x = exit program
		return 0;
	}
    }

    return 0;
}

SUBR int main(int argc, char **argv)
    // call MetecBD_Test [ testoutputfile ] ("-" = standard output
{

    if (argc > 1) BrdSetTestData(argv[1], &testsw);
    else  BrdSetTestData(0, &testsw);

	read(stdin);
    return 0;
}

