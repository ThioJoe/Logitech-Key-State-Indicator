namespace G915X_KeyState_Indicator
{
    partial class MainForm
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
            this.labelNumLock = new System.Windows.Forms.Label();
            this.labelScrollLock = new System.Windows.Forms.Label();
            this.labelCapsLock = new System.Windows.Forms.Label();
            this.labelDebug = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelNumLock
            // 
            this.labelNumLock.AutoSize = true;
            this.labelNumLock.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNumLock.Location = new System.Drawing.Point(48, 50);
            this.labelNumLock.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelNumLock.Name = "labelNumLock";
            this.labelNumLock.Size = new System.Drawing.Size(228, 32);
            this.labelNumLock.TabIndex = 0;
            this.labelNumLock.Text = "Num Lock Status";
            // 
            // labelScrollLock
            // 
            this.labelScrollLock.AutoSize = true;
            this.labelScrollLock.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelScrollLock.Location = new System.Drawing.Point(48, 122);
            this.labelScrollLock.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelScrollLock.Name = "labelScrollLock";
            this.labelScrollLock.Size = new System.Drawing.Size(241, 32);
            this.labelScrollLock.TabIndex = 1;
            this.labelScrollLock.Text = "Scroll Lock Status";
            // 
            // labelCapsLock
            // 
            this.labelCapsLock.AutoSize = true;
            this.labelCapsLock.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCapsLock.Location = new System.Drawing.Point(48, 192);
            this.labelCapsLock.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelCapsLock.Name = "labelCapsLock";
            this.labelCapsLock.Size = new System.Drawing.Size(235, 32);
            this.labelCapsLock.TabIndex = 2;
            this.labelCapsLock.Text = "Caps Lock Status";
            // 
            // labelDebug
            // 
            this.labelDebug.AutoSize = true;
            this.labelDebug.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDebug.Location = new System.Drawing.Point(49, 310);
            this.labelDebug.Name = "labelDebug";
            this.labelDebug.Size = new System.Drawing.Size(151, 29);
            this.labelDebug.TabIndex = 3;
            this.labelDebug.Text = "Debug Label";
            this.labelDebug.Visible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(835, 454);
            this.Controls.Add(this.labelDebug);
            this.Controls.Add(this.labelCapsLock);
            this.Controls.Add(this.labelScrollLock);
            this.Controls.Add(this.labelNumLock);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MainForm";
            this.Text = "Logitech Key Status Monitor - Statuses";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelNumLock;
        private System.Windows.Forms.Label labelScrollLock;
        private System.Windows.Forms.Label labelCapsLock;
        private System.Windows.Forms.Label labelDebug;
    }
}

