using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace CC_Auto_Start
{
    public partial class Form1 : Form
    {
        ProcessStartInfo processStartInfo = new ProcessStartInfo();
        XmlDocument xmlDoc = new XmlDocument();
        List<Game> games = new List<Game>();
        string exitArgument = "-c_exit";
        bool iStartedIt = false;

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
                processStartInfo.FileName = node.InnerText;

                if (!File.Exists(processStartInfo.FileName))
                {
                    MessageBox.Show("'" + processStartInfo.FileName + "' could not be found." + "\n\nEdit the config file and restart the application." + "\nClosing...", "Whoopsie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        processStartInfo.Arguments = game.parameter;
                        Process.Start(processStartInfo);
                        iStartedIt = true;
                        break;
                    }
                }
            }

            if (!GameIsRunning() && CCIsRunning() && iStartedIt)
            {
                processStartInfo.Arguments = exitArgument;
                Process.Start(processStartInfo);
                iStartedIt = false;
            }
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            Work();
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void notifyIcon1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (timer1.Enabled)
                {
                    notifyIcon1.Icon = Properties.Resources.icon_off;
                    timer1.Stop();
                }
                else
                {
                    notifyIcon1.Icon = Properties.Resources.icon_on;
                    timer1.Start();
                }
            }
        }
    }
}
