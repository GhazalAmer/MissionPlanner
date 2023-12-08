using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Org.BouncyCastle.Asn1.Ocsp;
using static SharpKml.Dom.IconStyle;

namespace MissionPlanner
{
    public partial class OBS : Form
    {

        Thread udpLoop;
        static byte[] data = new byte[1024];
        static IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9121);
        static IPEndPoint Sender = new IPEndPoint(IPAddress.Any, 0);
        static UdpClient newsock = new UdpClient(ipep);
        private static bool isOnline = false;
        private static readonly object lockObject = new object();

        public OBS()
        {
            InitializeComponent();
            setup();
        }
        void setup()
        {
            //// Set the port number and create an IPEndPoint for the server
            //int port = 12345;
            //IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);

            //// Create a UDP socket
            //using (UdpClient udpClient = new UdpClient(endPoint))
            //{
            //    Console.WriteLine($"UDP server is listening on port {port}");
            { 
                udpLoop = new Thread(new ThreadStart(udp_connect))
                {
                    IsBackground = true,
                    Name = "OBSx"
                };

            }

            udpLoop.Start();


        }



        void udp_connect()
        {
            while (true)
            {
                Console.WriteLine("Waiting for a client...");
                // recive the client id and port
                data = newsock.Receive(ref Sender);
                //print the massage of the sender and ip , port 
                Console.WriteLine("sended From:" + Sender.ToString() + "//" + Encoding.ASCII.GetString(data, 0, data.Length));
                // send a massege back to the clients 
                string welcome = "Welcome to my test server";
                byte[] welcomeBytes = Encoding.ASCII.GetBytes(welcome);
                newsock.Send(welcomeBytes, welcomeBytes.Length, Sender);
                lock (lockObject)
                {
                    isOnline = true;
                    label1.Text = "ONLINE";
                    label1.ForeColor = Color.Green;
                }

            }
        }




        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void OBS_Load(object sender, EventArgs e)
        {
           
        }

        private void button3_Click(object sender, EventArgs e)
        {

            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void udp_check_Tick(object sender, EventArgs e)
        {
            label1.Text = "OFFLINE";
            label1.ForeColor = Color.Red;
        }

        private void myButton1_Click(object sender, EventArgs e)
        {
            try
            {
                Byte[] msg;
                msg = Encoding.ASCII.GetBytes("$OBA;ACTIVE=TRUE");
                Console.WriteLine("Starting OBS ");
                newsock.Send(msg, msg.Length, Sender);
            }

            catch
            {
                Console.WriteLine("No one to send to");
            }
        }
    

        private void myButton2_Click(object sender, EventArgs e)
        {
            try
            {
                Byte[] msg;
                msg = Encoding.ASCII.GetBytes("$OBA;ACTIVE=FALSE");
                Console.WriteLine("Stopping OBS ");
                newsock.Send(msg, msg.Length, Sender);
            }

            catch
            {
                Console.WriteLine("No one to send to");
            }
        }
    }
}
    
  
