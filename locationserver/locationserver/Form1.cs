using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace locationserver
{
    public partial class Form1 : Form
    {
        int readTform = 1000;
        int writeTform = 1000;
        public Form1()
        {
            InitializeComponent();
            textBox1.Text = "1000";
            textBox2.Text = "1000";
            button1.FlatStyle = FlatStyle.Flat;
            button1.FlatAppearance.BorderSize = 0;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                readTform = 1000;
            }
            else
            {
                readTform = int.Parse(textBox1.Text);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            writeTform = int.Parse(textBox2.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new Thread(Whois.runServer).Start();
            label3.Text = "ONLINE";
            label3.ForeColor = Color.Lime;
            Whois.readTimeout = readTform;
            Whois.WriteTimeout = writeTform;
        }
    }
}
