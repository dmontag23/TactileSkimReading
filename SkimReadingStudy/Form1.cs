using System;
using System.Collections.Specialized;
using System.Windows.Forms;

namespace SkimReadingStudy
{
    public partial class Form1 : Form
    {

        private static HyperBrailleInterface hb = new HyperBrailleInterface();      // interfaces with the HyperBraille Device
        private GestureInterpreter gestinterp = new GestureInterpreter();           // interprets gestures
        private TextToSpeech txtToSpeech = new TextToSpeech(hb, 3);                 // controls text to speech output
        private Selection select = new Selection();                                 // selects the closest view range to the currently selected view range

        public Form1()
        {
            InitializeComponent();
            hb.SetTextToSpeech(txtToSpeech);   // set the textToSpeech variable in the HyperBraille object
        }

        // display the first paper on the HyperBraille Device
        private void paper1_Click(object sender, EventArgs e)
        {
            DisplayPaper("paper1", 4);  
        }

        // display the second paper on the HyperBraille Device
        private void paper2_Click(object sender, EventArgs e)
        {
            DisplayPaper("paper2", 5);  
        }

        // display the third paper on the HyperBraille Device
        private void paper3_Click(object sender, EventArgs e)
        {
            DisplayPaper("paper3", 5);
        }

        // clear the HyperBraille Device
        private void clear_Click(object sender, EventArgs e)
        {
            hb.SetPaperOnDisplay(null);  // make sure no paper is being displayed on the device
            hb.Clear();
        }

        // exit the application
        private void exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        # region Keyboard Listener Methods

        // flag to see if the zero numpad key is being held down
        private bool zeroKeyDown = false;
        
        // if the 0 numpad key is being held down, set the flag to true
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.NumPad0) zeroKeyDown = true;
        }

        // if the 0 numpad key is released, set the flag to false
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.NumPad0) zeroKeyDown = false;
        }

        // Listener for the keyboard
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (hb.GetPaperOnDisplay() != null)  // ensures there is a paper on the HyperBraille Device before listening to any keyboard buttons
            {
                // select the next view range left from the current view range via the left arrow key
                if (keyData == Keys.Left) hb.SelectViewRange(select.SelectClosestViewRange("left", hb.GetMainscreen(), hb.GetViewRangeCurrentlySelected()));

                // select the next view range right from the current view range via the right arrow key
                if (keyData == Keys.Right) hb.SelectViewRange(select.SelectClosestViewRange("right", hb.GetMainscreen(), hb.GetViewRangeCurrentlySelected()));

                // select the next view range up from the current view range via the up arrow key
                if (keyData == Keys.Up) hb.SelectViewRange(select.SelectClosestViewRange("up", hb.GetMainscreen(), hb.GetViewRangeCurrentlySelected()));

                // select the next view range down from the current view range via the down arrow key
                if (keyData == Keys.Down) hb.SelectViewRange(select.SelectClosestViewRange("down", hb.GetMainscreen(), hb.GetViewRangeCurrentlySelected()));

                // flip to the previous page via the PageDown key
                if (keyData == Keys.PageUp) hb.FlipPage("previous");

                // flip to the next page via the PageUp key
                if (keyData == Keys.Next) hb.FlipPage("next");

                // select the content to read via 0 & 5 on Numpad
                if (zeroKeyDown && keyData == Keys.NumPad5) hb.SpeakViewRangeCurrentlySelected();

                // deselect content via 0 & 2 on Numpad
                if (zeroKeyDown && keyData == Keys.NumPad2) hb.DeselectAll();

                // go to the previous sequential section via 0 & 4 on Numpad
                if (zeroKeyDown && keyData == Keys.NumPad4) hb.SelectOrderedSectionToRead("previous");

                // continue onto the next sequential section via 0 & 6 on Numpad
                if (zeroKeyDown && keyData == Keys.NumPad6) hb.SelectOrderedSectionToRead("next");

                // restart reading the current section via 0 & 7 on Numpad
                if (zeroKeyDown && keyData == Keys.NumPad7)
                {
                    txtToSpeech.CancelSpeaking();             // canel whatever is currently being spoken
                    hb.SpeakViewRangeCurrentlySelected();     // speak the currently selected view range
                }

                // toggle automatic mode via 0 & 8 on Numpad
                if (zeroKeyDown && keyData == Keys.NumPad8) txtToSpeech.ToggleAutomaticMode();

                // speak out the current page
                if (zeroKeyDown && keyData == Keys.NumPad9) txtToSpeech.SpeakPageNum();

                // start gesture detection via the Space key
                if (keyData == Keys.Space)
                {
                    hb.StartGestureDetection();
                    return true;  // do not process the space key press any further
                }
            }

            return false;  // process key data further (send to Key_Down, Key_Up listeners)
        }

        #endregion

        // display the given paper name on the HyperBraille Device
        private void DisplayPaper(String paperName, int numOfPages)
        {
            Paper paperToDisplay = new Paper(paperName, numOfPages);  // instantiate the new paper
            hb.SetPaperOnDisplay(paperToDisplay);                     // tell the HyperBraille the paper to interact with
            OrderedDictionary viewRangesFromFirstPage = paperToDisplay.GetViewRangesOfPage(1);  // get all of the View Ranges for the first page
            hb.Clear();
            hb.DisplayViewRanges(viewRangesFromFirstPage);  //  display these View Ranges on the device
        }
    }
}
