using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace TrayRadio
{
    public partial class Form1 : Form
    {

        Uri defaultUri = new Uri("http://www.bbc.co.uk/iplayer/console/bbc_6music");

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            if (this.Enabled)
            {
                timer1.Enabled = true;
            }
            updateIcon();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            TopMost = false;
            ShowInTaskbar = false;
            this.Enabled = false;
            timer1.Enabled = false;
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            webBrowser1.Navigate(defaultUri);
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void notifyIcon1_MouseDown(object sender, MouseEventArgs e)
        {            
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                this.Enabled = true;
                timer1.Enabled = false;
                ShowInTaskbar = true;
                this.WindowState = FormWindowState.Normal;
                TopMost = false;
                BringToFront();
                TopMost = true;
            }
        }

        private void muteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clickButton("volume-mute");
            updateIcon();
        }

        private void updateIcon()
        {
            notifyIcon1.Icon = new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(isMuted() ? "TrayRadio.Dark.ico" : "TrayRadio.Light.ico")); 
        }

        private void playStopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isPlaying())
            {
                clickButton("stop");
            }
            else
            {
                clickButton("play");
            }
        } 
        /*
            play
            pause
            stop
            volume-1
            volume-2
            volume-3
            volume-mute
            search-button
         * */

        private bool isLoading()
        {
            HtmlElement element = findElement("duration", "div");

            return (element!=null && element.Style != null && !element.Style.Contains("none"));
        }

        private bool isPlaying()
        {
            HtmlElement element = findElement("play", "button");

            return (element != null && element.Style != null && element.Style.Contains("none"));
        }

        private bool isMuted()
        {
            HtmlElement element = findElement("volume-mute", "button");

            return (element!=null &&  element.Style != null && !element.Style.Contains("none"));
        }

        public string getTooltip()
        {
            if (isLoading())
            {
                return "Loading...";
            }

            String showName = getShowName();
            if (showName == null)
            {
                return "trayradio";
            }
            else
            {
                String artist = null;
                String track = null;

                HtmlElement element = findElement("realtime", "div");

                if (element != null && element.Style != null && !element.Style.Contains("none"))
                {
                    artist = getElementText("artists", "p");
                    track = getElementText("track", "p");
                }

                if (artist != null && track != null)
                {
                    return showName + "\r\n\r\n" + artist + " - " + track;
                }
            }
            return showName;
        }

        public string getShowName()
        {
            return getElementText("parent-title", "h2");
        }

        public string getElementText(String name, String type)
        {
            HtmlElement element = findElement(name, type);
            if (element != null)
            {
                return StripHTML(element.InnerHtml);
            }

            return null;
        }

        public static string StripHTML(string htmlString)
        {
            string pattern = @"<(.|\n)*?>";

            return Regex.Replace(htmlString, pattern, string.Empty).Trim();
        }

        private void clickButton(String name)
        {
            HtmlElement element = findElement(name, "button");
            if (element!=null)
            {
                element.InvokeMember("click");
            }
        }

        private HtmlElement findElement(String name, String type)
        {
            var elements = webBrowser1.Document.GetElementsByTagName(type);
            foreach (HtmlElement element in elements)
            {
                if (element.Id == name)
                {
                    return element;
                }
            }

            return null;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (timer1.Enabled)
            {
                timer1.Enabled = false;
            }

            playStopToolStripMenuItem.Text = isPlaying() ? "Stop" : "Play";
            muteToolStripMenuItem.Text = isMuted() ? "Unmute" : "Mute";
        }

        private void contextMenuStrip1_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            if (this.Enabled)
            {
                timer1.Enabled = true;
            }
        }

        private void notifyIcon1_MouseMove(object sender, MouseEventArgs e)
        {
            string tip = getTooltip();
            if (tip.Length >= 64)
                tip = tip.Substring(0, 60) + "...";
            notifyIcon1.Text = tip;
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            clickButton("volume-mute");
            updateIcon();
        }

    }
}
