using AForge.Math.Geometry;
using BrailleIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;

namespace SkimReadingStudy
{
    // manages the process of finding the closest view range to the one given
    class Selection
    {  
        // selects the closest view range in the given direction to the given view range
        public BrailleIOViewRange SelectClosestViewRange(String direction, BrailleIOScreen mainscreen, BrailleIOViewRange viewRangeCurrentlySelected)
        {                
            OrderedDictionary viewRangeOrderedDict = mainscreen.GetViewRanges();                              // get all of the view ranges on the screen
            if (viewRangeCurrentlySelected == null) return viewRangeOrderedDict[1] as BrailleIOViewRange;     // if no view range is currently selected, select the first one on the screen
            else
            {
                AForge.Point centerPoint = FindCenter(viewRangeCurrentlySelected.ViewBox);                               // find the center point of the view range currently selected
                List<AForge.Point> pointOptions = ExtractPointsFromViewRangeDict(centerPoint, viewRangeOrderedDict);     // extract the center points from all other view ranges on the screen
                pointOptions = ScanForPoints(centerPoint, pointOptions, direction, 52);                                  // elimate points that fall above a certain angle threshold
                AForge.Point nearestPoint = FindNearestPoint(centerPoint, pointOptions);                                 // out of the points remaining, find the one nearest to the current view range
                BrailleIOViewRange viewRangeToDisplay = SearchViewRanges(nearestPoint, viewRangeOrderedDict);            // get the view range that corresponds to that point
                return viewRangeToDisplay;
            }
        }

        // given a Rectangle, find the center point of that Rectangle
        private AForge.Point FindCenter(Rectangle viewBox)
        {
            float widthCenter = viewBox.Width / 2;
            float heightCenter = viewBox.Height / 2;
            return new AForge.Point(widthCenter + viewBox.X, heightCenter + viewBox.Y);
        }

        // given a starting point and a dictionary of view ranges, find all the other center points of the view ranges that are not the starting point
        private List<AForge.Point> ExtractPointsFromViewRangeDict(AForge.Point startingPoint, OrderedDictionary viewRangeDict)
        {
            List<AForge.Point> pointsToReturn = new List<AForge.Point>();

            // get the center points for all view ranges in the dictionary that are not the starting point
            for (int i = 1; i < viewRangeDict.Count; i++)  // start at 1 so you don't get the blank screen - the blank screen is a view range that should never be selected
            {
                BrailleIOViewRange viewRange = viewRangeDict[i] as BrailleIOViewRange;
                AForge.Point location = FindCenter(viewRange.ViewBox);
                if (startingPoint != location) pointsToReturn.Add(location);
            }

            return pointsToReturn;
        }

        // determines whether view ranges should still be considered based on the angle between the view range currently selected and the other view ranges
        // excludes view ranges where the angle is too high
        private List<AForge.Point> ScanForPoints(AForge.Point startingPoint, List<AForge.Point> allPoints, String directionToScan, float angleThreshold)
        {
            List<AForge.Point> pointsToReturn = new List<AForge.Point>();
            
            // for every center point of the view ranges, determine if the angle between the startingPoint and the center point is small enough to still be considered
            foreach (AForge.Point point in allPoints)
            {
                if (directionToScan == "up" && startingPoint.X >= point.X) pointsToReturn.Add(GetPointsWithinAngle(startingPoint, point, -1, 0, angleThreshold));
                if (directionToScan == "right" && startingPoint.Y >= point.Y) pointsToReturn.Add(GetPointsWithinAngle(startingPoint, point, 0, -1, angleThreshold));
                if (directionToScan == "down" && startingPoint.X <= point.X) pointsToReturn.Add(GetPointsWithinAngle(startingPoint, point, 1, 0, angleThreshold));
                if (directionToScan == "left" && startingPoint.Y <= point.Y) pointsToReturn.Add(GetPointsWithinAngle(startingPoint, point, 0, 1, angleThreshold));
            }

            pointsToReturn.RemoveAll(x => x == new AForge.Point(-1, -1)); // remove all (-1, -1) points
            return pointsToReturn;
        }

        // find the angle between two points and calculate to see if it is below a certain threshold
        private AForge.Point GetPointsWithinAngle(AForge.Point startingPoint, AForge.Point endPoint, int deltaX, int deltaY, float angleThreshold)
        {
            double angleToConsider = GeometryTools.GetAngleBetweenLines(new AForge.Point(startingPoint.X, startingPoint.Y),
                                                                        new AForge.Point(startingPoint.X + deltaX, startingPoint.Y + deltaY),  // extend the starting point into a line in the appropraite direction as dictated by delta x or delta y
                                                                        new AForge.Point(startingPoint.X, startingPoint.Y),
                                                                        new AForge.Point(endPoint.X, endPoint.Y));                             // line between the starting point and the ending point
            
            if (angleToConsider < angleThreshold) return endPoint;    // if the angle is smaller than the threshold, return it
            return new AForge.Point(-1, -1);  // otherwise return the point (-1, -1)
        }

        // given a starting point and a list of points, find the point nearest to the starting point
        private AForge.Point FindNearestPoint(AForge.Point startingPoint, List<AForge.Point> pointsToConsider)
        {
            if (pointsToConsider.Count == 0) return startingPoint;    // return the starting point if no other points are to be considered
            List<Double> listOfDistances = new List<Double>();
            foreach (AForge.Point point in pointsToConsider) listOfDistances.Add(startingPoint.DistanceTo(point));  // calculate the distance between the startingPoint and each point to consider
            double minDistance = listOfDistances.Min();             
            int indexOfMin = listOfDistances.IndexOf(minDistance);  // find the index of the item with the shortest distance in the listOfDistances
            return pointsToConsider[indexOfMin];                    // return the point with the minimum distance from the startingPoint
        }

        // find the view range that corresponds to the given center point - return null if no view range has the given center point
        private BrailleIOViewRange SearchViewRanges(AForge.Point centerPoint, OrderedDictionary viewRanges)
        {
            // for each view range, check to see if the center point given matches the center point of that view range
            for (int i = 1; i < viewRanges.Count; i++)
            {
                BrailleIOViewRange viewRange = viewRanges[i] as BrailleIOViewRange;
                AForge.Point viewRangeCenterPoint = FindCenter(viewRange.ViewBox);
                if (centerPoint == viewRangeCenterPoint) return viewRange;  
            }
            return null;
        }

    }
}
