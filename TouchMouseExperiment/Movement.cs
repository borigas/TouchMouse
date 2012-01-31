using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TouchMouseExperiment
{
    public enum MovementDirection
    {
        None = -101,
        New = -100,
        Up = 0,
        UpRight = 1,
        Right = 2,
        DownRight = 3,
        Down = 4,
        DownLeft = 5,
        Left = 6,
        UpLeft = 7,
    }

    class Movement
    {
        internal const double MOVEMENT_THRESHOLD = 2.5;
        internal const int INACTIVITY_FRAME_THRESHOLD = 1;
        internal MovementDirection Direction { get; set; }
        internal double Magnitude { get; set; }
        internal int XMovement { get; set; }
        internal int YMovement { get; set; }
        internal int InactiveFrameCount { get; set; }

        public override string ToString()
        {
            return Direction.ToString() + ": " + XMovement + ", " + YMovement + ": " + Magnitude.ToString("0.#");
        }

        internal static Movement Create(TouchPoint previous, TouchPoint current)
        {
            Movement movement = new Movement();
            if (previous == null)
            {
                movement.Direction = MovementDirection.New;
                movement.XMovement = 0;
                movement.YMovement = 0;
                movement.InactiveFrameCount = 0;
            }
            else
            {
                movement.XMovement = current.FocalPointX - previous.FocalPointX;
                // Y is flipped because positive needs to be UP
                movement.YMovement = previous.FocalPointY - current.FocalPointY;
                // Add last 1 if it didn't break threshold
                if (previous.Movement.Magnitude < MOVEMENT_THRESHOLD)
                {
                    movement.XMovement += previous.Movement.XMovement;
                    movement.YMovement += previous.Movement.YMovement;
                    movement.InactiveFrameCount = previous.Movement.InactiveFrameCount;
                }
                movement.Magnitude = previous.DistanceTo(current);
                
                if (movement.Magnitude >= MOVEMENT_THRESHOLD)
                {
                    movement.Direction = movement.FindDirection();
                }
                else
                {
                    movement.Direction = MovementDirection.None;
                    movement.InactiveFrameCount++;
                }
            }

            return movement;
        }

        private MovementDirection FindDirection()
        {
            //System.Diagnostics.Trace.WriteLine("**" + XMovement + ", " + YMovement + ": " +
            //    (Math.Atan2(-1 * XMovement, -1 * YMovement) * 4 / Math.PI + 4.5) * 45 + "D" +
            //    (MovementDirection)(Math.Atan2(-1 * XMovement, -1 * YMovement) * 4 / Math.PI + 4.5));
            int direction = (int)(MathHelpers.FindDirection(XMovement, YMovement) + 22.5) / 45;
            MovementDirection result = (MovementDirection)(direction % 8);

            System.Diagnostics.Debug.Assert(direction <= 8 && direction >= 0, 
                "Direction " + direction + " does not fall in expected bounds");
            System.Diagnostics.Debug.Assert(Enum.GetName(typeof(MovementDirection), direction % 8) != null,
                "Enum " + result + " for value " + direction % 8 + " not found");

            return result;
        }
    }
}