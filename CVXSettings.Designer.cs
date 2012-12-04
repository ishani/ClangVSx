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
      this.label1 = new System.Windows.Forms.Label();
      this.linkLabel3 = new System.Windows.Forms.LinkLabel();
      this.cvxCancel = new System.Windows.Forms.Button();
      this.bridgeOps = new System.Windows.Forms.GroupBox();
      this.cvxPhases = new System.Windows.Forms.CheckBox();
      this.cvxEcho = new System.Windows.Forms.CheckBox();
      this.cvxBatch = new System.Windows.Forms.CheckBox();
      this.cvxShowCmds = new System.Windows.Forms.CheckBox();
      this.label2 = new System.Windows.Forms.Label();
      this.cvxCommonArgs = new System.Windows.Forms.TextBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.label5 = new System.Windows.Forms.Label();
      this.cvxTripleARM = new System.Windows.Forms.ComboBox();
      this.label4 = new System.Windows.Forms.Label();
      this.cvxTripleX64 = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.cvxTripleWin32 = new System.Windows.Forms.ComboBox();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.cvxCOptCPP11 = new System.Windows.Forms.CheckBox();
      this.cvxCOptMSABI = new System.Windows.Forms.CheckBox();
      this.cvxTOptOldSyntax = new System.Windows.Forms.CheckBox();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.cvxPic)).BeginInit();
      this.panel1.SuspendLayout();
      this.bridgeOps.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // pictureBox1
      // 
      this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
      this.pictureBox1.Location = new System.Drawing.Point(12, 12);
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
      this.groupBox1.Location = new System.Drawing.Point(184, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(310, 91);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Location of Clang Executable";
      // 
      // cvxPic
      // 
      this.cvxPic.Image = ((System.Drawing.Image)(resources.GetObject("cvxPic.Image")));
      this.cvxPic.Location = new System.Drawing.Point(267, 47);
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
      this.cvxStats.Size = new System.Drawing.Size(252, 34);
      this.cvxStats.TabIndex = 3;
      // 
      // cvxBrowse
      // 
      this.cvxBrowse.Location = new System.Drawing.Point(267, 19);
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
      this.cvxLocation.Size = new System.Drawing.Size(252, 20);
      this.cvxLocation.TabIndex = 0;
      this.cvxLocation.TextChanged += new System.EventHandler(this.cvxLocation_TextChanged);
      // 
      // linkLabel2
      // 
      this.linkLabel2.AutoSize = true;
      this.linkLabel2.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
      this.linkLabel2.Location = new System.Drawing.Point(33, 249);
      this.linkLabel2.Name = "linkLabel2";
      this.linkLabel2.Size = new System.Drawing.Size(108, 13);
      this.linkLabel2.TabIndex = 4;
      this.linkLabel2.TabStop = true;
      this.linkLabel2.Text = "http://clang.llvm.org/";
      this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.url_LinkClicked);
      // 
      // cvxDone
      // 
      this.cvxDone.Location = new System.Drawing.Point(679, 302);
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
      this.panel1.Controls.Add(this.label1);
      this.panel1.Controls.Add(this.linkLabel3);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 335);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(798, 24);
      this.panel1.TabIndex = 6;
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
      this.label1.Text = "ClangVSx Compiler Bridge - Harry Denholm, ishani.org 2012";
      // 
      // linkLabel3
      // 
      this.linkLabel3.AutoSize = true;
      this.linkLabel3.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
      this.linkLabel3.Location = new System.Drawing.Point(611, 4);
      this.linkLabel3.Name = "linkLabel3";
      this.linkLabel3.Size = new System.Drawing.Size(178, 13);
      this.linkLabel3.TabIndex = 10;
      this.linkLabel3.TabStop = true;
      this.linkLabel3.Text = "https://github.com/ishani/ClangVSx";
      this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.url_LinkClicked);
      // 
      // cvxCancel
      // 
      this.cvxCancel.Location = new System.Drawing.Point(561, 302);
      this.cvxCancel.Name = "cvxCancel";
      this.cvxCancel.Size = new System.Drawing.Size(112, 23);
      this.cvxCancel.TabIndex = 7;
      this.cvxCancel.Text = "Cancel";
      this.cvxCancel.UseVisualStyleBackColor = true;
      this.cvxCancel.Click += new System.EventHandler(this.cvxCancel_Click);
      // 
      // bridgeOps
      // 
      this.bridgeOps.Controls.Add(this.cvxPhases);
      this.bridgeOps.Controls.Add(this.cvxEcho);
      this.bridgeOps.Controls.Add(this.cvxBatch);
      this.bridgeOps.Controls.Add(this.cvxShowCmds);
      this.bridgeOps.Location = new System.Drawing.Point(500, 12);
      this.bridgeOps.Name = "bridgeOps";
      this.bridgeOps.Size = new System.Drawing.Size(291, 110);
      this.bridgeOps.TabIndex = 8;
      this.bridgeOps.TabStop = false;
      this.bridgeOps.Text = "Output";
      // 
      // cvxPhases
      // 
      this.cvxPhases.AutoSize = true;
      this.cvxPhases.Location = new System.Drawing.Point(10, 83);
      this.cvxPhases.Name = "cvxPhases";
      this.cvxPhases.Size = new System.Drawing.Size(228, 17);
      this.cvxPhases.TabIndex = 11;
      this.cvxPhases.Text = "Show compiler phases ( -ccc-print-phases )";
      this.cvxPhases.UseVisualStyleBackColor = true;
      // 
      // cvxEcho
      // 
      this.cvxEcho.AutoSize = true;
      this.cvxEcho.Location = new System.Drawing.Point(10, 62);
      this.cvxEcho.Name = "cvxEcho";
      this.cvxEcho.Size = new System.Drawing.Size(261, 17);
      this.cvxEcho.TabIndex = 10;
      this.cvxEcho.Text = "Echo internal compiler command line ( -ccc-echo )";
      this.cvxEcho.UseVisualStyleBackColor = true;
      // 
      // cvxBatch
      // 
      this.cvxBatch.AutoSize = true;
      this.cvxBatch.Location = new System.Drawing.Point(10, 41);
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
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.ForeColor = System.Drawing.Color.Black;
      this.label2.Location = new System.Drawing.Point(7, 20);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(199, 16);
      this.label2.TabIndex = 5;
      this.label2.Text = "Compiler arguments to add to all files :";
      // 
      // cvxCommonArgs
      // 
      this.cvxCommonArgs.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.cvxCommonArgs.Location = new System.Drawing.Point(9, 39);
      this.cvxCommonArgs.Name = "cvxCommonArgs";
      this.cvxCommonArgs.Size = new System.Drawing.Size(290, 21);
      this.cvxCommonArgs.TabIndex = 2;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.cvxTOptOldSyntax);
      this.groupBox2.Controls.Add(this.label5);
      this.groupBox2.Controls.Add(this.cvxTripleARM);
      this.groupBox2.Controls.Add(this.label4);
      this.groupBox2.Controls.Add(this.cvxTripleX64);
      this.groupBox2.Controls.Add(this.label3);
      this.groupBox2.Controls.Add(this.cvxTripleWin32);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.Controls.Add(this.cvxCommonArgs);
      this.groupBox2.Location = new System.Drawing.Point(184, 109);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(310, 179);
      this.groupBox2.TabIndex = 9;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Global ";
      // 
      // label5
      // 
      this.label5.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label5.ForeColor = System.Drawing.Color.Black;
      this.label5.Location = new System.Drawing.Point(10, 118);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(142, 23);
      this.label5.TabIndex = 11;
      this.label5.Text = "ARM Platform Triple :";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // cvxTripleARM
      // 
      this.cvxTripleARM.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.cvxTripleARM.FormattingEnabled = true;
      this.cvxTripleARM.Items.AddRange(new object[] {
            "i686-pc-win32",
            "x86_64-pc-win32"});
      this.cvxTripleARM.Location = new System.Drawing.Point(158, 118);
      this.cvxTripleARM.Name = "cvxTripleARM";
      this.cvxTripleARM.Size = new System.Drawing.Size(141, 23);
      this.cvxTripleARM.TabIndex = 10;
      this.cvxTripleARM.Text = "armv7-apple-darwin10";
      // 
      // label4
      // 
      this.label4.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label4.ForeColor = System.Drawing.Color.Black;
      this.label4.Location = new System.Drawing.Point(10, 92);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(142, 23);
      this.label4.TabIndex = 9;
      this.label4.Text = "x64 Platform Triple :";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // cvxTripleX64
      // 
      this.cvxTripleX64.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.cvxTripleX64.FormattingEnabled = true;
      this.cvxTripleX64.Items.AddRange(new object[] {
            "i686-pc-win32",
            "x86_64-pc-win32"});
      this.cvxTripleX64.Location = new System.Drawing.Point(158, 92);
      this.cvxTripleX64.Name = "cvxTripleX64";
      this.cvxTripleX64.Size = new System.Drawing.Size(141, 23);
      this.cvxTripleX64.TabIndex = 8;
      this.cvxTripleX64.Text = "x86_64-pc-win32";
      // 
      // label3
      // 
      this.label3.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.ForeColor = System.Drawing.Color.Black;
      this.label3.Location = new System.Drawing.Point(10, 66);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(142, 23);
      this.label3.TabIndex = 7;
      this.label3.Text = "Win32 Platform Triple :";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // cvxTripleWin32
      // 
      this.cvxTripleWin32.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.cvxTripleWin32.FormattingEnabled = true;
      this.cvxTripleWin32.Items.AddRange(new object[] {
            "i686-pc-win32",
            "x86_64-pc-win32"});
      this.cvxTripleWin32.Location = new System.Drawing.Point(158, 66);
      this.cvxTripleWin32.Name = "cvxTripleWin32";
      this.cvxTripleWin32.Size = new System.Drawing.Size(141, 23);
      this.cvxTripleWin32.TabIndex = 6;
      this.cvxTripleWin32.Text = "i686-pc-win32";
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.cvxCOptCPP11);
      this.groupBox3.Controls.Add(this.cvxCOptMSABI);
      this.groupBox3.Location = new System.Drawing.Point(500, 129);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(291, 158);
      this.groupBox3.TabIndex = 10;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Compilation Options";
      // 
      // cvxCOptCPP11
      // 
      this.cvxCOptCPP11.AutoSize = true;
      this.cvxCOptCPP11.Location = new System.Drawing.Point(10, 23);
      this.cvxCOptCPP11.Name = "cvxCOptCPP11";
      this.cvxCOptCPP11.Size = new System.Drawing.Size(93, 17);
      this.cvxCOptCPP11.TabIndex = 13;
      this.cvxCOptCPP11.Text = "Enable C++11";
      this.cvxCOptCPP11.UseVisualStyleBackColor = true;
      // 
      // cvxCOptMSABI
      // 
      this.cvxCOptMSABI.AutoSize = true;
      this.cvxCOptMSABI.Location = new System.Drawing.Point(10, 46);
      this.cvxCOptMSABI.Name = "cvxCOptMSABI";
      this.cvxCOptMSABI.Size = new System.Drawing.Size(201, 17);
      this.cvxCOptMSABI.TabIndex = 12;
      this.cvxCOptMSABI.Text = "Force Microsoft C++ ABI (incomplete)";
      this.cvxCOptMSABI.UseVisualStyleBackColor = true;
      // 
      // cvxTOptOldSyntax
      // 
      this.cvxTOptOldSyntax.AutoSize = true;
      this.cvxTOptOldSyntax.Location = new System.Drawing.Point(10, 152);
      this.cvxTOptOldSyntax.Name = "cvxTOptOldSyntax";
      this.cvxTOptOldSyntax.Size = new System.Drawing.Size(156, 17);
      this.cvxTOptOldSyntax.TabIndex = 14;
      this.cvxTOptOldSyntax.Text = "Use Clang 3.2 target syntax";
      this.cvxTOptOldSyntax.UseVisualStyleBackColor = true;
      // 
      // CVXSettings
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(798, 359);
      this.ControlBox = false;
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.groupBox2);
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
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
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
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button cvxCancel;
    private System.Windows.Forms.GroupBox bridgeOps;
    private System.Windows.Forms.CheckBox cvxShowCmds;
    private System.Windows.Forms.CheckBox cvxBatch;
    private System.Windows.Forms.PictureBox cvxPic;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox cvxCommonArgs;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.LinkLabel linkLabel3;
    private System.Windows.Forms.CheckBox cvxEcho;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox cvxTripleWin32;
    private System.Windows.Forms.CheckBox cvxPhases;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.ComboBox cvxTripleX64;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.ComboBox cvxTripleARM;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.CheckBox cvxCOptCPP11;
    private System.Windows.Forms.CheckBox cvxCOptMSABI;
    private System.Windows.Forms.CheckBox cvxTOptOldSyntax;


  }
}