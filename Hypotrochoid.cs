using System;
using System.Drawing;

namespace Spirograph
{
	// inside
	public class Hypotrochoid : SpiroInstance
	{
		public Hypotrochoid()
		{
		}

		public Hypotrochoid(int R, int r, int f, Point location)
		{
			this.R = R;
			this.r = r;
			this.f = f;
			this.location = location;
			Reset();
		}

		#region SpiroInstance overrides
		// see description in base class
		protected override PointF CalcPoint(double t)
		{
			double a = t+baseAngle;
			double b = ((1.0*R/r) - 1)*t+baseMovingAngle;
			double x = (R - r)*Math.Cos(a) + f*Math.Cos(b);
			double y = (R - r)*Math.Sin(a) - f*Math.Sin(b);
			return new PointF((float) x, (float) y);
		}

		// see description in base class
		protected override PointF CurrentMovingCircleCenter
		{
			get
			{
				// R-r is the circle inscribed by the moving circle center
				int A=R-r;
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
				return ((1.0*R/r) - 1)*AbsoluteAngle+baseMovingAngle;
			}
		}

		protected override int MaxPenDistance
		{
			// R-r is distance of center of inner circle
			get { return R-r+f; }
		}

		// see description in base class
		protected override int MaxPenDistanceWithinBounds(int n)
		{
			return n - (R-r);
		}

		// see description in base class
		protected override int MaxMovingRadiusWithinBounds(int R)
		{
			// inner circle can't be bigger than outer
			return R - 5;
		}

		// see description in base class
		protected override Rectangle TrueBoundingRect
		{
			get
			{
				Point pt = location;
				// if pen is inside inner circle then max is R,
				// otherwise it's the max pen distance
				int m = Math.Max(R, MaxPenDistance);
				pt.Offset(-m, -m);
				Rectangle rc = new Rectangle(pt, new Size(m*2, m*2));
				return rc;
			}
		}

		// see description in base class
		public override SpiroInstance Clone()
		{
			Hypotrochoid clone = new Hypotrochoid();
			clone.Clone(this);
			return clone;
		}

		// see description in base class
		protected override int MinFixedRadius
		{
			// outer cannot be smaller than inner
			get { return r + 5; }
		}

		// see description in base class
		protected override int MaxMovingRadius
		{
			// inner cannot be bigger than outer
			get { return R - 5; }
		}

		protected override int CalcResolution
		{
			get 
			{
				// this is the max velocity (found through differentiating)
				double dxy=R-r+f*((float) R/r - 1);
				return InternalCalcResolution(dxy);
			}
		}

		#endregion
	}

}
