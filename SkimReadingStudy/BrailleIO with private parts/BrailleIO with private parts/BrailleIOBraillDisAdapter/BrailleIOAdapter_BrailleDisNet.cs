using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using HyperBraille.HBBrailleDis;
using BrailleIO.Interface;
using BrailleIO;

namespace BrailleIOBrailleDisAdapter
{
    public class BrailleIOAdapter_BrailleDisNet : AbstractBrailleIOAdapterBase
    {
        #region Members

        override public float DpiX { get { return 10.16f; } }
        override public float DpiY { get { return 10.16f; } }

        IBrailleIOAdapterManager manager;

        /// <summary>
        /// thread for creating and connecting a BrailleDis
        /// </summary>
        volatile Thread connectionThread;
        /// <summary>
        /// flag for causing the threads and so on to stop
        /// </summary>
        private volatile bool _run = true;

        private volatile bool isCalibrating = false;

        /// <summary>
        /// last received Matrix to send to the device
        /// </summary>
        private volatile bool[,] LastMatrix;

        /// <summary>
        /// marker of showing and identifying errors while working with the BrailleDis
        /// </summary>
        private volatile ErrorCode _error = ErrorCode.NONE;


        private volatile string type = "";

        private volatile BrailleDisNet _bdn = null;
        private readonly Object bdnLock = new Object();
        /// <summary>
        /// Gets the BrailleDis.Net Object.
        /// </summary>
        /// <value>BrailleDis.Net Object</value>
        protected BrailleDisNet BrailleDis
        {
            get
            {
                lock (bdnLock)
                {
                    if (_bdn == null
                        //|| !_bdn.DeviceIsInitialized
                                        )
                    {
                        try
                        {
                            if (connectionThread == null || !connectionThread.IsAlive)
                            {
                                connectionThread = new Thread(tryConnectBrailleDis);
                                connectionThread.Name = "BrailleDisConnectionThread";
                                connectionThread.IsBackground = true;
                                connectionThread.Start();
                            }
                        }
                        catch (System.Threading.ThreadStartException) { }
                        catch (System.Threading.ThreadStateException) { }
                    }
                    else if (_bdn.UsedDevice == null
                        || _bdn.UsedDevice.DeviceNumber < 0
                        //|| _error != ErrorCode.NONE
                        )
                    {
                        if (!isCalibrating)
                        {
                            _error = ErrorCode.NONE;
                            closeBrailleDis(_bdn);
                            _bdn = null;
                            return BrailleDis;
                        }
                    }
                    return _bdn;
                }
            }
            set
            {
                lock (bdnLock)
                {
                    closeBrailleDis(_bdn);
                    _bdn = value;
                }
            }
        }
        
        private void closeBrailleDis(BrailleDisNet _bdn)
        {
            if (_bdn != null)
            {
                unregisterEventListener(_bdn);
                try
                {
                    _bdn.StopTouchEvaluation();
                    _bdn.Close();
                }
                catch (Exception)
                { } 
            }
        }

        #endregion

        public BrailleIOAdapter_BrailleDisNet(IBrailleIOAdapterManager manager)
            : base(manager)
        {
            this.manager = manager;
            if (BrailleDis != null)
            {
                BrailleDis.NewDeviceAttached += new Action<BrailleDisNet, DeviceInformation_T>(BrailleDis_NewDeviceAttached);
                this.Device = new BrailleIODevice(BrailleDisConsts.NUMBER_OF_PIN_COLUMNS, BrailleDisConsts.NUMBER_OF_PIN_ROWS, "BrailleDis_" + (BrailleDis.UsedDevice).DeviceInformation.SerialNo, true, true, 30, this);
            }
        }

        ~BrailleIOAdapter_BrailleDisNet()
        {
            try
            {
                _run = false;
                if (_bdn != null)
                {
                    _bdn.Close();
                }
            }
            catch { }
        }
        #region initalisation

        /// <summary>
        /// Tries to create and connect the BrailleDis.
        /// Should be started in separate thread because this takes some time.
        /// </summary>
        private void tryConnectBrailleDis()
        {
            int trys = 0;
            const int max = 30;
            if (BrailleDis == null)
            {
                try
                {
                    BrailleDis = new BrailleDisNet();
                }
                catch
                {
                    BrailleDis = null;
                    return;
                }
            }

            while (_run && (BrailleDis == null || !BrailleDis.DeviceIsInitialized || BrailleDis.UsedDevice == null))
            {
                if (!_run) return;
                if (trys++ >= max || BrailleDis == null)
                {
                    BrailleDis = null;
                    return;
                }
                if (BrailleDis != null && BrailleDis.UsedDevice != null) { BrailleDis.UsedDevice.OpenDevice(); }
            }

            Connect();
            Synchronize(LastMatrix);
            Thread.CurrentThread.Abort();
        }

        #endregion

        #region IBrailleIOAdapter Members
        public override void Synchronize(bool[,] m)
        {
            try
            {
                if (!LockPins && BrailleDis != null)
                {
                    var success = BrailleDis.SetCompleteMatrix(m, true);

                    if (!success)
                    {
                        _error = ErrorCode.CANT_SENT_MATRIX;
                    }
                }
            }
            catch
            {
                _error = ErrorCode.CANT_SENT_MATRIX;
            }
            finally
            {
                if (m != null) LastMatrix = m;
            }
        }

        public override bool Connect()
        {
            base.Connect();
            _run = true;
            if (BrailleDis != null)
            {
                this.Device = new BrailleIODevice(BrailleDisConsts.NUMBER_OF_PIN_COLUMNS, BrailleDisConsts.NUMBER_OF_PIN_ROWS, "BrailleDis_" + (BrailleDis.UsedDevice).DeviceInformation.SerialNo, true, true, 30, this);
                BrailleDis.StartTouchEvaluation(20); //Absolutely important. No Events will be throws from BrailleDis if the TouchEvaluation is not active!
                registerEventListener();                
                try
                {
                    type = BrailleDis.UsedDevice.DeviceInformation.DeviceType;
                }
                catch { }

                Thread.Sleep(400);
                fireInitialized(new BrailleIO_Initialized_EventArgs(Device));

                return true;
            }
            return false;
        }

        private void registerEventListener()
        {
            unregisterEventListener(BrailleDis);
            BrailleDis.ErrorOccured += new BrailleDisNet.ErrorOccuredHandler(BrailleDis_ErrorOccured);
            BrailleDis.inputChangedEvent += new BrailleDisNet.InputChangedEventHandler(BrailleDis_inputChangedEvent);
            BrailleDis.keyStateChangedEvent += new BrailleDisNet.KeyStateChangedEventHandler(BrailleDis_keyStateChangedEvent);
            BrailleDis.pinStateChangedEvent += new BrailleDisNet.PinStateChangeEventHandler(BrailleDis_pinStateChangedEvent);
            BrailleDis.touchValuesChangedEvent += new BrailleDisNet.TouchValuesChangedEventHandler(BrailleDis_touchValuesChangedEvent);
            BrailleDis.NewDeviceAttached += new Action<BrailleDisNet, DeviceInformation_T>(BrailleDis_NewDeviceAttached);
        }

        private void unregisterEventListener(BrailleDisNet BrailleDis)
        {
            if (BrailleDis != null)
            {
                BrailleDis.ErrorOccured -= new BrailleDisNet.ErrorOccuredHandler(BrailleDis_ErrorOccured);
                BrailleDis.inputChangedEvent -= new BrailleDisNet.InputChangedEventHandler(BrailleDis_inputChangedEvent);
                BrailleDis.keyStateChangedEvent -= new BrailleDisNet.KeyStateChangedEventHandler(BrailleDis_keyStateChangedEvent);
                BrailleDis.pinStateChangedEvent -= new BrailleDisNet.PinStateChangeEventHandler(BrailleDis_pinStateChangedEvent);
                BrailleDis.touchValuesChangedEvent -= new BrailleDisNet.TouchValuesChangedEventHandler(BrailleDis_touchValuesChangedEvent);
                BrailleDis.NewDeviceAttached -= new Action<BrailleDisNet, DeviceInformation_T>(BrailleDis_NewDeviceAttached);
            }
        }


        public override bool Disconnect()
        {
            _run = false;
            if (base.Disconnect())
            {
                return true;
            }
            return false;
        }

        protected override BrailleIODevice createDevice()
        {
            if (BrailleDis != null && BrailleDis.UsedDevice != null && BrailleDis.UsedDevice.DeviceTypeInformation != null)
                this.Device = new BrailleIODevice(BrailleDisConsts.NUMBER_OF_PIN_COLUMNS, BrailleDisConsts.NUMBER_OF_PIN_ROWS, "BrailleDis_" + (BrailleDis.UsedDevice).DeviceInformation.SerialNo, true, true, 30, this);
            return base.createDevice();
        }

        #endregion

        #region BrailleDis Event handler
        void BrailleDis_NewDeviceAttached(BrailleDisNet arg1, DeviceInformation_T arg2)
        {
            Thread.Sleep(200);
            fireInitialized(new BrailleIO_Initialized_EventArgs(Device));
            System.Diagnostics.Debug.WriteLine("new Device attached");
        }

        private void BrailleDis_ErrorOccured(BrailleDisNet.ErrorType e)
        {
            OrderedDictionary raw = new OrderedDictionary();
            raw.Add("errorCode", e);
            fireErrorOccured(ErrorCode.UNKNOWN, ref raw);

            //TODO: check when this 100% indicates that the device was disconnected
            //if (_bdn == null)
            //{
            //    fireClosed(new BrailleIO_Initialized_EventArgs(this.Device));
            //}
        }

        private void BrailleDis_inputChangedEvent(
                    bool touchInputAvailable,
                    int[,] valueMatrix,
                    BrailleDisKeyboard keyboardState,
                    int timeStampTickCount)
        {
            OrderedDictionary raw = new OrderedDictionary();
            raw.Add("touchInputAvailable", touchInputAvailable);
            raw.Add("valueMatrix", valueMatrix);
            raw.Add("keyBoardState", keyboardState);
            raw.Add("timeStampTickCount", timeStampTickCount);
            bool[,] touches = new bool[valueMatrix.GetLength(0), valueMatrix.GetLength(1)];
            for (int i = 0; i < valueMatrix.GetLength(0); i++)
                for (int j = 0; j < valueMatrix.GetLength(1); j++)
                    touches[i, j] = ((double)Math.Round((double)valueMatrix[i, j]) != 0) ? true : false;

            fireInputChanged(touches, timeStampTickCount, ref raw);
        }

        private void BrailleDis_keyStateChangedEvent(
                    BrailleDisKeyboard pressedKeys,
                    BrailleDisKeyboard releasedKeys,
                    BrailleDisKeyboard keyboardState,
                    int timeStampTickCount)
        {
            OrderedDictionary raw = new OrderedDictionary();
            raw.Add("pressedKeys", pressedKeys);
            raw.Add("releasedKeys", releasedKeys);
            raw.Add("keyboardState", keyboardState);
            raw.Add("timeStampTickCount", timeStampTickCount);
            raw.Add("allPressedKeys", BraillDisButtonInterpreter.toSingleBrailleKeyEventList(pressedKeys.AllKeys));
            raw.Add("allReleasedKeys", BraillDisButtonInterpreter.toSingleBrailleKeyEventList(releasedKeys.AllKeys));
            var bs = getButtonStates(raw);
            fireKeyStateChanged(bs, ref raw); //TODO: map generic buttons!
        }

        private static void BrailleDis_pinStateChangedEvent(BrailleDisPinState[] changedPins)
        {
            OrderedDictionary raw = new OrderedDictionary();
            raw.Add("changedPins", changedPins);
        }

        private void BrailleDis_touchValuesChangedEvent(
                    BrailleDisModuleState[] changedModules,
                    BrailleDisModuleState[] activeModules,
                    int timeStampTickCount)
        {
            var raw = new OrderedDictionary();
            raw.Add("changedModules", changedModules);
            raw.Add("activeModules", activeModules);
            if (BrailleDis != null && BrailleDis.UsedDevice != null)
            {
                var a = BrailleDisTouchHandler.getTouchMatrixCorrespondingToBrailleMatrix(activeModules, BrailleDis.UsedDevice.DeviceTypeInformation, true, touchCorrectionMatrix);
                this.fireTouchValuesChanged(a, timeStampTickCount, ref raw);
            }
        }
        #endregion

        /// <summary>
        /// Starts the touch evaluation.
        /// </summary>
        public void startTouch()
        {
            BrailleDis.StartTouchEvaluation(20);
        }

        public override bool Recalibrate(double threshold)
        {
            if (BrailleDis != null)
            {
                BrailleDis.Recalibrate();
                Thread.Sleep(20000);
                bool re = base.Recalibrate(threshold);
                return re; 
            }
            return false;
        }

        #region Utils
        /// <summary>
        /// Gets BrailleIO_DeviceButtonStates flag int for the pressed and released keys.
        /// </summary>
        /// <param name="raw">The raw.</param>
        /// <returns></returns>
        private BrailleIO_DeviceButtonStates getButtonStates(OrderedDictionary raw)
        {
            BrailleIO_DeviceButtonStates b = BrailleIO_DeviceButtonStates.None;
            BrailleIO_DeviceButtonStates u = getButtonUpStates(raw);
            BrailleIO_DeviceButtonStates d = getButtonDownStates(raw);
            return b | u | d;
        }

        private const string DOWN_KEYS = "pressedKeys";
        /// <summary>
        /// Gets BrailleIO_DeviceButtonStates flag int for the pressed keys.
        /// </summary>
        /// <param name="raw">The raw data set containing a key [DOWN_KEYS].</param>
        /// <returns>combined BrailleIO_DeviceButtonStates</returns>
        private BrailleIO_DeviceButtonStates getButtonDownStates(OrderedDictionary raw)
        {
            BrailleIO_DeviceButtonStates b = BrailleIO_DeviceButtonStates.None;
            try
            {
                if (raw.Contains(DOWN_KEYS) && raw[DOWN_KEYS] is HyperBraille.HBBrailleDis.BrailleDisKeyboard)
                {
                    HyperBraille.HBBrailleDis.BrailleDisKeyboard allDownKey = (HyperBraille.HBBrailleDis.BrailleDisKeyboard)raw[DOWN_KEYS];
                    var buttons = BraillDisButtonInterpreter.toSingleBrailleKeyEventList(allDownKey.AllKeys);

                    switch (type)
                    {
                        case "1":
                            b = getBasicButtonDownStates(buttons);
                            break;
                        default:
                            b = getNewButtonDownStates(buttons);
                            break;
                    }
                }
            }
            catch { }
            return b;
        }

        private BrailleIO_DeviceButtonStates getBasicButtonDownStates(List<String> buttons)
        {
            BrailleIO_DeviceButtonStates b = BrailleIO_DeviceButtonStates.None;
            foreach (var button in buttons)
            {
                #region switch
                switch (button)
                {
                    case "k2":
                        b |= BrailleIO_DeviceButtonStates.EnterDown;
                        break;
                    case "k3":
                        b |= BrailleIO_DeviceButtonStates.AbortDown;
                        break;
                    case "l":
                        b |= BrailleIO_DeviceButtonStates.GestureDown;
                        break;
                    case "k4":
                        b |= BrailleIO_DeviceButtonStates.LeftDown;
                        break;
                    case "k8":
                        b |= BrailleIO_DeviceButtonStates.RightDown;
                        break;
                    case "k5":
                        b |= BrailleIO_DeviceButtonStates.UpDown;
                        break;
                    case "k6":
                        b |= BrailleIO_DeviceButtonStates.DownDown;
                        break;
                    case "k1":
                        b |= BrailleIO_DeviceButtonStates.ZoomInDown;
                        break;
                    case "k7":
                        b |= BrailleIO_DeviceButtonStates.ZoomOutDown;
                        break;
                    default:
                        b |= BrailleIO_DeviceButtonStates.Unknown;
                        break;
                }
                #endregion
            }
            return b;
        }

        private BrailleIO_DeviceButtonStates getNewButtonDownStates(List<String> buttons)
        {
            BrailleIO_DeviceButtonStates b = BrailleIO_DeviceButtonStates.None;
            foreach (var button in buttons)
            {
                #region switch
                switch (button)
                {
                    case "crc":
                        b |= BrailleIO_DeviceButtonStates.EnterDown;
                        break;
                    case "hbr":
                        b |= BrailleIO_DeviceButtonStates.AbortDown;
                        break;
                    case "hbl":
                        b |= BrailleIO_DeviceButtonStates.GestureDown;
                        break;
                    case "nsl":
                        b |= BrailleIO_DeviceButtonStates.LeftDown;
                        break;
                    case "nsr":
                        b |= BrailleIO_DeviceButtonStates.RightDown;
                        break;
                    case "nsu":
                        b |= BrailleIO_DeviceButtonStates.UpDown;
                        break;
                    case "nsd":
                        b |= BrailleIO_DeviceButtonStates.DownDown;
                        break;
                    case "rslu":
                        b |= BrailleIO_DeviceButtonStates.ZoomInDown;
                        break;
                    case "rsld":
                        b |= BrailleIO_DeviceButtonStates.ZoomOutDown;
                        break;
                    default:
                        b |= BrailleIO_DeviceButtonStates.Unknown;
                        break;
                }
                #endregion
            }
            return b;
        }
        private const string UP_KEYS = "releasedKeys";
        /// <summary>
        /// Gets BrailleIO_DeviceButtonStates flag int for the released keys.
        /// </summary>
        /// <param name="raw">The raw data set containing a key [UP_KEYS].</param>
        /// <returns>combined BrailleIO_DeviceButtonStates</returns>
        private BrailleIO_DeviceButtonStates getButtonUpStates(OrderedDictionary raw)
        {
            BrailleIO_DeviceButtonStates b = BrailleIO_DeviceButtonStates.None;
            try
            {
                if (raw.Contains(UP_KEYS) && raw[UP_KEYS] is HyperBraille.HBBrailleDis.BrailleDisKeyboard)
                {
                    HyperBraille.HBBrailleDis.BrailleDisKeyboard allDownKey = (HyperBraille.HBBrailleDis.BrailleDisKeyboard)raw[UP_KEYS];
                    var buttons = BraillDisButtonInterpreter.toSingleBrailleKeyEventList(allDownKey.AllKeys);

                    switch (type)
                    {
                        case "1":
                            b = getBasicButtonUpStates(buttons);
                            break;
                        default:
                            b = getNewButtonUpStates(buttons);
                            break;
                    }
                }
            }
            catch { }
            return b;
        }


        private BrailleIO_DeviceButtonStates getBasicButtonUpStates(List<String> buttons)
        {
            BrailleIO_DeviceButtonStates b = BrailleIO_DeviceButtonStates.None;
            foreach (var button in buttons)
            {
                #region switch
                switch (button)
                {
                    case "k2":
                        b |= BrailleIO_DeviceButtonStates.EnterUp;
                        break;
                    case "k3":
                        b |= BrailleIO_DeviceButtonStates.AbortUp;
                        break;
                    case "l":
                        b |= BrailleIO_DeviceButtonStates.GestureUp;
                        break;
                    case "k4":
                        b |= BrailleIO_DeviceButtonStates.LeftUp;
                        break;
                    case "k8":
                        b |= BrailleIO_DeviceButtonStates.RightUp;
                        break;
                    case "k5":
                        b |= BrailleIO_DeviceButtonStates.UpUp;
                        break;
                    case "k6":
                        b |= BrailleIO_DeviceButtonStates.DownUp;
                        break;
                    case "k1":
                        b |= BrailleIO_DeviceButtonStates.ZoomInUp;
                        break;
                    case "k7":
                        b |= BrailleIO_DeviceButtonStates.ZoomOutUp;
                        break;
                    default:
                        b |= BrailleIO_DeviceButtonStates.Unknown;
                        break;
                }
                #endregion
            }
            return b;
        }

        private BrailleIO_DeviceButtonStates getNewButtonUpStates(List<String> buttons)
        {
            BrailleIO_DeviceButtonStates b = BrailleIO_DeviceButtonStates.None;
            foreach (var button in buttons)
            {
                #region switch
                switch (button)
                {
                    case "crc":
                        b |= BrailleIO_DeviceButtonStates.EnterUp;
                        break;
                    case "hbr":
                        b |= BrailleIO_DeviceButtonStates.AbortUp;
                        break;
                    case "hbl":
                        b |= BrailleIO_DeviceButtonStates.GestureUp;
                        break;
                    case "nsl":
                        b |= BrailleIO_DeviceButtonStates.LeftUp;
                        break;
                    case "nsr":
                        b |= BrailleIO_DeviceButtonStates.RightUp;
                        break;
                    case "nsu":
                        b |= BrailleIO_DeviceButtonStates.UpUp;
                        break;
                    case "nsd":
                        b |= BrailleIO_DeviceButtonStates.DownUp;
                        break;
                    case "rslu":
                        b |= BrailleIO_DeviceButtonStates.ZoomInUp;
                        break;
                    case "rsld":
                        b |= BrailleIO_DeviceButtonStates.ZoomOutUp;
                        break;
                    default:
                        b |= BrailleIO_DeviceButtonStates.Unknown;
                        break;
                }
                #endregion
            }
            return b;
        }


        /// <summary>
        /// Transposes a matrix.
        /// </summary>
        /// <param name="m">The matrix that should be transposed.</param>
        /// <returns></returns>
        private bool[,] transposeMatrix(bool[,] m)
        {
            if (m == null) return m;
            bool[,] n = new bool[m.GetLength(1), m.GetLength(0)];
            for (int i = 0; i < m.GetLength(0); i++)
            {
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    n[j, i] = m[i, j];
                }
            }
            return n;
        }

        #endregion;
    }

    /// <summary>
    /// static class for interpreting BrailleDisNet buttons
    /// </summary>
    public static class BraillDisButtonInterpreter
    {
        internal static BrailleDisKeyState ks;
        private static TimeSpan _masking = new TimeSpan(0, 0, 0, 0, 200);
        private const ulong vKeys = BrailleDisConsts.KEY_NAV_UP_2 | BrailleDisConsts.KEY_NAV_UP | BrailleDisConsts.KEY_NAV_DOWN | BrailleDisConsts.KEY_NAV_DOWN_2;
        private const ulong hKeys = BrailleDisConsts.KEY_NAV_LEFT | BrailleDisConsts.KEY_NAV_LEFT_2 | BrailleDisConsts.KEY_NAV_RIGHT | BrailleDisConsts.KEY_NAV_RIGHT_2;
        /// <summary> Converts the Combo to a list of single BrailleKey events. </summary>
        /// <returns> Array of single BrailleKey events. </returns>
        public static List<String> toSingleBrailleKeyEventList(ulong AllKeys)
        {
            DateTime _now = DateTime.Now;
            List<String> l = new List<String>();

            if ((AllKeys & BrailleDisConsts.KEY_THUMB_RIGHT_HAND_RIGHT) == BrailleDisConsts.KEY_THUMB_RIGHT_HAND_RIGHT) l.Add("r");
            if ((AllKeys & BrailleDisConsts.KEY_THUMB_LEFT_HAND_LEFT) == BrailleDisConsts.KEY_THUMB_LEFT_HAND_LEFT) l.Add("l");
            if ((AllKeys & BrailleDisConsts.KEY_THUMB_RIGHT_HAND_LEFT) == BrailleDisConsts.KEY_THUMB_RIGHT_HAND_LEFT) l.Add("rl");
            if ((AllKeys & BrailleDisConsts.KEY_THUMB_LEFT_HAND_RIGHT) == BrailleDisConsts.KEY_THUMB_LEFT_HAND_RIGHT) l.Add("lr");
            if ((AllKeys & BrailleDisConsts.KEY_DOT1) == BrailleDisConsts.KEY_DOT1) l.Add("k1");
            if ((AllKeys & BrailleDisConsts.KEY_DOT2) == BrailleDisConsts.KEY_DOT2) l.Add("k2");
            if ((AllKeys & BrailleDisConsts.KEY_DOT3) == BrailleDisConsts.KEY_DOT3) l.Add("k3");
            if ((AllKeys & BrailleDisConsts.KEY_DOT4) == BrailleDisConsts.KEY_DOT4) l.Add("k4");
            if ((AllKeys & BrailleDisConsts.KEY_DOT5) == BrailleDisConsts.KEY_DOT5) l.Add("k5");
            if ((AllKeys & BrailleDisConsts.KEY_DOT6) == BrailleDisConsts.KEY_DOT6) l.Add("k6");
            if ((AllKeys & BrailleDisConsts.KEY_DOT7) == BrailleDisConsts.KEY_DOT7) l.Add("k7");
            if ((AllKeys & BrailleDisConsts.KEY_DOT8) == BrailleDisConsts.KEY_DOT8) l.Add("k8");
            if ((AllKeys & BrailleDisConsts.KEY_HYPERBRAILLE_KEY_LEFT) == BrailleDisConsts.KEY_HYPERBRAILLE_KEY_LEFT) l.Add("hbl");
            if ((AllKeys & BrailleDisConsts.KEY_LEFT_ROCKER_SWITCH_UP) == BrailleDisConsts.KEY_LEFT_ROCKER_SWITCH_UP) l.Add("rslu");
            if ((AllKeys & BrailleDisConsts.KEY_LEFT_ROCKER_SWITCH_DOWN) == BrailleDisConsts.KEY_LEFT_ROCKER_SWITCH_DOWN) l.Add("rsld");
            if ((AllKeys & BrailleDisConsts.KEY_LEFT_CURSORS_UP) == BrailleDisConsts.KEY_LEFT_CURSORS_UP) l.Add("clu");
            if ((AllKeys & BrailleDisConsts.KEY_LEFT_CURSORS_RIGHT) == BrailleDisConsts.KEY_LEFT_CURSORS_RIGHT) l.Add("clr");
            if ((AllKeys & BrailleDisConsts.KEY_LEFT_CURSORS_CENTER) == BrailleDisConsts.KEY_LEFT_CURSORS_CENTER) l.Add("clc");
            if ((AllKeys & BrailleDisConsts.KEY_LEFT_CURSORS_DOWN) == BrailleDisConsts.KEY_LEFT_CURSORS_DOWN) l.Add("cld");
            if ((AllKeys & BrailleDisConsts.KEY_LEFT_CURSORS_LEFT) == BrailleDisConsts.KEY_LEFT_CURSORS_LEFT) l.Add("cll");
            if ((AllKeys & BrailleDisConsts.KEY_HYPERBRAILLE_KEY_RIGHT) == BrailleDisConsts.KEY_HYPERBRAILLE_KEY_RIGHT) l.Add("hbr");
            if ((AllKeys & BrailleDisConsts.KEY_RIGHT_ROCKER_SWITCH_UP) == BrailleDisConsts.KEY_RIGHT_ROCKER_SWITCH_UP) l.Add("rsru");
            if ((AllKeys & BrailleDisConsts.KEY_RIGHT_ROCKER_SWITCH_DOWN) == BrailleDisConsts.KEY_RIGHT_ROCKER_SWITCH_DOWN) l.Add("rsrd");
            if ((AllKeys & BrailleDisConsts.KEY_RIGHT_CURSORS_UP) == BrailleDisConsts.KEY_RIGHT_CURSORS_UP) l.Add("cru");
            if ((AllKeys & BrailleDisConsts.KEY_RIGHT_CURSORS_RIGHT) == BrailleDisConsts.KEY_RIGHT_CURSORS_RIGHT) l.Add("crr");
            if ((AllKeys & BrailleDisConsts.KEY_RIGHT_CURSORS_CENTER) == BrailleDisConsts.KEY_RIGHT_CURSORS_CENTER) l.Add("crc");
            if ((AllKeys & BrailleDisConsts.KEY_RIGHT_CURSORS_DOWN) == BrailleDisConsts.KEY_RIGHT_CURSORS_DOWN) l.Add("crd");
            if ((AllKeys & BrailleDisConsts.KEY_RIGHT_CURSORS_LEFT) == BrailleDisConsts.KEY_RIGHT_CURSORS_LEFT) l.Add("crl");

            if ((AllKeys & vKeys) != 0)
            {
                ks.NCB_vertical = _now;
                if ((AllKeys & BrailleDisConsts.KEY_NAV_UP_2) == BrailleDisConsts.KEY_NAV_UP_2) l.Add("nsuu");
                else if ((AllKeys & BrailleDisConsts.KEY_NAV_UP) == BrailleDisConsts.KEY_NAV_UP) l.Add("nsu");

                if ((AllKeys & BrailleDisConsts.KEY_NAV_DOWN_2) == BrailleDisConsts.KEY_NAV_DOWN_2) l.Add("nsdd");
                else if ((AllKeys & BrailleDisConsts.KEY_NAV_DOWN) == BrailleDisConsts.KEY_NAV_DOWN) l.Add("nsd");
            }

            if ((AllKeys & hKeys) != 0)
            {
                ks.NCB_horizontal = _now;
                if ((AllKeys & BrailleDisConsts.KEY_NAV_RIGHT_2) == BrailleDisConsts.KEY_NAV_RIGHT_2) l.Add("nsrr");
                else if ((AllKeys & BrailleDisConsts.KEY_NAV_RIGHT) == BrailleDisConsts.KEY_NAV_RIGHT) l.Add("nsr");

                if ((AllKeys & BrailleDisConsts.KEY_NAV_LEFT_2) == BrailleDisConsts.KEY_NAV_LEFT_2) l.Add("nsll");
                else if ((AllKeys & BrailleDisConsts.KEY_NAV_LEFT) == BrailleDisConsts.KEY_NAV_LEFT) l.Add("nsl");
            }
            return l;
        }

        internal struct BrailleDisKeyState
        {
            #region Member
            public DateTime NCB_horizontal { get; set; }
            public DateTime NCB_vertical { get; set; }
            #endregion
        }
    }
}
