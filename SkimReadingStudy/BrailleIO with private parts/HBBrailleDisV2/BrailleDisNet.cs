using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace HyperBraille.HBBrailleDis
{

    /// <summary>
    /// Wrapper class to the MetecBD.dll. Generates
    /// an object to use the pin device.
    /// </summary>
    public partial class BrailleDisNet
    {

        #region Delegates

        // delegates
        /// <summary>
        /// will be called by the <see cref="inputChangedEvent"/>.
        /// </summary>
        /// <param name="touchInputAvailable">should be true</param>
        /// <param name="valueMatrix">the current state of the touchvalues</param>
        /// <param name="keyboardState">the current keyboard state</param>
        /// <param name="timeStampTickCount">the tickcount timestamp from the moment, the values were fully received by the PC</param>
        public delegate void InputChangedEventHandler(bool touchInputAvailable, int[,] valueMatrix, BrailleDisKeyboard
            keyboardState, int timeStampTickCount);
        /// <summary>
        /// Will be called by <see cref="touchValuesChangedEvent"/>
        /// </summary>
        /// <param name="changedModules">a list with all chanded modules, since the last call.</param>
        /// <param name="activeModules">the list with currently all active modules</param>
        /// <param name="timeStampTickCount">the tickcount timestamp from the moment, the values were fully received by the PC</param>
        public delegate void TouchValuesChangedEventHandler(BrailleDisModuleState[] changedModules, BrailleDisModuleState[]
            activeModules, int timeStampTickCount);
        /// <summary>
        /// will be called by <see cref="keyStateChangedEvent"/>
        /// </summary>
        /// <param name="pressedKeys">the keys which were pressed here</param>
        /// <param name="releasedKeys">the keys which were released here</param>
        /// <param name="keyboardState">the current state of each key</param>
        /// <param name="timeStampTickCount">the tickcount timestamp from the moment, the values were fully received by the PC</param>
        public delegate void KeyStateChangedEventHandler(BrailleDisKeyboard pressedKeys, BrailleDisKeyboard releasedKeys,
            BrailleDisKeyboard keyboardState, int timeStampTickCount);
        /// <summary>
        /// Will be called, if the output changes. Used by <see cref="pinStateChangedEvent"/>
        /// </summary>
        /// <param name="changedPins">the changed pins</param>
        public delegate void PinStateChangeEventHandler(BrailleDisPinState[] changedPins);

        /// <summary>
        /// Type, which defines the error, which occurs in <see cref="ErrorOccuredHandler"/>.
        /// </summary>
        public enum ErrorType
        {
            /// <summary>
            /// Mentiones, that the internal messages queue is already half full
            /// </summary>
            MSG_QUEUE_HALF_FULL,
            /// <summary>
            /// Mentiones, that the internal messages queue is already full. Some messages
            /// may already be lost.
            /// </summary>
            MSG_QUEUE_FULL,
            /// <summary>
            /// Mentiones, that the communication to the USB-Device lost one or more
            /// pakets.
            /// </summary>
            USB_PAKET_LOST,
            /// <summary>
            /// Occurs, when the PC receives a Paket with a wrong format
            /// </summary>
            USB_PAKET_DEFECTIVE,
            /// <summary>
            /// occurs, when a new HW is connected to the software
            /// </summary>
            NEW_DEVICE,
            /// <summary>
            /// Occurs, when the device-string of the device is not correct
            /// </summary>
            DEVICE_STRING_DEFECTIVE,
            /// <summary>
            /// Occurs, when the device cannot been reached
            /// </summary>
            SEH_EXCEPTION
        };
        /// <summary>
        /// Will be called, if an error occurs in the BrailleDis
        /// </summary>
        /// <param name="type">The type of error</param>
        public delegate void ErrorOccuredHandler(ErrorType type);
        #endregion


        #region Events
        #region Events to sign up for
        /// <summary>
        /// Will be called, when a new device is attached on the USB-Port.
        /// </summary>
        public event Action<BrailleDisNet, DeviceInformation_T> NewDeviceAttached;
        /// <summary>
        /// event for notifying clients of touch input changes
        /// </summary>
        public event InputChangedEventHandler inputChangedEvent;
        /// <summary>
        /// event that notify when the touch values have changed
        /// </summary>
        public event TouchValuesChangedEventHandler touchValuesChangedEvent;
        /// <summary>
        /// event that notifies, when the keys on the stiftplatte have changed.
        /// </summary>
        public event KeyStateChangedEventHandler keyStateChangedEvent;
        /// <summary>
        /// Will be called, if an error occurs in the BrailleDis
        /// </summary>
        public event ErrorOccuredHandler ErrorOccured;

        /// <summary>
        /// Occurs when pin states change on braille display.
        /// </summary>
        public event PinStateChangeEventHandler pinStateChangedEvent;

        #endregion

        #region Eventmanagement
        enum callType
        {
            inputchanged, touchvalueschanged, keystatechanged, pinstatechanged, erroroccurred
        };
        private struct _internalMessage
        {
            public MulticastDelegate pleaseCall;
            public callType calltype;
            public object[] parameters;
        };
        private AutoResetEvent fireEventsFlag = new AutoResetEvent(false);
        //private Thread FireEventThread;
        private Thread EvaluationThread;
        private object lockFireEventThreadImpl = new object();
        private void FireEventThreadImpl(object o, bool timedOut)
        {
            if (Monitor.TryEnter(lockFireEventThreadImpl))
            {
                try
                {
                    #region Do all messages within queue
                    while (true) //Loop for dealing all Messages within the Queue; KEINE Endlosschleife!
                    {
                        if (!TouchInputRunning) return; //Programm beendet sich!
                        try
                        {
                            _internalMessage msg;

                            var errorHandlers = this.ErrorOccured;
                            if (errorHandlers != null)
                            {
                                bool doSend = false;
                                ErrorType sendWhat = ErrorType.MSG_QUEUE_HALF_FULL;
                                lock (_internalMessageQueue)
                                {
                                    if (_internalMessageQueue.Count >= MaximumMessageInQueue)
                                    {
                                        doSend = true;
                                        sendWhat = ErrorType.MSG_QUEUE_FULL;
                                    }
                                    else if (_internalMessageQueue.Count >= MaximumMessageInQueue / 2)
                                    {
                                        doSend = true;
                                        sendWhat = ErrorType.MSG_QUEUE_HALF_FULL;
                                    }
                                }
                                if (doSend)
                                    errorHandlers(sendWhat);
                            }

                            lock (_internalMessageQueue)
                            {
                                if (_internalMessageQueue.Count < 1) break; //goto Wait for any Message
                                msg = _internalMessageQueue.Dequeue();
                            }

                            //This is a Reflection-Invocation. This is very slow.
                            //The Switch-case is used for more speed!
                            //if (msg.pleaseCall != null)
                            //    msg.pleaseCall.DynamicInvoke(msg.parameters); //Does DynamicInvoke create a false new thread?
                            switch (msg.calltype)
                            {
                                case callType.inputchanged:
                                    if (inputChangedEvent != null)
                                        //inputChangedEvent((bool)msg.parameters[0],msg.parameters[1] as int[,],(BrailleDisKeyboard)msg.parameters[2]);
                                        //Es wird nicht der direkte event verwendet, sondern der angemeldete
                                        //somit werden (nur) die Listener aufgerufen, die zum Zeitpunkt
                                        //des ereignisses angemeldet waren. (Da nicht der event an sich, sondern nur sein multicastdelegate verwendet wird)
                                        (msg.pleaseCall as InputChangedEventHandler)((bool)msg.parameters[0], msg.parameters[1] as int[,], (BrailleDisKeyboard)msg.parameters[2], (int)msg.parameters[3]);
                                    break;
                                case callType.keystatechanged:
                                    if (keyStateChangedEvent != null)
                                        //keyStateChangedEvent((BrailleDisKeyboard)msg.parameters[0], (BrailleDisKeyboard)msg.parameters[1], (BrailleDisKeyboard)msg.parameters[2]);
                                        //Es wird nicht der direkte event verwendet, sondern der angemeldete
                                        //somit werden (nur) die Listener aufgerufen, die zum Zeitpunkt
                                        //des ereignisses angemeldet waren. (Da nicht der event an sich, sondern nur sein multicastdelegate verwendet wird)
                                        (msg.pleaseCall as KeyStateChangedEventHandler)((BrailleDisKeyboard)msg.parameters[0], (BrailleDisKeyboard)msg.parameters[1], (BrailleDisKeyboard)msg.parameters[2], (int)msg.parameters[3]);
                                    break;
                                case callType.touchvalueschanged:
                                    if (touchValuesChangedEvent != null)
                                        //touchValuesChangedEvent((BrailleDisModuleState[])msg.parameters[0], (BrailleDisModuleState[])msg.parameters[1]);
                                        //Es wird nicht der direkte event verwendet, sondern der angemeldete
                                        //somit werden (nur) die Listener aufgerufen, die zum Zeitpunkt
                                        //des ereignisses angemeldet waren. (Da nicht der event an sich, sondern nur sein multicastdelegate verwendet wird)
                                        foreach (var del in (msg.pleaseCall as TouchValuesChangedEventHandler).GetInvocationList())
                                        {
                                            try
                                            {
                                                del.DynamicInvoke((BrailleDisModuleState[])msg.parameters[0], (BrailleDisModuleState[])msg.parameters[1], (int)msg.parameters[2]);
                                            }
                                            catch (Exception e)
                                            {

                                            }
                                        }
                                    break;
                                case callType.pinstatechanged:
                                    if (pinStateChangedEvent != null)
                                    {
                                        (msg.pleaseCall as PinStateChangeEventHandler)((BrailleDisPinState[])msg.parameters[0]);
                                    }
                                    break;
                                case callType.erroroccurred:
                                    if (ErrorOccured != null)
                                    {
                                        (msg.pleaseCall as ErrorOccuredHandler)((ErrorType)msg.parameters[0]);
                                    }
                                    break;

                            }
                        }
                        catch (ThreadAbortException tae)
                        {
                            throw tae;
                        }
                        catch
                        {
                        }
                    }
                    #endregion
                }
                finally
                {
                    Monitor.Exit(lockFireEventThreadImpl);
                }
            }

        }
        #endregion

        #region Events to be locally fired
        private void fireEventInFireEventThread(callType ct, MulticastDelegate ev, params object[] parameter)
        {
            if (ev == null) return; //return if Multicast-delegate has no content
            //if (Thread.CurrentThread != EvaluationThread) return; //Do not call while Initialization

            _internalMessage msg =
                new _internalMessage()
                {
                    pleaseCall = ev,
                    calltype = ct,
                    parameters = parameter
                };

            lock (_internalMessageQueue)
            {
                while (_internalMessageQueue.Count >= MaximumMessageInQueue) _internalMessageQueue.Dequeue();
                _internalMessageQueue.Enqueue(msg);
            }

            fireEventsFlag.Set();
        }
        private void fireErrorOccurredEvent(ErrorType errorType)
        {
            fireEventInFireEventThread(callType.erroroccurred, this.ErrorOccured, errorType);
        }
        private void fireInputChangedEvent(bool touchInputAvailable, int[,] valueMatrix, BrailleDisKeyboard keyboardState, int timestamptickcount)
        {
            fireEventInFireEventThread(callType.inputchanged,
                inputChangedEvent, touchInputAvailable, valueMatrix, keyboardState, timestamptickcount);
        }
        private void fireTouchValuesChangedEvent(BrailleDisModuleState[] changedModules, BrailleDisModuleState[]
            activeModules, int timestamptickcount)
        {
#if DEBUG
            /*
            int minchanged = 0;
            int minactive = 0;

            if (changedModules.Length>0) minchanged = changedModules.Min(x => x.CurrentValue);
            if (activeModules.Length > 0) minactive = activeModules.Min(x => x.CurrentValue);

            if (minchanged < 0)// || minchanged > this.m_touch_threshold)
                System.Diagnostics.Debug.WriteLine("changed: " + minchanged.ToString());
            if (minactive < 0)// || minactive > this.m_touch_threshold)
                System.Diagnostics.Debug.WriteLine("acvive: " + minactive.ToString());/**/
#endif
            fireEventInFireEventThread(callType.touchvalueschanged,
                touchValuesChangedEvent, changedModules, activeModules, timestamptickcount);
        }
        // gibt nur die row und column und den wert der geänderten module zurück
        private void fireKeyStateChangedEvent(BrailleDisKeyboard pressedKeys, BrailleDisKeyboard releasedKeys,
            BrailleDisKeyboard keyboardState, int timestamptickcount)
        {
            fireEventInFireEventThread(callType.keystatechanged,
                keyStateChangedEvent, pressedKeys, releasedKeys, keyboardState, timestamptickcount);
        }

        private void firePinStateChangedEvent(BrailleDisPinState[] changedPins)
        {
            fireEventInFireEventThread(callType.pinstatechanged, pinStateChangedEvent, changedPins);
        }

        #endregion
        #endregion


        #region Private Fields

        private readonly object synchLock = new object();

        // number of module rows
        //private int m_numRows;

        // number of module columns
        //private int m_numColumns;

        // device number
        //private int m_deviceNumber;

        // BrailleDis-Generation 1 = old, 2 = new
        internal int m_deviceGeneration = 0;

        // current matrix state [row, column]
        private byte[] m_matrix;


        // current bool matrix [row, column]
        private bool[,] m_boolMatrix, m_tempBoolMatrix;

        // array to set all pins
        private byte[] m_matrix_set_all;

        // array to lower all pins
        private byte[] m_matrix_lower_all;

        // array for reference capacity values
        internal int[,] m_reference_touch_input;

        private int[,] m_lastValues = new int[BrailleDisConsts.NUMBER_OF_SENSOR_ROWS, BrailleDisConsts.NUMBER_OF_MODULE_COLUMNS];

        // array for reading touchinput (always holds the last input read to determine changes of touch values during the next
        //  reading operation)
        internal int[,] m_touch_input;

        // threshold for touch input
        private int m_touch_threshold;
        //private int m_touch_threshold_exception;

        // saves the last keyboard state to determine changes of the key states during the next reading operation
        private BrailleDisKeyboard m_keyboard_state;

        // last paket-number
        private byte? m_lastPaketNumber = null; // Startup

        List<BrailleDisModuleState> changedModules;
        List<BrailleDisModuleState> activeModules;

        internal struct ModulePos
        {
            public int X;
            public int Y;
        }
        internal List<ModulePos> m_damagedTouches = new List<ModulePos>();

        #endregion


        #region Private Properties

        private bool deviceIsInitialized = false;

        public bool DeviceIsInitialized
        {
            get { return deviceIsInitialized; }
            internal set { deviceIsInitialized = value; }
        }

        private volatile bool touchInputRunning = false;

        UInt64 m_keyfilter = 0xffffffffffffffffu;

        private bool TouchInputRunning
        {
            get
            {
                //Was sollte hier schon passerieren - Lock - Wozu. Man riskiert nur einen Deadlock,
                //Weil sendente Threads mit den Empangenden Probleme machen.
                //lock (synchLock)
                {
                    return touchInputRunning;
                }
            }
            set
            {
                //Was sollte hier schon passerieren - Lock - Wozu. Man riskiert nur einen Deadlock,
                //Weil sendente Threads mit den Empangenden Probleme machen.
                //lock (synchLock)
                {
                    touchInputRunning = value;
                }
            }
        }

        private const int MaximumMessageInQueue = 120;
        private Queue<_internalMessage> _internalMessageQueue = new Queue<_internalMessage>(MaximumMessageInQueue + 2);

        //IConfigurationManager _config;
        //private IConfigurationManager Config
        //{
        //    get { return _config; }
        //}
        Object _config;
        private Object Config
        {
            get { return _config; }
        }

        #endregion


        #region Private Methods

        List<BrailleDisPinState> changedPins = new List<BrailleDisPinState>();
        private void UpdateOnPinStateChanges(bool[,] pinStatesOld, bool[,] pinStatesNew)
        {
            changedPins.Clear();
            for (int row = 0; row < pinStatesOld.GetLength(0); row++)
            {
                if (row <= pinStatesNew.GetLength(0)) break;
                for (int column = 0; column < pinStatesOld.GetLength(1); column++)
                {
                    if (column <= pinStatesNew.GetLength(1)) break;
                    if (pinStatesOld[row, column] != pinStatesNew[row, column])
                    {
                        changedPins.Add(new BrailleDisPinState(row, column, pinStatesNew[row, column]));
                        pinStatesOld[row, column] = pinStatesNew[row, column];
                    }
                }
            }
            if (changedPins.Count > 0)
            {
                this.firePinStateChangedEvent(changedPins.ToArray());
            }
        }

        /// <summary>
        /// Calibrates the values to which the changes of touch are compared to.
        /// Before calling this method, ensure, that noone touches the modules
        /// for at least 3 seconds.
        /// </summary>
        public void Recalibrate()
        {
            lock (synchLock)
            {
                this.UsedDevice.Recalibrate(this);

                //Keiner hat die Finger auf der Stiftplatte, => ist guter Zeitpunkt das hier zu tun ;)
                //Wer kalibriert, will danach ja ein sauberes System haben.
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }
        }

        internal bool empfindlich = false;
        private void Configure(Object node, Object defau)
        {
            #region Empfindlichkeit
            ////var empf = node.GetChildNode("Empfindlich");
            //Object empf = null;
            //if (empf == null)
            //{
            //    if (defau == null)
            //        node.AddChildNode("Empfindlich", false);
            //    else
            //        node.AddChildNode("Empfindlich", "");
            //}
            //else
            //{
            try
            {
                empfindlich = false;
            }
            catch { }
            //}
            #endregion

            #region KeyFilter
            //var keyFilter = node.GetChildNode("KeyFilter");
            //if (keyFilter == null)
            //{
            //    if (defau == null)
            //        node.AddChildNode("KeyFilter", 0xFFFFFFFFFFFF /*0xFF03*/);
            //    else
            //        node.AddChildNode("KeyFilter", "");
            //}
            //else
            //{
            // if (Convert.ToString(keyFilter.Value) != "")
            m_keyfilter = 0xFFFFFFFFFFFF;
            //}
            #endregion

            #region Filter Damaged Touches
            //var damagedTouchesNode = node.GetChildNode("DamagedTouches");
            //if (damagedTouchesNode == null)
            //{
            //    var n1 = node.AddChildNode("DamagedTouches").AddChildNode("1");
            //    n1.AddChildNode("X", -1);
            //    n1.AddChildNode("Y", -1);
            //}
            //else
            //{
            //    var children = damagedTouchesNode.ChildNodes;
            //    foreach (var damagedTouch in children)
            //    {
            //        var coordinates = damagedTouch.ChildNodes;
            //        int x = Convert.ToInt32(coordinates.byName("X").Value);
            //        if (x == -1) continue;
            //        int y = Convert.ToInt32(coordinates.byName("Y").Value);
            //        if (y == -1) continue;
            //        m_damagedTouches.Add(new ModulePos() { X = x - 1, Y = y - 1 });
            //    }
            //}
            #endregion

            #region Treshold(s)
            {
                //var touchTreshold = node.GetChildNode("TouchTreshold");
                Object touchTreshold = null;
                if (touchTreshold == null)
                {
                    this.m_touch_threshold = BrailleDisConsts.INITIAL_TOUCH_THRESHOLD;
                    //    if (defau == null)
                    //        node.AddChildNode("TouchTreshold", BrailleDisConsts.INITIAL_TOUCH_THRESHOLD);
                    //    else
                    //        node.AddChildNode("TouchTreshold", "");
                }
                //else
                //{
                //    if (Convert.ToString(touchTreshold.Value) != "")
                //        m_touch_threshold = Convert.ToInt32(touchTreshold.Value);
                //}
            }

            //{
            //    var touchTreshold_exceptional = node.GetChildNode("TouchTresholdExceptional");
            //    if (touchTreshold_exceptional == null)
            //    {
            //        this.m_touch_threshold_exception = ushort.MaxValue;
            //        if (defau == null)
            //            node.AddChildNode("TouchTresholdExceptional", ushort.MaxValue);
            //        else
            //            node.AddChildNode("TouchTresholdExceptional", "");
            //    }
            //    else
            //    {
            //        if (Convert.ToString(touchTreshold_exceptional.Value) != "")
            //            m_touch_threshold_exception = Convert.ToInt32(touchTreshold_exceptional.Value);
            //    }
            //}
            #endregion
        }



        public void ReloadConfiguration()
        {
            #region Read Default config
            //ConfigurationNode config = null;
            //try
            //{
            //    config = ConfigurationManager.Instance.GetNode(ConfigurationType.UserProfile, this.GetType().ToString());
            //    this.Configure(config, null);
            //    var serialNode = config[this.UsedDevice.DeviceInformation.SerialNo];
            //    this.Configure(serialNode, config);
            //}
            //catch
            //{
            //}
            #endregion

        }

        // initializes the pin matrix device
        private bool Initialize()
        {
            #region Read Default config
            Object config = null;
            try
            {
                //config = ConfigurationManager.Instance.GetNode(ConfigurationType.UserProfile,this.GetType().ToString());
                this.Configure(config, null);
            }
            catch
            {
            }
            #endregion

            changedModules = new List<BrailleDisModuleState>();
            activeModules = new List<BrailleDisModuleState>();



            #region config device 0;
            string devicestring = "";
            try
            {
                this.UsedDevice =
                    DeviceChooser.CreateDeviceObject(
                        LoadFirstDevice(out devicestring), this);
                if (this.UsedDevice.DeviceInformation.DeviceName == null) return false;

                var serialNode = UsedDevice.DeviceInformation.SerialNo;
                //if (serialNode == null) serialNode = config.AddChildNode(di.SerialNo);

                this.Configure(serialNode, config);
                #region Register Devicename
                //var node = serialNode.GetChildNode("DeviceName");
                //if (node == null) node = serialNode.AddChildNode("DeviceName");
                //if (!node.HasValue || node.Value.ToString() != UsedDevice.DeviceInformation.DeviceName)
                //{
                //    node.Value = UsedDevice.DeviceInformation.DeviceName;
                //}
                #endregion
                #region Register USBinterfaceNo
                //node = serialNode.GetChildNode("USBinterfaceNo");
                //if (node == null) node = serialNode.AddChildNode("USBinterfaceNo");
                //if (!node.HasValue || node.Value.ToString() != UsedDevice.DeviceInformation.USBinterfaceNo)
                //{
                //    node.Value = UsedDevice.DeviceInformation.USBinterfaceNo;
                //}
                #endregion
                #region Register DeviceType
                //node = serialNode.GetChildNode("DeviceType");
                //if (node == null) node = serialNode.AddChildNode("DeviceType");
                //if (!node.HasValue || node.Value.ToString() != UsedDevice.DeviceInformation.DeviceType)
                //{
                //    node.Value = UsedDevice.DeviceInformation.DeviceType;
                //}
                #endregion
                #region Register FirmwareBuild
                //node = serialNode.GetChildNode("FirmwareBuild");
                //if (node == null) node = serialNode.AddChildNode("FirmwareBuild");
                //if (!node.HasValue || node.Value.ToString() != UsedDevice.DeviceInformation.FirmwareBuild)
                //{
                //    node.Value = UsedDevice.DeviceInformation.FirmwareBuild;
                //}
                #endregion
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Can't load BrailleDis\n" + ex);

                //
                // Dialog box that is right-aligned (not useful). [7]
                //
                MessageBox.Show("Can't load BrailleDis\n" + ex,
                "Critical Warning",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RightAlign,
                true);

            }
            #endregion

            lock (synchLock)
            {

                #region close already open Device
                // init device (only one device expected!)
                // if one device is already open, try to close this
                if (devicestring.StartsWith("*"))
                {
                    int signIndex = devicestring.IndexOf("=");
                    String numberString = null;

                    if (signIndex == -1)
                    {
                        numberString = devicestring.Substring(1);
                    }
                    else
                    {
                        numberString = devicestring.Substring(1, signIndex - 1);
                    }

                    try
                    {
                        int devNr = Int32.Parse(numberString);
                        BrdCloseDevice(devNr);
                    }
                    catch (ArgumentNullException) { }
                    catch (FormatException) { }
                    catch (OverflowException) { }
                    catch (System.Runtime.InteropServices.SEHException)
                    {
                        fireErrorOccurredEvent(ErrorType.SEH_EXCEPTION);
                        Thread.Sleep(100);
                    }
                }
                #endregion
                if (this.UsedDevice == null)
                {
                    deviceIsInitialized = false;
                    return false;
                }

                this.UsedDevice.Initialize();
                deviceIsInitialized = true;
                #region device info splash
                //try
                //{
                //    Action splash = () =>
                //    {
                //        var serial = this.UsedDevice.DeviceInformation.SerialNo;
                //        bool witherror = false;
                //        if (serial.Length != 4)
                //        {
                //            serial = devicestring;
                //            witherror = true;
                //        }

                //        var ssi = new HBBrailleDisV2.SplashSerialInfo(serial, witherror);
                //        ssi.Show();
                //    };
                //    if (!Thread.CurrentThread.IsBackground)
                //    {
                //        splash();
                //    }
                //    else
                //    {
                //        System.Windows.Forms.Application.OpenForms[0].Invoke(
                //            splash
                //            );
                //    }

                //}
                //catch
                //{
                //}
                #endregion
                this.UsedDevice.DoAfterInitialization();

            }
            /* read initial capacity values */
            Recalibrate();

            var NewDeviceAtachedEvent = this.NewDeviceAttached;
            if (NewDeviceAtachedEvent != null)
            {
                foreach (var call in NewDeviceAtachedEvent.GetInvocationList())
                {
                    try
                    {
                        call.Method.Invoke(call.Target, new object[] { this, this.UsedDevice.DeviceInformation });
                    }
                    catch
                    {
                    }
                }
            }

            return true;
        }

        #endregion


        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BrailleDisNet"/> class.
        /// </summary>
        public BrailleDisNet()
            :
            this(null)
        {
        }
        /// <summary>
        /// Created the object to talk with the stiftplatte.
        /// </summary>
        /// <param name="config">the configurationManager to be used. Can be null, but it should not.</param>
        /// <exception cref="System.IO.IOException">Exception occurs if no stiftplatte is connected</exception>
        public BrailleDisNet(Object config)
        {
            _config = config;
            // initialize array for current matrix state
            this.m_matrix = new byte[2 * BrailleDisConsts.MODULE_COUNT];

            this.m_boolMatrix = new bool[BrailleDisConsts.NUMBER_OF_PIN_ROWS, BrailleDisConsts.NUMBER_OF_PIN_COLUMNS];
            this.m_tempBoolMatrix = new bool[BrailleDisConsts.NUMBER_OF_PIN_ROWS, BrailleDisConsts.NUMBER_OF_PIN_COLUMNS];

            for (int i = 0; i < 2 * BrailleDisConsts.MODULE_COUNT; i++)
            {
                this.m_matrix[i] = BrailleDisConsts.BYTE_LOWER_ALL_PINS;
            }

            // initialize array for setting all pins
            this.m_matrix_set_all = new byte[2 * BrailleDisConsts.MODULE_COUNT];
            for (int i = 0; i < 2 * BrailleDisConsts.MODULE_COUNT; i++)
            {
                this.m_matrix_set_all[i] = BrailleDisConsts.BYTE_SET_ALL_PINS;
            }

            // initialize array for lowering all pins
            this.m_matrix_lower_all = new byte[2 * BrailleDisConsts.MODULE_COUNT];
            for (int i = 0; i < 2 * BrailleDisConsts.MODULE_COUNT; i++)
            {
                this.m_matrix_lower_all[i] = BrailleDisConsts.BYTE_LOWER_ALL_PINS;
            }

            // init keyboard state
            this.m_keyboard_state = new BrailleDisKeyboard(0);

            // initialize device
            bool initializationResult = this.Initialize();
            /*if (!initializationResult)
            {
                String exceptionMessage = "Initialization of BrailleDis Device failed.";
                throw new System.IO.IOException(exceptionMessage);
            }*/
        }

        #endregion

        #region Public Methods


        /// <summary>
        /// Starts the touch evaluation (automatic touch input reading).
        /// </summary>
        /// <param name="_intervalPauseInMilliseconds">The interval pause in milliseconds.</param>
        public void StartTouchEvaluation(int _intervalPauseInMilliseconds)
        {
            this.StopTouchEvaluation();

            lock (synchLock)
            {
                if (!touchInputRunning)
                {
                    TouchInputRunning = true;
                    //ParameterizedThreadStart tStart = new ParameterizedThreadStart(EvaluateTouchInput);
                    EvaluationThread = new Thread(EvaluateTouchInput);
                    EvaluationThread.IsBackground = true;
                    EvaluationThread.Name = "BrailleDisNet.EvaluationThread";
                    EvaluationThread.Start();

                    //FireEventThread = new Thread(FireEventThreadImpl);
                    //FireEventThread.Name = "BrailleDisNet.FireEventThread";
                    //FireEventThread.IsBackground = true;
                    //FireEventThread.Start();
                    ThreadPool.RegisterWaitForSingleObject(fireEventsFlag, FireEventThreadImpl, null, -1, false);
                }
            }
        }

        /// <summary>
        /// Stops the touch evaluation (stops automatic touch input reading).
        /// </summary>
        public void StopTouchEvaluation()
        {
            TouchInputRunning = false;

            #region Check killing of EvaluationThread
            if (EvaluationThread != null)
            {
                while (!EvaluationThread.Join(5000))
                {
                    EvaluationThread.Abort();
                }
                EvaluationThread = null;
            }
            #endregion
            lock (_internalMessageQueue)
            {
                _internalMessageQueue.Clear();
            }
        }

        #region Braille Display Properties
        public Device UsedDevice { get; private set; }


        /// <summary>
        /// Gets or sets the touch threshold.
        /// </summary>
        /// <value>The touch threshold.</value>
        public int TouchThreshold
        {
            get
            {
                //Das ist doch nur ein int-Wert
                //Was sollte hier schon passerieren - Lock - Wozu. Man riskiert nur einen Deadlock,
                //Weil sendente Threads mit den Empangenden Probleme machen. VHA
                //lock (synchLock)
                {
                    return this.m_touch_threshold;
                }
            }
            set
            {
                //Das ist doch nur ein int-Wert
                //Was sollte hier schon passerieren - Lock - Wozu. Man riskiert nur einen Deadlock,
                //Weil sendente Threads mit den Empangenden Probleme machen. VHA
                //lock (synchLock)
                {
                    //Wert wird gelockt, damit er sich nicht während der Verarbeitung eines
                    //einzelnen Paket ändert.
                    this.m_touch_threshold = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the touch threshold. Wenn sich bei der Änderung seit dem letzten Werteabruf der Wert um 
        /// TouchThresholdExceptional geändert hat, dann wird der Wert gesendet, selbst wenn er kleiner ist, als 
        /// das TouchThreshold
        /// </summary>
        /// <value>The touch threshold.</value>
        //public int TouchThresholdExceptional
        //{
        //    get
        //    {
        //        lock (synchLock)
        //        {
        //            return this.m_touch_threshold_exception;
        //        }
        //    }
        //    set
        //    {
        //        lock (synchLock)
        //        {
        //            //Wert wird gelockt, damit er sich nicht während der Verarbeitung eines
        //            //einzelnen Paket ändert.
        //            this.m_touch_threshold_exception = value;
        //        }
        //    }
        //}

        public DeviceTypeInformation DeviceTypeInformation
        {
            get
            {
                return this.UsedDevice.DeviceTypeInformation;
            }
        }

        #endregion


        #endregion

    }
}
