using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace locationserver
{
    public partial class ServerForm : Form
    {

        int ReadTOForm = 1000;
        int writeTOForm = 1000;
        public ServerForm()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            new Thread(Whois.runServer).Start();
            label5.Text = "ONLINE";
            label5.ForeColor = Color.Green;
            Whois.readTimeout = ReadTOForm;
            Whois.WriteTimeout = writeTOForm;
        }

        private void ReadTOBox_TextChanged(object sender, EventArgs e)
        {
            if (ReadTOBox.Text == "")
            {
                ReadTOForm = 1000;
            }
            else
            {
                ReadTOForm = int.Parse(ReadTOBox.Text);
            }
        }

        private void writeTOBox_TextChanged(object sender, EventArgs e)
        {
            writeTOForm = int.Parse(writeTOBox.Text);
        }
    }
}
