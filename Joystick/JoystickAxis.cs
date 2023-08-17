using System;
using System.Windows.Forms;

namespace MissionPlanner.Joystick
{
    public class JoystickAxis : UserControl
    {
        public JoystickAxis()
        {
            InitializeComponent();
        }

        private Controls.MyButton BUT_detch;
        private CheckBox revCH;
        private Label label13;
        private TextBox expo_ch;
        private Controls.HorizontalProgressBar ProgressBarCH;
        private ComboBox CMB_CH;

        public Func<string> Detect;
        public Action SetAxis;
        public Action Reverse;
        public Action Expo;
        public Func<int> GetValue;

        private Timer timer1;
        private Controls.MyTrackBar myTrackBar1;
        private Label label1;
        private Controls.MyTrackBar myTrackBar2;
        private Controls.MyTrackBar myTrackBar3;
        private Label label2;
        private Label label3;
        private Controls.MyButton myButton1;
        private Label label4;
        private ComboBox comboBox1;
        private Controls.MyButton myButton2;
        private Label label5;
        private ComboBox comboBox2;

        public int ChannelNo
        {
            get;
            set;
        }

        public string Label
        {
            get { return label13.Text; }
            set { label13.Text = value; }
        }

        public Array AxisArray
        {
            get { return (Array)CMB_CH.DataSource; }
            set { CMB_CH.DataSource = value; }
        }

        public string ChannelValue
        {
            get { return CMB_CH.Text; }
            set { CMB_CH.Text = value; }
        }

        public string ExpoValue
        {
            get { return expo_ch.Text; }
            set { expo_ch.Text = value; }
        }

        public bool ReverseValue
        {
            get { return revCH.Checked; }
            set { revCH.Checked = value; }
        }

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.BUT_detch = new MissionPlanner.Controls.MyButton();
            this.revCH = new System.Windows.Forms.CheckBox();
            this.label13 = new System.Windows.Forms.Label();
            this.expo_ch = new System.Windows.Forms.TextBox();
            this.ProgressBarCH = new MissionPlanner.Controls.HorizontalProgressBar();
            this.CMB_CH = new System.Windows.Forms.ComboBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.myTrackBar1 = new MissionPlanner.Controls.MyTrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.myTrackBar2 = new MissionPlanner.Controls.MyTrackBar();
            this.myTrackBar3 = new MissionPlanner.Controls.MyTrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.myButton1 = new MissionPlanner.Controls.MyButton();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.myButton2 = new MissionPlanner.Controls.MyButton();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.myTrackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.myTrackBar2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.myTrackBar3)).BeginInit();
            this.SuspendLayout();
            // 
            // BUT_detch
            // 
            this.BUT_detch.BGGradBot = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.BUT_detch.BGGradTop = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.BUT_detch.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.BUT_detch.Location = new System.Drawing.Point(381, 51);
            this.BUT_detch.Margin = new System.Windows.Forms.Padding(4);
            this.BUT_detch.Name = "BUT_detch";
            this.BUT_detch.Outline = System.Drawing.Color.Black;
            this.BUT_detch.Size = new System.Drawing.Size(88, 27);
            this.BUT_detch.TabIndex = 62;
            this.BUT_detch.Text = "Auto Detect";
            this.BUT_detch.TextColor = System.Drawing.Color.White;
            this.BUT_detch.TextColorNotEnabled = System.Drawing.Color.White;
            this.BUT_detch.UseVisualStyleBackColor = true;
            this.BUT_detch.Click += new System.EventHandler(this.BUT_detch_Click);
            // 
            // revCH
            // 
            this.revCH.AutoSize = true;
            this.revCH.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.revCH.Location = new System.Drawing.Point(547, 68);
            this.revCH.Margin = new System.Windows.Forms.Padding(4);
            this.revCH.Name = "revCH";
            this.revCH.Size = new System.Drawing.Size(18, 17);
            this.revCH.TabIndex = 61;
            this.revCH.UseVisualStyleBackColor = true;
            this.revCH.Visible = false;
            this.revCH.CheckedChanged += new System.EventHandler(this.revCH_CheckedChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label13.Location = new System.Drawing.Point(411, 4);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(26, 16);
            this.label13.TabIndex = 60;
            this.label13.Text = "CH";
            // 
            // expo_ch
            // 
            this.expo_ch.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.expo_ch.Location = new System.Drawing.Point(304, 0);
            this.expo_ch.Margin = new System.Windows.Forms.Padding(4);
            this.expo_ch.Name = "expo_ch";
            this.expo_ch.Size = new System.Drawing.Size(133, 15);
            this.expo_ch.TabIndex = 59;
            this.expo_ch.Text = "0";
            this.expo_ch.Visible = false;
            this.expo_ch.TextChanged += new System.EventHandler(this.expo_ch_TextChanged);
            // 
            // ProgressBarCH
            // 
            this.ProgressBarCH.DrawLabel = true;
            this.ProgressBarCH.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ProgressBarCH.Label = null;
            this.ProgressBarCH.Location = new System.Drawing.Point(441, 0);
            this.ProgressBarCH.Margin = new System.Windows.Forms.Padding(4);
            this.ProgressBarCH.Maximum = 2200;
            this.ProgressBarCH.maxline = 0;
            this.ProgressBarCH.Minimum = 800;
            this.ProgressBarCH.minline = 0;
            this.ProgressBarCH.Name = "ProgressBarCH";
            this.ProgressBarCH.Size = new System.Drawing.Size(149, 28);
            this.ProgressBarCH.TabIndex = 58;
            this.ProgressBarCH.Value = 800;
            this.ProgressBarCH.Visible = false;
            this.ProgressBarCH.Click += new System.EventHandler(this.ProgressBarCH_Click);
            // 
            // CMB_CH
            // 
            this.CMB_CH.FormattingEnabled = true;
            this.CMB_CH.Items.AddRange(new object[] {
            "RZ",
            "X",
            "Y",
            "SL1"});
            this.CMB_CH.Location = new System.Drawing.Point(373, 24);
            this.CMB_CH.Margin = new System.Windows.Forms.Padding(4);
            this.CMB_CH.Name = "CMB_CH";
            this.CMB_CH.Size = new System.Drawing.Size(107, 24);
            this.CMB_CH.TabIndex = 57;
            this.CMB_CH.SelectedIndexChanged += new System.EventHandler(this.CMB_CH_SelectedIndexChanged);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // myTrackBar1
            // 
            this.myTrackBar1.LargeChange = 1F;
            this.myTrackBar1.Location = new System.Drawing.Point(39, 172);
            this.myTrackBar1.Maximum = 2000F;
            this.myTrackBar1.Minimum = 1000F;
            this.myTrackBar1.Name = "myTrackBar1";
            this.myTrackBar1.Size = new System.Drawing.Size(269, 56);
            this.myTrackBar1.SmallChange = 1F;
            this.myTrackBar1.TabIndex = 63;
            this.myTrackBar1.TickFrequency = 500F;
            this.myTrackBar1.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.myTrackBar1.Value = 1500F;
            this.myTrackBar1.Scroll += new System.EventHandler(this.myTrackBar1_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(107, 213);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 39);
            this.label1.TabIndex = 64;
            this.label1.Text = "Rudder";
            // 
            // myTrackBar2
            // 
            this.myTrackBar2.LargeChange = 1F;
            this.myTrackBar2.Location = new System.Drawing.Point(296, 25);
            this.myTrackBar2.Maximum = 2000F;
            this.myTrackBar2.Minimum = 1000F;
            this.myTrackBar2.Name = "myTrackBar2";
            this.myTrackBar2.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.myTrackBar2.Size = new System.Drawing.Size(70, 153);
            this.myTrackBar2.SmallChange = 1F;
            this.myTrackBar2.TabIndex = 65;
            this.myTrackBar2.TickFrequency = 500F;
            this.myTrackBar2.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.myTrackBar2.Value = 1500F;
            // 
            // myTrackBar3
            // 
            this.myTrackBar3.LargeChange = 1F;
            this.myTrackBar3.Location = new System.Drawing.Point(3, 25);
            this.myTrackBar3.Maximum = 2000F;
            this.myTrackBar3.Minimum = 1000F;
            this.myTrackBar3.Name = "myTrackBar3";
            this.myTrackBar3.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.myTrackBar3.Size = new System.Drawing.Size(56, 153);
            this.myTrackBar3.SmallChange = 1F;
            this.myTrackBar3.TabIndex = 66;
            this.myTrackBar3.TickFrequency = 500F;
            this.myTrackBar3.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.myTrackBar3.Value = 1500F;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(66, 85);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 32);
            this.label2.TabIndex = 67;
            this.label2.Text = "THR1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(202, 85);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 32);
            this.label3.TabIndex = 68;
            this.label3.Text = "THR2";
            // 
            // myButton1
            // 
            this.myButton1.BGGradBot = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.myButton1.BGGradTop = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.myButton1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.myButton1.Location = new System.Drawing.Point(381, 134);
            this.myButton1.Margin = new System.Windows.Forms.Padding(4);
            this.myButton1.Name = "myButton1";
            this.myButton1.Outline = System.Drawing.Color.Black;
            this.myButton1.Size = new System.Drawing.Size(88, 28);
            this.myButton1.TabIndex = 71;
            this.myButton1.Text = "Auto Detect";
            this.myButton1.TextColor = System.Drawing.Color.White;
            this.myButton1.TextColorNotEnabled = System.Drawing.Color.White;
            this.myButton1.UseVisualStyleBackColor = true;
            this.myButton1.Click += new System.EventHandler(this.myButton1_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(411, 85);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(26, 16);
            this.label4.TabIndex = 70;
            this.label4.Text = "CH";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "RZ",
            "X",
            "Y",
            "SL1"});
            this.comboBox1.Location = new System.Drawing.Point(373, 106);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(107, 24);
            this.comboBox1.TabIndex = 69;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // myButton2
            // 
            this.myButton2.BGGradBot = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.myButton2.BGGradTop = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.myButton2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.myButton2.Location = new System.Drawing.Point(381, 216);
            this.myButton2.Margin = new System.Windows.Forms.Padding(4);
            this.myButton2.Name = "myButton2";
            this.myButton2.Outline = System.Drawing.Color.Black;
            this.myButton2.Size = new System.Drawing.Size(88, 28);
            this.myButton2.TabIndex = 74;
            this.myButton2.Text = "Auto Detect";
            this.myButton2.TextColor = System.Drawing.Color.White;
            this.myButton2.TextColorNotEnabled = System.Drawing.Color.White;
            this.myButton2.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(411, 167);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(26, 16);
            this.label5.TabIndex = 73;
            this.label5.Text = "CH";
            // 
            // comboBox2
            // 
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Items.AddRange(new object[] {
            "RZ",
            "X",
            "Y",
            "SL1"});
            this.comboBox2.Location = new System.Drawing.Point(373, 188);
            this.comboBox2.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(107, 24);
            this.comboBox2.TabIndex = 72;
            // 
            // JoystickAxis
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.CMB_CH);
            this.Controls.Add(this.myButton2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.comboBox2);
            this.Controls.Add(this.myButton1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.myTrackBar3);
            this.Controls.Add(this.myTrackBar2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.myTrackBar1);
            this.Controls.Add(this.BUT_detch);
            this.Controls.Add(this.revCH);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.expo_ch);
            this.Controls.Add(this.ProgressBarCH);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "JoystickAxis";
            this.Size = new System.Drawing.Size(594, 281);
            this.Load += new System.EventHandler(this.JoystickAxis_Load);
            ((System.ComponentModel.ISupportInitialize)(this.myTrackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.myTrackBar2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.myTrackBar3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private void CMB_CH_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetAxis?.Invoke();
        }

        private void BUT_detch_Click(object sender, EventArgs e)
        {
            Detect?.Invoke();
        }

        private void expo_ch_TextChanged(object sender, EventArgs e)
        {
            Expo?.Invoke();
        }

        private void revCH_CheckedChanged(object sender, EventArgs e)
        {
            Reverse?.Invoke();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
           
            if (GetValue()<= 800)
                return;

            ProgressBarCH.maxline = GetValue();
            ProgressBarCH.Value = GetValue();
           // myTrackBar1.Minimum = GetValue();
            myTrackBar1.Value = GetValue();
          


        }

        private void JoystickAxis_Load(object sender, EventArgs e)
        {

        }

        private void myTrackBar1_Scroll(object sender, EventArgs e)
        {

        }

        private void ProgressBarCH_Click(object sender, EventArgs e)
        {

        }

        private void myButton1_Click(object sender, EventArgs e)
        {
            Detect?.Invoke();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetAxis?.Invoke();
        }
    }
}
