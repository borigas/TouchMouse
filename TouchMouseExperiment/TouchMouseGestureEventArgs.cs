using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TouchMouseExperiment
{
    public class TouchMouseGestureEventArgs
    {
        public List<TouchPoint> TouchPoints { get; set; }
        public TouchPoint TriggeringTouchPoint { get; set; }
    }
}
