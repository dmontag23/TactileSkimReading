using System;
using System.Collections.Specialized;
using System.Windows.Forms;

namespace SkimReadingStudy
{
    public partial class Form1 : Form
    {

        private static HyperBrailleInterface hb = new HyperBrailleInterface();      // interfaces with the HyperBraille Device
        private GestureInterpreter gestinterp = new GestureInterpreter();           // interprets gestures
        private Selection select = new Selection();                                 // selects the closest view range to the currently selected view range

        public Form1()
        {
            InitializeComponent();
            hb.SetParagraphTextBox(selectedText);
            hb.SetPageTextBox(pageNumDisplay);
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

        // select the previous piece of content
        private void previous_Click(object sender, EventArgs e)
        {
            hb.SelectOrderedSectionToRead("previous");
        }

        // select the next piece of content
        private void next_Click(object sender, EventArgs e)
        {
            hb.SelectOrderedSectionToRead("next");
        }

        # region Keyboard Listener Methods

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

                // select the previous logical block of content
                if (keyData == Keys.P) hb.SelectOrderedSectionToRead("previous");

                // select the next logical block of content
                if (keyData == Keys.N) hb.SelectOrderedSectionToRead("next");

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
