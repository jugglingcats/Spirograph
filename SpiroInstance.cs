using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Spirograph
{
	/// Represents the various areas that a point can be on a spiro
	public enum HitInfo
	{
		None,				// not on the spiro at all
		Bounds,				// somewhere in the BoundingRect
		FixedCircle,		// on the fixed cirle
		MovingCircle,		// on the moving circle
		Pen,				// on or near the pen
		Curve,				// near the actual curve
		Resize				// near the resize handle
	}

	/// Represents the different modes used for hit testing
	public enum HitTestMode
	{
		Default,			// the default
		Selected			// when selected
	}

	public enum RedrawMode
	{
		None,
		Draw,
		Invalidate
	}

	public abstract class SpiroInstance
	{
		protected Point location = new Point(250, 250);

		protected int R = 210;							// radius of fixed circle
		protected int r = 30;							// radius of moving circle
		protected int f = 40;							// distance of pen from moving circle center
		protected int counter = 0;						// travel around the spiro (t)

		private PointF[] points = null;					// collection of points to plot
		private int penWidth=2;							// pen thickness
		private int revolutions;						// number of orbits of the inner circle
		protected int resolution = 10;					// number of points to plot per orbit
		private bool selected = false;					// are we selected
		private static readonly int snapRange = 3;		// tolerance for hit tests
		private bool complete=false;					// have we finished drawing (for animate/repeat)
		private bool animate=true;						// should we animate
		private bool showTrace=true;					// should we show trace (circles moving)
		private bool repeat=true;						// after animating once, should we start again
		private Color colour=Color.BlueViolet;			// pen colour
		protected double baseAngle=0;					// offset used to retain angle after adjustment
		private bool randomise=false;					// after each complete draw, generate a new random spiro
		private bool showInfo=false;					// should we show info even if not selected
		protected double baseMovingAngle=0;
		private double speed=35.0;						// measure of how fast we draw (lower = faster)
		public int MaxRandomDiameter=0;
		private int finalPauseCount=0;

		public event EventHandler CycleComplete;

		public SpiroInstance()
		{
			// we don't actually calc the points at this stage
			// (in case this spiro is never actually drawn)
			Reset();
		}

		protected abstract double CurrentMovingAngle
		{
			get;
		}

		private void ResetCounter()
		{
			counter=0;
			finalPauseCount=0;
		}

		public Point Location
		{
			get { return location; }
			set { location = value; }
		}

		public void Clone(SpiroInstance from)
		{
			// copy all properties from the other spiro

			this.location=from.location;

			this.R=from.R;
			this.r=from.r;
			this.f=from.f;
			this.counter=from.counter;

			// we can copy the points since they are essentially immutable
			// (if this spiro is changed in any way a new array is created)
			this.points=from.points;
			this.penWidth=from.penWidth;
			this.revolutions=from.revolutions;
			this.resolution=from.resolution;

			this.selected=from.selected;

			this.complete=from.complete;
			this.animate=from.animate;
			this.repeat=from.repeat;
			this.randomise=from.randomise;
			this.showInfo=from.showInfo;
			this.showTrace=from.ShowTrace;
			this.colour=from.colour;
			this.baseAngle=from.baseAngle;
			this.baseMovingAngle=from.baseMovingAngle;
			this.speed=from.speed;
			this.MaxRandomDiameter=from.MaxRandomDiameter;

			// only need to reset if we're a different type (ie. different points)
			if ( this.GetType() != from.GetType() )
				Reset();

			// ensure the circle sizes are valid
			if ( R < MinFixedRadius )
			{
				R=MinFixedRadius;
				Reset();
			}
			// it's not actually necessary to do this
			if ( r > MaxMovingRadius )
			{
				r=MaxMovingRadius;
				Reset();
			}
		}

		public void Draw(Graphics g, bool printmode)
		{
			if (points == null)
				// make sure we have some points
				Init();

			// don't anti-alias the highlight
			g.SmoothingMode=SmoothingMode.None;

			// only draw the highlight under certain conditions
			if ( !printmode && showTrace && !selected && animate )
				DrawHighlight(g, HitInfo.None);

			// show the info box when selected or specifically requested
			if ( (selected && !printmode) || showInfo )
			{
				Point infoLocation=new Point(FocusRect.X, FocusRect.Bottom);
				DrawInfo(g, infoLocation, colour);
			}

			// shift into spiro coords
			g.TranslateTransform(location.X, location.Y, MatrixOrder.Append);

			// by default draw everything
			PointF[] subshape=points;
			if ( counter > 0 )
			{
				subshape = points;
				if ( !printmode && animate && !complete )
				{
					// copy subset of points into new array
					subshape=new PointF[counter + 1];
					Array.Copy(points, subshape, counter + 1);
				}
			} else if ( animate && !complete )
				// nothing to draw
				subshape=null;

			Pen p=new Pen(colour, penWidth);
			p.LineJoin=LineJoin.Round;
			if ( !printmode )
				g.SmoothingMode=SmoothingMode.AntiAlias;

			if ( !printmode && selected && animate )
				// show trace when selected
				g.DrawLines(Pens.LightGray, points);

			// show main drawing
			if ( subshape != null )
				g.DrawLines(p, subshape);

			// return to canvas coords
			g.TranslateTransform(-location.X, -location.Y);
		}

		private void DrawFocusRect(Graphics g)
		{
			ControlPaint.DrawFocusRectangle(g, FocusRect);
			g.FillRectangle(Brushes.Black, ResizeHandleRect);
		}

		private Rectangle ResizeHandleRect
		{
			get
			{
				Point pt=new Point(FocusRect.Right, FocusRect.Bottom);
				Rectangle rc=new Rectangle(pt, new Size(0,0));
				rc.Inflate(3,3);
				return rc;
			}
		}

		protected double AbsoluteAngle
		{
			get { return (Math.PI/resolution*counter); }

		}

		protected double CurrentAngle
		{
			get { return AbsoluteAngle+baseAngle; }
		}

		/// Return information about the area under the specified point
		public HitInfo HitTest(Point pt, HitTestMode mode)
		{
			if ( points == null )
				Init();

			// shift to spiro coords
			Point translatedPoint=pt;
			translatedPoint.Offset(-location.X, -location.Y);

			// calc distance from center
			double distance = LineUtil.Distance(new Point(0, 0), translatedPoint);

			if ( mode == HitTestMode.Default )
			{
				if (distance > MaxExtent + 5)
					// we're outside the area covered by the spiro
					return HitInfo.None;

				// is it near the line of the fixed circle
				if ( distance > R - 5 && distance < R + 5 )
					// yes
					return HitInfo.FixedCircle;

				// when testing in this mode, we only want to know about a couple
				// of key areas
				if (PointNearSegment(translatedPoint))
					// point is on or near the curve
					return HitInfo.Curve;

				// not interested, in this mode
				return HitInfo.None;
			}

			if ( ResizeHandleRect.Contains(pt) )
				return HitInfo.Resize;

			if ( !FocusRect.Contains(pt) )
				// point is outside focus rect
				return HitInfo.None;

			// determine if the point is on or near the pen
			RectangleF rc = new RectangleF(points[counter], new Size(0, 0));
			rc.Inflate(5, 5);
			if (rc.Contains(translatedPoint))
				return HitInfo.Pen;

			// is it near the line of the moving circle
			double distToMoving = LineUtil.Distance(CurrentMovingCircleCenter, translatedPoint);
			if ( distToMoving < r + 5 && distToMoving > r - 5 )
				// yes
				return HitInfo.MovingCircle;

			// is it near the line of the fixed circle
			if ( distance > R - 5 && distance < R + 5 )
				// yes
				return HitInfo.FixedCircle;

			// nowhere in particular but is within bounds
			return HitInfo.Bounds;
		}

		/// This represents all the parts of the spiro that need
		/// redrawing when the counter changes
		private Region RedrawRegion
		{
			get
			{
				if ( !showTrace )
					return new Region(Rectangle.Empty);

				// add the moving circle
				RectangleF rc=MovingCircleBounds;
				rc.Inflate(2, 2);
				Region rgn=new Region(rc);

				// add the cross area
				rc=CrossRect;
				rc.Offset(location.X, location.Y);
				rc.Inflate(1,1);
				rgn.Union(rc);

				// add the line between moving center and cross
				Region rgnLine=LineRegion;
				rgn.Union(rgnLine);

				return rgn;
			}
		}

		/// Called to animate the spiro
		public Region Tick()
		{
			if (points == null)
				Init();

			if ( selected || !animate )
				// nothing to do if not animated
				return null;

			// this is the entire area (default redraw)
			Region rgnAll=new Region(BoundingRect);

			Region rgnIncrement=RedrawRegion;

			counter++;

			if (counter >= points.Length - 1)
			{
				if ( finalPauseCount < 100 ) 
				{
					counter = points.Length - 1;
					finalPauseCount++;
				} 
				else 
				{
					// need to restart from the beginning
					ResetCounter();
					// if not repeating then we keep drawn (counter becomes unused)
					complete=!repeat;

					if ( this.CycleComplete != null ) 
					{
						this.CycleComplete(this, new EventArgs());
					}

					if ( randomise )
						// create a new shape
						ResetRandom();

					// need to invalidate everything
					return rgnAll;
				}
			}
			// add the region covered by the new line segment
			GraphicsPath p=new GraphicsPath();
			p.AddLine(points[counter], points[counter-1]);
			p.Widen(new Pen(Color.White, penWidth+2));
			RectangleF rc=p.GetBounds();
			rc.Inflate(penWidth, penWidth);
			rc.Offset(location.X, location.Y);
			rgnIncrement.Union(rc);

			// add the region after increment
			if ( showTrace )
				rgnIncrement.Union(RedrawRegion);

			return rgnIncrement;
		}

		/// Create a new shape within the bounds of the current one
		/// (not as easy as it looks)
		public void ResetRandom()
		{
			Random rnd=new Random();

			int n;
			if ( MaxRandomDiameter == 0 ) 
			{
				n=MaxPenDistance;
			} 
			else 
			{
				n=rnd.Next(50, MaxRandomDiameter);
			}

			// this is just some jiggering to get some sensible values (not
			// always perfect but gives reasonable results)
			int min=Math.Min(20, n/2);
			int max=n-20;
			int R=OptimalFixedVal(this.r, min, rnd.Next(min, max), max);

			max=MaxMovingRadiusWithinBounds(R)-5;
			if ( max <= 10 )
				max=10;

			int r=OptimalMovingVal(R, 10, rnd.Next(10, max), max);

			this.R=R;
			this.r=r;
			f=MaxPenDistanceWithinBounds(n);
			Reset();
		}

		/// Returns the maximum distance the pen can get from the center of the fixed circle
		protected abstract int MaxPenDistance
		{
			get;
		}

		/// Returns the maximum pen distance (from the moving circle center, ie. f), that
		/// is possible while still having MaxPenDistance <= n
		protected abstract int MaxPenDistanceWithinBounds(int n);

		/// Returns the maximum moving circle radius possible while still staying within
		/// the given bounds
		protected abstract int MaxMovingRadiusWithinBounds(int R);

		/// Called by clients, typically when mouse moves to set a new circle size
		public void ResizeFixedCircle(Point pt)
		{
			float d = (int) LineUtil.Distance(location, pt);

			if (d < MinFixedRadius)
				d = MinFixedRadius;

			// set to an optimal value
			int newR = OptimalFixedVal(r, MinFixedRadius, (int) Math.Round(d), int.MaxValue);
			if ( newR != R )
			{
				SaveMovingAngle();
				R=newR;
				Reset();
			}
		}

		// this is used to resize the moving circle
		private PointF CurrentFixedCirclePoint
		{
			get
			{
				// return the point where the moving circle and fixed circle touch
				return new PointF((float) (R*Math.Cos(CurrentAngle)), (float) (R*Math.Sin(CurrentAngle)));
			}
		}

		/// Called by clients, typically when mouse moves to set a new moving circle size
		public void ResizeMovingCircle(Point pt)
		{
			// this is a little involved because we need to work out the circle that
			// will pass through the point the circle touches the fixed circle and the
			// given point - because the center of the moving circle moves when resized 
			// and it jumps about and looks bad if we just use that
			pt.Offset(-location.X, -location.Y);
			Point pt2 = Point.Round(CurrentFixedCirclePoint);
			double XA = LineUtil.Distance(pt2, pt)/2;
			int OA = pt2.X - pt.X;
			int OB = pt2.Y - pt.Y;
			// adjust for rotation of the moving circle
			double alpha = Math.Atan2(OA, OB)+CurrentAngle;
			if ( this is Epitrochoid )
				// correction for flip side of circle
				alpha+=Math.PI;

			int d = (int) Math.Round(XA/Math.Sin(alpha));

			if (d > MaxMovingRadius)
				d = MaxMovingRadius;

			if (d < 5)
				d = 5;

			// set to an optimal value
			int newr = OptimalMovingVal(R, 5, d, MaxMovingRadius);
			if ( newr != r )
			{
				SaveMovingAngle();
				r=newr;
				Reset();
			}
		}

		public void Resize(SpiroInstance current, int delta)
		{
			// this is a bit tricky - we need to resize both circles but give the same
			// (integer) ratio between them

			// in order to do this, each circle must increase/decrease by a multiple
			// of GCD(r,R)/size, ie. GCD(r,R)/R or GCD(r,R)/r

			// so we use the fixed circle as a base

			if ( delta == 0 )
				return;

			int d=current.R + delta;

			int multiple_R=R/GCD(r,R);
			int multiple_r=r/GCD(r,R);
			int new_R=(int) Math.Round((double) d/multiple_R)*multiple_R;
			int new_r=new_R * multiple_r / multiple_R;
			int new_f=(int) Math.Round(1.0 * f * (float) new_R / R);

			if ( new_R < 5 || new_r < 5 )
				// too small
				return;

			if ( new_R == R && new_r == r )
				return;

			SaveMovingAngle();
			R=new_R;
			r=new_r;
			f=new_f;
			Reset();
		}

		/// Called by clients, typically when mouse moves to move the pen in relation to the
		/// moving circle
		public void MovePen(Point pt)
		{
			// shift the point to spiro coords
			pt.Offset(-location.X, -location.Y);
			// measure distance to moving circle center
			float d = LineUtil.Distance(CurrentMovingCircleCenter, pt);
			// set new distance
			int newf = (int) Math.Round(d);
			if ( newf != f )
			{
				SaveMovingAngle();
				f=newf;
				Reset();
			}
		}

		[XmlIgnore]
		public bool Selected
		{
			set { this.selected = value; }
			get { return selected; }
		}

		[XmlIgnore]
		public Rectangle FocusRect
		{
			get
			{
				Rectangle rc=TrueBoundingRect;
				rc.Inflate(penWidth+2,penWidth+2);
				return rc;
			}
		}
		/// Calculate the true bounding rect for this spiro
		/// (the BoundingRect property adds some additional space)
		protected abstract Rectangle TrueBoundingRect { get; }

		/// Create a copy of this spiro
		public abstract SpiroInstance Clone();

		/// Calculate an actual point on the curve based on "time"
		/// (time is not an actual time but an abstract notion of travel around
		///  the curve - can also be thought of as angle)
		protected abstract PointF CalcPoint(double t);

		/// Get the center of the moving circle (at current t)
		protected abstract PointF CurrentMovingCircleCenter { get; }

		/// Get the min size of the fixed circle
		protected abstract int MinFixedRadius { get; }

		/// Get the max size of the moving circle
		protected abstract int MaxMovingRadius { get; }

		private void SaveMovingAngle()
		{
			// saved when shape is changed to give consistent start point for new
			// shape (after a reset)
			baseMovingAngle=CurrentMovingAngle % (2*Math.PI);
		}

		/// Reset everything (after a change, eg. to the size of a circle)
		protected void Reset()
		{
			// this sets the new start point for drawing
			// (this means that the trace highlight will continue from
			//  same spot after the reset - it's just visually nice)
			baseAngle=CurrentAngle % (2*Math.PI);

			CalcOrbits();

			// points are created in the Init method
			points = null;

			counter = 0;

			// resolution is a measure of how many points are calculated
			// (making it a factor of R/r is very crude - should be improved)
			resolution = CalcResolution;
		}

		protected int CheckResolution(int n)
		{
			if ( n < 10 )
				n=10;

			// can set an absolute limit here but it's hard because
			// some shapes can be quite small but very fast and they get jagged
//			if ( n > 300 )
//				n=300;
			
			return n;
		}

		protected abstract int CalcResolution
		{
			get;
		}

		/// Calculate the Greatest Common Divisor of a and b
		/// (this this is Euclid's algorithm)
		private static int GCD(int a, int b)
		{
			int r1 = b - (b/a)*a;
			if (r1 == 0)
				return a;
			int r2 = a - (a/r1)*r1;
			if (r2 == 0)
				return r1;
			int r3 = -1;

			while (r3 != 0)
			{
				r3 = r1 - (r1/r2)*r2;
				r1 = r2;
				if (r3 != 0)
					r2 = r3;
			}
			return r2;
		}

		private static void DrawCircle(Graphics g, Color c, int width, int x, int y, int r)
		{
			Pen p = new Pen(c, width);
			g.DrawEllipse(p, x - r, y - r, r*2, r*2);
		}

		/// This is the number of revolutions (or orbits) that the pen takes before
		/// it returns to the start point
		private static int CalcOrbits(int A, int a)
		{
			int gcd = GCD(A, a);
			return a/gcd;
		}

		/// Draw the highlight (fixed and moving circles, plus pen)
		public void DrawHighlight(Graphics g, HitInfo hi)
		{
			if (points == null)
				Init();

			g.TranslateTransform(location.X, location.Y);

			// save the current smoothing mode so we can reset
			SmoothingMode sm=g.SmoothingMode;
			g.SmoothingMode=SmoothingMode.None;
			if ( selected )
				g.SmoothingMode=SmoothingMode.AntiAlias;

			// set the default colours
			Color colFixedCircle = Selected ? Color.LightGray : Color.Gray;
			Color colMovingCircle = Color.DarkGray;
			Color colPen = colMovingCircle;

			// adjust colours based on hit test info
			switch (hi)
			{
				case HitInfo.FixedCircle:
					colFixedCircle = Color.Blue;
					break;
				case HitInfo.MovingCircle:
					colMovingCircle = Color.Blue;
					break;
				case HitInfo.Pen:
					colPen = Color.Blue;
					break;
			}

			int penWidth = Selected ? 2 : 1;
			Pen pen = new Pen(colPen, penWidth);

			// draw the fixed circle
			DrawCircle(g, colFixedCircle, penWidth, 0, 0, R);

			// find position of moving circle
			Point cmc = Point.Round(CurrentMovingCircleCenter);

			// and draw it
			DrawCircle(g, colMovingCircle, penWidth, cmc.X, cmc.Y, r);

			// get location of pen
			PointF cross = CurrentPenPoint;
			// draw line from center of moving circle to pen
			g.DrawLine(pen, cmc, cross);

			// draw the cross at the pen position
			RectangleF rc=CrossRect;
			g.DrawLine(pen, rc.Left, rc.Top, rc.Right, rc.Bottom);
			g.DrawLine(pen, rc.Left, rc.Bottom, rc.Right, rc.Top);

			// shift back to canvas coords
			g.TranslateTransform(-location.X, -location.Y);

			if ( selected )
				DrawFocusRect(g);

			// reset smoothing mode
			g.SmoothingMode=sm;
		}

		private RectangleF MovingCircleBounds
		{
			get
			{
				Point cmc=Point.Round(CurrentMovingCircleCenter);
				cmc.Offset(location.X, location.Y);
				RectangleF rc=new RectangleF(cmc, SizeF.Empty);
				rc.Inflate(r+penWidth, r+penWidth);
				return rc;
			}
		}

		// This is used when invalidating - it gives a region covering the line
		// between center of moving circle and the pen
		private Region LineRegion
		{
			get
			{
				PointF cross = CurrentPenPoint;
				PointF center= CurrentMovingCircleCenter;

				GraphicsPath path=new GraphicsPath();
				path.AddLine(center, cross);
				path.Widen(new Pen(Color.White, 2));
				Region rgn=new Region(path);
				rgn.Translate(location.X, location.Y);
				return rgn;
			}
		}

		private PointF CurrentPenPoint
		{
			get { return new PointF(points[counter].X, points[counter].Y); }
		}

		private RectangleF CrossRect
		{
			get
			{
				RectangleF rc = new RectangleF(CurrentPenPoint, SizeF.Empty);
				rc.Inflate(6, 6);
				return rc;
			}
		}

		private string Pad(object o)
		{
			return string.Format("{0}", o).PadLeft(4);
		}

		/// Display the details of the current spiro
		public void DrawInfo(Graphics g, Point pt, Color col)
		{
			string msg = string.Format(
					"Fixed Radius\t{0}\n"+
					"Moving Radius\t{1}\n"+
					"Pen Distance\t{2}\n"+
					"Number of Orbits\t{3}\n"+
					"GCD of Radii\t{4}\n"+
					"Resolution\t{5}", 
				Pad(R), Pad(r), Pad(f), Pad(revolutions), Pad(GCD(R, r)), Pad(resolution));
	
			Font fnt = new Font("Arial", 8);

			StringFormat fmt=new StringFormat();
			fmt.SetTabStops(0, new float[] {100});
			g.DrawString(msg, fnt, new SolidBrush(col), pt, fmt);
		}

		/// Calculate an optimal value for new fixed circle size within a given range
		private int OptimalFixedVal(int r, int min, int R, int max)
		{
			// we're only interested in sizes within snapRange of the requested new size
			min = Math.Max(min, R - snapRange);
			max = Math.Min(max, R + snapRange);

			// calculate new revolutions
			int t = SpiroInstance.CalcOrbits(R, r);
			int o = R;

			// we test progressively further either side of the requested size, because
			// we want the nearest value with the lowest revolutions
			int sign=1; // used to flip between above / below
			for (int dx = 1; dx <= snapRange;)
			{
				int tv = R + sign*dx;

				// reverse the sign
				sign=1-sign;
				if ( sign > 0 )
					// increment every other loop
					dx++;

				if (tv < min || tv > max)
					continue;

				int revs = SpiroInstance.CalcOrbits(tv, r);
				if (revs < t)
				{
					// test val gives smaller number of revs so use it
					t = revs;
					o = tv;
				}
			}
			return o;
		}

		/// Calculate an optimal value for new moving circle size within a given range
		private int OptimalMovingVal(int R, int min, int r, int max)
		{
			min = Math.Max(min, r - snapRange);
			max = Math.Min(max, r + snapRange);

			int t = SpiroInstance.CalcOrbits(R, r);
			int o = r;
			int sign=1; // used to flip between above / below
			for (int dx = 1; dx <= snapRange;)
			{
				int tv = r + sign*dx;

				// reverse the sign
				sign=1-sign;
				if ( sign > 0 )
					// increment every other loop
					dx++;

				if (tv < min || tv > max)
					continue;

				int revs = SpiroInstance.CalcOrbits(R, tv);
				if (revs < t)
				{
					// test val gives smaller number of revs so use it
					t = revs;
					o = tv;
				}
			}
			return o;
		}

		protected int InternalCalcResolution(double dxy) 
		{
			// find the hypotenuse (max dx and dy on the edges)
			double val=speed*Math.Sqrt(2*Math.Pow(dxy, 2)) / MaxPenDistance;
			return CheckResolution((int) Math.Round(val)) ;
		}

		/// Calculate how far a given point is from the spiro curve
		private bool PointNearSegment(Point pt)
		{
			if (points == null)
				Init();

			// go around the shape and construct a segment from consecutive
			// points and see how far the point is from the segment
			PointF pt1 = new PointF(points[0].X, points[0].Y);
			for (int n = 1; n < points.Length; n++)
			{
				PointF pt2 = new PointF(points[n].X, points[n].Y);
				Segment s = new Segment(pt1, pt2);

				if (LineUtil.PointToSegment(pt, s) < 5)
					return true;

				pt1 = pt2;
			}
			return false;
		}

		/// Rebuild the points of the spiro (after a significant change, eg. resize)
		private void Init()
		{
			points = new PointF[revolutions*resolution*2+1];

			float totalLen=0;
			for (int t = 0; t < revolutions*resolution*2+1; t++)
			{
				double theta = (Math.PI/resolution*t);
				PointF pt = CalcPoint(theta);
				points[t] = pt;
				if ( t > 0 )
				{
					float d=LineUtil.Distance(points[t-1], points[t]);
					totalLen+=d;
				}
			}
		}

		/// Recalculate the revolutions needed to return to start point
		private void CalcOrbits()
		{
			revolutions = CalcOrbits(R, r);
		}

		/// Calculate the max distance of any drawing from the center (includes the circles
		/// shown in a trace so can be greater than MaxPenDistance)
		protected int MaxExtent
		{
			get { return TrueBoundingRect.Width/2; }
		}

		/// Get or set whether to animate the shape
		public bool Animate
		{
			get { return animate; }
			set { animate=value; }
		}

		/// Get or set whether to repeat animation after the first time
		public bool Repeat
		{
			get { return repeat; }
			set 
			{
				if ( repeat != value )
				{
					repeat=value;
					if ( repeat )
						complete=false;

					ResetCounter();
				}
			}
		}

		/// Get or set whether to always show spiro info underneath
		[XmlIgnore]
		public bool ShowInfo
		{
			get { return showInfo; }
			set { showInfo=value; }
		}

		/// Get or set the pen thickness
		public int PenWidth
		{
			get { return penWidth; }
			set { penWidth=value; }
		}

		/// Get or set the spiro colour
		public Color Colour
		{
			set { colour=value; }
		}

		/// Get the bounding rect, including info box if shown
		[XmlIgnore]
		public Rectangle BoundingRect
		{
			get
			{
				Rectangle rc=TrueBoundingRect;
				if ( selected || showInfo )
					rc.Height+=80; // allow room for info at bottom

				rc.Inflate(10,10);
				return rc;
			}
		}

		/// Get or set whether to show the trace (the moving circle) during animation
		public bool ShowTrace
		{
			get { return showTrace; }
			set { showTrace=value; }
		}

		/// Get or set whether to randomise the shape after each full drawn
		public bool Randomise
		{
			set { randomise=value; }
			get { return randomise;}
		}

		public int FixedRadius
		{
			get { return R; }
			set
			{
				R=value;
				Reset();
			}
		}

		public int MovingRadius
		{
			get { return r; }
			set
			{
				r=value;
				Reset();
			}
		}

		public int PenDistance
		{
			get { return f; }
			set
			{
				f=value;
				Reset();
			}
		}

		public double Speed
		{
			get { return speed; }
			set { speed=value; Reset(); }
		}
	}
}