using MissionPlanner.Controls;
using MissionPlanner.Utilities;
using SharpDX.DirectInput;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;


namespace MissionPlanner.Joystick
{
    public partial class JoystickSetup : MyUserControl, IDeactivate
    {
        bool startup = true;

        int noButtons = 0;
        private int maxaxis = 16;

        public JoystickSetup()
        {
            InitializeComponent();

            MissionPlanner.Utilities.Tracking.AddPage(this.GetType().ToString(), this.Text);
        }

        private void Joystick_Load(object sender, EventArgs e)
        {
            try
            {
                var joystick = JoystickBase.getDevices();

                foreach (var device in joystick)
                {
                    CMB_joysticks.Items.Add(device);
                }
            }
            catch
            {
                CustomMessageBox.Show("Error geting joystick list: do you have the directx redist installed?");
                this.Close();
                return;
            }

            if (CMB_joysticks.Items.Count > 0 && CMB_joysticks.SelectedIndex == -1)
                CMB_joysticks.SelectedIndex = 0;

            try
            {
                if (Settings.Instance.ContainsKey("joystick_name") && Settings.Instance["joystick_name"].ToString() != "")
                    CMB_joysticks.Text = Settings.Instance["joystick_name"].ToString();
            }
            catch
            {
            }

            try
            {
                if (Settings.Instance.ContainsKey("joy_elevons"))
                    CHK_elevons.Checked = bool.Parse(Settings.Instance["joy_elevons"].ToString());
            }
            catch
            {
            } // IF 1 DOESNT EXIST NONE WILL

            var tempjoystick = JoystickBase.Create(() => MainV2.comPort);

            label14.Text += " " + MainV2.comPort.MAV.cs.firmware.ToString();

            var y = label8.Bottom;

            this.SuspendLayout();

            for (int a = 1; a <= maxaxis; a++)
            {
                var config = tempjoystick.getChannel(a);

                var ax = new JoystickAxis()
                {
                    ChannelNo = a,
                    Label = "RC " + a,
                    AxisArray = (Enum.GetValues(typeof(joystickaxis))),
                    ChannelValue = config.axis.ToString(),
                    ExpoValue = config.expo.ToString(),
                    ReverseValue = config.reverse,
                    Location = new Point(0, y),
                    Name = "axis" + a
                };

                ax.Detect = () => JoystickBase.getMovingAxis(CMB_joysticks.Text, 16000).ToString();
                ax.Reverse = () => MainV2.joystick?.setReverse(ax.ChannelNo, ax.ReverseValue);
                ax.SetAxis = () => MainV2.joystick?.setAxis(ax.ChannelNo,
                    (joystickaxis)Enum.Parse(typeof(joystickaxis), ax.ChannelValue));
                ax.GetValue = () =>
                {
                    return (short)MainV2.comPort.MAV.cs.GetType().GetField("rcoverridech" + ax.ChannelNo)
                        .GetValue(MainV2.comPort.MAV.cs);
                };

                ax.Visible = false;
                Controls.Add(ax);

                y += ax.Height;


                if ((ax.Bottom + 30) > this.Height)
                    this.Height = ax.Bottom;

                if ((ax.Right) > this.Width)
                    this.Width = ax.Right;
            }

            this.ResumeLayout();

            if (MainV2.joystick != null && MainV2.joystick.enabled)
            {
                timer1.Start();
                BUT_enable.Text = "Disable";
            }

            startup = false;
        }

        int[] getButtonNumbers()
        {
            int[] temp = new int[128];
            temp[0] = -1;
            for (int a = 0; a < temp.Length - 1; a++)
            {
                temp[a + 1] = a;
            }
            return temp;
        }

        private void BUT_enable_Click(object sender, EventArgs e)
        {
            if (MainV2.joystick == null || MainV2.joystick.enabled == false)
            {
                try
                {
                    if (MainV2.joystick != null)
                        MainV2.joystick.UnAcquireJoyStick();
                }
                catch
                {
                }

                // all config is loaded from the xmls
                var joy = JoystickBase.Create(() => MainV2.comPort);

                joy.elevons = CHK_elevons.Checked;

                //show error message if a joystick is not connected when Enable is clicked
                if (!joy.start(CMB_joysticks.Text))
                {
                    CustomMessageBox.Show("Please Connect a Joystick", "No Joystick");
                    joy.Dispose();
                    return;
                }

                Settings.Instance["joystick_name"] = CMB_joysticks.Text;

                MainV2.joystick = joy;
                MainV2.joystick.enabled = true;

                BUT_enable.Text = "Disable";

                //timer1.Start();
            }
            else
            {
                MainV2.joystick.enabled = false;

                MainV2.joystick.clearRCOverride();

                MainV2.joystick = null;


                //timer1.Stop();

                BUT_enable.Text = "Enable";
            }
        }

        private void BUT_save_Click(object sender, EventArgs e)
        {
            if (MainV2.joystick == null)
            {
                CustomMessageBox.Show("Please select a joystick");
                return;
            }
            MainV2.joystick.saveconfig();

            Settings.Instance["joy_elevons"] = CHK_elevons.Checked.ToString();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (MainV2.joystick == null || MainV2.joystick.enabled == false)
                {
                    //Console.WriteLine(DateTime.Now.Millisecond + " start ");
                    var joy = MainV2.joystick;
                    if (joy == null)
                    {
                        joy = JoystickBase.Create(() => MainV2.comPort);
                        for (int a = 1; a <= maxaxis; a++)
                        {
                            var config = joy.getChannel(a);

                            joy.setChannel(a, config.axis, config.reverse, config.expo);
                        }

                        joy.elevons = CHK_elevons.Checked;

                        joy.AcquireJoystick(CMB_joysticks.Text);

                        joy.name = CMB_joysticks.Text;

                        noButtons = joy.getNumButtons();

                        noButtons = Math.Min(16, noButtons);

                        SuspendLayout();

                        MainV2.joystick = joy;

                        var maxctl = Controls.Find("axis" + 1, false).FirstOrDefault();

                        for (int f = 0; f < noButtons; f++)
                        {
                            string name = (f).ToString();

                            doButtontoUI(name, maxctl.Right + 100, maxctl.Top + f * maxctl.Height);

                            var config = joy.getButton(f);

                            joy.setButton(f, config);
                        }

                        ResumeLayout();

                        ThemeManager.ApplyThemeTo(this);

                        CMB_joysticks.SelectedIndex = CMB_joysticks.Items.IndexOf(joy.name);
                    }

                    MainV2.joystick.elevons = CHK_elevons.Checked;

                    MainV2.comPort.MAV.cs.rcoverridech1 = joy.getValueForChannel(1);
                    MainV2.comPort.MAV.cs.rcoverridech2 = joy.getValueForChannel(2);
                    MainV2.comPort.MAV.cs.rcoverridech3 = joy.getValueForChannel(3);
                    MainV2.comPort.MAV.cs.rcoverridech4 = joy.getValueForChannel(4);
                    MainV2.comPort.MAV.cs.rcoverridech5 = joy.getValueForChannel(5);
                    MainV2.comPort.MAV.cs.rcoverridech6 = joy.getValueForChannel(6);
                    MainV2.comPort.MAV.cs.rcoverridech7 = joy.getValueForChannel(7);
                    MainV2.comPort.MAV.cs.rcoverridech8 = joy.getValueForChannel(8);
                    MainV2.comPort.MAV.cs.rcoverridech9 = joy.getValueForChannel(9);
                    MainV2.comPort.MAV.cs.rcoverridech10 = joy.getValueForChannel(10);
                    MainV2.comPort.MAV.cs.rcoverridech11 = joy.getValueForChannel(11);
                    MainV2.comPort.MAV.cs.rcoverridech12 = joy.getValueForChannel(12);
                    MainV2.comPort.MAV.cs.rcoverridech13 = joy.getValueForChannel(13);
                    MainV2.comPort.MAV.cs.rcoverridech14 = joy.getValueForChannel(14);
                    MainV2.comPort.MAV.cs.rcoverridech15 = joy.getValueForChannel(15);
                    MainV2.comPort.MAV.cs.rcoverridech16 = joy.getValueForChannel(16);
                    MainV2.comPort.MAV.cs.rcoverridech17 = joy.getValueForChannel(17);
                    MainV2.comPort.MAV.cs.rcoverridech18 = joy.getValueForChannel(18);

                    //Console.WriteLine(DateTime.Now.Millisecond + " end ");
                }
            }
            catch (SharpDX.SharpDXException ex)
            {
                ex.ToString();
                if (MainV2.joystick != null && MainV2.joystick.enabled == true)
                {
                    BUT_enable_Click(null, null);
                }

                if (ex.Message.Contains("DIERR_NOTACQUIRED"))
                    MainV2.joystick = null;
            }
            catch
            {
                
            }

            // =====================================================
            float rc1_min = 0;
            float rc1_max = 0;
            float rc1_trim = 0;
            float rc3_min = 0;
            float rc3_max = 0;
            float rc3_trim = 0;
            int green;
            int red;
            if (MainV2.comPort.MAV.param.ContainsKey("RC" + 1 + "_MIN"))
            {
                rc1_min = (float)(MainV2.comPort.MAV.param["RC" + 1 + "_MIN"]);
                rc1_max = (float)(MainV2.comPort.MAV.param["RC" + 1 + "_MAX"]);
                rc1_trim = (float)(MainV2.comPort.MAV.param["RC" + 1 + "_TRIM"]);
            }
            else
            {
                rc1_min = 1100;
                rc1_max = 1900;
                rc1_trim = 1500;
            }
            if (MainV2.comPort.MAV.param.ContainsKey("RC" + 3 + "_MIN"))
            {
                rc3_min = (float)(MainV2.comPort.MAV.param["RC" + 3 + "_MIN"]);
                rc3_max = (float)(MainV2.comPort.MAV.param["RC" + 3 + "_MAX"]);
                rc3_trim = (float)(MainV2.comPort.MAV.param["RC" + 3 + "_TRIM"]);
            }
            else
            {

                rc3_min = 1100;
                rc3_max = 1900;
                rc3_trim = 1500;
            }
            float steering_percent;
            float throttle_percent;
            if (MainV2.comPort.MAV.cs.rcoverridech1 > rc1_trim)
            {
                steering_percent = (float)((MainV2.comPort.MAV.cs.rcoverridech1 - rc1_trim) / (rc1_max - rc1_trim));
                //int red = (int)(float)(255 - steering_percent * 255);
                green = (int)(float)(steering_percent * 255);
                if (green <= 255 && green >= 0)
                {
                    SteerCircBar.ProgressColor = Color.FromArgb(0, green, 0);
                }
            }
            else
            {
                steering_percent = (float)((MainV2.comPort.MAV.cs.rcoverridech1 - rc1_min) / (rc1_trim - rc1_min)) - 1;
                red = (int)(float)(-steering_percent * 255);
                //int green = (int)(float)(steering_percent * 255);
                if (red <= 255 && red >= 0)
                {
                    SteerCircBar.ProgressColor = Color.FromArgb(red, 0, 0);
                }
            }

            if (MainV2.comPort.MAV.cs.rcoverridech2 > 1600)
            { GearState.Text = "F"; }
            else if (MainV2.comPort.MAV.cs.rcoverridech2 < 1400)
            { GearState.Text = "R"; }
            else
            { GearState.Text = "N"; }
            if (MainV2.comPort.MAV.cs.rcoverridech3 > rc3_trim + 50)
            {


                throttle_percent = (float)((MainV2.comPort.MAV.cs.rcoverridech3 - rc3_trim) / (rc3_max - rc3_trim)) * 255;
                if (throttle_percent <= 255 && throttle_percent >= 0)
                {
                    ThrottleCircBar.ProgressColor = Color.FromArgb(0, (int)throttle_percent, 0);
                }
                throttle_percent = (float)(throttle_percent / 255.0 * 1.0);
                // GearState.Text = throttle_percent.ToString();
            }
            else if (MainV2.comPort.MAV.cs.rcoverridech3 < rc3_trim - 50)
            {


                throttle_percent = (float)((MainV2.comPort.MAV.cs.rcoverridech3 - rc3_trim) / (rc3_min - rc3_trim)) * 255;
                if (throttle_percent <= 255 && throttle_percent >= 0)
                {
                    ThrottleCircBar.ProgressColor = Color.FromArgb((int)throttle_percent, 0, 0);
                }
                throttle_percent = (float)(throttle_percent / 255.0 * -1.0);
                // GearState.Text = throttle_percent.ToString();
            }
            else
            {
                ThrottleCircBar.ProgressColor = Color.FromArgb(255, 165, 0);
                throttle_percent = 0;
                //GearState.Text = throttle_percent.ToString();
            }
            if (MainV2.comPort.MAV.cs.rcoverridech4 > 1600)
            {
                ENG_label.Text = "ENG STR";
                ENG_label.ForeColor = Color.FromArgb(0, 255, 0);
            }
            else if (MainV2.comPort.MAV.cs.rcoverridech4 < 1400)
            {
                ENG_label.Text = "ENG STP";
                ENG_label.ForeColor = Color.FromArgb(255, 0, 0);
            }
            else
            {
                ENG_label.Text = "ENG ---";
                ENG_label.ForeColor = Color.FromArgb(0, 0, 0);
            }
            if (MainV2.comPort.MAV.cs.rcoverridech6 > 1600)
            {
                ANCH_label.Text = "Gear H";
                ANCH_label.ForeColor = Color.FromArgb(0, 0, 255);
            }
            else if (MainV2.comPort.MAV.cs.rcoverridech6 < 1400)
            {
                ANCH_label.Text = "Gear L";
                ANCH_label.ForeColor = Color.FromArgb(0, 0, 255);
            }
            /*if (MainV2.comPort.MAV.cs.rcoverridech6 > 1600)
            {
                AUX_label.ForeColor = Color.FromArgb(0, 255, 0);
            }
            else
            {
                AUX_label.ForeColor = Color.FromArgb(0, 0, 0);
            }*/
            // GearState.Text = MainV2.comPort.MAV.cs.rcoverridech3.ToString();

            progressBarRoll.Value = MainV2.comPort.MAV.cs.rcoverridech1;
            progressBarPith.Value = MainV2.comPort.MAV.cs.rcoverridech2;
            SteerCircBar.StartAngle = (int)(80 - steering_percent * 90);
            progressBarThrottle.Value = MainV2.comPort.MAV.cs.rcoverridech3;
            ThrottleCircBar.StartAngle = 260 + (int)(float)(((float)(throttle_percent + 1.0) / 2.0 * 90.0) - 45);
            //label3.Text = ThrottleCircBar.Value.ToString();
            progressBarRudder.Value = MainV2.comPort.MAV.cs.rcoverridech4;
            ProgressBarCH5.Value = MainV2.comPort.MAV.cs.rcoverridech5;
            ProgressBarCH6.Value = MainV2.comPort.MAV.cs.rcoverridech6;
            ProgressBarCH7.Value = MainV2.comPort.MAV.cs.rcoverridech7;
            ProgressBarCH8.Value = MainV2.comPort.MAV.cs.rcoverridech8;

            try
            {
                progressBarRoll.maxline = MainV2.joystick.getRawValueForChannel(1);
                progressBarPith.maxline = MainV2.joystick.getRawValueForChannel(2);
                progressBarThrottle.maxline = MainV2.joystick.getRawValueForChannel(3);
                progressBarRudder.maxline = MainV2.joystick.getRawValueForChannel(4);
                ProgressBarCH5.maxline = MainV2.joystick.getRawValueForChannel(5);
                ProgressBarCH6.maxline = MainV2.joystick.getRawValueForChannel(6);
                ProgressBarCH7.maxline = MainV2.joystick.getRawValueForChannel(7);
                ProgressBarCH8.maxline = MainV2.joystick.getRawValueForChannel(8);
            }
            catch
            {
                //Exception Error in the application. -2147024866 (DIERR_INPUTLOST)

            }
            // =====================================================



            try
            {
                for (int f = 0; f < noButtons; f++)
                {
                    string name = (f).ToString();

                    var items = this.Controls.Find("hbar" + name, false);

                    if (items.Length > 0)
                        ((HorizontalProgressBar)items[0]).Value =
                            MainV2.joystick.isButtonPressed(f) ? 100 : 0;
                }
            }
            catch
            {
            } // this is for buttons - silent fail
        }

        private void CMB_joysticks_Click(object sender, EventArgs e)
        {
            CMB_joysticks.Items.Clear();

            var joysticklist = JoystickBase.getDevices();

            foreach (var device in joysticklist)
            {
                CMB_joysticks.Items.Add(device);
            }

            if (CMB_joysticks.Items.Count > 0 && CMB_joysticks.SelectedIndex == -1)
                CMB_joysticks.SelectedIndex = 0;
        }

        private void cmbbutton_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startup)
                return;

            string name = ((ComboBox)sender).Name.Replace("cmbbutton", "");

            MainV2.joystick.changeButton((int.Parse(name)), int.Parse(((ComboBox)sender).Text));
        }

        private void BUT_detbutton_Click(object sender, EventArgs e)
        {
            string name = ((MyButton)sender).Name.Replace("mybut", "");

            ComboBox cmb = (ComboBox)(this.Controls.Find("cmbbutton" + name, false)[0]);
            cmb.Text = JoystickBase.getPressedButton(CMB_joysticks.Text).ToString();
        }

        void doButtontoUI(string name, int x, int y)
        {
            MyLabel butlabel = new MyLabel();
            ComboBox butnumberlist = new ComboBox();
            Controls.MyButton but_detect = new Controls.MyButton();
            HorizontalProgressBar hbar = new HorizontalProgressBar();
            ComboBox cmbaction = new ComboBox();
            Controls.MyButton but_settings = new Controls.MyButton();

            if (MainV2.joystick == null)
            {
                butlabel.Dispose();
                butnumberlist.Dispose();
                but_detect.Dispose();
                hbar.Dispose();
                cmbaction.Dispose();
                but_settings.Dispose();
                return;
            }

            var config = MainV2.joystick.getButton(int.Parse(name));


            butlabel.Location = new Point(x, y);
            butlabel.Size = new Size(47, 13);
            butlabel.Text = "But " + (int.Parse(name) + 1);

            butnumberlist.Location = new Point(butlabel.Right, y);
            butnumberlist.Size = new Size(70, 21);
            //butnumberlist.DataSource = getButtonNumbers();

            butnumberlist.Items.AddRange(getButtonNumbers().Select(item => item.ToString()).ToArray());
            butnumberlist.SelectedIndex = 0;


            butnumberlist.DropDownStyle = ComboBoxStyle.DropDownList;
            butnumberlist.Name = "cmbbutton" + name;

            //butnumberlist.SelectedItem = "-1";
            butnumberlist.SelectedItem = config.buttonno.ToString();

            //if (Settings.Instance["butno" + name] != null)
            //    butnumberlist.Text = (Settings.Instance["butno" + name].ToString());
            //if (config.buttonno != -1)

            //butnumberlist.Text = config.buttonno.ToString();

            butnumberlist.SelectedIndexChanged += new EventHandler(cmbbutton_SelectedIndexChanged);

            but_detect.Location = new Point(butnumberlist.Right, y);
            //but_detect.Size = BUT_detch1.Size;
            but_detect.Text = "Detect";
            but_detect.AutoSize = true;

            but_detect.Name = "mybut" + name;
            but_detect.Click += new EventHandler(BUT_detbutton_Click);

            hbar.Location = new Point(but_detect.Right, y);
            hbar.Size = new Size(100, 21);
            hbar.Name = "hbar" + name;

            cmbaction.Location = new Point(hbar.Right + 5, y);
            cmbaction.Size = new Size(100, 21);

            //cmbaction.DataSource = Enum.GetNames(typeof(buttonfunction));
            cmbaction.Items.AddRange(Enum.GetNames(typeof(buttonfunction)));


            //Common.getModesList(MainV2.comPort.MAV.cs);
            //cmbaction.ValueMember = "Key";
            //cmbaction.DisplayMember = "Value";
            cmbaction.Tag = name;
            cmbaction.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbaction.Name = "cmbaction" + name;
            //if (Settings.Instance["butaction" + name] != null)
            //  cmbaction.Text = Settings.Instance["butaction" + name].ToString();
            //if (config.function != buttonfunction.ChangeMode)
            cmbaction.Text = config.function.ToString();
            cmbaction.SelectedIndexChanged += cmbaction_SelectedIndexChanged;

            but_settings.Location = new Point(cmbaction.Right + 5, y);
            //but_settings.Size = BUT_detch1.Size;
            but_settings.Text = "Settings";
            but_settings.Name = "butsettings" + name;
            but_settings.Click += but_settings_Click;
            but_settings.Tag = cmbaction;

            // do this here so putting in text works
            this.Controls.AddRange(new Control[] { butlabel, butnumberlist, but_detect, hbar, cmbaction, but_settings });

            if ((but_settings.Bottom + 30) > this.Height)
                this.Height += 25;

            if ((but_settings.Right) > this.Width)
                this.Width = but_settings.Right + 5;
        }

        void cmbaction_SelectedIndexChanged(object sender, EventArgs e)
        {
            int num = int.Parse(((Control)sender).Tag.ToString());
            var config = MainV2.joystick.getButton(num);
            config.function =
                (buttonfunction)Enum.Parse(typeof(buttonfunction), ((Control)sender).Text);
            MainV2.joystick.setButton(num, config);
        }

        void but_settings_Click(object sender, EventArgs e)
        {
            var cmb = ((Control)sender).Tag as ComboBox;

            switch ((buttonfunction)Enum.Parse(typeof(buttonfunction), cmb.SelectedItem.ToString()))
            {
                case buttonfunction.ChangeMode:
                    new Joy_ChangeMode((string)cmb.Tag).ShowDialog();
                    break;
                case buttonfunction.Mount_Mode:
                    new Joy_Mount_Mode((string)cmb.Tag).ShowDialog();
                    break;
                case buttonfunction.Do_Repeat_Relay:
                    new Joy_Do_Repeat_Relay((string)cmb.Tag).ShowDialog();
                    break;
                case buttonfunction.Do_Repeat_Servo:
                    new Joy_Do_Repeat_Servo((string)cmb.Tag).ShowDialog();
                    break;
                case buttonfunction.Do_Set_Relay:
                    new Joy_Do_Set_Relay((string)cmb.Tag).ShowDialog();
                    break;
                case buttonfunction.Do_Set_Servo:
                    new Joy_Do_Set_Servo((string)cmb.Tag).ShowDialog();
                    break;
                case buttonfunction.Button_axis0:
                    new Joy_Button_axis((string)cmb.Tag).ShowDialog();
                    break;
                case buttonfunction.Button_axis1:
                    new Joy_Button_axis((string)cmb.Tag).ShowDialog();
                    break;
                default:
                    CustomMessageBox.Show("No settings to set", "No settings");
                    break;
            }
        }

        private void CMB_joysticks_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (MainV2.joystick != null && MainV2.joystick.enabled == false)
                    MainV2.joystick.UnAcquireJoyStick();
            }
            catch
            {
            }
        }

       
        private void JoystickSetup_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Stop();

            if (MainV2.joystick != null && MainV2.joystick.enabled == false)
            {
                MainV2.joystick.UnAcquireJoyStick();
                MainV2.joystick = null;
            }
        }

        private void CHK_elevons_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.joystick == null)
            {
                return;
            }
            MainV2.joystick.elevons = CHK_elevons.Checked;
        }

        private void chk_manualcontrol_CheckedChanged(object sender, EventArgs e)
        {
            /*MainV2.joystick.manual_control = chk_manualcontrol.Checked;*/
        }

        public void Deactivate()
        {
            timer1.Stop();

            if (MainV2.joystick != null && MainV2.joystick.enabled == false)
            {
                MainV2.joystick.UnAcquireJoyStick();
                MainV2.joystick = null;
            }
        }

        private void CMB_joysticks_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            try
            {
                if (MainV2.joystick != null && MainV2.joystick.enabled == false)
                    MainV2.joystick.UnAcquireJoyStick();
            }
            catch { }
        }

         
         

        private void BUT_enable_Click_1(object sender, EventArgs e)
        {
            if (MainV2.joystick == null || MainV2.joystick.enabled == false)
            {
                try
                {
                    if (MainV2.joystick != null)
                        MainV2.joystick.UnAcquireJoyStick();
                }
                catch
                {
                }

                // all config is loaded from the xmls
                var joy = JoystickBase.Create(() => MainV2.comPort);

                joy.elevons = CHK_elevons.Checked;

                //show error message if a joystick is not connected when Enable is clicked
                if (!joy.start(CMB_joysticks.Text))
                {
                    CustomMessageBox.Show("Please Connect a Joystick", "No Joystick");
                    joy.Dispose();
                    return;
                }

                Settings.Instance["joystick_name"] = CMB_joysticks.Text;

                MainV2.joystick = joy;
                MainV2.joystick.enabled = true;

                BUT_enable.Text = "Disable";

                //timer1.Start();
            }
            else
            {
                MainV2.joystick.enabled = false;

                MainV2.joystick.clearRCOverride();

                MainV2.joystick = null;


                //timer1.Stop();

                BUT_enable.Text = "Enable";
            }
        }

        private void BUT_detch1_Click(object sender, EventArgs e)
        {
            CMB_CH1.Text = JoystickBase.getMovingAxis(CMB_joysticks.Text, 16000).ToString();
        }

        private void BUT_detch3_Click(object sender, EventArgs e)
        {
            CMB_CH3.Text = JoystickBase.getMovingAxis(CMB_joysticks.Text, 16000).ToString();
        }

        private void BUT_save_Click_1(object sender, EventArgs e)
        {
            if (MainV2.joystick == null)
            {
                CustomMessageBox.Show("Please select a joystick");
                return;
            }
            MainV2.joystick.saveconfig();

            Settings.Instance["joy_elevons"] = CHK_elevons.Checked.ToString();
        }
    }
}