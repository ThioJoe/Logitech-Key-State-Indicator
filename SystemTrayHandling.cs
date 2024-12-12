using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace G915X_KeyState_Indicator
{
    public partial class MainForm : Form
    {
        private NotifyIcon trayIcon;

        private void CreateTrayIcon()
        {
            trayIcon = new NotifyIcon
            {
                Text = ProgramName,
                Icon = SystemIcons.Application,
                Visible = true
            };

            trayIcon.MouseClick += trayIcon_MouseClick;
            trayIcon.MouseDoubleClick += trayIcon_MouseDoubleClick;

            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem menuItem = new ToolStripMenuItem();

            
            contextMenu.Items.Add(AddMenuItem("Show", RestoreFromTray));
            contextMenu.Items.Add(AddMenuItem("About", ShowAbout));
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(AddMenuItem("Exit", ExitApplication));

            trayIcon.ContextMenuStrip = contextMenu;
        }

        // -------------------- MENU ITEM FUNCTIONS ------------------------------

        private void ExitApplication(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            ShutDown_HooksAndLogi();
            Application.Exit();
        }

        private void RestoreFromTray(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.Show();
        }

        private void ShowAbout(object sender, EventArgs e)
        {
            MessageBox.Show($"{ProgramName}\n\n" +
                "This application shows the current state of the a Logitech keyboard's NumLock, CapsLock, and ScrollLock keys.\n\n" +
                "Author: ThioJoe",
                "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //--------------------------- GENERAL TRAY FUNCTIONS --------------------------------------

        private ToolStripMenuItem AddMenuItem(string text, EventHandler onClick)
        {
            ToolStripMenuItem menuItem = new ToolStripMenuItem();
            menuItem.Text = text;
            menuItem.Click += onClick;

            return menuItem;
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            RestoreFromTray(sender, e);
        }

        // Left mouse click
        private void trayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            // If it's a left click, restore, otherwise nothing. Right click is automatically handled by the context menu
            if (e.Button == MouseButtons.Left)
            {
                RestoreFromTray(sender, e);
            }
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            // Nothing here yet
        }

        // Intercept the form closing event to minimize to tray instead of closing
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                this.Hide();
            }
        }

    } // ---------------------------------------------------------------
}
