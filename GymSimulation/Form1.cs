using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GymSimulation
{
    public partial class Form1 : Form
    {
        private TcpIF tcp;

        public Form1()
        {
            InitializeComponent();
            this.button1.Click += (sender, e) => { open(); };
            this.button2.Click += (sender, e) => { close(); };
            this.button3.Click += (sender, e) => { send(); };
            this.tcp = new TcpIF();
        }

        private void open()
        {
            if (this.tcp.IsOpen) return;
            this.tcp.SendIpAddr = "192.168.254.31";
            this.tcp.RecvIpAddr = "192.168.254.249";
            this.tcp.SendPortNo = 8080;
            this.tcp.RecvPortNo = 8080;

            this.tcp.Open();
        }

        private void close()
        {
            if(!this.tcp.IsOpen) return;
            this.tcp.Close();
        }

        private void send()
        {
            //byte[] s = Util.ToByteArray("Try hard");
            byte[] s = new byte[] { 1, 2, 3, 4, 5, 6 };
            this.tcp.Send(s, s.Length);
        }
    }
}
