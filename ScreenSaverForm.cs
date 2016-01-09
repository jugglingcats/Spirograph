using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Spirograph
{
	/// <summary>
	/// Summary description for ScreenSaverForm.
	/// </summary>
	public class ScreenSaverForm : System.Windows.Forms.Form
	{
		private Spirograph.SpiroCanvas spiroCanvas1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private Point mouseXY;
		private Rectangle visibleRect;
		private int numSpiros=1;

		public ScreenSaverForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			spiroCanvas1.SpiroComplete+=new EventHandler(SpiroComplete);
			spiroCanvas1.Clear();;
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.spiroCanvas1 = new Spirograph.SpiroCanvas();
			this.SuspendLayout();
			// 
			// spiroCanvas1
			// 
			this.spiroCanvas1.AutoScroll = true;
			this.spiroCanvas1.BackColor = System.Drawing.Color.Black;
			this.spiroCanvas1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.spiroCanvas1.Enabled = false;
			this.spiroCanvas1.Location = new System.Drawing.Point(0, 0);
			this.spiroCanvas1.Name = "spiroCanvas1";
			this.spiroCanvas1.Size = new System.Drawing.Size(544, 528);
			this.spiroCanvas1.TabIndex = 0;
			// 
			// ScreenSaverForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(544, 528);
			this.Controls.Add(this.spiroCanvas1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "ScreenSaverForm";
			this.Text = "ScreenSaverForm";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
			this.Load += new System.EventHandler(this.OnLoad);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
			this.ResumeLayout(false);

		}
		#endregion

		private void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (!mouseXY.IsEmpty)
			{
				if (mouseXY != new Point(e.X, e.Y))
					Close();
				if (e.Clicks > 0)
					Close();
			}
			mouseXY = new Point(e.X, e.Y);
		
		}

		private void OnLoad(object sender, System.EventArgs e)
		{
			int xmin=0;
			int xmax=0;
			int ymin=0;
			int ymax=0;

			for(int x = 0; x <= Screen.AllScreens.GetUpperBound(0); x++)
			{
				if(Screen.AllScreens[x].Bounds.Left<xmin)
					xmin=Screen.AllScreens[x].Bounds.Left;
				if(Screen.AllScreens[x].Bounds.Top<ymin)
					ymin=Screen.AllScreens[x].Bounds.Top;
				if(Screen.AllScreens[x].Bounds.Right>xmax)
					xmax=Screen.AllScreens[x].Bounds.Right;
				if(Screen.AllScreens[x].Bounds.Bottom>ymax)
					ymax=Screen.AllScreens[x].Bounds.Bottom;
			}
			Rectangle rectBounds = new Rectangle(xmin, ymin, xmax-xmin, ymax-ymin);
			this.Bounds = rectBounds;
//			VisibleRect = new Rectangle(0, 0, Bounds.Width-pictureBox.Width, Bounds.Height-pictureBox.Height);
			Cursor.Hide();
			TopMost = true;

			SpiroInstance si=new Hypotrochoid(65, 20, 28, new Point(100, 100));
			si.ShowTrace=false;
			si.Randomise=false;
			si.MaxRandomDiameter=Math.Min(rectBounds.Width, rectBounds.Height) / 4;
			AutoRandom(si);
			spiroCanvas1.AddSpiro(si);
		}

		private Point RandomLocation() 
		{
			Random rnd=new Random();

			while (true) 
			{
				int w=Bounds.Width;
				int h=Bounds.Height;
				int x=rnd.Next(100, w-100);
				int y=rnd.Next(100, h-100);

				Point pt=new Point(x, y);

				foreach ( Screen s in Screen.AllScreens ) 
				{
					Rectangle rc=s.Bounds;
					rc.Inflate(-100,-100);
					if ( rc.Contains(pt) ) 
					{
						return pt;
					}
				}
			}
		}

		private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			Close();
		}

		private void SpiroComplete(object sender, EventArgs e)
		{
			SpiroInstance si=sender as SpiroInstance;

			if ( numSpiros < 4 ) 
			{
				SpiroInstance next;
				if ( (numSpiros % 2) == 1 ) 
				{
					next=new Epitrochoid();
				} 
				else
				{
					next=new Hypotrochoid();
				}
				next.ShowTrace=false;
				next.Randomise=false;
				next.MaxRandomDiameter=Math.Min(Bounds.Width, Bounds.Height) / 4;
				AutoRandom(next);
				spiroCanvas1.AddSpiro(next);
				numSpiros++;
			}

			AutoRandom(si);
		}

		private void AutoRandom(SpiroInstance si) 
		{
			while ( true ) 
			{
				si.ResetRandom();
				si.Colour=RandomColour();
				si.Location=RandomLocation();
				si.Speed=RandomSpeed();
				si.PenWidth=RandomLineThickness();
				if ( !spiroCanvas1.IntersectsAny(si) ) 
					break;
			}
		}

		private Color RandomColour() 
		{
			Random rnd=new Random();
			int r=128+rnd.Next(0, 127);
			int g=128+rnd.Next(0, 127);
			int b=128+rnd.Next(0, 127);
			return Color.FromArgb(r, g, b);
		}

		private int RandomSpeed() 
		{
			return 20;
		}

		private int RandomLineThickness() 
		{
			return new Random().Next(1, 5);
		}
	}
}
