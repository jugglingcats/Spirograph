using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Spirograph
{
	/// <summary>
	/// This is the main form - a simple wrapper around the SpiroCanvas control.
	/// </summary>
	public class SpiroMainForm : Form
    {
        private IContainer components;
		private PrintPreviewDialog printPreviewDialog;
		private MainMenu mainMenu;
		private MenuItem menuFile;
		private MenuItem menuPrintPreview;
		private MenuItem menuPageSetup;
		private MenuItem menuPrint;
		private PrintDialog printDialog;
		private PageSetupDialog pageSetupDialog;
		private System.Windows.Forms.MenuItem menuFileOpen;
		private System.Windows.Forms.MenuItem menuFileSave;
		private System.Windows.Forms.MenuItem menuFileSaveAs;
		private System.Windows.Forms.MenuItem menuItem2;
		private SpiroCanvas spiroCanvas;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private string currentFilename=null;

		public SpiroMainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpiroMainForm));
            this.printPreviewDialog = new System.Windows.Forms.PrintPreviewDialog();
            this.mainMenu = new System.Windows.Forms.MainMenu(this.components);
            this.menuFile = new System.Windows.Forms.MenuItem();
            this.menuFileOpen = new System.Windows.Forms.MenuItem();
            this.menuFileSave = new System.Windows.Forms.MenuItem();
            this.menuFileSaveAs = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuPrintPreview = new System.Windows.Forms.MenuItem();
            this.menuPageSetup = new System.Windows.Forms.MenuItem();
            this.menuPrint = new System.Windows.Forms.MenuItem();
            this.printDialog = new System.Windows.Forms.PrintDialog();
            this.pageSetupDialog = new System.Windows.Forms.PageSetupDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.spiroCanvas = new Spirograph.SpiroCanvas();
            this.SuspendLayout();
            // 
            // printPreviewDialog
            // 
            this.printPreviewDialog.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.printPreviewDialog.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.printPreviewDialog.ClientSize = new System.Drawing.Size(400, 300);
            this.printPreviewDialog.Enabled = true;
            this.printPreviewDialog.Icon = ((System.Drawing.Icon)(resources.GetObject("printPreviewDialog.Icon")));
            this.printPreviewDialog.Name = "printPreviewDialog";
            this.printPreviewDialog.Visible = false;
            // 
            // mainMenu
            // 
            this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuFile});
            // 
            // menuFile
            // 
            this.menuFile.Index = 0;
            this.menuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuFileOpen,
            this.menuFileSave,
            this.menuFileSaveAs,
            this.menuItem2,
            this.menuPrintPreview,
            this.menuPageSetup,
            this.menuPrint});
            this.menuFile.Text = "&File";
            // 
            // menuFileOpen
            // 
            this.menuFileOpen.Index = 0;
            this.menuFileOpen.Text = "&Open...";
            this.menuFileOpen.Click += new System.EventHandler(this.OpenCanvas);
            // 
            // menuFileSave
            // 
            this.menuFileSave.Index = 1;
            this.menuFileSave.Text = "&Save";
            this.menuFileSave.Click += new System.EventHandler(this.SaveCanvas);
            // 
            // menuFileSaveAs
            // 
            this.menuFileSaveAs.Index = 2;
            this.menuFileSaveAs.Text = "Save &As...";
            this.menuFileSaveAs.Click += new System.EventHandler(this.SaveNewCanvas);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 3;
            this.menuItem2.Text = "-";
            // 
            // menuPrintPreview
            // 
            this.menuPrintPreview.Index = 4;
            this.menuPrintPreview.Text = "Print Pre&view...";
            this.menuPrintPreview.Click += new System.EventHandler(this.PrintPreview);
            // 
            // menuPageSetup
            // 
            this.menuPageSetup.Index = 5;
            this.menuPageSetup.Text = "Page Set&up...";
            this.menuPageSetup.Click += new System.EventHandler(this.PageSetup);
            // 
            // menuPrint
            // 
            this.menuPrint.Index = 6;
            this.menuPrint.Text = "&Print...";
            this.menuPrint.Click += new System.EventHandler(this.Print);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "scv";
            this.saveFileDialog.Filter = "Spirograph files|*.scv|All files|*.*";
            this.saveFileDialog.Title = "Save Spirograph";
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "scv";
            this.openFileDialog.Filter = "Spirograph files|*.scv|All files|*.*";
            // 
            // spiroCanvas
            // 
            this.spiroCanvas.AutoScroll = true;
            this.spiroCanvas.AutoScrollMinSize = new System.Drawing.Size(368, 368);
            this.spiroCanvas.BackColor = System.Drawing.Color.White;
            this.spiroCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spiroCanvas.Location = new System.Drawing.Point(0, 0);
            this.spiroCanvas.Name = "spiroCanvas";
            this.spiroCanvas.Size = new System.Drawing.Size(776, 584);
            this.spiroCanvas.TabIndex = 1;
            // 
            // SpiroMainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(776, 584);
            this.Controls.Add(this.spiroCanvas);
            this.Menu = this.mainMenu;
            this.Name = "SpiroMainForm";
            this.Text = "Spirograph";
            this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) 
		{
			if (args.Length > 0)
			{
				if (args[0].ToLower().Trim().Substring(0,2) == "/c")
				{
					MessageBox.Show("This Screen Saver has no options you can set.", "Spirograph Screen Saver", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				else if (args[0].ToLower() == "/s")
				{
					Application.Run(new ScreenSaverForm());
				}
			}
			else
			{
				Application.Run(new SpiroMainForm());
			}
		}

		/// <summary>
		/// Handle the Print Preview menu item click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PrintPreview(object sender, EventArgs e)
		{
			try
			{
				printPreviewDialog.Document=spiroCanvas.PrintDocument;
				printPreviewDialog.ShowDialog(this);
			}
			catch (Exception ex)
			{
				MessageBox.Show("The operation failed: "+ex.Message);
			}
		}

		/// <summary>
		/// Handle the Page Setup menu item click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PageSetup(object sender, EventArgs e)
		{
			try
			{
				pageSetupDialog.Document=spiroCanvas.PrintDocument;
				pageSetupDialog.ShowDialog(this);
			}
			catch (Exception ex)
			{
				MessageBox.Show("The operation failed: "+ex.Message);
			}
		}

		/// <summary>
		/// Handle the Print menu item click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Print(object sender, EventArgs e)
		{
			try
			{
				printDialog.Document=spiroCanvas.PrintDocument;
				DialogResult result=printDialog.ShowDialog(this);
				if ( result == DialogResult.OK )
					spiroCanvas.PrintDocument.Print();
			}
			catch ( Exception ex )
			{
				MessageBox.Show("The operation failed: "+ex.Message);
			}
		}

		// handler for File Save
		private void SaveCanvas(object sender, System.EventArgs e)
		{
			// no existing filename, so treat as a Save As...
			if ( currentFilename == null )
			{
				SaveNewCanvas();
				return;
			}
			SaveCurrentCanvas();
		}

		// handler for File Save As...
		private void SaveNewCanvas(object sender, System.EventArgs e)
		{
			SaveNewCanvas();
		}

		// save the current canvas (assumes currentFilename not null)
		private void SaveCurrentCanvas()
		{
			spiroCanvas.SaveAsXml(currentFilename);
		}

		// handler for File Open
		private void OpenCanvas(object sender, System.EventArgs e)
		{
			DialogResult result=openFileDialog.ShowDialog();
			if ( result != DialogResult.OK )
				return;

			try
			{
				spiroCanvas.LoadFromXml(openFileDialog.FileName);
				currentFilename=openFileDialog.FileName;
			}
			catch ( Exception ex )
			{
				MessageBox.Show("Failed to open file: "+ex.Message, "Open Spirograph", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		// prompt for new filename and save it
		private void SaveNewCanvas()
		{
			DialogResult result=saveFileDialog.ShowDialog();
			if ( result != DialogResult.OK )
				return;

			currentFilename=saveFileDialog.FileName;
			SaveCurrentCanvas();
		}
	}
}
