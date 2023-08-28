using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;

namespace MissionPlanner
{
    public partial class boatView : Form
    {

        System.Threading.Thread tBoatView;


        public boatView()
        {
            InitializeComponent();
            setup();
        }

        void setup()
        {

            n2k_client = new UdpClient(N2K_PORT);
            n2k_client.Client.ReceiveTimeout = 2000;
            RemoteIpEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0);


            connect_msg = Encoding.ASCII.GetBytes("Can i get a cake");
            //connect_msg1 = Encoding.ASCII.GetBytes("Engine1 OK");
            //connect_msg2 = Encoding.ASCII.GetBytes("Engine2 OK");

            try
            {
                n2k_client.Connect(N2K_IP, N2K_PORT);
                n2k_client.Send(connect_msg, connect_msg.Length);
            //    n2k_client1.Connect(N2K_IP1, N2K_PORT1);
            //    n2k_client1.Send(connect_msg1, connect_msg1.Length);
            //    n2k_client2.Connect(N2K_IP2, N2K_PORT2);
            //    n2k_client2.Send(connect_msg2, connect_msg2.Length);
            //
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

            }

            tBoatView = new System.Threading.Thread(new System.Threading.ThreadStart(mainLoop))
            {
                IsBackground = true,
                Name = "Boat View"
            };
            tBoatView.Start();

        }
        // engine 1 
        int N2K_PORT = 49414;                     // Will be Overridden by Text File
        string N2K_IP = "192.168.1.88";     // Will be Overridden by Text File
        UdpClient n2k_client;
        System.Net.IPEndPoint RemoteIpEndPoint;
        Byte[] connect_msg;

        //// engine 1 
        //int N2K_PORT1 = 8085;                     // Will be Overridden by Text File
        //string N2K_IP1 = "192.168.10.8";     // Will be Overridden by Text File
        //UdpClient n2k_client1;
        //System.Net.IPEndPoint RemoteIpEndPoint1;
        //Byte[] connect_msg1;

        ////engine 2
        //int N2K_PORT2 = 8085;                     // Will be Overridden by Text File
        //string N2K_IP2 = "192.168.10.9";     // Will be Overridden by Text File
        //UdpClient n2k_client2;
        //System.Net.IPEndPoint RemoteIpEndPoint2;
        //Byte[] connect_msg2;


        void mainLoop()
        {
            //char[] delimiterChars = { '=', '{', '}', ',', ':' };

            //StreamReader reader = File.OpenText("SETTINGS.txt");
            //string line;
            //while ((line = reader.ReadLine()) != null)
            //{
            //    string[] items = line.Split(delimiterChars);
            //    // Now let's find the path.

            //    if (items[0] == "N2K_ADDRESS")
            //    {
            //        N2K_IP = items[1];
            //        N2K_PORT = Convert.ToInt32(items[2]);
            //        Console.WriteLine("IP/PORT DONE");
            //    }
            //}

            bool oklol = false;
            while (true)
            {

                try
                {
                    //Parse Data shouldn't be causing Errors, get it out of here lol
                    Byte[] rxBytes = n2k_client.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.ASCII.GetString(rxBytes);
                    Console.WriteLine(returnData);
                    oklol = true;
                    status_text.Text = "N2K Link ACTIVE";
                    status_text.ForeColor = Color.LimeGreen;
                    parseData(returnData);

                }

                catch (Exception e)
                {
                    oklol = false;
                    status_text.Text = "N2K Link INACTIVE";
                    status_text.ForeColor = Color.Red;
                }

                if (oklol == false)
                {
                    try
                    {
                        Console.WriteLine("Reconnecting");
                        n2k_client.Close();
                        n2k_client = new UdpClient(N2K_PORT);
                        n2k_client.Client.SendTimeout = 2000;
                        n2k_client.Client.ReceiveTimeout = 2000;
                        n2k_client.Connect(N2K_IP, N2K_PORT);
                        n2k_client.Send(connect_msg, connect_msg.Length);
                        // success_etk = true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }

                }
                System.Threading.Thread.Sleep(10);
            }

        }

        void parseData(string data)
        {

            // This is where Rx'd data is parsed
            // Maybe make a seperate function for this
            string[] words = data.Split(',');

            for (int i = 0; i < words.Length; i++)
            {
                Console.WriteLine(words[i].ToString());
                string [] items = words[i].Split(':');
                

                if (items[0] == "A_VOLTAGE")
                {
                    bat1_label.Text = Convert.ToDouble(items[1]).ToString("0.0") + "V";
                }

                if (items[0] == "A_VOLTAGE2")
                {
                    bat2_label.Text = Convert.ToDouble(items[1]).ToString("0.0") + "V";
                }

                if (items[0] == "BAT")
                {
                    bat_label.Text = Convert.ToDouble(items[1]).ToString("0.0") + "V";
                }


                if (items[0] == "FUEL")
                {
                    fuel_label.Text = Convert.ToDouble(items[1]).ToString("0") + "%";
                }

                if (items[0] == "RPM")
                {
                    rpm1_label.Text = Convert.ToDouble(items[1]).ToString();
                }

                if (items[0] == "RPM2")
                {
                    rpm2_label.Text = Convert.ToDouble(items[1]).ToString();
                }

                if (items[0] == "GEAR")
                {
                    if (items[1] == "0")
                    {
                        gear_label.Text = "F";
                    }

                    if (items[1] == "1")
                    {
                        gear_label.Text = "N";
                    }

                    if (items[1] == "2")
                    {
                        gear_label.Text = "R";
                    }

                }


                if (items[0] == "GEAR2")
                {
                    if (items[1] == "0")
                    {
                        gear2_label.Text = "F";
                    }

                    if (items[1] == "1")
                    {
                        gear2_label.Text = "N";
                    }

                    if (items[1] == "2")
                    {
                        gear2_label.Text = "R";
                    }

                }

            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label7.Text = MainV2.comPort.MAV.cs.yaw.ToString() + "°";
            label8.Text = MainV2.comPort.MAV.cs.target_bearing.ToString("0") + "°";
            label9.Text = MainV2.comPort.MAV.cs.groundspeed.ToString("0.0");
            //Application.DoEvents();

        }

        private void myButton4_Click(object sender, EventArgs e)
        {
            //connect_msg2 = Encoding.ASCII.GetBytes("ENGINE2=STOP");
            //Console.WriteLine("Stoping PORT Engine");
            //n2k_client2.Send(connect_msg2, connect_msg2.Length);
        }

        private void myButton1_Click(object sender, EventArgs e)
        {
        //    connect_msg1 = Encoding.ASCII.GetBytes("ENGINE1=START");
        //    Console.WriteLine("Starting STBD Engine");
        //    n2k_client1.Send(connect_msg1, connect_msg1.Length);
        }

        private void myLabel1_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {
           //yahString a = Console.ReadLine();
           //String b = "$FRFIR,E,132225,FS,FA,003,001,A,V,Fire PreAl: 004 STEERING ROOM - STEERING RO*71\r\n$FREVE,132225,AJ004,DZ PreAlarm   : STEERING ROOM*6E\r\n$FRFIR,E,132231,FS,FA,003,001,A,V,Fire Alarm: 004 STEERING ROOM - STEERING RO*4D\r\n";
           // if (a == b)
           // {
           //     label14.ForeColor = Color.Red;
           // }
        }

        private void label18_Click(object sender, EventArgs e)
        {

        }

        private void myButton2_Click(object sender, EventArgs e)
        {
            //connect_msg1 = Encoding.ASCII.GetBytes("ENGINE1=STOP");
            //Console.WriteLine("Stop STBD Engine");
            //n2k_client1.Send(connect_msg1, connect_msg1.Length);
        }

        private void myButton3_Click(object sender, EventArgs e)
        {
            //connect_msg2 = Encoding.ASCII.GetBytes("ENGINE2=START");
            //Console.WriteLine("Starting PORT Engine");
            //n2k_client2.Send(connect_msg2, connect_msg2.Length);
        }
    }
}
