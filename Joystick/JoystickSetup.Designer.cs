using MissionPlanner.Controls;
using System.Windows.Forms;

namespace MissionPlanner.Joystick
{
    partial class JoystickSetup
    {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JoystickSetup));
            this.AuxBtn = new System.Windows.Forms.ComboBox();
            this.AnchUpBtn = new System.Windows.Forms.ComboBox();
            this.AnchDown = new System.Windows.Forms.ComboBox();
            this.EngStBtn = new System.Windows.Forms.ComboBox();
            this.EngStpBtn = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.BUT_detch8 = new MissionPlanner.Controls.MyButton();
            this.revCH8 = new System.Windows.Forms.CheckBox();
            this.label13 = new System.Windows.Forms.Label();
            this.expo_ch8 = new System.Windows.Forms.TextBox();
            this.ProgressBarCH8 = new MissionPlanner.Controls.HorizontalProgressBar();
            this.CMB_CH8 = new System.Windows.Forms.ComboBox();
            this.BUT_detch7 = new MissionPlanner.Controls.MyButton();
            this.revCH7 = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this.expo_ch7 = new System.Windows.Forms.TextBox();
            this.ProgressBarCH7 = new MissionPlanner.Controls.HorizontalProgressBar();
            this.CMB_CH7 = new System.Windows.Forms.ComboBox();
            this.BUT_detch6 = new MissionPlanner.Controls.MyButton();
            this.revCH6 = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.expo_ch6 = new System.Windows.Forms.TextBox();
            this.ProgressBarCH6 = new MissionPlanner.Controls.HorizontalProgressBar();
            this.CMB_CH6 = new System.Windows.Forms.ComboBox();
            this.BUT_detch5 = new MissionPlanner.Controls.MyButton();
            this.revCH5 = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            this.expo_ch5 = new System.Windows.Forms.TextBox();
            this.ProgressBarCH5 = new MissionPlanner.Controls.HorizontalProgressBar();
            this.CMB_CH5 = new System.Windows.Forms.ComboBox();
            this.CHK_elevons = new System.Windows.Forms.CheckBox();
            this.BUT_detch4 = new MissionPlanner.Controls.MyButton();
            this.BUT_detch2 = new MissionPlanner.Controls.MyButton();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.BUT_enable = new MissionPlanner.Controls.MyButton();
            this.BUT_save = new MissionPlanner.Controls.MyButton();
            this.revCH4 = new System.Windows.Forms.CheckBox();
            this.revCH2 = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.expo_ch4 = new System.Windows.Forms.TextBox();
            this.expo_ch3 = new System.Windows.Forms.TextBox();
            this.expo_ch2 = new System.Windows.Forms.TextBox();
            this.expo_ch1 = new System.Windows.Forms.TextBox();
            this.progressBarRudder = new MissionPlanner.Controls.HorizontalProgressBar();
            this.progressBarThrottle = new MissionPlanner.Controls.HorizontalProgressBar();
            this.progressBarPith = new MissionPlanner.Controls.HorizontalProgressBar();
            this.progressBarRoll = new MissionPlanner.Controls.HorizontalProgressBar();
            this.CMB_CH4 = new System.Windows.Forms.ComboBox();
            this.CMB_CH2 = new System.Windows.Forms.ComboBox();
            this.CMB_joysticks = new System.Windows.Forms.ComboBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.chk_manualcontrol = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // AuxBtn
            // 
            this.AuxBtn.FormattingEnabled = true;
            this.AuxBtn.Items.AddRange(new object[] {
            resources.GetString("AuxBtn.Items"),
            resources.GetString("AuxBtn.Items1"),
            resources.GetString("AuxBtn.Items2"),
            resources.GetString("AuxBtn.Items3")});
            resources.ApplyResources(this.AuxBtn, "AuxBtn");
            this.AuxBtn.Name = "AuxBtn";
            // 
            // AnchUpBtn
            // 
            this.AnchUpBtn.FormattingEnabled = true;
            this.AnchUpBtn.Items.AddRange(new object[] {
            resources.GetString("AnchUpBtn.Items"),
            resources.GetString("AnchUpBtn.Items1"),
            resources.GetString("AnchUpBtn.Items2"),
            resources.GetString("AnchUpBtn.Items3")});
            resources.ApplyResources(this.AnchUpBtn, "AnchUpBtn");
            this.AnchUpBtn.Name = "AnchUpBtn";
            // 
            // AnchDown
            // 
            this.AnchDown.FormattingEnabled = true;
            this.AnchDown.Items.AddRange(new object[] {
            resources.GetString("AnchDown.Items"),
            resources.GetString("AnchDown.Items1"),
            resources.GetString("AnchDown.Items2"),
            resources.GetString("AnchDown.Items3")});
            resources.ApplyResources(this.AnchDown, "AnchDown");
            this.AnchDown.Name = "AnchDown";
            // 
            // EngStBtn
            // 
            this.EngStBtn.FormattingEnabled = true;
            this.EngStBtn.Items.AddRange(new object[] {
            resources.GetString("EngStBtn.Items"),
            resources.GetString("EngStBtn.Items1"),
            resources.GetString("EngStBtn.Items2"),
            resources.GetString("EngStBtn.Items3")});
            resources.ApplyResources(this.EngStBtn, "EngStBtn");
            this.EngStBtn.Name = "EngStBtn";
            // 
            // EngStpBtn
            // 
            this.EngStpBtn.FormattingEnabled = true;
            this.EngStpBtn.Items.AddRange(new object[] {
            resources.GetString("EngStpBtn.Items"),
            resources.GetString("EngStpBtn.Items1"),
            resources.GetString("EngStpBtn.Items2"),
            resources.GetString("EngStpBtn.Items3")});
            resources.ApplyResources(this.EngStpBtn, "EngStpBtn");
            this.EngStpBtn.Name = "EngStpBtn";
            // 
            // label14
            // 
            resources.ApplyResources(this.label14, "label14");
            this.label14.Name = "label14";
            // 
            // BUT_detch8
            // 
            resources.ApplyResources(this.BUT_detch8, "BUT_detch8");
            this.BUT_detch8.Name = "BUT_detch8";
            this.BUT_detch8.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.BUT_detch8.UseVisualStyleBackColor = true;
            // 
            // revCH8
            // 
            resources.ApplyResources(this.revCH8, "revCH8");
            this.revCH8.Name = "revCH8";
            this.revCH8.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            resources.ApplyResources(this.label13, "label13");
            this.label13.Name = "label13";
            // 
            // expo_ch8
            // 
            this.expo_ch8.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.expo_ch8, "expo_ch8");
            this.expo_ch8.Name = "expo_ch8";
            // 
            // ProgressBarCH8
            // 
            this.ProgressBarCH8.DrawLabel = true;
            resources.ApplyResources(this.ProgressBarCH8, "ProgressBarCH8");
            this.ProgressBarCH8.Label = null;
            this.ProgressBarCH8.Maximum = 2200;
            this.ProgressBarCH8.maxline = 0;
            this.ProgressBarCH8.Minimum = 800;
            this.ProgressBarCH8.minline = 0;
            this.ProgressBarCH8.Name = "ProgressBarCH8";
            this.ProgressBarCH8.Value = 800;
            // 
            // CMB_CH8
            // 
            this.CMB_CH8.FormattingEnabled = true;
            this.CMB_CH8.Items.AddRange(new object[] {
            resources.GetString("CMB_CH8.Items"),
            resources.GetString("CMB_CH8.Items1"),
            resources.GetString("CMB_CH8.Items2"),
            resources.GetString("CMB_CH8.Items3")});
            resources.ApplyResources(this.CMB_CH8, "CMB_CH8");
            this.CMB_CH8.Name = "CMB_CH8";
            // 
            // BUT_detch7
            // 
            resources.ApplyResources(this.BUT_detch7, "BUT_detch7");
            this.BUT_detch7.Name = "BUT_detch7";
            this.BUT_detch7.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.BUT_detch7.UseVisualStyleBackColor = true;
            // 
            // revCH7
            // 
            resources.ApplyResources(this.revCH7, "revCH7");
            this.revCH7.Name = "revCH7";
            this.revCH7.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            resources.ApplyResources(this.label12, "label12");
            this.label12.Name = "label12";
            // 
            // expo_ch7
            // 
            this.expo_ch7.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.expo_ch7, "expo_ch7");
            this.expo_ch7.Name = "expo_ch7";
            // 
            // ProgressBarCH7
            // 
            this.ProgressBarCH7.DrawLabel = true;
            resources.ApplyResources(this.ProgressBarCH7, "ProgressBarCH7");
            this.ProgressBarCH7.Label = null;
            this.ProgressBarCH7.Maximum = 2200;
            this.ProgressBarCH7.maxline = 0;
            this.ProgressBarCH7.Minimum = 800;
            this.ProgressBarCH7.minline = 0;
            this.ProgressBarCH7.Name = "ProgressBarCH7";
            this.ProgressBarCH7.Value = 800;
            // 
            // CMB_CH7
            // 
            this.CMB_CH7.FormattingEnabled = true;
            this.CMB_CH7.Items.AddRange(new object[] {
            resources.GetString("CMB_CH7.Items"),
            resources.GetString("CMB_CH7.Items1"),
            resources.GetString("CMB_CH7.Items2"),
            resources.GetString("CMB_CH7.Items3")});
            resources.ApplyResources(this.CMB_CH7, "CMB_CH7");
            this.CMB_CH7.Name = "CMB_CH7";
            // 
            // BUT_detch6
            // 
            resources.ApplyResources(this.BUT_detch6, "BUT_detch6");
            this.BUT_detch6.Name = "BUT_detch6";
            this.BUT_detch6.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.BUT_detch6.UseVisualStyleBackColor = true;
            // 
            // revCH6
            // 
            resources.ApplyResources(this.revCH6, "revCH6");
            this.revCH6.Name = "revCH6";
            this.revCH6.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            resources.ApplyResources(this.label11, "label11");
            this.label11.Name = "label11";
            // 
            // expo_ch6
            // 
            this.expo_ch6.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.expo_ch6, "expo_ch6");
            this.expo_ch6.Name = "expo_ch6";
            // 
            // ProgressBarCH6
            // 
            this.ProgressBarCH6.DrawLabel = true;
            resources.ApplyResources(this.ProgressBarCH6, "ProgressBarCH6");
            this.ProgressBarCH6.Label = null;
            this.ProgressBarCH6.Maximum = 2200;
            this.ProgressBarCH6.maxline = 0;
            this.ProgressBarCH6.Minimum = 800;
            this.ProgressBarCH6.minline = 0;
            this.ProgressBarCH6.Name = "ProgressBarCH6";
            this.ProgressBarCH6.Value = 800;
            // 
            // CMB_CH6
            // 
            this.CMB_CH6.FormattingEnabled = true;
            this.CMB_CH6.Items.AddRange(new object[] {
            resources.GetString("CMB_CH6.Items"),
            resources.GetString("CMB_CH6.Items1"),
            resources.GetString("CMB_CH6.Items2"),
            resources.GetString("CMB_CH6.Items3")});
            resources.ApplyResources(this.CMB_CH6, "CMB_CH6");
            this.CMB_CH6.Name = "CMB_CH6";
            // 
            // BUT_detch5
            // 
            resources.ApplyResources(this.BUT_detch5, "BUT_detch5");
            this.BUT_detch5.Name = "BUT_detch5";
            this.BUT_detch5.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.BUT_detch5.UseVisualStyleBackColor = true;
            // 
            // revCH5
            // 
            resources.ApplyResources(this.revCH5, "revCH5");
            this.revCH5.Name = "revCH5";
            this.revCH5.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Name = "label10";
            // 
            // expo_ch5
            // 
            this.expo_ch5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.expo_ch5, "expo_ch5");
            this.expo_ch5.Name = "expo_ch5";
            // 
            // ProgressBarCH5
            // 
            this.ProgressBarCH5.DrawLabel = true;
            resources.ApplyResources(this.ProgressBarCH5, "ProgressBarCH5");
            this.ProgressBarCH5.Label = null;
            this.ProgressBarCH5.Maximum = 2200;
            this.ProgressBarCH5.maxline = 0;
            this.ProgressBarCH5.Minimum = 800;
            this.ProgressBarCH5.minline = 0;
            this.ProgressBarCH5.Name = "ProgressBarCH5";
            this.ProgressBarCH5.Value = 800;
            // 
            // CMB_CH5
            // 
            this.CMB_CH5.FormattingEnabled = true;
            this.CMB_CH5.Items.AddRange(new object[] {
            resources.GetString("CMB_CH5.Items"),
            resources.GetString("CMB_CH5.Items1"),
            resources.GetString("CMB_CH5.Items2"),
            resources.GetString("CMB_CH5.Items3")});
            resources.ApplyResources(this.CMB_CH5, "CMB_CH5");
            this.CMB_CH5.Name = "CMB_CH5";
            // 
            // CHK_elevons
            // 
            resources.ApplyResources(this.CHK_elevons, "CHK_elevons");
            this.CHK_elevons.Name = "CHK_elevons";
            this.CHK_elevons.UseVisualStyleBackColor = true;
            // 
            // BUT_detch4
            // 
            resources.ApplyResources(this.BUT_detch4, "BUT_detch4");
            this.BUT_detch4.Name = "BUT_detch4";
            this.BUT_detch4.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.BUT_detch4.UseVisualStyleBackColor = true;
            // 
            // BUT_detch2
            // 
            resources.ApplyResources(this.BUT_detch2, "BUT_detch2");
            this.BUT_detch2.Name = "BUT_detch2";
            this.BUT_detch2.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.BUT_detch2.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Name = "label5";
            // 
            // BUT_enable
            // 
            this.BUT_enable.BGGradBot = System.Drawing.Color.DimGray;
            this.BUT_enable.BGGradTop = System.Drawing.Color.DarkGray;
            resources.ApplyResources(this.BUT_enable, "BUT_enable");
            this.BUT_enable.Name = "BUT_enable";
            this.BUT_enable.Outline = System.Drawing.Color.Black;
            this.BUT_enable.TextColor = System.Drawing.Color.Black;
            this.BUT_enable.TextColorNotEnabled = System.Drawing.Color.Black;
            this.BUT_enable.UseVisualStyleBackColor = true;
            this.BUT_enable.Click += new System.EventHandler(this.BUT_enable_Click);
            // 
            // BUT_save
            // 
            this.BUT_save.BGGradBot = System.Drawing.Color.DimGray;
            this.BUT_save.BGGradTop = System.Drawing.Color.DarkGray;
            resources.ApplyResources(this.BUT_save, "BUT_save");
            this.BUT_save.Name = "BUT_save";
            this.BUT_save.Outline = System.Drawing.Color.Black;
            this.BUT_save.TextColor = System.Drawing.Color.Black;
            this.BUT_save.TextColorNotEnabled = System.Drawing.Color.Black;
            this.BUT_save.UseVisualStyleBackColor = true;
            this.BUT_save.Click += new System.EventHandler(this.BUT_save_Click);
            // 
            // revCH4
            // 
            resources.ApplyResources(this.revCH4, "revCH4");
            this.revCH4.Name = "revCH4";
            this.revCH4.UseVisualStyleBackColor = true;
            // 
            // revCH2
            // 
            resources.ApplyResources(this.revCH2, "revCH2");
            this.revCH2.Name = "revCH2";
            this.revCH2.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // expo_ch4
            // 
            this.expo_ch4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.expo_ch4, "expo_ch4");
            this.expo_ch4.Name = "expo_ch4";
            // 
            // expo_ch3
            // 
            this.expo_ch3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.expo_ch3, "expo_ch3");
            this.expo_ch3.Name = "expo_ch3";
            // 
            // expo_ch2
            // 
            this.expo_ch2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.expo_ch2, "expo_ch2");
            this.expo_ch2.Name = "expo_ch2";
            // 
            // expo_ch1
            // 
            this.expo_ch1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.expo_ch1, "expo_ch1");
            this.expo_ch1.Name = "expo_ch1";
            // 
            // progressBarRudder
            // 
            this.progressBarRudder.DrawLabel = true;
            resources.ApplyResources(this.progressBarRudder, "progressBarRudder");
            this.progressBarRudder.Label = null;
            this.progressBarRudder.Maximum = 2200;
            this.progressBarRudder.maxline = 0;
            this.progressBarRudder.Minimum = 800;
            this.progressBarRudder.minline = 0;
            this.progressBarRudder.Name = "progressBarRudder";
            this.progressBarRudder.Value = 800;
            // 
            // progressBarThrottle
            // 
            this.progressBarThrottle.DrawLabel = true;
            resources.ApplyResources(this.progressBarThrottle, "progressBarThrottle");
            this.progressBarThrottle.Label = null;
            this.progressBarThrottle.Maximum = 2200;
            this.progressBarThrottle.maxline = 0;
            this.progressBarThrottle.Minimum = 800;
            this.progressBarThrottle.minline = 0;
            this.progressBarThrottle.Name = "progressBarThrottle";
            this.progressBarThrottle.Value = 800;
            // 
            // progressBarPith
            // 
            this.progressBarPith.DrawLabel = true;
            resources.ApplyResources(this.progressBarPith, "progressBarPith");
            this.progressBarPith.Label = null;
            this.progressBarPith.Maximum = 2200;
            this.progressBarPith.maxline = 0;
            this.progressBarPith.Minimum = 800;
            this.progressBarPith.minline = 0;
            this.progressBarPith.Name = "progressBarPith";
            this.progressBarPith.Value = 800;
            // 
            // progressBarRoll
            // 
            this.progressBarRoll.DrawLabel = true;
            resources.ApplyResources(this.progressBarRoll, "progressBarRoll");
            this.progressBarRoll.Label = null;
            this.progressBarRoll.Maximum = 2200;
            this.progressBarRoll.maxline = 0;
            this.progressBarRoll.Minimum = 800;
            this.progressBarRoll.minline = 0;
            this.progressBarRoll.Name = "progressBarRoll";
            this.progressBarRoll.Value = 800;
            // 
            // CMB_CH4
            // 
            this.CMB_CH4.FormattingEnabled = true;
            this.CMB_CH4.Items.AddRange(new object[] {
            resources.GetString("CMB_CH4.Items"),
            resources.GetString("CMB_CH4.Items1"),
            resources.GetString("CMB_CH4.Items2"),
            resources.GetString("CMB_CH4.Items3")});
            resources.ApplyResources(this.CMB_CH4, "CMB_CH4");
            this.CMB_CH4.Name = "CMB_CH4";
            // 
            // CMB_CH2
            // 
            this.CMB_CH2.FormattingEnabled = true;
            this.CMB_CH2.Items.AddRange(new object[] {
            resources.GetString("CMB_CH2.Items"),
            resources.GetString("CMB_CH2.Items1"),
            resources.GetString("CMB_CH2.Items2"),
            resources.GetString("CMB_CH2.Items3")});
            resources.ApplyResources(this.CMB_CH2, "CMB_CH2");
            this.CMB_CH2.Name = "CMB_CH2";
            // 
            // CMB_joysticks
            // 
            this.CMB_joysticks.FormattingEnabled = true;
            resources.ApplyResources(this.CMB_joysticks, "CMB_joysticks");
            this.CMB_joysticks.Name = "CMB_joysticks";
            this.CMB_joysticks.SelectedIndexChanged += new System.EventHandler(this.CMB_joysticks_SelectedIndexChanged);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // chk_manualcontrol
            // 
            resources.ApplyResources(this.chk_manualcontrol, "chk_manualcontrol");
            this.chk_manualcontrol.ForeColor = System.Drawing.Color.White;
            this.chk_manualcontrol.Name = "chk_manualcontrol";
            this.chk_manualcontrol.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Name = "label1";
            // 
            // checkBox1
            // 
            resources.ApplyResources(this.checkBox1, "checkBox1");
            this.checkBox1.ForeColor = System.Drawing.Color.White;
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Name = "label3";
            // 
            // label15
            // 
            resources.ApplyResources(this.label15, "label15");
            this.label15.ForeColor = System.Drawing.Color.White;
            this.label15.Name = "label15";
            // 
            // label16
            // 
            resources.ApplyResources(this.label16, "label16");
            this.label16.ForeColor = System.Drawing.Color.White;
            this.label16.Name = "label16";
            // 
            // label17
            // 
            resources.ApplyResources(this.label17, "label17");
            this.label17.ForeColor = System.Drawing.Color.White;
            this.label17.Name = "label17";
            // 
            // JoystickSetup
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
            this.Controls.Add(this.chk_manualcontrol);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.BUT_enable);
            this.Controls.Add(this.BUT_save);
            this.Controls.Add(this.CMB_joysticks);
            this.Name = "JoystickSetup";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.JoystickSetup_FormClosed);
            this.Load += new System.EventHandler(this.Joystick_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        #endregion
        private ComboBox AuxBtn;
        private ComboBox AnchUpBtn;
        private ComboBox AnchDown;
        private ComboBox EngStBtn;
        private ComboBox EngStpBtn;
        private CircularProgressBar.CircularProgressBar SteerCircBar;
        private CircularProgressBar.CircularProgressBar ThrottleCircBar;
        private Label label14;
        private MyButton BUT_detch8;
        private CheckBox revCH8;
        private Label label13;
        private TextBox expo_ch8;
        private HorizontalProgressBar ProgressBarCH8;
        private ComboBox CMB_CH8;
        private MyButton BUT_detch7;
        private CheckBox revCH7;
        private Label label12;
        private TextBox expo_ch7;
        private HorizontalProgressBar ProgressBarCH7;
        private ComboBox CMB_CH7;
        private MyButton BUT_detch6;
        private CheckBox revCH6;
        private Label label11;
        private TextBox expo_ch6;
        private HorizontalProgressBar ProgressBarCH6;
        private ComboBox CMB_CH6;
        private MyButton BUT_detch5;
        private CheckBox revCH5;
        private Label label10;
        private TextBox expo_ch5;
        private HorizontalProgressBar ProgressBarCH5;
        private ComboBox CMB_CH5;
        private CheckBox CHK_elevons;
        private MyButton BUT_detch4;
        private MyButton BUT_detch2;
        private Label label9;
        private Label label8;
        private Label label7;
        private Label label6;
        private Label label5;
        private MyButton BUT_enable;
        private MyButton BUT_save;
        private CheckBox revCH4;
        private CheckBox revCH2;
        private Label label4;
        private Label label2;
        private TextBox expo_ch4;
        private TextBox expo_ch3;
        private TextBox expo_ch2;
        private TextBox expo_ch1;
        private HorizontalProgressBar progressBarRudder;
        private HorizontalProgressBar progressBarThrottle;
        private HorizontalProgressBar progressBarPith;
        private HorizontalProgressBar progressBarRoll;
        private ComboBox CMB_CH4;
        private ComboBox CMB_CH2;
        private ComboBox CMB_joysticks;
        private Timer timer1;
        private CheckBox chk_manualcontrol;
        private Label label1;
        private CheckBox checkBox1;
        private Label label3;
        private Label label15;
        private Label label16;
        private Label label17;
    }
}