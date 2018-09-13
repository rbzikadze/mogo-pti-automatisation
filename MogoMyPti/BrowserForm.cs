using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MogoMyPti
{
    public partial class Login : Form
    {
        public delegate void LoggedInEventhandler();
        public event LoggedInEventhandler OnLoggedIn;

        public Login()
        {
            InitializeComponent();
        }

        public string Username { get { return textBox1.Text; } }
        public string Password { get { return textBox2.Text; } }

        private void button1_Click(object sender, EventArgs e)
        {
            OnLoggedIn();
        }
    }
}
