namespace R_Factory_Tools
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            notifyIcon = new NotifyIcon(components);
            menuNotify = new ContextMenuStrip(components);
            thoátToolStripMenuItem = new ToolStripMenuItem();
            panel1 = new Panel();
            btnStop = new Button();
            btnStart = new Button();
            lblConnectionStatusValue = new Label();
            lblConnectionStatusText = new Label();
            panel2 = new Panel();
            grvData = new DataGridView();
            colDeviceParameterId = new DataGridViewTextBoxColumn();
            colConfigValue = new DataGridViewTextBoxColumn();
            menuNotify.SuspendLayout();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grvData).BeginInit();
            SuspendLayout();
            // 
            // notifyIcon
            // 
            notifyIcon.ContextMenuStrip = menuNotify;
            notifyIcon.Icon = (Icon)resources.GetObject("notifyIcon.Icon");
            notifyIcon.Text = "RTC";
            notifyIcon.Visible = true;
            // 
            // menuNotify
            // 
            menuNotify.Items.AddRange(new ToolStripItem[] { thoátToolStripMenuItem });
            menuNotify.Name = "menuNotify";
            menuNotify.Size = new Size(105, 26);
            // 
            // thoátToolStripMenuItem
            // 
            thoátToolStripMenuItem.Name = "thoátToolStripMenuItem";
            thoátToolStripMenuItem.Size = new Size(104, 22);
            thoátToolStripMenuItem.Text = "Thoát";
            // 
            // panel1
            // 
            panel1.Controls.Add(btnStop);
            panel1.Controls.Add(btnStart);
            panel1.Controls.Add(lblConnectionStatusValue);
            panel1.Controls.Add(lblConnectionStatusText);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1287, 132);
            panel1.TabIndex = 1;
            // 
            // btnStop
            // 
            btnStop.Location = new Point(401, 49);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(75, 23);
            btnStop.TabIndex = 4;
            btnStop.Text = "Stop";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // btnStart
            // 
            btnStart.Location = new Point(320, 49);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(75, 23);
            btnStart.TabIndex = 3;
            btnStart.Text = "Start";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // lblConnectionStatusValue
            // 
            lblConnectionStatusValue.AutoSize = true;
            lblConnectionStatusValue.BackColor = Color.OrangeRed;
            lblConnectionStatusValue.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblConnectionStatusValue.ForeColor = Color.White;
            lblConnectionStatusValue.Location = new Point(128, 44);
            lblConnectionStatusValue.Name = "lblConnectionStatusValue";
            lblConnectionStatusValue.Size = new Size(139, 30);
            lblConnectionStatusValue.TabIndex = 2;
            lblConnectionStatusValue.Text = "Disconnected";
            // 
            // lblConnectionStatusText
            // 
            lblConnectionStatusText.AutoSize = true;
            lblConnectionStatusText.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblConnectionStatusText.Location = new Point(12, 44);
            lblConnectionStatusText.Name = "lblConnectionStatusText";
            lblConnectionStatusText.Size = new Size(110, 30);
            lblConnectionStatusText.TabIndex = 0;
            lblConnectionStatusText.Text = "Trạng thái:";
            // 
            // panel2
            // 
            panel2.Controls.Add(grvData);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 132);
            panel2.Name = "panel2";
            panel2.Size = new Size(1287, 513);
            panel2.TabIndex = 2;
            // 
            // grvData
            // 
            grvData.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grvData.Columns.AddRange(new DataGridViewColumn[] { colDeviceParameterId, colConfigValue });
            grvData.Dock = DockStyle.Fill;
            grvData.Location = new Point(0, 0);
            grvData.Name = "grvData";
            grvData.Size = new Size(1287, 513);
            grvData.TabIndex = 0;
            // 
            // colDeviceParameterId
            // 
            colDeviceParameterId.DataPropertyName = "DeviceParameterId";
            colDeviceParameterId.HeaderText = "DeviceParameterId";
            colDeviceParameterId.Name = "colDeviceParameterId";
            colDeviceParameterId.Width = 500;
            // 
            // colConfigValue
            // 
            colConfigValue.DataPropertyName = "LogValue";
            colConfigValue.HeaderText = "LogValue";
            colConfigValue.Name = "colConfigValue";
            colConfigValue.Width = 500;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1287, 645);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FormMain";
            FormClosing += FormMain_FormClosing;
            Load += FormMain_Load;
            menuNotify.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)grvData).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private NotifyIcon notifyIcon;
        private ContextMenuStrip menuNotify;
        private ToolStripMenuItem thoátToolStripMenuItem;
        private Panel panel1;
        private Panel panel2;
        private DataGridView grvData;
        private Label lblConnectionStatusText;
        private Label lblConnectionStatusValue;
        private Button btnStart;
        private Button btnStop;
        private DataGridViewTextBoxColumn colDeviceParameterId;
        private DataGridViewTextBoxColumn colConfigValue;
    }
}
