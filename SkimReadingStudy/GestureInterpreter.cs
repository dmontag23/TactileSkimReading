using Gestures.Recognition;
using Gestures.Recognition.Interfaces;
using Gestures.Recognition.Preprocessing;
using System;
using System.Drawing;

namespace SkimReadingStudy
{
    // manages gestures on the device
    class GestureInterpreter
    {
        private static BlobTracker blobTracker = new BlobTracker();                         // set up a new blob tracker for tracking related finger blobs as continuous trajectory
        public GestureRecognizer gestureRecognizer = new GestureRecognizer(blobTracker);    // create new gesture recognizer and hand over the blob tracker
        
        public bool interpretGesture = false;                                               // flag for sending touch values to the gesture recognizer

        // constructor - add the tap classifier that interprets taps on the HyperBraille
        public GestureInterpreter()
        {
            // add classifier to interpret the tracked blobs as taps
            gestureRecognizer.AddClassifier(new TapClassifier());
        }

        // start sending touch values to the gesture recognizer
        public void StartGestureDetection()
        {
            gestureRecognizer.StartEvaluation();
            interpretGesture = true;
        }

        // stop sending touch values to the gesture recognizer and return the Point where the user tapped, if applicable
        public Point FinishGestureDetection()
        {
            interpretGesture = false;                 // stop receiving gestures on the device
            Point pointToSelect = new Point(-1, -1);  // initialize the point to be returned
            IClassificationResult gesture = gestureRecognizer.FinishEvaluation();  // interpret the gestures given to the device
           
            // if the user tapped the device in a specified location, return that location - otherwise return (-1,-1)
            if (gesture != null)
            {
                pointToSelect = new Point(Convert.ToInt32(gesture.NodeParameters[0].X), Convert.ToInt32(gesture.NodeParameters[0].Y));
            }
            return pointToSelect;
        }
    }
}
