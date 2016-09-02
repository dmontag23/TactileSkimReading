using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;


namespace HyperBraille.HBBrailleDis
{

    /// <summary>
    /// Wrapper class to the MetecBD.dll. Generates
    /// an object to use the Stiftplatte.
    /// </summary>
    public partial class BrailleDisNet
    {
        // evaluates the touch input data regarding the given threshold
        void EvaluateTouchInput()
        {
            const int TimeToCheckDevice = 500; //ms
            int last_Check = Environment.TickCount;

            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            while (TouchInputRunning)
            {
                int timeToWait = 50;
                if (deviceIsInitialized)
                {
                    this.ReadTouchInput(out timeToWait);
                }
                else if (Environment.TickCount - last_Check > TimeToCheckDevice)
                {
                    CheckForDevice();
                    last_Check = Environment.TickCount;
                }

                if (m_deviceGeneration < 2)
                    Thread.Sleep(timeToWait);
            }
        }

        private void ReadTouchInput()
        {
            int dummy;
            ReadTouchInput(out dummy);
        }

        // read draw touch input data and evaluate it with the threshold
        private void ReadTouchInput(out int timeToWait)
        {

            // lock (synchLock) //  the lock is now in the MetecBD.dll
            {
                changedModules.Clear();
                activeModules.Clear();
                
                byte[] m_readBuffer = new byte[BrailleDisConsts.TOTAL_INPUT_LENGTH + 192]; // inkl. padding for too long blocks

                int readResult = 0;
                int timestamp;
                try
                {
                    //System.Threading.Thread.BeginCriticalRegion();
                    readResult = BrdReadData(this.UsedDevice.DeviceNumber, m_readBuffer.Length, m_readBuffer);
                    timestamp = Environment.TickCount;

                    //timeToWait ist die Variable, die von ReadTouchinput zurückgeliefert wird
                    //Zeit in ms bis zum nchsten Scan des Touch-Inputs
                    //Dies hilft der Synchronisation mit der Hardware.
                    timeToWait = m_readBuffer[BrailleDisConsts.TIME_WAIT_BYTE];

                    if (readResult != BrailleDisConsts.TOTAL_INPUT_LENGTH)
                    {
                        this.fireErrorOccurredEvent(ErrorType.USB_PAKET_DEFECTIVE);
                        timeToWait = BrailleDisConsts.TIME_TOUCH_SCAN_INTERVAL;
                        Close();
                        return;
                    }
                }
                finally
                {
                    //System.Threading.Thread.EndCriticalRegion();
                }

                // new paket?
                if (m_lastPaketNumber == m_readBuffer[BrailleDisConsts.TIME_PAKET_BYTE])
                {
                    return;
                }

                //#if DEBUG
                if (m_lastPaketNumber != null)
                {
                    byte thatShouldBeThePaketNumber = (byte)(m_lastPaketNumber + 1);
                    if (thatShouldBeThePaketNumber != m_readBuffer[BrailleDisConsts.TIME_PAKET_BYTE])
                    {
                        //System.Diagnostics.Debug.WriteLine("Lost a Paket... oldPaketnr=" + m_lastPaketNumber.ToString() + " " +
                        // thatShouldBeThePaketNumber.ToString() + " newpaketnr=" +
                        // m_readBuffer[BrailleDisConsts.TIME_PAKET_BYTE].ToString());
                        this.fireErrorOccurredEvent(ErrorType.USB_PAKET_LOST);
                    }
                }
                //#endif

                m_lastPaketNumber = m_readBuffer[BrailleDisConsts.TIME_PAKET_BYTE];

                #region touch input handling

                int row;
                int column;
                int differenceCapacity;
                int generation = 0;

                if (m_readBuffer[BrailleDisConsts.INPUT_TYPE] == BrailleDisConsts.OLD_BRAILLEDIS)
                { // first generation of BrailleDis with 720 sensors
                    // todo: if you wish to treat the old BrailleDis withh 1440 sensors
                    // then set generation to 2 instead of 1
                    generation = 1;
                    foreach (var damagedTouches in m_damagedTouches)
                    { // duplicate because of duplication of sensors
                        int pos = damagedTouches.X + damagedTouches.Y * BrailleDisConsts.NUMBER_OF_MODULE_COLUMNS * 2;
                        if (pos >= BrailleDisConsts.TOUCH_ARRAY_SIZE) continue;
                        m_readBuffer[BrailleDisConsts.FIRST_TOCH_BYTE + pos] = 0;
                        pos += 60; // second occurence of sensor
                        if (pos >= BrailleDisConsts.TOUCH_ARRAY_SIZE) continue;
                        m_readBuffer[BrailleDisConsts.FIRST_TOCH_BYTE + pos] = 0;
                    }
                }
                else
                {
                    // todo: if you wish to treat the new BrailleDis withh 720 sensors
                    // then set generation to 1 instead of 2
                    generation = 2;
                    foreach (var damagedTouches in m_damagedTouches)
                    {
                        int pos = damagedTouches.X + damagedTouches.Y * BrailleDisConsts.NUMBER_OF_MODULE_COLUMNS;
                        if (pos >= BrailleDisConsts.TOUCH_ARRAY_SIZE) continue;
                        m_readBuffer[BrailleDisConsts.FIRST_TOCH_BYTE + pos] = 0;
                    }
                }

                var d = this.UsedDevice;
                int[,] differenceCapacityArray = new int[DeviceTypeInformation.NumberOfSensorRows, DeviceTypeInformation.NumberOfModuleColumns];
                for (int i = 0; i < BrailleDisConsts.TOUCH_ARRAY_SIZE; i++)
                {
                    row = i / BrailleDisConsts.NUMBER_OF_MODULE_COLUMNS;
                    column = i - row * BrailleDisConsts.NUMBER_OF_MODULE_COLUMNS;
                    // for the old BrailleDis you should only treat 720 sensors
                    if (generation == 1)
                    {
                        if ((row & 1) == 1) // second sensor on a module
                        {
                            i += 59; // do not process this sensor line
                            continue;
                        }
                        row /= 2; // correct number
                    }
                    // evaluate input regarding threshold
                    differenceCapacity = this.m_reference_touch_input[row, column]
                    - m_readBuffer[BrailleDisConsts.FIRST_TOCH_BYTE + i];

                    var reallyLastDifCap = m_lastValues[row, column];
                    if (d != null)
                    {
                        differenceCapacity = d.Glaetten(reallyLastDifCap, differenceCapacity);
                    }
                    this.m_lastValues[row, column] = differenceCapacity;

                    //Werte mit zu kleinem Wert komplett ausblenden
                    if (differenceCapacity < this.m_touch_threshold)
                    {
                        differenceCapacity = 0;
                    }

                    differenceCapacityArray[row, column] = differenceCapacity;
                }

                if (d != null)
                {
                    d.Filter(differenceCapacityArray, m_touch_threshold);
                }
                for (row=0;row<differenceCapacityArray.GetLength(0);row++) {
                    for (column = 0; column < differenceCapacityArray.GetLength(1); column++)
                    {
                        differenceCapacity = differenceCapacityArray[row, column];

                        //Änderungen in changedModules speichern
                        if (differenceCapacity != this.m_touch_input[row, column])
                        {
                            changedModules.Add(new BrailleDisModuleState(row, column, this.m_touch_input[row, column],
                            differenceCapacity, generation));
                            this.m_touch_input[row, column] = differenceCapacity;
                        }
                        //ActiceModules-Array füllen
                        if (this.m_touch_input[row, column] != 0)
                        {
                            activeModules.Add(new BrailleDisModuleState(row, column, -1, this.m_touch_input[row, column], generation));
                        }
                    }
                }

                #endregion

                #region key input handling

                // the oldest keyklick is the last keyklick; therefore go down
                for (int k = BrailleDisConsts.KEYBOARD_LAST_POS;
                k >= BrailleDisConsts.KEYBOARD_START;
                k -= BrailleDisConsts.KEYBOARD_LENGTH)
                {
                    BrailleDisKeyboard newKeyboardState = new BrailleDisKeyboard(m_readBuffer, k, m_keyfilter);

                    // raise keyboard event, if necesary
                    if (m_keyboard_state.AllKeys != newKeyboardState.AllKeys)
                    {
                        UInt64 pressed = ~m_keyboard_state.AllKeys & newKeyboardState.AllKeys;
                        UInt64 released = m_keyboard_state.AllKeys & ~newKeyboardState.AllKeys;
                        this.fireKeyStateChangedEvent(new BrailleDisKeyboard(pressed),
                                  new BrailleDisKeyboard(released),
                                  newKeyboardState, timestamp);
                    }
                    this.m_keyboard_state.AllKeys = newKeyboardState.AllKeys;
                } // end for k

                #endregion

                #region raise 50ms events
                if (changedModules.Count > 0)
                {
                    this.fireTouchValuesChangedEvent(changedModules.ToArray(), activeModules.ToArray(), timestamp);
                }
                this.fireInputChangedEvent((changedModules.Count > 0), (int[,])this.m_touch_input.Clone(), this.m_keyboard_state,
                    timestamp);
                #endregion

                #region Jitter-Korrektur
                int timeForPC = Environment.TickCount - timestamp;
                timestamp -= timeForPC;
                #endregion
            }
        }
    }
}
