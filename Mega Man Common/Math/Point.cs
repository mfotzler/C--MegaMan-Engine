using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MegaMan.Common.Math
{
    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point(int x, int y) : this()
        {
            X = x;
            Y = y;
        }

        public static implicit operator PointF(Point p)
        {
            return new PointF(p.X, p.Y);
        }

        public static Point Empty
        {
            get
            {
                return new Point(0, 0);
            }
        }

        public override bool Equals(Object obj)
        {
            if (!(obj is Point))
            {
                return false;
            }

            Point p = (Point)obj;

            return (X == p.X && Y == p.Y);
        }

        public static bool operator ==(Point a, Point b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Point a, Point b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return X ^ Y;
        }
    }

    public struct PointF
    {
        public float X { get; set; }
        public float Y { get; set; }

        public PointF(float x, float y) : this()
        {
            X = x;
            Y = y;
        }

        public static explicit operator Point(PointF p)
        {
            return new Point(Convert.ToInt32(p.X), Convert.ToInt32(p.Y));
        }

        public static PointF Empty
        {
            get
            {
                return new PointF(0.0f, 0.0f);
            }
        }

        public override bool Equals(Object obj)
        {
            if (!(obj is PointF))
            {
                return false;
            }

            PointF p = (PointF)obj;

            return (X == p.X && Y == p.Y);
        }

        public static bool operator ==(PointF a, PointF b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(PointF a, PointF b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return (Int32)X ^ (Int32)Y;
        }
    }
}
