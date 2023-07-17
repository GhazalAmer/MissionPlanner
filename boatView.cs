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

            try
            {
                n2k_client.Connect(N2K_IP, N2K_PORT);
                n2k_client.Send(connect_msg, connect_msg.Length);
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

        int N2K_PORT = 8080;                     // Will be Overridden by Text File
        string N2K_IP = "192.168.1.10";     // Will be Overridden by Text File
        UdpClient n2k_client;
        System.Net.IPEndPoint RemoteIpEndPoint;
        Byte[] connect_msg;
        


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
    }
}
