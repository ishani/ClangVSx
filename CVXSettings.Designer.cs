namespace ClangVSx
{
  /// <summary>
  /// Settings Dialog Box
  /// </summary>
  partial class CVXSettings
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CVXSettings));
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.cvxPic = new System.Windows.Forms.PictureBox();
      this.cvxStats = new System.Windows.Forms.TextBox();
      this.cvxBrowse = new System.Windows.Forms.Button();
      this.cvxLocation = new System.Windows.Forms.TextBox();
      this.linkLabel2 = new System.Windows.Forms.LinkLabel();
      this.cvxDone = new System.Windows.Forms.Button();
      this.findClangExe = new System.Windows.Forms.OpenFileDialog();
      this.panel1 = new System.Windows.Forms.Panel();
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.label1 = new System.Windows.Forms.Label();
      this.cvxCancel = new System.Windows.Forms.Button();
      this.bridgeOps = new System.Windows.Forms.GroupBox();
      this.label2 = new System.Windows.Forms.Label();
      this.cvxCommonArgs = new System.Windows.Forms.TextBox();
      this.cvxBatch = new System.Windows.Forms.CheckBox();
      this.cvxShowCmds = new System.Windows.Forms.CheckBox();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.cvxPic)).BeginInit();
      this.panel1.SuspendLayout();
      this.bridgeOps.SuspendLayout();
      this.SuspendLayout();
      // 
      // pictureBox1
      // 
      this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
      this.pictureBox1.Location = new System.Drawing.Point(11, 12);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(157, 234);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.pictureBox1.TabIndex = 0;
      this.pictureBox1.TabStop = false;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.cvxPic);
      this.groupBox1.Controls.Add(this.cvxStats);
      this.groupBox1.Controls.Add(this.cvxBrowse);
      this.groupBox1.Controls.Add(this.cvxLocation);
      this.groupBox1.Location = new System.Drawing.Point(174, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(419, 91);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Location of Clang Executable";
      // 
      // cvxPic
      // 
      this.cvxPic.Image = ((System.Drawing.Image)(resources.GetObject("cvxPic.Image")));
      this.cvxPic.Location = new System.Drawing.Point(376, 47);
      this.cvxPic.Name = "cvxPic";
      this.cvxPic.Size = new System.Drawing.Size(32, 32);
      this.cvxPic.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.cvxPic.TabIndex = 2;
      this.cvxPic.TabStop = false;
      // 
      // cvxStats
      // 
      this.cvxStats.ImeMode = System.Windows.Forms.ImeMode.NoControl;
      this.cvxStats.Location = new System.Drawing.Point(9, 45);
      this.cvxStats.Multiline = true;
      this.cvxStats.Name = "cvxStats";
      this.cvxStats.ReadOnly = true;
      this.cvxStats.Size = new System.Drawing.Size(361, 34);
      this.cvxStats.TabIndex = 3;
      // 
      // cvxBrowse
      // 
      this.cvxBrowse.Location = new System.Drawing.Point(376, 19);
      this.cvxBrowse.Name = "cvxBrowse";
      this.cvxBrowse.Size = new System.Drawing.Size(32, 20);
      this.cvxBrowse.TabIndex = 2;
      this.cvxBrowse.Text = "...";
      this.cvxBrowse.UseVisualStyleBackColor = true;
      this.cvxBrowse.Click += new System.EventHandler(this.cvxBrowse_Click);
      // 
      // cvxLocation
      // 
      this.cvxLocation.Location = new System.Drawing.Point(9, 19);
      this.cvxLocation.Name = "cvxLocation";
      this.cvxLocation.Size = new System.Drawing.Size(361, 20);
      this.cvxLocation.TabIndex = 0;
      this.cvxLocation.TextChanged += new System.EventHandler(this.cvxLocation_TextChanged);
      // 
      // linkLabel2
      // 
      this.linkLabel2.AutoSize = true;
      this.linkLabel2.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
      this.linkLabel2.Location = new System.Drawing.Point(29, 249);
      this.linkLabel2.Name = "linkLabel2";
      this.linkLabel2.Size = new System.Drawing.Size(108, 13);
      this.linkLabel2.TabIndex = 4;
      this.linkLabel2.TabStop = true;
      this.linkLabel2.Text = "http://clang.llvm.org/";
      this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.url_LinkClicked);
      // 
      // cvxDone
      // 
      this.cvxDone.Location = new System.Drawing.Point(481, 328);
      this.cvxDone.Name = "cvxDone";
      this.cvxDone.Size = new System.Drawing.Size(112, 23);
      this.cvxDone.TabIndex = 5;
      this.cvxDone.Text = "Accept";
      this.cvxDone.UseVisualStyleBackColor = true;
      this.cvxDone.Click += new System.EventHandler(this.cvxDone_Click);
      // 
      // findClangExe
      // 
      this.findClangExe.FileName = "clang.exe";
      this.findClangExe.Filter = "Clang C++ Compiler|clang.exe|All Files|*.*";
      this.findClangExe.Title = "Find Clang...";
      // 
      // panel1
      // 
      this.panel1.BackColor = System.Drawing.Color.Wheat;
      this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.panel1.Controls.Add(this.linkLabel1);
      this.panel1.Controls.Add(this.label1);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 362);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(608, 24);
      this.panel1.TabIndex = 6;
      // 
      // linkLabel1
      // 
      this.linkLabel1.AutoSize = true;
      this.linkLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
      this.linkLabel1.Location = new System.Drawing.Point(476, 4);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(115, 13);
      this.linkLabel1.TabIndex = 5;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "http://www.ishani.org/";
      this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.url_LinkClicked);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.ForeColor = System.Drawing.Color.Black;
      this.label1.Location = new System.Drawing.Point(6, 3);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(325, 16);
      this.label1.TabIndex = 4;
      this.label1.Text = "ClangVSx Compiler Bridge - Harry Denholm, ishani.org 2011";
      // 
      // cvxCancel
      // 
      this.cvxCancel.Location = new System.Drawing.Point(363, 328);
      this.cvxCancel.Name = "cvxCancel";
      this.cvxCancel.Size = new System.Drawing.Size(112, 23);
      this.cvxCancel.TabIndex = 7;
      this.cvxCancel.Text = "Cancel";
      this.cvxCancel.UseVisualStyleBackColor = true;
      this.cvxCancel.Click += new System.EventHandler(this.cvxCancel_Click);
      // 
      // bridgeOps
      // 
      this.bridgeOps.Controls.Add(this.label2);
      this.bridgeOps.Controls.Add(this.cvxCommonArgs);
      this.bridgeOps.Controls.Add(this.cvxBatch);
      this.bridgeOps.Controls.Add(this.cvxShowCmds);
      this.bridgeOps.Location = new System.Drawing.Point(174, 109);
      this.bridgeOps.Name = "bridgeOps";
      this.bridgeOps.Size = new System.Drawing.Size(419, 213);
      this.bridgeOps.TabIndex = 8;
      this.bridgeOps.TabStop = false;
      this.bridgeOps.Text = "Options";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.ForeColor = System.Drawing.Color.Black;
      this.label2.Location = new System.Drawing.Point(6, 68);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(199, 16);
      this.label2.TabIndex = 5;
      this.label2.Text = "Compiler arguments to add to all files :";
      // 
      // cvxCommonArgs
      // 
      this.cvxCommonArgs.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.cvxCommonArgs.Location = new System.Drawing.Point(8, 87);
      this.cvxCommonArgs.Name = "cvxCommonArgs";
      this.cvxCommonArgs.Size = new System.Drawing.Size(399, 21);
      this.cvxCommonArgs.TabIndex = 2;
      // 
      // cvxBatch
      // 
      this.cvxBatch.AutoSize = true;
      this.cvxBatch.Location = new System.Drawing.Point(10, 39);
      this.cvxBatch.Name = "cvxBatch";
      this.cvxBatch.Size = new System.Drawing.Size(242, 17);
      this.cvxBatch.TabIndex = 1;
      this.cvxBatch.Text = "Generate batch files for compilation sequence";
      this.cvxBatch.UseVisualStyleBackColor = true;
      // 
      // cvxShowCmds
      // 
      this.cvxShowCmds.AutoSize = true;
      this.cvxShowCmds.Location = new System.Drawing.Point(10, 20);
      this.cvxShowCmds.Name = "cvxShowCmds";
      this.cvxShowCmds.Size = new System.Drawing.Size(271, 17);
      this.cvxShowCmds.TabIndex = 0;
      this.cvxShowCmds.Text = "Dump all command line arguments to output window";
      this.cvxShowCmds.UseVisualStyleBackColor = true;
      // 
      // CVXSettings
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(608, 386);
      this.ControlBox = false;
      this.Controls.Add(this.bridgeOps);
      this.Controls.Add(this.cvxCancel);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.cvxDone);
      this.Controls.Add(this.linkLabel2);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.pictureBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "CVXSettings";
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "ClangVSx Settings";
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.cvxPic)).EndInit();
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.bridgeOps.ResumeLayout(false);
      this.bridgeOps.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Button cvxBrowse;
    private System.Windows.Forms.TextBox cvxLocation;
    private System.Windows.Forms.LinkLabel linkLabel2;
    private System.Windows.Forms.Button cvxDone;
    private System.Windows.Forms.OpenFileDialog findClangExe;
    private System.Windows.Forms.TextBox cvxStats;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.LinkLabel linkLabel1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button cvxCancel;
    private System.Windows.Forms.GroupBox bridgeOps;
    private System.Windows.Forms.CheckBox cvxShowCmds;
    private System.Windows.Forms.CheckBox cvxBatch;
    private System.Windows.Forms.PictureBox cvxPic;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox cvxCommonArgs;


  }
}