﻿using System;
using System.Collections.Generic;

namespace Gestures.Recognition.GestureData
{
    /// <summary>
    /// Touch contact on the surface, i.e. closed area or cluster of sensor values.
    /// </summary>
    public class Touch
    {
        public Touch(int id, double x, double y, double cx, double cy, double intense)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            this.cx = cx;
            this.cy = cy;
            this.intense = intense;
        }
        public int id;
        public double x;
        public double y;
        public double cx;
        public double cy;
        public double intense;
    }

    /// <summary>
    /// A sample of the sensor data, i.e. all current touches during one sampling step.
    /// </summary>
    public class Frame : IEnumerable<Touch>
    {
        IList<Touch> touches = new List<Touch>();
        IDictionary<int, Touch> dict = new Dictionary<int, Touch>();
        public Frame(DateTime timeStamp)
        {
            TimeStamp = timeStamp;
        }

        public Frame(DateTime timeStamp, params Touch[] touches)
        {
            TimeStamp = timeStamp;
            foreach (Touch t in touches)
            {
                AddTouch(t);
            }
        }

        public Frame(DateTime timeStamp, double[,] touches)
        {
            TimeStamp = timeStamp;
            if (touches != null && touches.GetLength(0) > 0 && touches.GetLength(1) > 0)
            {
                int id = 0;
                for (int x = 0; x < touches.GetLength(0); x++)
                {
                    for (int y = 0; y < touches.GetLength(1); y++)
                    {
                        if (touches[x, y] > 0)
                        {
                            AddTouch(new Touch(id, x, y, 1, 1, touches[x, y]));
                            id++;
                        }
                    }
                }
            }
        }

        public Frame(double[,] sampleSet) : this(sampleSet, DateTime.Now){}

        public Frame(double[,] sampleSet, DateTime timestamp)
        {
            TimeStamp = timestamp;
            if (sampleSet != null)
            {
                var clusterer = new GestureRecognition.Clusterer(sampleSet.Length);
                var cluster = clusterer.Cluster(sampleSet, 0);

                foreach (var c in cluster.Values)
                {
                    //get blob extension in x and y axis
                    double cX = 1, cY = 1;
                    int count = 0;
                    double val = 0;
                    foreach (var m in c.ClusterSet.Values)
                    {
                        int x = m / sampleSet.GetLength(1);
                        int y = m % sampleSet.GetLength(1);
                        try
                        {
                            val += sampleSet[x, y];
                            count++;
                        }
                        catch { }
                        if (Math.Abs(x - c.Mean[0]) >= cX) cX = Math.Abs(x - c.Mean[0]);
                        if (Math.Abs(y - c.Mean[1]) >= cY) cY = Math.Abs(y - c.Mean[1]);
                    }
                    Touch t = new Touch(c.Id, c.Mean[0], c.Mean[1], cX, cY, val / count);
                    AddTouch(t);
                }
            }
        }

        public void AddTouch(Touch touch)
        {
            touches.Add(touch);
            dict.Add(touch.id, touch);
        }

        public int Count { get { return touches.Count; } }
        public Touch this[int index] { get { return touches[index]; } }
        public DateTime TimeStamp { get; set; }
        public Touch GetTouch(int id)
        {
            Touch touch = null;
            if (dict.TryGetValue(id, out touch))
            {
                return touch;
            }
            return null;
        }

        #region IEnumerable<Touch> Members

        public IEnumerator<Touch> GetEnumerator()
        {
            return touches.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return touches.GetEnumerator();
        }

        #endregion

        
        


    }

}