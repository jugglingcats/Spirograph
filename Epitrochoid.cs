using System;
using System.Drawing;

namespace Spirograph
{
	public class Epitrochoid : SpiroInstance
	{
		public Epitrochoid()
		{
		}

		#region SpiroInstance overrides

		// see description in base class
		protected override PointF CalcPoint(double t)
		{
			double a=t+baseAngle; // angle moving circle has moved through relative to center point
			double b=((1.0*R/r) + 1)*t+baseMovingAngle; // rotation of moving cirle about its center
			double x = (double) (R + r)*Math.Cos(a) - f*Math.Cos(b);
			double y = (double) (R + r)*Math.Sin(a) - f*Math.Sin(b);
			return new PointF((float) x, (float) y);
		}

		// see description in base class
		protected override PointF CurrentMovingCircleCenter
		{
			get 
			{ 
				// R+r is the radius of the circle inscribed by the moving circle center
				int A=R+r;
				double a=CurrentAngle;
				double x=A*Math.Cos(a);
				double y=A*Math.Sin(a);
				return new PointF((float) x, (float) y); 
			}
		}


		// see description in base class
		protected override double CurrentMovingAngle
		{
			get
			{
				// this is the angle that the moving circle has turned through
				return ((1.0*R/r) + 1)*AbsoluteAngle+baseMovingAngle;
			}
		}

		protected override int MaxPenDistance
		{
			// inner plus outer plus distance from outer
			get { return R+r+f; }
		}

		// see description in base class
		protected override int MaxPenDistanceWithinBounds(int n)
		{
			return n - (R+r);
		}

		// see description in base class
		protected override int MaxMovingRadiusWithinBounds(int R)
		{
			return MaxPenDistance - R;
		}

		// see description in base class
		protected override Rectangle TrueBoundingRect
		{
			get
			{
				Point pt = location;
				// we're calculating the rect we operate in, including
				// highlight of the outer circle, so if the pen is inside
				// the circle we need to take r*2 rather than r+f
				int m = R + Math.Max(r*2, r+f);
				// adjust for top-left of bounding rect
				pt.Offset(-m, -m);
				// and size to twice max extent
				Rectangle rc = new Rectangle(pt, new Size(m*2, m*2));
				return rc;
			}
		}

		// see description in base class
		public override SpiroInstance Clone()
		{
			SpiroInstance clone=new Epitrochoid();
			clone.Clone(this);
			return clone;
		}

		// see description in base class
		protected override int MinFixedRadius
		{
			// don't let it get too small
			get { return 5; }
		}

		// see description in base class
		protected override int MaxMovingRadius
		{
			// arbitrary max - to avoid overflow when resizing
			get { return 1000; }
		}

		protected override int CalcResolution
		{
			get
			{
				// this is the max velocity (found through differentiating)
				double dxy=R+r+f*((float) R/r + 1);
				return InternalCalcResolution(dxy);
			}
		}

		#endregion
	}
}
