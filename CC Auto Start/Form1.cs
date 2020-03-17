using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace CC_Auto_Start
{
    public partial class Form1 : Form
    {
        public string ccLocation;
        XmlDocument xmlDoc = new XmlDocument();
        List<Game> games = new List<Game>();

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);


        public class Game
        {
            public string processName { get; private set; }
            public string parameter { get; private set; }

            public Game(string pName, string param)
            {
                processName = pName;
                parameter = param;
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Hide();
            LoadConfig();
        }

        private void LoadConfig()
        {
            try
            {
                xmlDoc.Load("CC Auto Start.xml");
                XmlNode node = xmlDoc.DocumentElement.SelectSingleNode("/Config/CCLocation");
                ccLocation = node.InnerText;
                if (!File.Exists(ccLocation))
                {
                    MessageBox.Show("'" + ccLocation + "' could not be found." + "\n\nEdit the config file and restart the application." + "\nClosing...", "Whoopsie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Application.Exit();
                }
                node = xmlDoc.DocumentElement.SelectSingleNode("/Config/Games");

                foreach (XmlNode childNode in node.ChildNodes)
                {
                    Game game = new Game(childNode.Attributes["processName"].Value, childNode.Attributes["parameter"].Value);
                    games.Add(game);
                }
            }
            catch (Exception e)
            { 
                MessageBox.Show(e.Message + "\n\nExiting the application.", "Whoopsie", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }
        }

        private void Work()
        {
            if (GameIsRunning() && !CCIsRunning())
            {
                foreach (Game game in games)
                {
                    if (Process.GetProcessesByName(game.processName).Length != 0)
                    {
                        Process.Start(ccLocation, game.parameter);
                        break;
                    }
                }
            }

            if (!GameIsRunning() && CCIsRunning())
                Process.Start(ccLocation, "C_EXIT");
        }

        private bool GameIsRunning()
        {
            foreach (Game game in games)
            {
                if (Process.GetProcessesByName(game.processName).Length != 0)
                    return true;
            }

            return false;
        }

        private bool CCIsRunning()
        {
            return (Process.GetProcessesByName("CrewChiefV4").Length != 0);
        }

        private void NotifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    this.Show();
                    WindowState = FormWindowState.Normal;
                }

                else if (WindowState == FormWindowState.Normal)
                {
                    this.Hide();
                    WindowState = FormWindowState.Minimized;
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Work();
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                pauseButton.Text = "Enable";
                timer1.Stop();
            }
            else
            {
                pauseButton.Text = "Disable";
                timer1.Start();
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
                this.Hide();
        }

        private void ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
