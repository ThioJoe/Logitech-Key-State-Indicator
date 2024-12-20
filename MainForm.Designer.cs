﻿namespace G915X_KeyState_Indicator
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.labelNumLock = new System.Windows.Forms.Label();
            this.labelScrollLock = new System.Windows.Forms.Label();
            this.labelCapsLock = new System.Windows.Forms.Label();
            this.labelDebug = new System.Windows.Forms.Label();
            this.labelColorNumLock = new System.Windows.Forms.Label();
            this.labelColorScrollLock = new System.Windows.Forms.Label();
            this.labelColorCapsLock = new System.Windows.Forms.Label();
            this.labelColorDefault = new System.Windows.Forms.Label();
            this.labelDefault = new System.Windows.Forms.Label();
            this.buttonOpenConfigFile = new System.Windows.Forms.Button();
            this.buttonOpenDirectory = new System.Windows.Forms.Button();
            this.labelLogitechStatus = new System.Windows.Forms.Label();
            this.labelClosesToTray = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.buttonOpenLogitechSDKPage = new System.Windows.Forms.Button();
            this.buttonOpenAppGitHubPage = new System.Windows.Forms.Button();
            this.buttonDownloadDLL = new System.Windows.Forms.Button();
            this.buttonReloadConfig = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelNumLock
            // 
            this.labelNumLock.AutoSize = true;
            this.labelNumLock.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNumLock.Location = new System.Drawing.Point(85, 50);
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
            this.labelScrollLock.Location = new System.Drawing.Point(85, 125);
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
            this.labelCapsLock.Location = new System.Drawing.Point(85, 200);
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
            this.labelDebug.Location = new System.Drawing.Point(340, 200);
            this.labelDebug.Name = "labelDebug";
            this.labelDebug.Size = new System.Drawing.Size(151, 29);
            this.labelDebug.TabIndex = 3;
            this.labelDebug.Text = "Debug Label";
            this.labelDebug.Visible = false;
            // 
            // labelColorNumLock
            // 
            this.labelColorNumLock.AutoSize = true;
            this.labelColorNumLock.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelColorNumLock.Location = new System.Drawing.Point(35, 40);
            this.labelColorNumLock.Name = "labelColorNumLock";
            this.labelColorNumLock.Size = new System.Drawing.Size(48, 46);
            this.labelColorNumLock.TabIndex = 4;
            this.labelColorNumLock.Text = "◼";
            this.toolTip1.SetToolTip(this.labelColorNumLock, "Shows the current color set for the key");
            // 
            // labelColorScrollLock
            // 
            this.labelColorScrollLock.AutoSize = true;
            this.labelColorScrollLock.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelColorScrollLock.Location = new System.Drawing.Point(35, 115);
            this.labelColorScrollLock.Name = "labelColorScrollLock";
            this.labelColorScrollLock.Size = new System.Drawing.Size(48, 46);
            this.labelColorScrollLock.TabIndex = 5;
            this.labelColorScrollLock.Text = "◼";
            this.toolTip1.SetToolTip(this.labelColorScrollLock, "Shows the current color set for the key");
            // 
            // labelColorCapsLock
            // 
            this.labelColorCapsLock.AutoSize = true;
            this.labelColorCapsLock.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelColorCapsLock.Location = new System.Drawing.Point(35, 190);
            this.labelColorCapsLock.Name = "labelColorCapsLock";
            this.labelColorCapsLock.Size = new System.Drawing.Size(48, 46);
            this.labelColorCapsLock.TabIndex = 6;
            this.labelColorCapsLock.Text = "◼";
            this.toolTip1.SetToolTip(this.labelColorCapsLock, "Shows the current color set for the key");
            // 
            // labelColorDefault
            // 
            this.labelColorDefault.AutoSize = true;
            this.labelColorDefault.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelColorDefault.Location = new System.Drawing.Point(35, 275);
            this.labelColorDefault.Name = "labelColorDefault";
            this.labelColorDefault.Size = new System.Drawing.Size(48, 46);
            this.labelColorDefault.TabIndex = 7;
            this.labelColorDefault.Text = "◼";
            this.toolTip1.SetToolTip(this.labelColorDefault, "Shows the current default key color");
            // 
            // labelDefault
            // 
            this.labelDefault.AutoSize = true;
            this.labelDefault.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDefault.Location = new System.Drawing.Point(85, 285);
            this.labelDefault.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDefault.Name = "labelDefault";
            this.labelDefault.Size = new System.Drawing.Size(236, 32);
            this.labelDefault.TabIndex = 8;
            this.labelDefault.Text = "Default Key Color";
            // 
            // buttonOpenConfigFile
            // 
            this.buttonOpenConfigFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonOpenConfigFile.Location = new System.Drawing.Point(43, 349);
            this.buttonOpenConfigFile.Name = "buttonOpenConfigFile";
            this.buttonOpenConfigFile.Size = new System.Drawing.Size(182, 42);
            this.buttonOpenConfigFile.TabIndex = 9;
            this.buttonOpenConfigFile.Text = "Open Config File";
            this.buttonOpenConfigFile.UseVisualStyleBackColor = true;
            this.buttonOpenConfigFile.Click += new System.EventHandler(this.buttonOpenConfigFile_Click);
            // 
            // buttonOpenDirectory
            // 
            this.buttonOpenDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonOpenDirectory.Location = new System.Drawing.Point(231, 349);
            this.buttonOpenDirectory.Name = "buttonOpenDirectory";
            this.buttonOpenDirectory.Size = new System.Drawing.Size(190, 42);
            this.buttonOpenDirectory.TabIndex = 10;
            this.buttonOpenDirectory.Text = "Open Exe Folder";
            this.buttonOpenDirectory.UseVisualStyleBackColor = true;
            this.buttonOpenDirectory.Click += new System.EventHandler(this.buttonOpenDirectory_Click);
            // 
            // labelLogitechStatus
            // 
            this.labelLogitechStatus.AutoSize = true;
            this.labelLogitechStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLogitechStatus.Location = new System.Drawing.Point(38, 414);
            this.labelLogitechStatus.Name = "labelLogitechStatus";
            this.labelLogitechStatus.Size = new System.Drawing.Size(248, 26);
            this.labelLogitechStatus.TabIndex = 11;
            this.labelLogitechStatus.Text = "Logitech Engine Status: ";
            // 
            // labelClosesToTray
            // 
            this.labelClosesToTray.AutoSize = true;
            this.labelClosesToTray.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelClosesToTray.Location = new System.Drawing.Point(568, 15);
            this.labelClosesToTray.Name = "labelClosesToTray";
            this.labelClosesToTray.Size = new System.Drawing.Size(224, 25);
            this.labelClosesToTray.TabIndex = 12;
            this.labelClosesToTray.Text = "(Closes to System Tray)";
            // 
            // buttonOpenLogitechSDKPage
            // 
            this.buttonOpenLogitechSDKPage.Location = new System.Drawing.Point(553, 408);
            this.buttonOpenLogitechSDKPage.Name = "buttonOpenLogitechSDKPage";
            this.buttonOpenLogitechSDKPage.Size = new System.Drawing.Size(225, 42);
            this.buttonOpenLogitechSDKPage.TabIndex = 13;
            this.buttonOpenLogitechSDKPage.Text = "Visit Logitech SDK Website";
            this.buttonOpenLogitechSDKPage.UseVisualStyleBackColor = true;
            this.buttonOpenLogitechSDKPage.Click += new System.EventHandler(this.buttonOpenLogitechSDKPage_Click);
            // 
            // buttonOpenAppGitHubPage
            // 
            this.buttonOpenAppGitHubPage.Location = new System.Drawing.Point(553, 360);
            this.buttonOpenAppGitHubPage.Name = "buttonOpenAppGitHubPage";
            this.buttonOpenAppGitHubPage.Size = new System.Drawing.Size(225, 42);
            this.buttonOpenAppGitHubPage.TabIndex = 14;
            this.buttonOpenAppGitHubPage.Text = "Visit App GitHub Page";
            this.buttonOpenAppGitHubPage.UseVisualStyleBackColor = true;
            this.buttonOpenAppGitHubPage.Click += new System.EventHandler(this.buttonOpenAppGitHubPage_Click);
            // 
            // buttonDownloadDLL
            // 
            this.buttonDownloadDLL.Location = new System.Drawing.Point(553, 285);
            this.buttonDownloadDLL.Name = "buttonDownloadDLL";
            this.buttonDownloadDLL.Size = new System.Drawing.Size(225, 42);
            this.buttonDownloadDLL.TabIndex = 15;
            this.buttonDownloadDLL.Text = "Download Required DLL";
            this.buttonDownloadDLL.UseVisualStyleBackColor = true;
            this.buttonDownloadDLL.Click += new System.EventHandler(this.buttonDownloadDLL_Click);
            // 
            // buttonReloadConfig
            // 
            this.buttonReloadConfig.Location = new System.Drawing.Point(616, 115);
            this.buttonReloadConfig.Name = "buttonReloadConfig";
            this.buttonReloadConfig.Size = new System.Drawing.Size(101, 78);
            this.buttonReloadConfig.TabIndex = 16;
            this.buttonReloadConfig.Text = "Reload Config\r\n";
            this.buttonReloadConfig.UseVisualStyleBackColor = true;
            this.buttonReloadConfig.Click += new System.EventHandler(this.buttonReloadConfig_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(808, 471);
            this.Controls.Add(this.buttonReloadConfig);
            this.Controls.Add(this.buttonDownloadDLL);
            this.Controls.Add(this.buttonOpenAppGitHubPage);
            this.Controls.Add(this.buttonOpenLogitechSDKPage);
            this.Controls.Add(this.labelClosesToTray);
            this.Controls.Add(this.labelLogitechStatus);
            this.Controls.Add(this.buttonOpenDirectory);
            this.Controls.Add(this.buttonOpenConfigFile);
            this.Controls.Add(this.labelDefault);
            this.Controls.Add(this.labelColorDefault);
            this.Controls.Add(this.labelColorCapsLock);
            this.Controls.Add(this.labelColorScrollLock);
            this.Controls.Add(this.labelColorNumLock);
            this.Controls.Add(this.labelDebug);
            this.Controls.Add(this.labelCapsLock);
            this.Controls.Add(this.labelScrollLock);
            this.Controls.Add(this.labelNumLock);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MainForm";
            this.Text = "Logitech Key State Indicator - Statuses";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.SizeChanged += new System.EventHandler(this.MainForm_SizeChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelNumLock;
        private System.Windows.Forms.Label labelScrollLock;
        private System.Windows.Forms.Label labelCapsLock;
        private System.Windows.Forms.Label labelDebug;
        private System.Windows.Forms.Label labelColorNumLock;
        private System.Windows.Forms.Label labelColorScrollLock;
        private System.Windows.Forms.Label labelColorCapsLock;
        private System.Windows.Forms.Label labelColorDefault;
        private System.Windows.Forms.Label labelDefault;
        private System.Windows.Forms.Button buttonOpenConfigFile;
        private System.Windows.Forms.Button buttonOpenDirectory;
        private System.Windows.Forms.Label labelLogitechStatus;
        private System.Windows.Forms.Label labelClosesToTray;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button buttonOpenLogitechSDKPage;
        private System.Windows.Forms.Button buttonOpenAppGitHubPage;
        private System.Windows.Forms.Button buttonDownloadDLL;
        private System.Windows.Forms.Button buttonReloadConfig;
    }
}

