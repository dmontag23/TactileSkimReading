using System;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows.Forms;

namespace SkimReadingStudy
{
    // controls text to speech output
    class TextToSpeech
    {
        private HyperBrailleInterface hb = null;                                // interfaces with the HyperBraille Device
        private SpeechSynthesizer textReader = new SpeechSynthesizer();         // convert text to speech
        private SpeechSynthesizer automaticReader = new SpeechSynthesizer();    // read whether automatic mode is on or off
        private SpeechSynthesizer pageReader = new SpeechSynthesizer();         // read the page number

        private bool automaticMode = true;         // flag that toggles automatic mode
        private bool speechCanceled = false;       // flag that keeps track of whether the user canceled the speaking on the device

        // constructor - set the HyperBraille device to interact with and the rate at which the speaker should talk
        public TextToSpeech(HyperBrailleInterface hb, int rate)
        {
            this.hb = hb;                                                              // set the HyperBraille device to interact with
            textReader.SpeakCompleted += textReader_SpeakCompleted;                    // set the method to determine what happens when the text reader stops speaking
            automaticReader.SpeakCompleted += automaticAndPageReader_SpeakCompleted;   // set the method to determine what happens when the automatic reader stops speaking
            pageReader.SpeakCompleted += automaticAndPageReader_SpeakCompleted;        // set the method to determine what happens when the page reader stops speaking

            // set the rates of the screenreaders
            textReader.Rate = rate;                                    
            automaticReader.Rate = rate;
            pageReader.Rate = rate;
        }

        // given a string, have the text reader read the string, pause, or resume reading the string (depending on the current state of the reader)
        public void SpeakOrPauseText(String text)
        {
            if (textReader.State.ToString() == "Speaking") textReader.Pause();       // if the reader is speaking, pause it
            else if (textReader.State.ToString() == "Paused") textReader.Resume();   // if the reader is paused, resume speaking
            else textReader.SpeakAsync(text);                                        // otherwise, if the speaker is ready, read out the given text
        }

        public void SpeakPageNum()
        {
            textReader.Pause();
            pageReader.SpeakAsync("Page " + hb.GetPaperOnDisplay().GetCurrentPageDisplayed().ToString() + "of " + hb.GetPaperOnDisplay().GetTotalNumberOfPages().ToString());
        }

        // cancels all prompts queued for the text reader
        public void CancelSpeaking()
        {
            if (textReader.State.ToString() != "Ready") speechCanceled = true;  // if the speaker was reading, raise the flag that the reading was canceled
            textReader.SpeakAsyncCancelAll();  // cancel all prompts queued for the text reader
            Thread.Sleep(500);                 // give the thread 100ms to update
            textReader.Resume();               // ensure that the state of the text reader is set to "Ready"
        }

        // toggles automatic mode on or off
        // when turned on, automatic mode will continue selecting and reading sequential text blocks on the device in the order given in the "Page" class
        public void ToggleAutomaticMode()
        {
            automaticMode = !automaticMode;  // toggle the flag for automatic mode
            
            // read out to the user whether automatic mode is on or off
            String onOrOff;                  
            if (automaticMode) onOrOff = "on";
            else onOrOff = "off";
            textReader.Pause();    // pause the text reader while the automatic reader speaks
            automaticReader.SpeakAsync("Automatic Mode " + onOrOff);
        }

        // listener for text reader - determines whether the reader has completely finished speaking a prompt or not
        void textReader_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            if (!speechCanceled && automaticMode)         // ensures automatic mode is on and the user did not cancel the reading of the text
            {
                hb.SelectOrderedSectionToRead("next");   // select the next sequential section on the HyperBraille
                Thread.Sleep(1000);                      // give the thread 1s to update and start the blinking
                hb.SpeakViewRangeCurrentlySelected();    // speak the newly selected section to the user
            }
            speechCanceled = false;  // reset the speech canceled flag
        }

        // listener for automatic and page readers - after the auto/page reader finishes speaking, resume the text reader
        void automaticAndPageReader_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            textReader.Resume();
        }
    }
}
