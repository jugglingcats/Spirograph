using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Spirograph
{
	public class FlatButton : Button
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private Container components = null;
		private bool hover;

		public FlatButton()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter (e);
			hover=true;
			Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave (e);
			hover=false;
			Invalidate();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if ( this.ImageList != null )
				this.ImageList.Draw(e.Graphics, ImageIndex, 0, 0);
			else
			{
				e.Graphics.FillRectangle(Brushes.White, ClientRectangle);
				e.Graphics.DrawString(this.Text, this.Font, new SolidBrush(ForeColor), 0, 0);
			}

			if ( hover )
				e.Graphics.DrawRectangle(Pens.Black, 0, 0, Width-1, Height-1);

			if ( Focused )
			{
				Rectangle rc=ClientRectangle;
				rc.Inflate(-1, -1);
				ControlPaint.DrawFocusRectangle(e.Graphics, rc);
			}
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
			components = new System.ComponentModel.Container();
		}
		#endregion
	}
}
