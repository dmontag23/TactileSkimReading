namespace SkimReadingStudy
{
    partial class Form1
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
            this.paper2 = new System.Windows.Forms.Button();
            this.paper3 = new System.Windows.Forms.Button();
            this.clear = new System.Windows.Forms.Button();
            this.exit = new System.Windows.Forms.Button();
            this.paper1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // paper2
            // 
            this.paper2.Location = new System.Drawing.Point(105, 45);
            this.paper2.Name = "paper2";
            this.paper2.Size = new System.Drawing.Size(84, 23);
            this.paper2.TabIndex = 0;
            this.paper2.Text = "Study Paper 1";
            this.paper2.UseVisualStyleBackColor = true;
            this.paper2.Click += new System.EventHandler(this.paper2_Click);
            // 
            // paper3
            // 
            this.paper3.Location = new System.Drawing.Point(195, 45);
            this.paper3.Name = "paper3";
            this.paper3.Size = new System.Drawing.Size(85, 23);
            this.paper3.TabIndex = 1;
            this.paper3.Text = "Study Paper 2";
            this.paper3.UseVisualStyleBackColor = true;
            this.paper3.Click += new System.EventHandler(this.paper3_Click);
            // 
            // clear
            // 
            this.clear.Location = new System.Drawing.Point(286, 45);
            this.clear.Name = "clear";
            this.clear.Size = new System.Drawing.Size(75, 23);
            this.clear.TabIndex = 2;
            this.clear.Text = "Clear Device";
            this.clear.UseVisualStyleBackColor = true;
            this.clear.Click += new System.EventHandler(this.clear_Click);
            // 
            // exit
            // 
            this.exit.Location = new System.Drawing.Point(367, 45);
            this.exit.Name = "exit";
            this.exit.Size = new System.Drawing.Size(75, 23);
            this.exit.TabIndex = 3;
            this.exit.Text = "Exit";
            this.exit.UseVisualStyleBackColor = true;
            this.exit.Click += new System.EventHandler(this.exit_Click);
            // 
            // paper1
            // 
            this.paper1.Location = new System.Drawing.Point(15, 45);
            this.paper1.Name = "paper1";
            this.paper1.Size = new System.Drawing.Size(84, 23);
            this.paper1.TabIndex = 4;
            this.paper1.Text = "Learning Paper 1";
            this.paper1.UseVisualStyleBackColor = true;
            this.paper1.Click += new System.EventHandler(this.paper1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(454, 114);
            this.Controls.Add(this.paper1);
            this.Controls.Add(this.exit);
            this.Controls.Add(this.clear);
            this.Controls.Add(this.paper3);
            this.Controls.Add(this.paper2);
            this.KeyPreview = true;
            this.Name = "Form1";
            this.Text = "Form1";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button paper2;
        private System.Windows.Forms.Button paper3;
        private System.Windows.Forms.Button clear;
        private System.Windows.Forms.Button exit;
        private System.Windows.Forms.Button paper1;
    }
}

