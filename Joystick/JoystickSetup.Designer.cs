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
            this.chk_manualcontrol = new System.Windows.Forms.CheckBox();
            this.label14 = new System.Windows.Forms.Label();
            this.BUT_save = new MissionPlanner.Controls.MyButton();
            this.BUT_enable = new MissionPlanner.Controls.MyButton();
            this.CHK_elevons = new System.Windows.Forms.CheckBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.CMB_joysticks = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // chk_manualcontrol
            // 
            resources.ApplyResources(this.chk_manualcontrol, "chk_manualcontrol");
            this.chk_manualcontrol.Name = "chk_manualcontrol";
            this.chk_manualcontrol.UseVisualStyleBackColor = true;
            this.chk_manualcontrol.CheckedChanged += new System.EventHandler(this.chk_manualcontrol_CheckedChanged);
            // 
            // label14
            // 
            resources.ApplyResources(this.label14, "label14");
            this.label14.Name = "label14";
            // 
            // BUT_save
            // 
            resources.ApplyResources(this.BUT_save, "BUT_save");
            this.BUT_save.Name = "BUT_save";
            this.BUT_save.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.BUT_save.UseVisualStyleBackColor = true;
            this.BUT_save.Click += new System.EventHandler(this.BUT_save_Click);
            // 
            // BUT_enable
            // 
            resources.ApplyResources(this.BUT_enable, "BUT_enable");
            this.BUT_enable.Name = "BUT_enable";
            this.BUT_enable.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.BUT_enable.UseVisualStyleBackColor = true;
            this.BUT_enable.Click += new System.EventHandler(this.BUT_enable_Click);
            // 
            // CHK_elevons
            // 
            resources.ApplyResources(this.CHK_elevons, "CHK_elevons");
            this.CHK_elevons.Name = "CHK_elevons";
            this.CHK_elevons.UseVisualStyleBackColor = true;
            this.CHK_elevons.CheckedChanged += new System.EventHandler(this.CHK_elevons_CheckedChanged);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
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
            this.label5.Name = "label5";
            // 
            // CMB_joysticks
            // 
            this.CMB_joysticks.FormattingEnabled = true;
            resources.ApplyResources(this.CMB_joysticks, "CMB_joysticks");
            this.CMB_joysticks.Name = "CMB_joysticks";
            this.CMB_joysticks.SelectedIndexChanged += new System.EventHandler(this.CMB_joysticks_SelectedIndexChanged);
            this.CMB_joysticks.Click += new System.EventHandler(this.CMB_joysticks_Click);
            // 
            // JoystickSetup
            // 
            this.Controls.Add(this.chk_manualcontrol);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.CHK_elevons);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.BUT_enable);
            this.Controls.Add(this.BUT_save);
            this.Controls.Add(this.CMB_joysticks);
            resources.ApplyResources(this, "$this");
            this.Name = "JoystickSetup";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.JoystickSetup_FormClosed);
            this.Load += new System.EventHandler(this.Joystick_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        #endregion

        private CheckBox chk_manualcontrol;
        private Label label14;
        private MyButton BUT_save;
        private MyButton BUT_enable;
        private CheckBox CHK_elevons;
        private Timer timer1;
        private Label label9;
        private Label label8;
        private Label label7;
        private Label label6;
        private Label label5;
        private ComboBox CMB_joysticks;
    }
}