﻿namespace R_Factory_Tools
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
            btnExit = new ToolStripMenuItem();
            panel1 = new Panel();
            btnHide = new Button();
            btnStop = new Button();
            btnStart = new Button();
            lblConnectionStatusValue = new Label();
            lblConnectionStatusText = new Label();
            menuNotify.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // notifyIcon
            // 
            notifyIcon.ContextMenuStrip = menuNotify;
            notifyIcon.Icon = (Icon)resources.GetObject("notifyIcon.Icon");
            notifyIcon.Text = "RTC";
            notifyIcon.Click += notifyIcon_Click;
            // 
            // menuNotify
            // 
            menuNotify.Items.AddRange(new ToolStripItem[] { btnExit });
            menuNotify.Name = "menuNotify";
            menuNotify.Size = new Size(105, 26);
            // 
            // btnExit
            // 
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(104, 22);
            btnExit.Text = "Thoát";
            btnExit.Click += btnExit_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnHide);
            panel1.Controls.Add(btnStop);
            panel1.Controls.Add(btnStart);
            panel1.Controls.Add(lblConnectionStatusValue);
            panel1.Controls.Add(lblConnectionStatusText);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(661, 122);
            panel1.TabIndex = 1;
            // 
            // btnHide
            // 
            btnHide.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnHide.Location = new Point(496, 49);
            btnHide.Name = "btnHide";
            btnHide.Size = new Size(148, 23);
            btnHide.TabIndex = 5;
            btnHide.Text = "Ẩn xuống taskbar";
            btnHide.UseVisualStyleBackColor = true;
            btnHide.Click += btnHide_Click;
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
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(661, 122);
            Controls.Add(panel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FormMain";
            FormClosing += FormMain_FormClosing;
            Load += FormMain_Load;
            menuNotify.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private NotifyIcon notifyIcon;
        private ContextMenuStrip menuNotify;
        private ToolStripMenuItem btnExit;
        private Panel panel1;
        private Label lblConnectionStatusText;
        private Label lblConnectionStatusValue;
        private Button btnStart;
        private Button btnStop;
        private Button btnHide;
    }
}
