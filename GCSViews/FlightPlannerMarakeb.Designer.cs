namespace MissionPlanner.GCSViews
{
    partial class FlightPlannerMarakeb
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
            this.panelMap = new System.Windows.Forms.Panel();
            
            this.lbl_homedist = new System.Windows.Forms.Label();
            this.lbl_prevdist = new System.Windows.Forms.Label();
            this.trackBar1 = new MissionPlanner.Controls.MyTrackBar();
            this.label11 = new System.Windows.Forms.Label();
            this.lbl_distance = new System.Windows.Forms.Label();
            this.cmb_missiontype = new System.Windows.Forms.ComboBox();
            this.MainMap = new MissionPlanner.Controls.myGMAP();
            this.panelMap.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // panelMap
            // 
            this.panelMap.Controls.Add(this.lbl_homedist);
            this.panelMap.Controls.Add(this.lbl_prevdist);
            this.panelMap.Controls.Add(this.trackBar1);
            this.panelMap.Controls.Add(this.label11);
            this.panelMap.Controls.Add(this.lbl_distance);
            this.panelMap.Controls.Add(this.cmb_missiontype);
            this.panelMap.Controls.Add(this.MainMap);
            this.panelMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMap.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panelMap.Location = new System.Drawing.Point(0, 0);
            this.panelMap.MinimumSize = new System.Drawing.Size(27, 27);
            this.panelMap.Name = "panelMap";
            this.panelMap.Size = new System.Drawing.Size(1096, 589);
            this.panelMap.TabIndex = 52;
            this.panelMap.Text = "panel6";
            // 
            // lbl_homedist
            // 
            this.lbl_homedist.AutoSize = true;
            this.lbl_homedist.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lbl_homedist.Location = new System.Drawing.Point(3, 31);
            this.lbl_homedist.Name = "lbl_homedist";
            this.lbl_homedist.Size = new System.Drawing.Size(35, 13);
            this.lbl_homedist.TabIndex = 50;
            this.lbl_homedist.Text = "Home";
            // 
            // lbl_prevdist
            // 
            this.lbl_prevdist.AutoSize = true;
            this.lbl_prevdist.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lbl_prevdist.Location = new System.Drawing.Point(3, 18);
            this.lbl_prevdist.Name = "lbl_prevdist";
            this.lbl_prevdist.Size = new System.Drawing.Size(29, 13);
            this.lbl_prevdist.TabIndex = 49;
            this.lbl_prevdist.Text = "Prev";
            // 
            // trackBar1
            // 
            this.trackBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBar1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.trackBar1.LargeChange = 0.005F;
            this.trackBar1.Location = new System.Drawing.Point(1059, 21);
            this.trackBar1.Maximum = 24F;
            this.trackBar1.Minimum = 1F;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBar1.Size = new System.Drawing.Size(45, 562);
            this.trackBar1.SmallChange = 0.001F;
            this.trackBar1.TabIndex = 46;
            this.trackBar1.TickFrequency = 1F;
            this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.trackBar1.Value = 2F;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label11.AutoSize = true;
            this.label11.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label11.Location = new System.Drawing.Point(1062, 5);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(34, 13);
            this.label11.TabIndex = 47;
            this.label11.Text = "Zoom";
            // 
            // lbl_distance
            // 
            this.lbl_distance.AutoSize = true;
            this.lbl_distance.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lbl_distance.Location = new System.Drawing.Point(3, 5);
            this.lbl_distance.Name = "lbl_distance";
            this.lbl_distance.Size = new System.Drawing.Size(49, 13);
            this.lbl_distance.TabIndex = 48;
            this.lbl_distance.Text = "Distance";
            // 
            // cmb_missiontype
            // 
            this.cmb_missiontype.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmb_missiontype.FormattingEnabled = true;
            this.cmb_missiontype.Items.AddRange(new object[] {
            "Mission",
            "Fence",
            "Rally"});
            this.cmb_missiontype.Location = new System.Drawing.Point(919, 3);
            this.cmb_missiontype.Name = "cmb_missiontype";
            this.cmb_missiontype.Size = new System.Drawing.Size(121, 21);
            this.cmb_missiontype.TabIndex = 51;
            // 
            // MainMap
            // 
            this.MainMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainMap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(39)))), ((int)(((byte)(40)))));
            this.MainMap.Bearing = 0F;
            this.MainMap.CanDragMap = true;
            this.MainMap.Cursor = System.Windows.Forms.Cursors.Default;
            this.MainMap.EmptyTileColor = System.Drawing.Color.Gray;
            this.MainMap.GrayScaleMode = false;
            this.MainMap.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow;
            this.MainMap.HoldInvalidation = false;
            this.MainMap.LevelsKeepInMemmory = 5;
            this.MainMap.Location = new System.Drawing.Point(0, 0);
            this.MainMap.MarkersEnabled = true;
            this.MainMap.MaxZoom = 24;
            this.MainMap.MinZoom = 0;
            this.MainMap.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;
            this.MainMap.Name = "MainMap";
            this.MainMap.NegativeMode = false;
            this.MainMap.PolygonsEnabled = true;
            this.MainMap.RetryLoadTile = 0;
            this.MainMap.RoutesEnabled = false;
            this.MainMap.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Fractional;
            this.MainMap.SelectedAreaFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(65)))), ((int)(((byte)(105)))), ((int)(((byte)(225)))));
            this.MainMap.ShowTileGridLines = false;
            this.MainMap.Size = new System.Drawing.Size(1056, 582);
            this.MainMap.TabIndex = 45;
            this.MainMap.Zoom = 0D;
            this.MainMap.Load += new System.EventHandler(this.MainMap_Load);
            // 
            // FlightPlannerMarakeb
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelMap);
            this.Name = "FlightPlannerMarakeb";
            this.Size = new System.Drawing.Size(1096, 589);
            this.Load += new System.EventHandler(this.FlightPlannerMarakeb_Load);
            this.panelMap.ResumeLayout(false);
            this.panelMap.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Panel panelMap;
        public System.Windows.Forms.Label lbl_homedist;
        public System.Windows.Forms.Label lbl_prevdist;
        public Controls.MyTrackBar trackBar1;
        public System.Windows.Forms.Label label11;
        public System.Windows.Forms.Label lbl_distance;
        public System.Windows.Forms.ComboBox cmb_missiontype;
        public Controls.myGMAP MainMap;
    }
}
