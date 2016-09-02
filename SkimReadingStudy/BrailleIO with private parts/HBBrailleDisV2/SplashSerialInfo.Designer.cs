namespace HBBrailleDisV2
{
    partial class SplashSerialInfo
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
            this.Seriennummer = new System.Windows.Forms.Label();
            this.timerToClose = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // Seriennummer
            // 
            this.Seriennummer.AutoSize = true;
            this.Seriennummer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Seriennummer.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Seriennummer.Location = new System.Drawing.Point(0, 0);
            this.Seriennummer.Name = "Seriennummer";
            this.Seriennummer.Size = new System.Drawing.Size(261, 108);
            this.Seriennummer.TabIndex = 0;
            this.Seriennummer.Text = "????";
            this.Seriennummer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // timerToClose
            // 
            this.timerToClose.Interval = 10000;
            // 
            // SplashSerialInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(125, 86);
            this.ControlBox = false;
            this.Controls.Add(this.Seriennummer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SplashSerialInfo";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Ihr Hyperbraillegerät hat die Seriennummer:";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public System.Windows.Forms.Label Seriennummer;
        private System.Windows.Forms.Timer timerToClose;
    }
}