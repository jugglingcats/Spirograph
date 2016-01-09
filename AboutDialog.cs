using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Spirograph
{
	/// <summary>
	/// Summary description for AboutDialog.
	/// </summary>
	public class AboutDialog : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox1;
		private Spirograph.SpiroCanvas sc;

		public AboutDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			SpiroInstance si=new Hypotrochoid(65, 20, 28, new Point(75, 75));
			si.ShowTrace=false;
			si.ShowInfo=true;
			si.Randomise=true;

			sc.Clear();;
			sc.AddSpiro(si);
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
			this.sc = new Spirograph.SpiroCanvas();
			this.button1 = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// sc
			// 
			this.sc.AutoScroll = true;
			this.sc.AutoScrollMinSize = new System.Drawing.Size(409, 409);
			this.sc.BackColor = System.Drawing.SystemColors.Control;
			this.sc.Enabled = false;
			this.sc.Location = new System.Drawing.Point(8, 0);
			this.sc.Name = "sc";
			this.sc.Size = new System.Drawing.Size(184, 248);
			this.sc.TabIndex = 0;
			// 
			// button1
			// 
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.Location = new System.Drawing.Point(160, 248);
			this.button1.Name = "button1";
			this.button1.TabIndex = 1;
			this.button1.Text = "OK";
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
			this.label1.Location = new System.Drawing.Point(188, 8);
			this.label1.Name = "label1";
			this.label1.TabIndex = 2;
			this.label1.Text = "Spirograph";
			// 
			// textBox1
			// 
			this.textBox1.BackColor = System.Drawing.SystemColors.Control;
			this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBox1.Location = new System.Drawing.Point(192, 40);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = new System.Drawing.Size(189, 192);
			this.textBox1.TabIndex = 3;
			this.textBox1.Text = @"Copyright (c) 2005, Alfie Kirkpatrick

This is free software. No warranty of any kind is provided. Please feel free to distribute this software.

Spirograph shows the dynamics of Epiclochoid and Hypoclochoid curves. More information about the mathematics of these curves is easily found on the Internet.

Spirograph was written in C# for Microsoft.NET 1.1.";
			// 
			// AboutDialog
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(394, 280);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.sc);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "AboutDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "About Spirograph";
			this.ResumeLayout(false);

		}
		#endregion
	}
}
