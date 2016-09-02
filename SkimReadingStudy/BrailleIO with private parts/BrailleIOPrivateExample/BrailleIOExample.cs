using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrailleIO;
using System.Drawing;
using BrailleIO.Interface;
using System.Collections.Specialized;
using System.Windows.Forms;
using Gestures.Recognition.Preprocessing;
using Gestures.Recognition;
using Gestures.Recognition.GestureData;
using Gestures.Recognition.Interfaces;

namespace BraillIOExample
{
    public class BrailleIOExampleWithBrailleDis : IDisposable
    {
        #region Members

        AbstractBrailleIOAdapterBase showOffAdapter;
        AbstractBrailleIOAdapterBase brailleDisAdapter;
        public BrailleIOMediator IO { get; private set; }
        public IBrailleIOShowOffMonitor Monitor { get; private set; }

        Timer captuteTimer = new Timer();

        GestureRecognizer showOffGestureRecognizer;
        GestureRecognizer brailleDisGestureRecognizer;

        #endregion

        public BrailleIOExampleWithBrailleDis()
        {
            initTimer();
            IO = BrailleIOMediator.Instance;
            IO.AdapterManager = new ShowOffBrailleIOAdapterManager();

            //create Debug pin device emulator
            getShowOff();

            // create BrailleDis device
            createBrailleDis();

            // register Gesturerecognizer to the devices
            showOffGestureRecognizer = registerGestureRecognizer(showOffAdapter);
            brailleDisGestureRecognizer = registerGestureRecognizer(brailleDisAdapter);

            /* use this gesture recognizers as follows:
             * to interpret gestures you have to add touch matrices for evaluation --> start adding those matrices e.g. while a special button is pressed
             * 
             * 1. activate the touch tracking:
             *      yourGestureRecognizer.StartEvaluation();
             *       
             * 2. add the touch values through
             *      2.1. build a Frame that the gesture recognizer can understand:
             *          Frame f = new Frame(double[,] sampleSet);
             *      2.2 add the frame to the recognizer
             *          yourGestureRecognizer.AddFrame(f);
             * 
             * Do this for every whole touch value matrix you receive from the device
             * 
             * After the special button is released, stop tracking and finish the gesture evaluation for the set of touch frames
             * 
             * 3. IClassificationResult gesture = yourGestureRecognizer.FinishEvaluation();
             * 
             * Your done!
             * */
            
            showExample();
        }

        #region Gesture Recognizer

        #region private Fields

        /// <summary>
        /// flag for decision if touch values of the ShowOff adapter should bee interpreted by the related gesture recognizer
        /// </summary>
        volatile bool interpretShowOfGesture = false;
        /// <summary>
        /// flag for decision if touch values of the BraileDis adapter should bee interpreted by the related gesture recognizer
        /// </summary>
        volatile bool interpretBrailleDisGesture = false;

        #endregion

        /// <summary>
        /// gesture recognizer registration for the device
        /// </summary>
        /// <param name="adapter">The adapter.</param>
        /// <returns>A gesture recognizer for recognizing gestures</returns>
        private GestureRecognizer registerGestureRecognizer(AbstractBrailleIOAdapterBase adapter)
        {
            if (adapter != null)
            {
                var blobTracker = new BlobTracker();
                var gestureRecognizer = new GestureRecognizer(blobTracker);
                                
                var tabClassifier = new TapClassifier();
                var multitouchClassifier = new MultitouchClassifier();

                // add several classifiers to interpret the tracked blobs
                gestureRecognizer.AddClassifier(tabClassifier);
                gestureRecognizer.AddClassifier(multitouchClassifier);
                
                // start tracking fo blobs
                blobTracker.InitiateTracking();
                return gestureRecognizer;
            }

            return null;
        }



        /// <summary>
        /// Recognizes the gesture.
        /// </summary>
        /// <param name="sender">The sender.</param>
        private void recognizeGesture(IBrailleIOAdapter sender)
        {
            IClassificationResult gesture = null;
            if (sender != null)
            {
                if (sender == brailleDisAdapter && brailleDisGestureRecognizer != null)
                {
                    gesture = brailleDisGestureRecognizer.FinishEvaluation();
                }
                else if (sender == showOffAdapter && showOffGestureRecognizer != null)
                {
                    gesture = showOffGestureRecognizer.FinishEvaluation();
                }
            }

            //TODO: do whatever you want with this gesture result
            if (Monitor != null)
            {
                if (gesture != null) { Monitor.SetStatusText("GESTURE form '" + sender + "' :" + gesture.ToString()); }
                else { Monitor.SetStatusText("No gesture recognized"); }
            }

        }

        #endregion

        #region Timer

        private void initTimer()
        {
            captuteTimer.Interval = 100;
            captuteTimer.Start();
            captuteTimer.Tick += new EventHandler(captuteTimer_Tick);
        }

        void captuteTimer_Tick(object sender, EventArgs e)
        {
            updateScreenshotInCenterVr();
        }

        #endregion

        #region Screen capturing

        private Image captureScreen()
        {
            // capture entire screen, and save it to a file
            Image bmp = ScreenCapture.CaptureScreen();
            return bmp;
        }

        internal void updateScreenshotInCenterVr()
        {
            if (IO != null)
            {
                var v = IO.GetView(BS_MAIN_NAME) as BrailleIOScreen;
                if (v != null)
                {
                    var cs = v.GetViewRange("center");
                    if (cs != null)
                    {
                        cs.SetBitmap(captureScreen());
                        IO.RefreshDisplay(true);
                    }
                }
            }
        }
        #endregion

        #region Adapters

        #region ShowOff

        private void getShowOff()
        {
            if (IO != null)
            {
                // if the current Adapter manager holds an debug dapter, use it
                if (IO.AdapterManager is ShowOffBrailleIOAdapterManager)
                {
                    Monitor = ((ShowOffBrailleIOAdapterManager)IO.AdapterManager).Monitor;
                    foreach (var adapter in IO.AdapterManager.GetAdapters())
                    {
                        if (adapter is BrailleIOAdapter_ShowOff)
                        {
                            showOffAdapter = adapter as AbstractBrailleIOAdapterBase;
                            break;
                        }
                    }
                }

                // if no debug device currently exists, create a new one
                if (showOffAdapter == null)
                {
                    Monitor = new ShowOff();
                    showOffAdapter = Monitor.GetAdapter(IO.AdapterManager);
                    if (showOffAdapter != null) IO.AdapterManager.AddAdapter(showOffAdapter);
                }

                // if a debug adapter could been created, register to its events
                if (showOffAdapter != null)
                {
                    showOffAdapter.Synch = true; // activate that this device receives the pin matrix of the active device, too.

                    #region events

                    showOffAdapter.touchValuesChanged += new EventHandler<BrailleIO_TouchValuesChanged_EventArgs>(_bda_touchValuesChanged);
                    showOffAdapter.keyStateChanged += new EventHandler<BrailleIO_KeyStateChanged_EventArgs>(_bda_keyStateChanged);

                    #endregion
                }

                if (Monitor != null)
                {
                    Monitor.Disposed += new EventHandler(monitor_Disposed);
                }

            }
        }

        #endregion

        #region BrailleDis

        private AbstractBrailleIOAdapterBase createBrailleDis()
        {
            if (IO != null && IO.AdapterManager != null)
            {
                brailleDisAdapter = new BrailleIOBrailleDisAdapter.BrailleIOAdapter_BrailleDisNet(IO.AdapterManager);
                IO.AdapterManager.ActiveAdapter = brailleDisAdapter;

                #region BrailleDis events
                brailleDisAdapter.touchValuesChanged += new EventHandler<BrailleIO_TouchValuesChanged_EventArgs>(_bda_touchValuesChanged);
                brailleDisAdapter.keyStateChanged += new EventHandler<BrailleIO_KeyStateChanged_EventArgs>(_bda_keyStateChanged);
                #endregion

                return brailleDisAdapter;
            }
            return null;
        }

        #region Events

        #region BrailleDis

        void _bda_keyStateChanged(object sender, BrailleIO.Interface.BrailleIO_KeyStateChanged_EventArgs e)
        {
            //throw new NotImplementedException();
            interpretGeneralButtons(e.keyCode, sender as IBrailleIOAdapter);
            if ((e.keyCode & BrailleIO_DeviceButtonStates.Unknown) == BrailleIO_DeviceButtonStates.Unknown
                || ((e.keyCode & BrailleIO_DeviceButtonStates.None) == BrailleIO_DeviceButtonStates.None && e.raw != null)
                ) { interpretGenericButtons(sender, e.raw); }

            monitorKeys(e.raw);

        }
        /// <summary>
        /// Shows the pressed an released keys from the Braille dis device at the debug monitor
        /// </summary>
        /// <param name="e">the ordered dictonary of the raw data sent form the braille dis device</param>
        private void monitorKeys(OrderedDictionary e)
        {
            if (e != null && Monitor != null)
            {
                if (e.Contains("allPressedKeys"))
                {
                    var keys = e["allPressedKeys"] as List<String>;
                    Monitor.MarkButtonAsPressed(keys);
                }

                if (e.Contains("allReleasedKeys"))
                {
                    var keys = e["allReleasedKeys"] as List<String>;
                    Monitor.UnmarkButtons(keys);
                }
            }
        }

        void _bda_touchValuesChanged(object sender, BrailleIO.Interface.BrailleIO_TouchValuesChanged_EventArgs e)
        {
            if (e != null)
            {
                if (Monitor != null) Monitor.PaintTouchMatrix(e.touches);

                // add touches to the gesture recognizers
                if (sender == brailleDisAdapter && interpretBrailleDisGesture && brailleDisGestureRecognizer != null)
                {
                    brailleDisGestureRecognizer.AddFrame(new Frame(e.touches));
                }
                else if (sender == showOffAdapter && interpretShowOfGesture && showOffGestureRecognizer != null)
                {
                    showOffGestureRecognizer.AddFrame(new Frame(e.touches));
                }
            }

        }

        #endregion

        #region ShowOff

        void monitor_Disposed(object sender, EventArgs e)
        {
            this.Dispose();
        }

        #endregion


        #endregion

        #region Button Interpretation & Functions

        const String BS_MAIN_NAME = "Mainscreen";
        /// <summary>
        /// Interprets the generic buttons. This are buttons that are not modeled as one of the standard buttons.
        /// This buttons have to be extracted from the raw data sent by the corresponding device and his 
        /// interpreting adapter implementation
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="orderedDictionary">The ordered dictionary.</param>
        private void interpretGenericButtons(object sender, System.Collections.Specialized.OrderedDictionary orderedDictionary)
        {
            List<String> pressedKeys = new List<String>();
            List<String> releasedKeys = new List<String>();

            // here you have to check for what kind of device which buttons are placed in the raw data sent by the device

            if (sender is BrailleIO.BrailleIOAdapter_ShowOff)
            {
                //interpret the raw data as data from as ShowOffAdapter
                pressedKeys = orderedDictionary["pressedKeys"] as List<String>;
                releasedKeys = orderedDictionary["releasedKeys"] as List<String>;
            }
            else if (sender is BrailleIOBrailleDisAdapter.BrailleIOAdapter_BrailleDisNet)
            {
                //interpret the raw data as data from as BrailleDis device
                pressedKeys = orderedDictionary["allPressedKeys"] as List<String>;
                releasedKeys = orderedDictionary["allReleasedKeys"] as List<String>;
            }
            else
            {
                // ... check for other device types
            }

            if (pressedKeys != null && pressedKeys.Count > 0)
            {
                if (pressedKeys.Contains("crc")) { zoomToRealSize(sender); }
                if (pressedKeys.Contains("rsru")) { updateContrast(BS_MAIN_NAME, "center", 10); }
                if (pressedKeys.Contains("rsrd")) { updateContrast(BS_MAIN_NAME, "center", -10); }
            }

            if (releasedKeys != null && releasedKeys.Count > 0)
            {
                if (releasedKeys.Contains("nsrr")) { moveHorizontal(BS_MAIN_NAME, "center", -25); }
                if (releasedKeys.Contains("nsr")) { moveHorizontal(BS_MAIN_NAME, "center", -5); }
                if (releasedKeys.Contains("nsll")) { moveHorizontal(BS_MAIN_NAME, "center", 25); }
                if (releasedKeys.Contains("nsl")) { moveHorizontal(BS_MAIN_NAME, "center", 5); }
                if (releasedKeys.Contains("nsuu")) { moveVertical(BS_MAIN_NAME, "center", 25); }
                if (releasedKeys.Contains("nsu")) { moveVertical(BS_MAIN_NAME, "center", 5); }
                if (releasedKeys.Contains("nsdd")) { moveVertical(BS_MAIN_NAME, "center", -25); }
                if (releasedKeys.Contains("nsd")) { moveVertical(BS_MAIN_NAME, "center", -5); }

            }

        }

        private void updateContrast(string viewName, string viewRangeName, int factor)
        {
            if (IO == null && IO.GetView(viewName) as BrailleIOScreen != null) return;
            // zoom in
            BrailleIOViewRange vr = ((BrailleIOScreen)IO.GetView(viewName)).GetViewRange(viewRangeName);
            if (vr != null)
            {
                vr.SetContrastThreshold(vr.GetContrastThreshold() + factor);
            }
            IO.RenderDisplay();
        }

        private void zoomToRealSize(object sender)
        {
            IBrailleIOAdapter adapter = sender as IBrailleIOAdapter;
            if (adapter != null)
            {
                float adapterRes = Utils.GetResoultion(adapter.DpiX, adapter.DpiY);
                adapterRes = 10.0f;
                float screenRes = Utils.GetScreenDpi();

                float zoom = adapterRes / (float)Math.Max(screenRes, 0.0000001);
                zoom = 0.10561666418313964f;
                zoomTo(BS_MAIN_NAME, "center", zoom);

            }
        }



        #endregion

        #region Helper functions
        private void interpretGeneralButtons(BrailleIO_DeviceButtonStates states, IBrailleIOAdapter sender)
        {
            if (states != BrailleIO_DeviceButtonStates.None)
            {
                if ((states & BrailleIO_DeviceButtonStates.AbortDown) == BrailleIO_DeviceButtonStates.AbortDown) { invertImage(BS_MAIN_NAME, "center"); }
                else if ((states & BrailleIO_DeviceButtonStates.AbortUp) == BrailleIO_DeviceButtonStates.AbortUp) { }

                if ((states & BrailleIO_DeviceButtonStates.DownDown) == BrailleIO_DeviceButtonStates.DownDown) { moveVertical(BS_MAIN_NAME, "center", -5); }
                else if ((states & BrailleIO_DeviceButtonStates.DownUp) == BrailleIO_DeviceButtonStates.DownUp) { }

                if ((states & BrailleIO_DeviceButtonStates.EnterDown) == BrailleIO_DeviceButtonStates.EnterDown) { }
                else if ((states & BrailleIO_DeviceButtonStates.EnterUp) == BrailleIO_DeviceButtonStates.EnterUp) { }

                if ((states & BrailleIO_DeviceButtonStates.GestureDown) == BrailleIO_DeviceButtonStates.GestureDown)
                {
                    /*if (io != null) { io.AllPinsDown(); }*/
                    if (sender == brailleDisAdapter) { interpretBrailleDisGesture = true; }
                    if (sender == showOffAdapter) { interpretShowOfGesture = true; }
                }
                else if ((states & BrailleIO_DeviceButtonStates.GestureUp) == BrailleIO_DeviceButtonStates.GestureUp) { 
                    /*if (io != null) { io.RestoreLastRendering(); }*/
                    // evaluate the result
                    if (sender == brailleDisAdapter) { interpretBrailleDisGesture = false; }
                    if (sender == showOffAdapter) { interpretShowOfGesture = false; }
                    recognizeGesture(sender);
                }

                if ((states & BrailleIO_DeviceButtonStates.LeftDown) == BrailleIO_DeviceButtonStates.LeftDown) { moveHorizontal(BS_MAIN_NAME, "center", 5); }
                else if ((states & BrailleIO_DeviceButtonStates.LeftUp) == BrailleIO_DeviceButtonStates.LeftUp) { }

                if ((states & BrailleIO_DeviceButtonStates.RightDown) == BrailleIO_DeviceButtonStates.RightDown) { moveHorizontal(BS_MAIN_NAME, "center", -5); }
                else if ((states & BrailleIO_DeviceButtonStates.RightUp) == BrailleIO_DeviceButtonStates.RightUp) { }

                if ((states & BrailleIO_DeviceButtonStates.UpDown) == BrailleIO_DeviceButtonStates.UpDown) { moveVertical(BS_MAIN_NAME, "center", 5); }
                else if ((states & BrailleIO_DeviceButtonStates.UpUp) == BrailleIO_DeviceButtonStates.UpUp) { }

                if ((states & BrailleIO_DeviceButtonStates.ZoomInDown) == BrailleIO_DeviceButtonStates.ZoomInDown)
                {
                    zoom(BS_MAIN_NAME, "center", 1.3);
                    //zoomPlus(BS_MAIN_NAME, "center", 0.00005); 
                }
                else if ((states & BrailleIO_DeviceButtonStates.ZoomInUp) == BrailleIO_DeviceButtonStates.ZoomInUp) { }

                if ((states & BrailleIO_DeviceButtonStates.ZoomOutDown) == BrailleIO_DeviceButtonStates.ZoomOutDown)
                {
                    //zoomPlus(BS_MAIN_NAME, "center", -0.00005);
                    zoom(BS_MAIN_NAME, "center", 0.6);
                }
                else if ((states & BrailleIO_DeviceButtonStates.ZoomOutUp) == BrailleIO_DeviceButtonStates.ZoomOutUp) { }
            }
        }
        
        #endregion

        #endregion

        #endregion

        #region Functions

        IBrailleIOAdapter getActiveAdapter()
        {
            if (IO != null && IO.AdapterManager != null)
            {
                return IO.AdapterManager.ActiveAdapter;
            }
            return null;
        }

        void zoom(string viewName, string viewRangeName, double factor)
        {
            if (IO == null && IO.GetView(viewName) as BrailleIOScreen != null) return;
            // zoom in
            BrailleIOViewRange vr = ((BrailleIOScreen)IO.GetView(viewName)).GetViewRange(viewRangeName);
            if (vr != null)
            {
                if (vr.GetZoom() > 0)
                {
                    //TODO: make zoom to center
                    var oldZoom = vr.GetZoom();
                    var newZoom = oldZoom * factor;
                    var oldvrdin = vr.ViewBox;
                    Point oldcenter = new Point(
                        (int)Math.Round(((double)oldvrdin.Width / 2) + (vr.GetXOffset() * -1)),
                        (int)Math.Round(((double)oldvrdin.Height / 2) + (vr.GetYOffset() * -1))
                        );

                    Point newCenter = new Point(
                        (int)Math.Round(oldcenter.X * newZoom / oldZoom),
                        (int)Math.Round(oldcenter.Y * newZoom / oldZoom)
                        );

                    Point newOffset = new Point(
                        (int)Math.Round((newCenter.X - ((double)oldvrdin.Width / 2)) * -1),
                        (int)Math.Round((newCenter.Y - ((double)oldvrdin.Height / 2)) * -1)
                        );

                    vr.SetZoom(newZoom);

                    vr.SetXOffset(newOffset.X);
                    vr.SetYOffset(newOffset.Y);
                }
            }

            this.
            IO.RenderDisplay();
        }
        void zoomTo(string viewName, string viewRangeName, double factor)
        {
            if (IO == null && IO.GetView(viewName) as BrailleIOScreen != null) return;
            // zoom in
            BrailleIOViewRange vr = ((BrailleIOScreen)IO.GetView(viewName)).GetViewRange(viewRangeName);
            if (vr != null)
            {
                //TODO: make zoom to center
                var oldZoom = vr.GetZoom();
                var newZoom = factor;
                var oldvrdin = vr.ViewBox;
                Point oldcenter = new Point(
                    (int)Math.Round(((double)oldvrdin.Width / 2) + (vr.GetXOffset() * -1)),
                    (int)Math.Round(((double)oldvrdin.Height / 2) + (vr.GetYOffset() * -1))
                    );

                Point newCenter = new Point(
                    (int)Math.Round(oldcenter.X * newZoom / oldZoom),
                    (int)Math.Round(oldcenter.Y * newZoom / oldZoom)
                    );

                Point newOffset = new Point(
                    (int)Math.Round((newCenter.X - ((double)oldvrdin.Width / 2)) * -1),
                    (int)Math.Round((newCenter.Y - ((double)oldvrdin.Height / 2)) * -1)
                    );

                vr.SetZoom(newZoom);

                vr.SetXOffset(newOffset.X);
                vr.SetYOffset(newOffset.Y);
            }
            IO.RenderDisplay();
        }
        void zoomPlus(string viewName, string viewRangeName, double factor)
        {
            if (IO == null && IO.GetView(viewName) as BrailleIOScreen != null) return;
            // zoom in
            BrailleIOViewRange vr = ((BrailleIOScreen)IO.GetView(viewName)).GetViewRange(viewRangeName);
            if (vr != null)
            {
                if (vr.GetZoom() > 0)
                {
                    //TODO: make zoom to center
                    var oldZoom = vr.GetZoom();
                    var newZoom = oldZoom + factor;
                    var oldvrdin = vr.ViewBox;
                    Point oldcenter = new Point(
                        (int)Math.Round(((double)oldvrdin.Width / 2) + (vr.GetXOffset() * -1)),
                        (int)Math.Round(((double)oldvrdin.Height / 2) + (vr.GetYOffset() * -1))
                        );

                    Point newCenter = new Point(
                        (int)Math.Round(oldcenter.X * newZoom / oldZoom),
                        (int)Math.Round(oldcenter.Y * newZoom / oldZoom)
                        );

                    Point newOffset = new Point(
                        (int)Math.Round((newCenter.X - ((double)oldvrdin.Width / 2)) * -1),
                        (int)Math.Round((newCenter.Y - ((double)oldvrdin.Height / 2)) * -1)
                        );

                    vr.SetZoom(newZoom);

                    vr.SetXOffset(newOffset.X);
                    vr.SetYOffset(newOffset.Y);
                }
            }
            IO.RenderDisplay();
        }

        void moveHorizontal(string viewName, string viewRangeName, int steps)
        {
            if (IO == null && IO.GetView(viewName) as BrailleIOScreen != null) return;
            BrailleIOViewRange vr = ((BrailleIOScreen)IO.GetView(viewName)).GetViewRange(viewRangeName);
            if (vr != null)
            {
                vr.MoveHorizontal(steps);
            }
            IO.RenderDisplay();
        }

        void moveVertical(string viewName, string viewRangeName, int steps)
        {
            if (IO == null && IO.GetView(viewName) as BrailleIOScreen != null) return;
            BrailleIOViewRange vr = ((BrailleIOScreen)IO.GetView(viewName)).GetViewRange(viewRangeName);
            if (vr != null)
            {
                vr.MoveVertical(steps);
            }
            IO.RenderDisplay();
        }

        private void invertImage(string viewName, string viewRangeName)
        {
            if (IO == null && IO.GetView(viewName) as BrailleIOScreen != null) return;
            BrailleIOViewRange vr = ((BrailleIOScreen)IO.GetView(viewName)).GetViewRange(viewRangeName);
            if (vr != null)
            {
                vr.InvertImage = !vr.InvertImage;
            }
            IO.RenderDisplay();
        }

        #endregion

        #region Example
        //     string path = "";
        private void showExample()
        {
            BrailleIOScreen s = new BrailleIOScreen();
            #region Center Region

            #region screenshot
            Image bmp = captureScreen();
            #endregion

            BrailleIOViewRange center = new BrailleIOViewRange(0, 0, 120, 60, new bool[120, 40]);
            center.SetMargin(7, 0, 0);
            center.Move(1, 1);

            center.SetBitmap(bmp);

            center.SetZoom(-1);
            center.SetBorder(0);
            center.SetContrastThreshold(150);

            s.AddViewRange("center", center);

            #endregion

            #region Top Region
            BrailleIOViewRange top = new BrailleIOViewRange(0, 0, 120, 7, new bool[0, 0]);

            top.SetBorder(0, 0, 1);
            top.SetMargin(0, 0, 1);
            top.SetPadding(0, 0, 1);

            top.SetText("ABCDEFGHIJKLMNOPQRSTUVWXYZ\r\nabcdefghijklmnopqrstuvwxyz\r\n0123456789!\"#$%&<=>?@©®\r\n*+-~:;[],.'^_`(){}/|\\r\nß\r\n%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%\r\n");
            top.SetText("Tactile screen capture");
            s.AddViewRange("top", top);

            #endregion

            IO.AddView(BS_MAIN_NAME, s);
            IO.ShowView(BS_MAIN_NAME);
            IO.RenderDisplay();
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Occurs when this example is [disposing].
        /// </summary>
        public event EventHandler Disposing;

        public void Dispose()
        {
            if (Monitor != null) { try { Monitor.Dispose(); } catch { } }
            if (brailleDisAdapter != null) { try { brailleDisAdapter.Disconnect(); } catch { } }
            try { Application.Exit(); }
            catch { }
            fireDisposingEvent();
        }

        void fireDisposingEvent()
        {
            if (Disposing != null)
            {
                try { Disposing.Invoke(this, new EventArgs()); }
                catch { }
            }
        }

        #endregion
    }
}
