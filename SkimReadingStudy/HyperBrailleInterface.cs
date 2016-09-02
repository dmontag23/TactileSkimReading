using BrailleIO;
using BrailleIOBrailleDisAdapter;
using Gestures.Recognition.GestureData;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;

namespace SkimReadingStudy
{
    // manages HyperBraille functionality
    // i.e. initialize, refresh, listeners, etc.
    class HyperBrailleInterface
    {
        private BrailleIOMediator io = BrailleIOMediator.Instance;         // variable of implementation
        private BrailleIOAdapter_BrailleDisNet brailledis = null;          // main connection variable
        private BrailleIOScreen mainscreen = new BrailleIOScreen();        // mainscreen representing the screen on the HyperBraille device
        private BrailleIOViewRange viewRangeCurrentlySelected = null;      // keeps track of the view range currently selected on the device

        private GestureInterpreter gestinterp = new GestureInterpreter();  // interprets gestures
        private TextToSpeech txtToSpeech = null;                           // controls text to speech output
        private Selection select = new Selection();                        // selects the closest view range to the currently selected view range
        private Paper paperOnDisplay = null;                               // keeps track of the paper to interact with

        // Timers
        private System.Timers.Timer selectionTimer = new System.Timers.Timer(500);  // controls blinking on HyperBraille Device
        System.Timers.Timer gestureTimer = new System.Timers.Timer(100);            // controls how often touch points are interpreted into gestures on the HyperBraille

        // constructor - initialize the HyperBraille Device
        public HyperBrailleInterface()
        {
            AbstractBrailleIOAdapterManagerBase manager = new BasicBrailleIOAdapterManager();   // main instance manager
            if (manager != null)
            {
                brailledis = new BrailleIOAdapter_BrailleDisNet(manager);
                brailledis.touchValuesChanged += brailledis_touchValuesChanged;              // add a listener for gesture recognition
                if (io != null)
                {
                    io.AdapterManager = manager;
                }
            }

            io.AddView("Mainscreen", mainscreen);   // add the mainscreen to the HyperBraille Device
            mainscreen.AddViewRange("Blank Screen", new BrailleIOViewRange(0, 0, 120, 60));  // add a blank view to cover the entirety of the device

            selectionTimer.Elapsed += selectionTimer_Elapsed;   // set the method to determine what happens when the selection timer blinks
            gestureTimer.Elapsed += analyzeGesture_Elapsed;     // set the method to determine what happens when the gesture timer blinks
        }

        #region Setters

        // set the paper for the HyperBraille to interact with
        public void SetPaperOnDisplay(Paper paper)
        {
            this.paperOnDisplay = paper;
        }

        // set the text to speech class to use from the Form class
        public void SetTextToSpeech(TextToSpeech txtToSpeech)
        {
            this.txtToSpeech = txtToSpeech;
        }

        #endregion

        #region Getters

        // returns the paper on display
        public Paper GetPaperOnDisplay()
        {
            return paperOnDisplay;
        }

        // return the mainscreen of the HyperBraille 
        public BrailleIOScreen GetMainscreen()
        {
            return mainscreen;
        }

        // return the view range currently selected on the HyperBraille
        public BrailleIOViewRange GetViewRangeCurrentlySelected()
        {
            return viewRangeCurrentlySelected;
        }

        #endregion

        // given a <NameOfViewRange, ViewRange> pair, display these View Ranges on the HyperBraille Device
        public void DisplayViewRanges(OrderedDictionary viewRangesToDisplay)
        {
            foreach (DictionaryEntry de in viewRangesToDisplay)
            {
                mainscreen.AddViewRange(de.Key as String, de.Value as BrailleIOViewRange);
            }
            Refresh();
        }

        // given a View Range, select it
        public void SelectViewRange(BrailleIOViewRange viewRange)
        {
            txtToSpeech.CancelSpeaking();                     // cancel anything that is being spoken
            if (selectionTimer.Enabled == true) StopBlink();  // stop the blinking of any previously selected section
            viewRangeCurrentlySelected = viewRange;           // update the view range currently selected
            selectionTimer.Start();                           // start the blinking cycle on this new selected section
        }

        // given a point, select the View Range on the HyperBraille at that point, return true if a valid view range is found and false otherwise
        public bool SelectViewRangeAtPoint(Point selectedPoint)
        {
            OrderedDictionary allViewRanges = mainscreen.GetViewRanges();  // get all the view ranges currently displayed
            bool foundViewRangeAtPoint = false;  // set a flag to determine if a view range was found at the given Point

            // select the first view range that contains the selected Point
            for (int i = 1; i < allViewRanges.Count; i++)
            {
                BrailleIOViewRange viewRange = allViewRanges[i] as BrailleIOViewRange;
                Rectangle viewBox = viewRange.ViewBox;
                if (viewBox.Contains(selectedPoint))
                {
                    SelectViewRange(viewRange);
                    foundViewRangeAtPoint = true;
                    break;
                }
            }

            return foundViewRangeAtPoint;
        }

        // given "next" or "previous", select the new view range in the logical structure of the paper as determined in the page class
        public void SelectOrderedSectionToRead(String direction)
        {
            OrderedDictionary viewRanges = paperOnDisplay.GetViewRangesOfPage(paperOnDisplay.GetCurrentPageDisplayed());  // get all of the view ranges currently displayed
            int indexToSelect = 0;  // if no view range is currently selected, select the first one
            
            if (viewRangeCurrentlySelected != null)
            {
                indexToSelect = GetIndexFromOrderedDict(viewRangeCurrentlySelected, viewRanges); // get the index of the current view range in the dict that holds all of the currently displayed view ranges
                
                if (indexToSelect != -1) // ensures the viewRangeCurrentlySelected in displayed on the device 
                {
                    if (direction == "previous") indexToSelect--;   // decrement the index to go backwards
                    if (direction == "next") indexToSelect++;       // increment the index to go forwards
                }

                int pageOnDisplayBeforeFlip = paperOnDisplay.GetCurrentPageDisplayed();
                int maxPage = viewRanges.Count;
                
                if (indexToSelect == -1) FlipPage("previous");        // flip the page backward if the first item on the current page is selected and the user wants to go back
                else if (indexToSelect == maxPage) FlipPage("next");  // flip the page forward if the last item on the current page is selected and the user wants to go forward
                viewRanges = paperOnDisplay.GetViewRangesOfPage(paperOnDisplay.GetCurrentPageDisplayed());  // get the viewRanges of the new page after it has been flipped
                
                if (indexToSelect == -1)
                {
                    // if the user has selected the first item on the first page, keep that item selected
                    // otherwise, select the last item on the previous page
                    if (pageOnDisplayBeforeFlip != 1) indexToSelect = viewRanges.Count - 1;
                    else indexToSelect = 0;
                }
                
                else if (indexToSelect == maxPage)
                {
                    // if the user has selected the last item on the last page, keep that item selected
                    // otherwise, select the first item on the next page
                    if (pageOnDisplayBeforeFlip != paperOnDisplay.GetTotalNumberOfPages()) indexToSelect = 0;
                    else indexToSelect = viewRanges.Count - 1;
                }
            }
            
            SelectViewRange(viewRanges[indexToSelect] as BrailleIOViewRange);  // select the new view range
        }

        // speaks or pauses the speaking of the view range currently selected
        public void SpeakViewRangeCurrentlySelected()
        {
            if (viewRangeCurrentlySelected != null)
            {
                txtToSpeech.SpeakOrPauseText(paperOnDisplay.GetContent(viewRangeCurrentlySelected.Name));  // get the content of the view range currently selected and either pause or read it
            }
        }

        // given a direction to flip, flip the page on the HyperBraille device either forward or backward
        public void FlipPage(String direction)
        {
            // ensures the HyperBraille has a paper to interact with
            if (paperOnDisplay != null)
            {
                txtToSpeech.CancelSpeaking();    // cancel anything that is being spoken

                // flip the page back
                if (direction == "previous")
                {
                    // prevents the user from flipping back past the first page
                    if (paperOnDisplay.GetCurrentPageDisplayed() > 1)
                    {
                        // clear the device and update the views on the device
                        Clear();
                        DisplayViewRanges(paperOnDisplay.FlipToPreviousPage());
                    }
                }

                // flip the page forward
                if (direction == "next")
                {
                    // prevents the user from flipping forward past the last page
                    if (paperOnDisplay.GetCurrentPageDisplayed() < paperOnDisplay.GetTotalNumberOfPages())
                    {
                        // clear the device and update the views on the device
                        Clear();
                        DisplayViewRanges(paperOnDisplay.FlipToNextPage());
                    }
                }
            }
        }

        // deselect everything on the HyperBraille Device
        public void DeselectAll()
        {
            txtToSpeech.CancelSpeaking();                     // cancel anything that is speaking
            if (selectionTimer.Enabled == true) StopBlink();  // stop the blinking of any previously selected section
            viewRangeCurrentlySelected = null;                // update the view range currently selected
        }

        // clear all images on the HyperBraille Device
        public void Clear()
        {
            DeselectAll();               // ensure everything is deselected
            OrderedDictionary allViewRanges = mainscreen.GetViewRanges();  // get all of the view ranges currently displayed on the device

            // remove all of the view ranges from the device that are not the blank screen view range
            for (int i = 1; i < allViewRanges.Count; i++)
            {
                BrailleIOViewRange viewRangeToRemove = allViewRanges[i] as BrailleIOViewRange;
                mainscreen.RemoveViewRange(viewRangeToRemove.Name);
            }
            Refresh();
        }

        // given a pair in an ordered dict and that dict, find the index of that element - return -1 if that element is not in the dict
        private int GetIndexFromOrderedDict(Object pair, OrderedDictionary orderedDict)
        {
            for (int i = 0; i < orderedDict.Count; i++)
            {
                if (orderedDict[i] == pair) return i;
            }
            return -1;
        }

        // refresh the display on the HyperBraille Device
        private void Refresh()
        {
            if (io != null)
            {
                io.RefreshDisplay();
            }
        }

        #region Timer to Control Blinking

        bool visible = true;  // flag to determine whether the current item blinking is showing on the display or not

        // on tick, reverse the visibility of the item blinking on the HyperBraille
        void selectionTimer_Elapsed(object sender, EventArgs e)
        {
            visible = !visible;
            viewRangeCurrentlySelected.SetVisibility(visible);
            Refresh();
        }

        // stop the item from blinking on the HyperBraille
        public void StopBlink()
        {
            selectionTimer.Stop();
            visible = true;
            viewRangeCurrentlySelected.SetVisibility(visible);  // ensure the item that was just blinking is set to be visible
            Refresh();
        }

        #endregion

        #region Timer for Gesture Detection

        // start detecting touch on the HyperBraille
        public void StartGestureDetection()
        {
            gestureTimer.Start();
            gestinterp.StartGestureDetection();
        }

        // on tick, interpret touch points on the HyperBraille 
        void analyzeGesture_Elapsed(object sender, EventArgs e)
        {
            Point newPoint = gestinterp.FinishGestureDetection();
            if (newPoint != new Point(-1, -1) && SelectViewRangeAtPoint(newPoint)) gestureTimer.Stop();  // if a tap is recognized and  the touch was over a valid section of the paper, select it and stop the timer
            else gestinterp.StartGestureDetection();   // otherwise, keep looking for gestures       
        }

        #endregion

        // listener for HyperBraille device - called every time a touch is sensed on the device
        void brailledis_touchValuesChanged(object sender, BrailleIO.Interface.BrailleIO_TouchValuesChanged_EventArgs e)
        {
            if (gestinterp.interpretGesture) gestinterp.gestureRecognizer.AddFrame(new Frame(e.touches));  // if the touches are to be interpreted, add touches to the gesture recognizers
        }
    }
}
