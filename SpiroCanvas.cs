using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Security;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Spirograph
{
	/// <summary>
	/// The SpiroCanvas control is a container for SpiroInstances. It manages all interaction
	/// between the user and individual spirographs.
	/// </summary>
	public class SpiroCanvas : UserControl
	{
		private IContainer components;

		private SpiroCollection spiros=new SpiroCollection();		// list of spirographs being managed
		private int current=-1;							// index of selected spirograph
		private HitInfo captureHitInfo=HitInfo.None;	// hit info for mouse move tracking
		private bool capture;							// is the mouse captured (drag)
		private Point dragOffset;						// marks the start point of a drag
		private Timer animateTimer;						// global timer for animating all spiros
		private int spiroUnderCursor=-1;				// index of spiro under cursor
		private FlatButton spiroMenuButton;				// arrow button to display popup menu
		private PrintDocument printDocument;			// used for printing
		private Point autoPanPosition=Point.Empty;		// used to ensure top-left spiro is fully visible

		// This is a spiro instance created when the current spiro is manipulated,
		// in order to give feedback on the new location, size, etc. On mouse down
		// it is cloned from the selected spiro. On mouse up, this spiro replaces the 
		// current spiro (which is discarded)
		private SpiroInstance captureSpiro;

		private ContextMenu popupMenu;					// popup menu for selected spiro
		private MenuItem menuAnimate;
		private MenuItem menuRepeat;
		private MenuItem menuChangeType;
		private MenuItem menuClone;
		private MenuItem menuDelete;
		private MenuItem menuColour;
		private MenuItem menuThickness;
		private MenuItem menuThicknessOne;
		private MenuItem menuThicknessTwo;
		private MenuItem menuThicknessThree;
		private MenuItem menuThicknessFour;
		private MenuItem menuThicknessFive;
		private MenuItem menuThicknessSix;
		private MenuItem menuThicknessSeven;
		private MenuItem menuThicknessEight;
		private MenuItem menuThicknessNine;
		private MenuItem menuAbout;
		private MenuItem menuSeparator1;
		private MenuItem menuRandomise;
		private MenuItem menuShowTrace;
		private MenuItem menuSeparator2;

		public event EventHandler SpiroComplete;

		public SpiroCanvas()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// set the right-hand arrow char into the button text
			// (we do this here rather than in designer because it gives
			//	a warning about unicode chars and file encoding)
			this.spiroMenuButton.Text = "\u25ba";

			// make sure we get nice smooth redraws
			SetStyle(ControlStyles.DoubleBuffer, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.ResizeRedraw, true);

			// add first spiro
			SpiroInstance si=new Hypotrochoid();
			AddSpiro(si);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			// nice and simple...
			Graphics g=e.Graphics;
			// translate world coords to canvas coords
			AdjustGraphics(g);
			// draw each spiro
			foreach ( SpiroInstance si in spiros )
				si.Draw(g, false);

			// If a spiro is selected and we're not already in mouse move then
			// highlight the current spiro. If we're already in a mouse move then
			// the highlight is done using the captureSpiro in the mouse move event
			if ( current >= 0 && !capture )
				DrawHighlight(CurrentSpiro, e.Graphics, captureHitInfo, false);

//			PaperSize paperSize=printDocument.DefaultPageSettings.PaperSize;
//			PrinterResolution resolution=printDocument.DefaultPageSettings.PrinterResolution;
//			Margins m=(Margins) printDocument.DefaultPageSettings.Margins.Clone();
//
//			float x=(float) paperSize.Width; // * resolution.X / 100;
//			float y=(float) paperSize.Height; // * resolution.Y / 100;

//			m.Left = (int) (1.0*m.Left*resolution.X/254);
//			m.Right = (int) (1.0*m.Right*resolution.X/254);
//			m.Top = (int) (1.0*m.Top*resolution.X/254);
//			m.Bottom = (int) (1.0*m.Bottom*resolution.X/254);

//			RectangleF rect=new RectangleF(0, 0, x, y);
//			rect.Inflate(-20, -20);
//			rect.X=m.Left;
//			rect.Y=m.Top;
//			rect.Width-=(m.Left+m.Right);
//			rect.Height-=(m.Top+m.Bottom);
//			g.DrawRectangle(Pens.Gray, Rectangle.Round(rect));
		}

		private void AdjustGraphics(Graphics g)
		{
			// adjust for scroll bars and the auto-pan feature
			g.TranslateTransform(AutoScrollPosition.X-autoPanPosition.X, AutoScrollPosition.Y-autoPanPosition.Y);
		}

		private void DrawHighlight(SpiroInstance si, Graphics g, HitInfo hi, bool showInfo)
		{
			// simple helper
			si.DrawHighlight(g, hi);
			if ( showInfo )
			{
				Point pt=si.FocusRect.Location;
				pt.Offset(2,2);
				si.DrawInfo(g, pt, Color.Gray);
			}
		}

		private Rectangle CanvasToClient(Rectangle rc)
		{
			// adjust for scroll bars and the auto-pan feature
			rc.Offset(AutoScrollPosition.X-autoPanPosition.X, AutoScrollPosition.Y-autoPanPosition.Y);
			return rc;
		}

		/// This method is called by the mouse move handler when we're dragging (ie. mouse down)
		private void ProcessDrag(MouseEventArgs e)
		{
			Point pt=new Point(e.X, e.Y);
			pt=ClientToCanvas(pt);

			// save the old rectangle so we can invalidate it
			Rectangle oldRect=CanvasToClient(captureSpiro.BoundingRect);

			bool drawInfo=false;

			// perform appropriate action based on the cursor position over the spiro
			// (when the mouse move was started)
			switch ( captureHitInfo )
			{
				case HitInfo.FixedCircle:
					captureSpiro.ResizeFixedCircle(pt);
					drawInfo=true;
					break;

				case HitInfo.MovingCircle:
					captureSpiro.ResizeMovingCircle(pt);
					drawInfo=true;
					break;

				case HitInfo.Bounds:
					captureSpiro.Location=pt;
					// adjust for mouse location at start of move otherwise
					// it's confusing for the user
					Point pt2=pt;
					pt2.Offset(-dragOffset.X,  -dragOffset.Y);
					captureSpiro.Location=pt2;
					break;

				case HitInfo.Resize:
					pt.Offset(-CurrentSpiro.Location.X, -CurrentSpiro.Location.Y);
					pt.Offset(-dragOffset.X, -dragOffset.Y);
					int diff=pt.Y;
					if ( Math.Abs(pt.X) > Math.Abs(pt.Y) )
						diff=pt.X;

					captureSpiro.Resize(CurrentSpiro, diff);
					drawInfo=true;
					break;

				case HitInfo.Pen:
					captureSpiro.MovePen(pt);
					drawInfo=true;
					break;

				default:
					return;
			}

			// remove any old highlight
			Invalidate(oldRect);

			// ensure this is done before we draw the new one
			// (otherwise it will get redraw with no highlight when
			//	the invalidate is eventually processed in idle time)
			Update();

			// draw the new highlight
			using ( Graphics g=CreateGraphics() )
			{
				AdjustGraphics(g);						
				DrawHighlight(captureSpiro, g, captureHitInfo, drawInfo);
			}
		}

		/// Helper combining the enabled and active flags
		protected bool Active
		{
			get { return Enabled && !DesignMode; }
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if ( !Active )
				// don't process event if we're not active
				return;

			Point pt=new Point(e.X, e.Y);
			pt=ClientToCanvas(pt);

			if ( capture )
			{
				// special handling while dragging
				ProcessDrag(e);
				return;
			}
			else if ( captureSpiro != null && LineUtil.Distance(dragOffset, pt) > 3 )
			{
				// this handles the case when the user starts dragging immediately after
				// selecting, ie. click and drag rather than click, click and drag
				capture=true;
				animateTimer.Enabled=false;
				ProcessDrag(e);
				return;
			}

			// record current so we can compare later
			HitInfo currentHitInfo=captureHitInfo;

			// set default if nothing found under cursor
			captureHitInfo=HitInfo.None;

			// determine the spiro under the cursor
			spiroUnderCursor=-1;
			for ( int n=0; n< spiros.Count; n++ )
			{
				SpiroInstance si=(SpiroInstance) spiros[n];

				// We have two different hit testing modes depending on whether
				// the spiro is active (eg. if active, any point in the bounds is
				// returned whereas if not active it must be near the line or on
				// the fixed circle)
				HitTestMode htm=HitTestMode.Default;
				if ( si.Selected )
					htm=HitTestMode.Selected;

				captureHitInfo=si.HitTest(pt, htm);
				if ( captureHitInfo == HitInfo.None )
					// nothing found, try the next one
					continue;

				// we found it
				spiroUnderCursor=n;
				break;
			}

			// set appropriate cursor based on result
			switch ( captureHitInfo )
			{
				case HitInfo.None:
					Cursor=Cursors.Default;
					break;

				case HitInfo.Bounds:
					Cursor=Cursors.SizeAll;
					break;

				case HitInfo.Curve:
					Cursor=Cursors.SizeAll;
					break;

				case HitInfo.Resize:
					Cursor=Cursors.SizeNWSE;
					break;

				default:
					Cursor=Cursors.Cross;
					break;
			}

			if ( current != spiroUnderCursor )
				// we're over a different spiro so no need to redraw anything
				return;

			// if spiro is active and the hit info has changed, we need to update the highlight
			// (because blue highlight may have changed)
			if ( current >= 0 && CurrentSpiro.Selected && currentHitInfo != captureHitInfo )
			{
				Invalidate(CurrentSpiro);
				Update();
//				using ( Graphics g = CreateGraphics() )
//				{
//					AdjustGraphics(g);
//					DrawHighlight(CurrentSpiro, g, captureHitInfo, false);
//				}
			}
		}

		/// Set the current (selected) spiro
		private void SetCurrentSpiro(int newIndex)
		{
			if ( current >= 0 && current < spiros.Count )
				// deactive any previous spiro
				CurrentSpiro.Selected=false;

			current=newIndex;
			if ( current >= 0 )
				// activate it
				CurrentSpiro.Selected=true;

			// move the floating button to the new spiro
			UpdateFloatingButton();
		}

		private Point CanvasToClient(Point pt)
		{
			pt.Offset(AutoScrollPosition.X-autoPanPosition.X, AutoScrollPosition.Y-autoPanPosition.Y);
			return pt;
		}

		/// Move the floating button
		private void UpdateFloatingButton()
		{
			if ( current < 0 )
			{
				// disable and hide it
				spiroMenuButton.Visible=false;
				spiroMenuButton.Enabled=false;
				return;
			}

			// move and show it
			Point pt=CanvasToClient(CurrentSpiro.FocusRect.Location);
			pt.Offset(2,2);
			spiroMenuButton.Location=pt;
			spiroMenuButton.Visible=true;
			spiroMenuButton.Enabled=true;
			spiroMenuButton.Invalidate();
		}

		private Point ClientToCanvas(Point pt)
		{
			pt.Offset(-AutoScrollPosition.X+autoPanPosition.X, -AutoScrollPosition.Y+autoPanPosition.Y);
			return pt;
		}

		/// Remove all spiros
		public void Clear()
		{
			SetCurrentSpiro(-1);
			foreach ( SpiroInstance si in spiros ) 
			{
				si.CycleComplete-=new EventHandler(SpiroCycleComplete);
			}
			spiros.Clear();
			Invalidate();
		}

		/// Add a new spiro provided
		public void AddSpiro(SpiroInstance si)
		{
			spiros.Add(si);
			si.CycleComplete+=new EventHandler(SpiroCycleComplete);
			RecalcBounds();
			Invalidate();
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);

			if ( e.KeyChar == 32 )
			{
				if ( capture )
					Stop();
				else
					Start(this.PointToClient(Cursor.Position));
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			Point pt=new Point(e.X, e.Y);

			if ( !Active )
				// nothing to do if not active
				return;

			Start(pt);
		}

		protected void Start(Point pt)
		{
			// hide the button
			spiroMenuButton.Enabled=false;
			spiroMenuButton.Visible=false;

			if ( spiroUnderCursor < 0 && current >= 0 )
			{
				// deselect all spiros
				// (click in region outside any spiro)
				CurrentSpiro.Selected=false;
				SetCurrentSpiro(-1);
				Invalidate();
				return;
			}

			pt=ClientToCanvas(pt);

			if ( spiroUnderCursor != current )
			{
				// click on a new (different) spiro

				if ( current >= 0 )
					// deactivate current
					CurrentSpiro.Selected=false;

				// activate this one
				SetCurrentSpiro(spiroUnderCursor);
				CurrentSpiro.Selected=true;

				// record the drag offset
				dragOffset=pt;
				dragOffset.Offset(-CurrentSpiro.Location.X,  -CurrentSpiro.Location.Y);

				// not too sure why we do this
				if ( captureHitInfo == HitInfo.Curve )
					captureHitInfo=HitInfo.Bounds;

				// create a clone to use to draw the highlight when dragging
				captureSpiro=CurrentSpiro.Clone();
				Invalidate();

				// all done
				return;
			}

			if ( current < 0 )
				// nothing selected so nothing to do - think this could be moved elsewhere
				return;

			// default case - click on current spiro

			// disable the animation
//			animateTimer.Enabled=false;

			// record the drag offset
			dragOffset=pt;
			dragOffset.Offset(-CurrentSpiro.Location.X,  -CurrentSpiro.Location.Y);
			// record the hit info for the point clicked
			captureHitInfo=CurrentSpiro.HitTest(pt, HitTestMode.Selected);
			// create a clone for highlighting
			captureSpiro=CurrentSpiro.Clone();
			capture=true;
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if ( !Active )
				// nothing to do if not active
				return;

			Stop();
		}

		protected void Stop()
		{
			if ( !capture )
			{
				// we're not captured so ignore
				captureSpiro=null;
				return;
			}

			if ( current < 0 )
				// nothing selected so ignore
				return;

			// make sure old and new get invalidated
			Invalidate(CurrentSpiro);
			Invalidate(captureSpiro);

			// replace current with the modified copy
			spiros[current]=captureSpiro;
			captureSpiro.Selected=true;

			// re-enable the animation
			animateTimer.Enabled=true;

			// tidy up
			capture=false;
			captureSpiro=null;

			// ensure bounds are updated and re-locate button
			// (spiro may have changed size)
			RecalcBounds();
			UpdateFloatingButton();
		}

		/// Helper to invalidate a given spiro region
		private void Invalidate(SpiroInstance si)
		{
			Rectangle rc=CanvasToClient(si.BoundingRect);
			Invalidate(rc);
		}

		/// Simple helper property
		private SpiroInstance CurrentSpiro
		{
			get { return (SpiroInstance) spiros[current]; }
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.animateTimer = new System.Windows.Forms.Timer(this.components);
			this.spiroMenuButton = new Spirograph.FlatButton();
			this.popupMenu = new System.Windows.Forms.ContextMenu();
			this.menuAnimate = new System.Windows.Forms.MenuItem();
			this.menuRepeat = new System.Windows.Forms.MenuItem();
			this.menuRandomise = new System.Windows.Forms.MenuItem();
			this.menuShowTrace = new System.Windows.Forms.MenuItem();
			this.menuSeparator2 = new System.Windows.Forms.MenuItem();
			this.menuChangeType = new System.Windows.Forms.MenuItem();
			this.menuClone = new System.Windows.Forms.MenuItem();
			this.menuDelete = new System.Windows.Forms.MenuItem();
			this.menuColour = new System.Windows.Forms.MenuItem();
			this.menuThickness = new System.Windows.Forms.MenuItem();
			this.menuThicknessOne = new System.Windows.Forms.MenuItem();
			this.menuThicknessTwo = new System.Windows.Forms.MenuItem();
			this.menuThicknessThree = new System.Windows.Forms.MenuItem();
			this.menuThicknessFour = new System.Windows.Forms.MenuItem();
			this.menuThicknessFive = new System.Windows.Forms.MenuItem();
			this.menuThicknessSix = new System.Windows.Forms.MenuItem();
			this.menuThicknessSeven = new System.Windows.Forms.MenuItem();
			this.menuThicknessEight = new System.Windows.Forms.MenuItem();
			this.menuThicknessNine = new System.Windows.Forms.MenuItem();
			this.menuSeparator1 = new System.Windows.Forms.MenuItem();
			this.menuAbout = new System.Windows.Forms.MenuItem();
			this.printDocument = new System.Drawing.Printing.PrintDocument();
			this.SuspendLayout();
			// 
			// animateTimer
			// 
			this.animateTimer.Enabled = true;
			this.animateTimer.Interval = 1;
			this.animateTimer.Tick += new System.EventHandler(this.Tick);
			// 
			// spiroMenuButton
			// 
			this.spiroMenuButton.Enabled = false;
			this.spiroMenuButton.Font = new System.Drawing.Font("Arial", 10F);
			this.spiroMenuButton.ForeColor = System.Drawing.SystemColors.GrayText;
			this.spiroMenuButton.Location = new System.Drawing.Point(40, 32);
			this.spiroMenuButton.Name = "spiroMenuButton";
			this.spiroMenuButton.Size = new System.Drawing.Size(16, 16);
			this.spiroMenuButton.TabIndex = 0;
			this.spiroMenuButton.Text = "x";
			this.spiroMenuButton.Visible = false;
			this.spiroMenuButton.Click += new System.EventHandler(this.ShowPopup);
			// 
			// popupMenu
			// 
			this.popupMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuAnimate,
																					  this.menuRepeat,
																					  this.menuRandomise,
																					  this.menuShowTrace,
																					  this.menuSeparator2,
																					  this.menuChangeType,
																					  this.menuClone,
																					  this.menuDelete,
																					  this.menuColour,
																					  this.menuThickness,
																					  this.menuSeparator1,
																					  this.menuAbout});
			this.popupMenu.Popup += new System.EventHandler(this.FeedbackMenu);
			// 
			// menuAnimate
			// 
			this.menuAnimate.Index = 0;
			this.menuAnimate.Text = "&Animate";
			this.menuAnimate.Click += new System.EventHandler(this.ToggleAnimate);
			// 
			// menuRepeat
			// 
			this.menuRepeat.Index = 1;
			this.menuRepeat.Text = "&Repeat";
			this.menuRepeat.Click += new System.EventHandler(this.ToggleRepeat);
			// 
			// menuRandomise
			// 
			this.menuRandomise.Index = 2;
			this.menuRandomise.Text = "&Randomise";
			this.menuRandomise.Click += new System.EventHandler(this.ToggleRandomise);
			// 
			// menuShowTrace
			// 
			this.menuShowTrace.Index = 3;
			this.menuShowTrace.Text = "&Show Trace";
			this.menuShowTrace.Click += new System.EventHandler(this.ToggleShowTrace);
			// 
			// menuSeparator2
			// 
			this.menuSeparator2.Index = 4;
			this.menuSeparator2.Text = "-";
			// 
			// menuChangeType
			// 
			this.menuChangeType.Index = 5;
			this.menuChangeType.Text = "&Make Epitrochoid";
			this.menuChangeType.Click += new System.EventHandler(this.ChangeType);
			// 
			// menuClone
			// 
			this.menuClone.Index = 6;
			this.menuClone.Text = "&Clone";
			this.menuClone.Click += new System.EventHandler(this.CloneCurrent);
			// 
			// menuDelete
			// 
			this.menuDelete.Index = 7;
			this.menuDelete.Text = "&Delete";
			this.menuDelete.Click += new System.EventHandler(this.DeleteCurrent);
			// 
			// menuColour
			// 
			this.menuColour.Index = 8;
			this.menuColour.Text = "&Set Color...";
			this.menuColour.Click += new System.EventHandler(this.ChangeColour);
			// 
			// menuThickness
			// 
			this.menuThickness.Index = 9;
			this.menuThickness.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						  this.menuThicknessOne,
																						  this.menuThicknessTwo,
																						  this.menuThicknessThree,
																						  this.menuThicknessFour,
																						  this.menuThicknessFive,
																						  this.menuThicknessSix,
																						  this.menuThicknessSeven,
																						  this.menuThicknessEight,
																						  this.menuThicknessNine});
			this.menuThickness.Text = "&Thickness";
			// 
			// menuThicknessOne
			// 
			this.menuThicknessOne.Index = 0;
			this.menuThicknessOne.Text = "&1";
			this.menuThicknessOne.Click += new System.EventHandler(this.ChangeThickness);
			// 
			// menuThicknessTwo
			// 
			this.menuThicknessTwo.Index = 1;
			this.menuThicknessTwo.Text = "&2";
			this.menuThicknessTwo.Click += new System.EventHandler(this.ChangeThickness);
			// 
			// menuThicknessThree
			// 
			this.menuThicknessThree.Index = 2;
			this.menuThicknessThree.Text = "&3";
			this.menuThicknessThree.Click += new System.EventHandler(this.ChangeThickness);
			// 
			// menuThicknessFour
			// 
			this.menuThicknessFour.Index = 3;
			this.menuThicknessFour.Text = "&4";
			this.menuThicknessFour.Click += new System.EventHandler(this.ChangeThickness);
			// 
			// menuThicknessFive
			// 
			this.menuThicknessFive.Index = 4;
			this.menuThicknessFive.Text = "&5";
			this.menuThicknessFive.Click += new System.EventHandler(this.ChangeThickness);
			// 
			// menuThicknessSix
			// 
			this.menuThicknessSix.Index = 5;
			this.menuThicknessSix.Text = "&6";
			this.menuThicknessSix.Click += new System.EventHandler(this.ChangeThickness);
			// 
			// menuThicknessSeven
			// 
			this.menuThicknessSeven.Index = 6;
			this.menuThicknessSeven.Text = "&7";
			this.menuThicknessSeven.Click += new System.EventHandler(this.ChangeThickness);
			// 
			// menuThicknessEight
			// 
			this.menuThicknessEight.Index = 7;
			this.menuThicknessEight.Text = "&8";
			this.menuThicknessEight.Click += new System.EventHandler(this.ChangeThickness);
			// 
			// menuThicknessNine
			// 
			this.menuThicknessNine.Index = 8;
			this.menuThicknessNine.Text = "&9";
			this.menuThicknessNine.Click += new System.EventHandler(this.ChangeThickness);
			// 
			// menuSeparator1
			// 
			this.menuSeparator1.Index = 10;
			this.menuSeparator1.Text = "-";
			// 
			// menuAbout
			// 
			this.menuAbout.Index = 11;
			this.menuAbout.Text = "A&bout...";
			this.menuAbout.Click += new System.EventHandler(this.ShowAboutDialog);
			// 
			// printDocument
			// 
			this.printDocument.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.PrintPage);
			// 
			// SpiroCanvas
			// 
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.spiroMenuButton);
			this.Name = "SpiroCanvas";
			this.Size = new System.Drawing.Size(320, 312);
			this.ResumeLayout(false);

		}
		#endregion

		/// Event handler for the timer
		private void Tick(object sender, EventArgs e)
		{
			if ( DesignMode )
				// don't animate if in design mode
				return;

			// take a copy of the spiros list because
			// event handlers can modify the collection
			Queue q=new Queue();
			foreach ( SpiroInstance si in spiros ) 
			{
				q.Enqueue(si);
			}

			while ( q.Count > 0 )
			{
				SpiroInstance si=(SpiroInstance) q.Dequeue();

				Region rm=si.Tick();
				if ( rm == null )
					continue;

				rm.Translate(AutoScrollPosition.X-autoPanPosition.X, AutoScrollPosition.Y-autoPanPosition.Y);

				Invalidate(rm);
			}
		}

		/// Checks total area for all spiros and adjusts scroll bars and auto-pan settings
		private void RecalcBounds()
		{
			Rectangle rc=new Rectangle();

			// add all spiro bounds to the region
			foreach ( SpiroInstance si in spiros )
			{
				Rectangle bounds=si.BoundingRect;
				rc=Rectangle.Union(rc, bounds);
			}

//			// record autoCurrent so we know whether it's changed
//			Point autoCurrent=autoPanPosition;
//			// the location make be negative if spiros dragged off client
//			autoPanPosition=rc.Location;
//
//			AutoScrollMinSize=rc.Size;
//
//			if ( !autoCurrent.Equals(autoPanPosition) )
//			{
//				// it's changed so all spiros will be offset and need redraw
//				Invalidate();
//			}

			if ( AutoScroll ) 
				AutoScrollMinSize=new Size(rc.Right, rc.Bottom);
		}

		/// Add a new default hypotrochoid
		public void AddHypotrochoid()
		{
			SpiroInstance si=new Hypotrochoid();
			spiros.Add(si);
		}

		/// Add a new default epitrochoid
		public void AddEpitrochoid()
		{
			SpiroInstance si=new Epitrochoid();
			spiros.Add(si);
		}

		/// Event handler for menu button
		private void ShowPopup(object sender, EventArgs e)
		{
			// show the popup menu
			popupMenu.Show(this, new Point(spiroMenuButton.Left+1, spiroMenuButton.Bottom));
		}

		/// Event handler for Animate menu item
		private void ToggleAnimate(object sender, EventArgs e)
		{
			CurrentSpiro.Animate=!CurrentSpiro.Animate;
			if ( CurrentSpiro.Animate )
				// de-select to get it moving (just a nice visual
				// feedback to the user)
				SetCurrentSpiro(-1);

			Invalidate();
		}

		/// Event handler for Repeat menu item
		private void ToggleRepeat(object sender, EventArgs e)
		{
			CurrentSpiro.Repeat=!CurrentSpiro.Repeat;
			Invalidate();
		}

		/// Event handler for Randomise menu item
		private void ToggleRandomise(object sender, EventArgs e)
		{
			CurrentSpiro.Randomise=!CurrentSpiro.Randomise;
		}

		/// Event handler for Show Trace menu item
		private void ToggleShowTrace(object sender, EventArgs e)
		{
			CurrentSpiro.ShowTrace=!CurrentSpiro.ShowTrace;
			Invalidate();
		}

		/// Event handler for menu popup. Sets menu state
		private void FeedbackMenu(object sender, EventArgs e)
		{
			menuAnimate.Checked=CurrentSpiro.Animate;
			menuRepeat.Checked=CurrentSpiro.Repeat;
			menuRandomise.Checked=CurrentSpiro.Randomise;
			menuShowTrace.Checked=CurrentSpiro.ShowTrace;

			// these options only enabled if animated
			menuRepeat.Enabled=CurrentSpiro.Animate;
			menuRandomise.Enabled=CurrentSpiro.Animate;
			menuShowTrace.Enabled=CurrentSpiro.Animate;

			// set appropriate text for flipping between types
			if ( CurrentSpiro is Epitrochoid )
				menuChangeType.Text="Make Hypotrochoid";
			else
				menuChangeType.Text="Make Epitrochoid";

			// only allow delete if more than one
			menuDelete.Enabled=spiros.Count > 1;

			// set the check on the thickness
			int n=1;
			foreach ( MenuItem mi in menuThickness.MenuItems )
			{
				mi.Checked=(n++ == CurrentSpiro.PenWidth);
			}
		}

		/// Event handler for type change menu item
		private void ChangeType(object sender, EventArgs e)
		{
			SpiroInstance si;
			if ( CurrentSpiro is Epitrochoid )
				si=new Hypotrochoid();
			else
				si=new Epitrochoid();

			// clone all values from current
			si.Clone(CurrentSpiro);
			// replace current with new
			spiros[current]=si;
			Invalidate();
			UpdateFloatingButton();
		}

		/// Event handler for Clone menu item
		private void CloneCurrent(object sender, EventArgs e)
		{
			SpiroInstance si=CurrentSpiro.Clone();
			Point pt=si.Location;
			pt.Offset(100, 100);
			si.Location=pt;
			si.Selected=false;
			si.Animate=false;
			spiros.Add(si);

			Invalidate();
		}

		/// Event handler for all thickness menu items
		private void ChangeThickness(object sender, EventArgs e)
		{
			MenuItem mi=(MenuItem) sender;
			// set thickness based on index of menu item in parent menu
			CurrentSpiro.PenWidth=mi.Index+1;
			Invalidate();
		}

		/// Event handler for Delete menu item
		private void DeleteCurrent(object sender, EventArgs e)
		{
			spiros.Remove(CurrentSpiro);
			SetCurrentSpiro(-1);
			Invalidate();
		}

		/// Event handler for Set Colour menu item
		private void ChangeColour(object sender, EventArgs e)
		{
			ColorDialog dlg=new ColorDialog();
			DialogResult ret=dlg.ShowDialog(this);
			if ( ret != DialogResult.OK )
				// user cancelled
				return;

			CurrentSpiro.Colour=dlg.Color;
			Invalidate();
		}

		/// Event handler for printing (fired by PrintDialog)
		private void PrintPage(object sender, PrintPageEventArgs e)
		{
			foreach ( SpiroInstance si in spiros )
				si.Draw(e.Graphics, true);
		}

		/// Event handler fired when canvas is enabled/disabled
		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			if ( !Active )
			{
				// de-select all spiros when disabled
				SetCurrentSpiro(-1);
				Invalidate();
			}
		}

		private void ShowAboutDialog(object sender, EventArgs e)
		{
			try
			{
				AboutDialog dlg=new AboutDialog();
				dlg.ShowDialog(this);
			}
			catch ( SecurityException )
			{
				// this can happen when running in a browser with minimum trust
				// (requires extended UIPermission)
				MessageBox.Show("The current security context does not allow this operation");
			}
		}

		/// Used by a form / container during printing
		public PrintDocument PrintDocument
		{
			get { return printDocument; }
		}

		public void SaveAsXml(string filename)
		{
			XmlTextWriter xtw=new XmlTextWriter(filename, Encoding.ASCII);
			xtw.Formatting=Formatting.Indented;

			try
			{
				XmlSerializer xs=new XmlSerializer(typeof(SpiroCollection));
				xs.Serialize(xtw, spiros);
			}
			finally
			{
				xtw.Close();
			}
		}

		public void LoadFromXml(string filename)
		{
			XmlTextReader xtr=new XmlTextReader(filename);

			try
			{
				XmlSerializer xs=new XmlSerializer(typeof(SpiroCollection));
				SpiroCollection spiros=(SpiroCollection) xs.Deserialize(xtr);

				Clear();
				this.spiros=spiros;
			}
			finally
			{
				xtr.Close();
			}
		}

		public bool IntersectsAny(SpiroInstance test) 
		{
			foreach ( SpiroInstance si in spiros ) 
			{
				if ( si.Equals(test) )
					continue;

				if ( si.BoundingRect.IntersectsWith(test.BoundingRect) ) 
				{
					return true;
				}
			}
			return false;
		}

		private void SpiroCycleComplete(object sender, EventArgs e)
		{
			if ( this.SpiroComplete != null ) 
			{
				SpiroComplete(sender, e);
			}
		}
	}
}
