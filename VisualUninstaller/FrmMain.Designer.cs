namespace VisualUninstaller
{
    partial class FrmMain
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.findTextBox = new System.Windows.Forms.TextBox();
            this.programsContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removefromthelistToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeSelectedBtn = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.versionLabel = new System.Windows.Forms.Label();
            this.placeholderPanel = new System.Windows.Forms.Panel();
            this.programsContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // findTextBox
            // 
            this.findTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.findTextBox.ForeColor = System.Drawing.SystemColors.GrayText;
            this.findTextBox.Location = new System.Drawing.Point(35, 6);
            this.findTextBox.Name = "findTextBox";
            this.findTextBox.Size = new System.Drawing.Size(361, 20);
            this.findTextBox.TabIndex = 0;
            this.findTextBox.Text = "Search by name";
            this.findTextBox.Enter += new System.EventHandler(this.findTextBox_Enter);
            this.findTextBox.Leave += new System.EventHandler(this.findTextBox_Leave);
            this.findTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.findTextBox_KeyPress);
            this.findTextBox.TextChanged += new System.EventHandler(this.findTextBox_TextChanged);
            this.findTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.findTextBox_KeyDown);
            // 
            // programsContextMenu
            // 
            this.programsContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeToolStripMenuItem,
            this.removefromthelistToolStripMenuItem});
            this.programsContextMenu.Name = "programsContextMenu";
            this.programsContextMenu.Size = new System.Drawing.Size(185, 48);
            this.programsContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.removeToolStripMenuItem.Text = "Remove application";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // removefromthelistToolStripMenuItem
            // 
            this.removefromthelistToolStripMenuItem.Name = "removefromthelistToolStripMenuItem";
            this.removefromthelistToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.removefromthelistToolStripMenuItem.Text = "Remove from the list";
            this.removefromthelistToolStripMenuItem.Click += new System.EventHandler(this.removefromthelistToolStripMenuItem_Click);
            // 
            // removeSelectedBtn
            // 
            this.removeSelectedBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.removeSelectedBtn.Enabled = false;
            this.removeSelectedBtn.Image = ((System.Drawing.Image)(resources.GetObject("removeSelectedBtn.Image")));
            this.removeSelectedBtn.Location = new System.Drawing.Point(276, 327);
            this.removeSelectedBtn.Name = "removeSelectedBtn";
            this.removeSelectedBtn.Size = new System.Drawing.Size(120, 24);
            this.removeSelectedBtn.TabIndex = 2;
            this.removeSelectedBtn.Text = "Remove selected";
            this.removeSelectedBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.removeSelectedBtn.Click += new System.EventHandler(this.button1_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(13, 6);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(16, 16);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // versionLabel
            // 
            this.versionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.versionLabel.AutoSize = true;
            this.versionLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.versionLabel.Location = new System.Drawing.Point(12, 333);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(41, 13);
            this.versionLabel.TabIndex = 8;
            this.versionLabel.Text = "version";
            // 
            // placeholderPanel
            // 
            this.placeholderPanel.Location = new System.Drawing.Point(12, 32);
            this.placeholderPanel.Name = "placeholderPanel";
            this.placeholderPanel.Size = new System.Drawing.Size(384, 289);
            this.placeholderPanel.TabIndex = 9;
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(408, 363);
            this.Controls.Add(this.placeholderPanel);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.removeSelectedBtn);
            this.Controls.Add(this.findTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmMain";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Visual Uninstaller";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.programsContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox findTextBox;
        private System.Windows.Forms.Button removeSelectedBtn;
        private System.Windows.Forms.ContextMenuStrip programsContextMenu;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removefromthelistToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.Panel placeholderPanel;
    }
}

