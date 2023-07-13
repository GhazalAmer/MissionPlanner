namespace MissionPlanner.GCSViews
{
    partial class SoftwareConfig
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SoftwareConfig));
            this.backstageView = new MissionPlanner.Controls.BackstageView.BackstageView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.myButton1 = new MissionPlanner.Controls.MyButton();
            this.myButton15 = new MissionPlanner.Controls.MyButton();
            this.myButton2 = new MissionPlanner.Controls.MyButton();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // backstageView
            // 
            resources.ApplyResources(this.backstageView, "backstageView");
            this.backstageView.HighlightColor1 = System.Drawing.SystemColors.Highlight;
            this.backstageView.HighlightColor2 = System.Drawing.SystemColors.MenuHighlight;
            this.backstageView.Name = "backstageView";
            this.backstageView.WidthMenu = 172;
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.myButton1);
            this.panel1.Controls.Add(this.myButton15);
            this.panel1.Name = "panel1";
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // myButton1
            // 
            this.myButton1.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.myButton1, "myButton1");
            this.myButton1.BGGradBot = System.Drawing.Color.Black;
            this.myButton1.BGGradTop = System.Drawing.Color.Black;
            this.myButton1.ForeColor = System.Drawing.Color.Black;
            this.myButton1.Name = "myButton1";
            this.myButton1.Outline = System.Drawing.Color.Black;
            this.myButton1.TextColor = System.Drawing.Color.White;
            this.myButton1.TextColorNotEnabled = System.Drawing.Color.Black;
            this.myButton1.UseVisualStyleBackColor = false;
            this.myButton1.Click += new System.EventHandler(this.myButton1_Click);
            // 
            // myButton15
            // 
            this.myButton15.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.myButton15, "myButton15");
            this.myButton15.BGGradBot = System.Drawing.Color.Black;
            this.myButton15.BGGradTop = System.Drawing.Color.Black;
            this.myButton15.ForeColor = System.Drawing.Color.White;
            this.myButton15.Name = "myButton15";
            this.myButton15.Outline = System.Drawing.Color.Black;
            this.myButton15.TextColor = System.Drawing.Color.White;
            this.myButton15.TextColorNotEnabled = System.Drawing.Color.Black;
            this.myButton15.UseVisualStyleBackColor = false;
            this.myButton15.Click += new System.EventHandler(this.myButton15_Click);
            // 
            // myButton2
            // 
            resources.ApplyResources(this.myButton2, "myButton2");
            this.myButton2.BackColor = System.Drawing.Color.Transparent;
            this.myButton2.BGGradBot = System.Drawing.Color.Black;
            this.myButton2.BGGradTop = System.Drawing.Color.Black;
            this.myButton2.ForeColor = System.Drawing.Color.Black;
            this.myButton2.Name = "myButton2";
            this.myButton2.Outline = System.Drawing.Color.Black;
            this.myButton2.TextColor = System.Drawing.Color.White;
            this.myButton2.TextColorNotEnabled = System.Drawing.Color.Black;
            this.myButton2.UseVisualStyleBackColor = false;
            this.myButton2.Click += new System.EventHandler(this.myButton2_Click);
            // 
            // SoftwareConfig
            // 
            this.Controls.Add(this.myButton2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.backstageView);
            this.Name = "SoftwareConfig";
            resources.ApplyResources(this, "$this");
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SoftwareConfig_FormClosing);
            this.Load += new System.EventHandler(this.SoftwareConfig_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.BackstageView.BackstageView backstageView;
        private System.Windows.Forms.Panel panel1;
        private Controls.MyButton myButton2;
        private Controls.MyButton myButton15;
        private Controls.MyButton myButton1;
    }
}
