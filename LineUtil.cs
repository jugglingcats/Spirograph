using System;
using System.Drawing;

namespace Spirograph
{
	/// <summary>
	/// This is a utility class for performing algorithms on lines (specifically used to
	/// calculate the distance of a point from a given line segment)
	/// </summary>
	public class LineUtil
	{
		public static float PointToSegment(Point p1, Segment s)
		{
			// this is from the web - don't ask exactly how it works!

			PointF p=new PointF(p1.X, p1.Y);

			Vector v = new Vector(s.P1, s.P0);
			Vector w = new Vector(p, s.P0);

			double c1 = w * v;
			if ( c1 <= 0 )
				// this handles the case when the point is
				// nearer the first end point than a point on the line
				return Distance(p, s.P0);

			double c2 = v * v;
			if ( c2 <= c1 )
				// this handles the case when the point is
				// nearer the second end point than a point on the line
				return Distance(p, s.P1);

			double b = c1 / c2;

			PointF Pb= s.P0 + (v * b);

			return Distance(p, Pb);
		}

		public static float Distance(PointF p, PointF pb)
		{
			Vector v=new Vector(p, pb);
			return (float) Math.Sqrt(v*v);
		}
	}

	public class Vector
	{
		private double dx;
		private double dy;

		public Vector(PointF p1, PointF p2)
		{
			dx=p1.X-p2.X;
			dy=p1.Y-p2.Y;
		}

		public Vector(double dx, double dy)
		{
			this.dx=dx;
			this.dy=dy;
		}

		public static double operator *(Vector u, Vector v)
		{
			// dot product
			return (u.dx*v.dx + u.dy*v.dy);
		}

		public static Vector operator *(Vector v, double d)
		{
			// scalar multiple
			return new Vector(d*v.dx, d*v.dy);
		}

		public static PointF operator +(PointF p, Vector v)
		{
			PointF ret=new PointF(p.X, p.Y);
			ret.X+=(float) v.dx;
			ret.Y+=(float) v.dy;
			return ret;
		}

	}

	public class Segment
	{
		public PointF P1;
		public PointF P0;

		public Segment(Point p0, Point p1)
		{
			this.P0=p0;
			this.P1=p1;
		}
		public Segment(PointF p0, PointF p1)
		{
			this.P0=p0;
			this.P1=p1;
		}
	}
}
