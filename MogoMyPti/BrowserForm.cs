using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MogoMyPti
{
    public partial class Login : Form
    {
        public delegate void LoggedInEventhandler();
        public event LoggedInEventhandler OnLoggedIn;

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public Login()
        {
            InitializeComponent();

#if DEBUG
            AllocConsole();
            Console.WriteLine("hi debugging");
#endif
        }

        public string Username { get { return textBox1.Text; } }
        public string Password { get { return textBox2.Text; } }

        private void button1_Click(object sender, EventArgs e)
        {
            OnLoggedIn();
        }
    }
}
