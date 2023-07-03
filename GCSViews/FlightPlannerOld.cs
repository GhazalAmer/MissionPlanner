using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using DotSpatial.Data;
using DotSpatial.Projections;
using GeoUtility.GeoSystem;
using GeoUtility.GeoSystem.Base;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using Ionic.Zip;
using log4net;
using MissionPlanner.Controls;
using MissionPlanner.Controls.Waypoints;
using MissionPlanner.Maps;
using MissionPlanner.Properties;
using MissionPlanner.Utilities;
using MissionPlanner.Joystick;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using SharpKml.Base;
using SharpKml.Dom;
using Feature = SharpKml.Dom.Feature;
using ILog = log4net.ILog;
using Placemark = SharpKml.Dom.Placemark;
using Point = System.Drawing.Point;

namespace MissionPlanner.GCSViews
{
    public partial class FlightPlanner : MyUserControl, IDeactivate, IActivate
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        int selectedrow;
        public bool quickadd;
        bool isonline = true;
        bool sethome;
        bool threadrun;
        bool polygongridmode;
        double[,] GlobalWps;
        public Thread thisthread;
        Hashtable param = new Hashtable();
        bool splinemode;
        altmode currentaltmode = altmode.Relative;
        Thread data_update;
        bool wp_coord_change_flag = false;
        bool wp_speed_change_flag = false;
        bool change_wp_key = false;
        bool grid;
        bool new_polygon_flag = false;
        bool polygon_edit_flag = false;
        public static FlightPlanner instance;

        public bool autopan { get; set; }

        public List<PointLatLngAlt> pointlist = new List<PointLatLngAlt>(); // used to calc distance
        public List<PointLatLngAlt> fullpointlist = new List<PointLatLngAlt>();
        int format_flag = 0;
        public GMapRoute route = new GMapRoute("wp route");
        public GMapRoute tracks = new GMapRoute("track route");
        public GMapRoute tracks2 = new GMapRoute("track route2");
        public GMapRoute homeroute = new GMapRoute("home route");
        public GMapMarkerRect PolgonToolTipMarker;
        public static GMapOverlay MeasurePts;
        public static GMapRoute MeasureRoute;
        public static GMapPolygon new_polygon;
        public GMapMarkerRect markerjustleft;
        public List<PointLatLng> new_polygon_list = new List<PointLatLng>();
        public List<GMapPolygon> list_of_polys = new List<GMapPolygon>();
        public List<PointLatLngAlt> MainWpsGlobal = new List<PointLatLngAlt>();
        static public Object thisLock = new Object();
        private ComponentResourceManager rm = new ComponentResourceManager(typeof(FlightPlanner));

        private Dictionary<string, string[]> cmdParamNames = new Dictionary<string, string[]>();
        public static void ConnectionLost()
        {
            //FlightPlanner A = new FlightPlanner();
            //A.myButton1.Text = "Connection Lost";
            //CustomMessageBox.Show("Lost");
        }
        List<List<Locationwp>> history = new List<List<Locationwp>>();
        DateTime lastscreenupdate = DateTime.Now;
        object updateBindingSourcelock = new object();
        volatile int updateBindingSourcecount;

        public void updateBindingSource()
        {
            //  run at 25 hz.
            if (lastscreenupdate.AddMilliseconds(40) < DateTime.Now)
            {
                // this is an attempt to prevent an invoke queue on the binding update on slow machines
                if (updateBindingSourcecount > 0)
                    return;

                lock (updateBindingSourcelock)
                {
                    updateBindingSourcecount++;
                }

                // async
                BeginInvoke((MethodInvoker)delegate
                {
                    try
                    {
                        if (this.Visible)
                        {
                            //Console.Write("bindingSource1 ");
                            MainV2.comPort.MAV.cs.UpdateCurrentSettings(currentStateBindingSource);
                        }
                        lastscreenupdate = DateTime.Now;

                    }
                    catch { }
                    lock (updateBindingSourcelock)
                    {
                        updateBindingSourcecount--;
                    }
                });
            }
        }
        List<int> groupmarkers = new List<int>();
        private void mainloop()
        {
            Thread.Sleep(40);
            while (this.threadrun)
            {
                try
                { updateBindingSource();

                  /* if (MainV2.comPort.MAV.cs.mode.ToLower() == "manual")
                    { CustomMessageBox.Show("Hi"); }
                    if (MainV2.comPort.MAV.cs.mode.ToLower() == "hold")
                    { CustomMessageBox.Show("No"); }*/
                }
                catch { }
            }
        }

            private void Reverse_my_PTs()
        {

            DataGridViewRowCollection rows = Commands.Rows;
            //Commands.Rows.Clear();

            int count = rows.Count;

            quickadd = true;

            for (int a = count; a > 0; a--)
            {
                DataGridViewRow row = Commands.Rows[a - 1];
                Commands.Rows.Remove(row);
                Commands.Rows.Add(row);
            }

            quickadd = false;

            writeKML();
        }
        private void Refresh_my_PTs()
        {

            if (Commands.Rows.Count > 1)
            {
                double torad = 3.1415926535897932384626433832795 / 180.0;
                double pi = 3.1415926535897932384626433832795;
                double lat1, lat2, lon1, lon2, bear, distance;
                double R = 6371000; // metres
                int extrapts, newcounter, overallcounter = 0;
                int counter_of_points = 0;
                int number_of_main_pts = 0;
                for (int a = 0; a < Commands.Rows.Count; a++)
                {
                   // if (Convert.ToInt32(Commands.Rows[a].Cells[7].Value) != 10)
                    //{
                        number_of_main_pts = number_of_main_pts + 1;

                    //}
                }
                double[,] MainWps = new double[number_of_main_pts, 3];
                int mainptscount = 0;
                for (int a = 0; a < Commands.Rows.Count; a++)
                {
                    
                        MainWps[mainptscount, 0] = Convert.ToDouble(Commands.Rows[a].Cells[5].Value);
                        MainWps[mainptscount, 1] = Convert.ToDouble(Commands.Rows[a].Cells[6].Value);
                        MainWps[mainptscount, 2] = Convert.ToDouble(Commands.Rows[a].Cells[7].Value);
                        mainptscount = mainptscount + 1;

                        // public List<PointLatLngAlt> fullpointlist = new List<PointLatLngAlt>();

                    
                }

                counter_of_points = number_of_main_pts;
                quickadd = true;

                // mono fix
                Commands.CurrentCell = null;

                Commands.Rows.Clear();

                selectedrow = 0;
                quickadd = false;
                writeKML();

                for (int a = 1; a < number_of_main_pts - 0; a++)
                {
                    lat1 = MainWps[a - 1, 0] * torad;
                    lon1 = MainWps[a - 1, 1] * torad;
                    lat2 = MainWps[a, 0] * torad;
                    lon2 = MainWps[a, 1] * torad;
                    distance = Math.Acos(Math.Sin(lat1) * Math.Sin(lat2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1)) * 6371000;

                    if (distance == -30)
                    {
                        extrapts = Convert.ToInt32(Math.Floor(distance / 15));
                        counter_of_points = counter_of_points + extrapts;
                        //   CustomMessageBox.Show("Add " + extrapts + " WPs between WPs " + a + " & " + (a + 1)); 
                    }
                }
                // CustomMessageBox.Show("Total number of pts is " + counter_of_points);
                double[,] newWps = new double[counter_of_points, 3];
                newWps[0, 0] = MainWps[0, 0];
                newWps[0, 1] = MainWps[0, 1];
                newWps[0, 2] = 20;
                for (int a = 1; a < number_of_main_pts - 0; a++)
                {
                    lat1 = MainWps[a - 1, 0] * torad;
                    lon1 = MainWps[a - 1, 1] * torad;
                    lat2 = MainWps[a, 0] * torad;
                    lon2 = MainWps[a, 1] * torad;
                    bear = Math.Atan2(Math.Sin(lon2 - lon1) * Math.Cos(lat2), Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1)) / torad;
                    distance = Math.Acos(Math.Sin(lat1) * Math.Sin(lat2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1)) * 6371000;
                    extrapts = Convert.ToInt32(Math.Floor(distance / 15));
                    //
                    ////////////////////////////

                    ////////////////////////////
                    //
                    if (distance == -30)
                    {


                        for (newcounter = 0; newcounter < extrapts; newcounter++)
                        {

                            lat1 = MainWps[a - 1, 0];
                            lon1 = MainWps[a - 1, 1];
                            PointLatLngAlt firstpoint = new PointLatLngAlt(lat1, lon1, 0);
                            // CustomMessageBox.Show(bear.ToString());
                            PointLatLngAlt secondpoint = firstpoint.newpos(bear, (15 * (newcounter + 1)));
                            overallcounter = overallcounter + 1;
                            newWps[overallcounter, 0] = secondpoint.Lat;
                            newWps[overallcounter, 1] = secondpoint.Lng;
                            newWps[overallcounter, 2] = 10;

                        }
                    }
                    overallcounter = overallcounter + 1;
                    newWps[overallcounter, 0] = MainWps[a, 0];
                    newWps[overallcounter, 1] = MainWps[a, 1];
                    newWps[overallcounter, 2] = 20;
                }
                GlobalWps = newWps;
                for (int a = 0; a < counter_of_points; a++)
                {


                    AddWPToMap(newWps[a, 0], newWps[a, 1], Convert.ToInt32(newWps[a, 2]));


                }
            }
        }
        public enum altmode
        {
            Relative = MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT,
            Absolute = MAVLink.MAV_FRAME.GLOBAL,
            Terrain = MAVLink.MAV_FRAME.GLOBAL_TERRAIN_ALT
        }

        private void poieditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentGMapMarker == null)
                return;

            POI.POIEdit(CurrentGMapMarker.Position);
        }

        private void poideleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentGMapMarker == null)
                return;

            POI.POIDelete(CurrentGMapMarker.Position);
            
        }

        private void poiaddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            POI.POIAdd(MouseDownStart);
        }

        /// <summary>
        /// used to adjust existing point in the datagrid including "H"
        /// </summary>
        /// <param name="pointno"></param>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="alt"></param>
        public void callMeDrag(string pointno, double lat, double lng, int alt)
        {
            if (pointno == "")
            {
                return;
            }

            // dragging a WP
            if (pointno == "H")
            {
                if (isonline && CHK_verifyheight.Checked)
                {
                    TXT_homealt.Text = getGEAlt(lat, lng).ToString();
                }
                TXT_homelat.Text = lat.ToString();
                TXT_homelng.Text = lng.ToString();
                return;
            }

            if (pointno == "Tracker Home")
            {
                MainV2.comPort.MAV.cs.TrackerLocation = new PointLatLngAlt(lat, lng, alt, "");
                return;
            }

            try
            {
                selectedrow = int.Parse(pointno) - 1;
                Commands.CurrentCell = Commands[1, selectedrow];
            }
            catch
            {
                return;
            }

            setfromMap(lat, lng, alt);
            //Refresh_my_PTs();
            double az1, az2;
            az1 = 0;
            int newrow = selectedrow - 1;
            int safe = 1;
            if (selectedrow > 1)
            {

                while (newrow >= 0 )
                {
                    az1 = Convert.ToDouble(Commands.Rows[newrow].Cells[13].Value);
                    newrow = newrow - 1;
                }
                az2 = Convert.ToDouble(Commands.Rows[selectedrow].Cells[13].Value);
                az1 = Math.Abs(az2 - az1);
                if (az1 > 180) { az1 = Math.Abs(az1 - 360); }
                // CustomMessageBox.Show(az1.ToString());
                if (az1 > 1000) { CustomMessageBox.Show("WARNING! Heading change at WP " + (selectedrow) + " exceeds 65 degrees.\n Please adjust before proceeding."); safe = 0; }
            }
            WPNumberUpdate();
        }
        /// <summary>
        /// Actualy Sets the values into the datagrid and verifys height if turned on
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="alt"></param>
        public void setfromMap(double lat, double lng, int alt, double p1 = 0)
        {
            if (selectedrow > Commands.RowCount)
            {
                CustomMessageBox.Show("Invalid coord, How did you do this?");
                return;
            }


            // add history
            history.Add(GetCommandList());

            // remove more than 20 revisions
            if (history.Count > 20)
            {
                history.RemoveRange(0, history.Count - 20);
            }

            DataGridViewTextBoxCell cell;
            if (Commands.Columns[Lat.Index].HeaderText.Equals(cmdParamNames["WAYPOINT"][4]/*"Lat"*/))
            {
                cell = Commands.Rows[selectedrow].Cells[Lat.Index] as DataGridViewTextBoxCell;
                cell.Value = lat.ToString("0.0000000");
                cell.DataGridView.EndEdit();
            }
            if (Commands.Columns[Lon.Index].HeaderText.Equals(cmdParamNames["WAYPOINT"][5]/*"Long"*/))
            {
                cell = Commands.Rows[selectedrow].Cells[Lon.Index] as DataGridViewTextBoxCell;
                cell.Value = lng.ToString("0.0000000");
                cell.DataGridView.EndEdit();
            }
            if (alt != -1 && Commands.Columns[Alt.Index].HeaderText.Equals(cmdParamNames["WAYPOINT"][6]/*"Alt"*/))
            {
                cell = Commands.Rows[selectedrow].Cells[Alt.Index] as DataGridViewTextBoxCell;

                {
                    double result;
                    bool pass = double.TryParse(TXT_homealt.Text, out result);

                    if (pass == false)
                    {
                        CustomMessageBox.Show("You must have a home altitude");
                        string homealt = "100";
                        if (DialogResult.Cancel == InputBox.Show("Home Alt", "Home Altitude", ref homealt))
                            return;
                        TXT_homealt.Text = homealt;
                    }
                    int results1;
                    if (!int.TryParse(TXT_DefaultAlt.Text, out results1))
                    {
                        CustomMessageBox.Show("Your default alt is not valid");
                        return;
                    }

                    if (results1 == 0)
                    {
                        string defalt = "100";
                        if (DialogResult.Cancel == InputBox.Show("Default Alt", "Default Altitude", ref defalt))
                            return;
                        TXT_DefaultAlt.Text = defalt;
                    }
                }

                cell.Value = TXT_DefaultAlt.Text;

                float ans;
                if (float.TryParse(cell.Value.ToString(), out ans))
                {
                    ans = (int)ans;
                    if (alt != 0) // use passed in value;
                        cell.Value = alt.ToString();
                    if (ans == 0) // default
                        cell.Value = 50;
                    if (ans == 0 && (MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.ArduCopter2))
                        cell.Value = 15;

                    // not online and verify alt via srtm
                    if (CHK_verifyheight.Checked) // use srtm data
                    {
                        // is absolute but no verify
                        if ((altmode)CMB_altmode.SelectedValue == altmode.Absolute)
                        {
                            //abs
                            cell.Value = ((srtm.getAltitude(lat, lng).alt) * CurrentState.multiplierdist + int.Parse(TXT_DefaultAlt.Text)).ToString();
                        }
                        else
                        {
                            //relative and verify
                            cell.Value = ((int)(srtm.getAltitude(lat, lng).alt) * CurrentState.multiplierdist + int.Parse(TXT_DefaultAlt.Text) - (int)srtm.getAltitude(MainV2.comPort.MAV.cs.HomeLocation.Lat, MainV2.comPort.MAV.cs.HomeLocation.Lng).alt * CurrentState.multiplierdist).ToString();

                        }
                    }

                    cell.DataGridView.EndEdit();
                }
                else
                {
                    CustomMessageBox.Show("Invalid Home or wp Alt");
                    cell.Style.BackColor = Color.Red;
                }

            }

            // Add more for other params
            if (Commands.Columns[Param1.Index].HeaderText.Equals(cmdParamNames["WAYPOINT"][1]/*"Delay"*/))
            {
                cell = Commands.Rows[selectedrow].Cells[Param1.Index] as DataGridViewTextBoxCell;
                cell.Value = p1;
                cell.DataGridView.EndEdit();
            }

            writeKML();
            Commands.EndEdit();
        }

        PointLatLngAlt mouseposdisplay = new PointLatLngAlt(0, 0);

        /// <summary>
        /// Used for current mouse position
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="alt"></param>
        public void SetMouseDisplay(double lat, double lng, int alt)
        {
            mouseposdisplay.Lat = lat;
            mouseposdisplay.Lng = lng;
            mouseposdisplay.Alt = alt;

            coords1.Lat = mouseposdisplay.Lat;
            coords1.Lng = mouseposdisplay.Lng;
            coords1.Alt = srtm.getAltitude(mouseposdisplay.Lat, mouseposdisplay.Lng, Joysticknutton.Zoom).alt;

            try
            {
                PointLatLng last;

                if (pointlist[pointlist.Count - 1] == null)
                    return;

                last = pointlist[pointlist.Count - 1];

                double lastdist = Joysticknutton.MapProvider.Projection.GetDistance(last, currentMarker.Position);

                double lastbearing = 0;

                if (pointlist.Count > 0)
                {
                    lastbearing = Joysticknutton.MapProvider.Projection.GetBearing(last, currentMarker.Position);
                }

                lbl_prevdist.Text = rm.GetString("lbl_prevdist.Text") + ": " + FormatDistance(lastdist, true) + " AZ: " + lastbearing.ToString("0");

                // 0 is home
                if (pointlist[0] != null)
                {
                    double homedist = Joysticknutton.MapProvider.Projection.GetDistance(currentMarker.Position, pointlist[0]);

                    lbl_homedist.Text = rm.GetString("lbl_homedist.Text") + ": " + FormatDistance(homedist, true);
                }
            }
            catch { }
        }

        /// <summary>
        /// Used to create a new WP
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="alt"></param>
        public void AddWPToMap(double lat, double lng, int alt)
        {
            if (polygongridmode)
            {
                addPolygonPointToolStripMenuItem_Click(null, null);
                return;
            }

            if (sethome)
            {
                sethome = false;
                callMeDrag("H", lat, lng, alt);
                return;
            }
            // creating a WP

            selectedrow = Commands.Rows.Add();

            if (splinemode)
            {
                Commands.Rows[selectedrow].Cells[Command.Index].Value = MAVLink.MAV_CMD.SPLINE_WAYPOINT.ToString();
                ChangeColumnHeader(MAVLink.MAV_CMD.SPLINE_WAYPOINT.ToString());
            }
            else
            {
                Commands.Rows[selectedrow].Cells[Command.Index].Value = MAVLink.MAV_CMD.WAYPOINT.ToString();

                ChangeColumnHeader(MAVLink.MAV_CMD.WAYPOINT.ToString());
            }

            setfromMap(lat, lng, alt);
            double az1, az2;
            if (selectedrow > 1)
            {
                az1 = Convert.ToDouble(Commands.Rows[selectedrow - 1].Cells[13].Value);
                az2 = Convert.ToDouble(Commands.Rows[selectedrow].Cells[13].Value);
                az1 = Math.Abs((az2 - az1));
                if (az1 > 180) { az1 = Math.Abs(az1 - 360); }
                // CustomMessageBox.Show(az1.ToString());
                if (az1 > 1000) { CustomMessageBox.Show("WARNING! Heading change at WP " + (selectedrow) + " exceeds 65 degrees.\n Please adjust before proceeding."); }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // undo
            if (keyData == (Keys.Control | Keys.Z))
            {
                if (history.Count > 0)
                {
                    int no = history.Count - 1;
                    var pop = history[no];
                    history.RemoveAt(no);
                    WPtoScreen(pop);
                }
                return true;
            }

            // open wp file
            if (keyData == (Keys.Control | Keys.O))
            {
                loadWPFileToolStripMenuItem_Click(null, null);
                return true;
            }

            // save wp file
            if (keyData == (Keys.Control | Keys.S))
            {
                saveWPFileToolStripMenuItem_Click(null, null);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public FlightPlanner()
        {

            instance = this;

            InitializeComponent();

            // config map             
            Joysticknutton.CacheLocation = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + "gmapcache" + Path.DirectorySeparatorChar;
            Joysticknutton.MapProvider = OpenStreetMapProvider.Instance; //(GMapProvider)BingSatelliteMapProvider.Instance; //OpenStreetMapProvider.Instance; //BingSatelliteMapProvider.Instance;//OpenStreetMapProvider.Instance;//GMapProviders.OpenStreetMapProvider; //GoogleSatelliteMapProvider.Instance;//

            // map events
            Joysticknutton.OnPositionChanged += MainMap_OnCurrentPositionChanged;
            Joysticknutton.OnTileLoadStart += MainMap_OnTileLoadStart;
            Joysticknutton.OnTileLoadComplete += MainMap_OnTileLoadComplete;
            Joysticknutton.OnMarkerClick += MainMap_OnMarkerClick;
            
            Joysticknutton.OnMapZoomChanged += MainMap_OnMapZoomChanged;
            Joysticknutton.OnMapTypeChanged += MainMap_OnMapTypeChanged;
            Joysticknutton.MouseMove += MainMap_MouseMove;
            Joysticknutton.MouseDown += MainMap_MouseDown;
            Joysticknutton.MouseUp += MainMap_MouseUp;
            Joysticknutton.OnMarkerEnter += MainMap_OnMarkerEnter;
            Joysticknutton.OnMarkerLeave += MainMap_OnMarkerLeave;
            Joysticknutton.OnPolygonEnter += MainMap_ShowPolygonName;
            Joysticknutton.OnPolygonLeave += MainMap_HidePolygonName;
            Joysticknutton.OnPolygonClick += MainMap_PolygonSelected;


            Joysticknutton.MapScaleInfoEnabled = false;
            Joysticknutton.ScalePen = new Pen(Color.Red);

            Joysticknutton.DisableFocusOnMouseEnter = true;

            Joysticknutton.ForceDoubleBuffer = false;

            //WebRequest.DefaultWebProxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

            // get map type
            comboBoxMapType.ValueMember = "Name";
            comboBoxMapType.DataSource = GMapProviders.List.ToArray();
            comboBoxMapType.SelectedItem = Joysticknutton.MapProvider;

            comboBoxMapType.SelectedValueChanged += comboBoxMapType_SelectedValueChanged;

            Joysticknutton.RoutesEnabled = true;

            //MainMap.MaxZoom = 18;

            // get zoom  
            Joysticknutton.MinZoom = 0;
            Joysticknutton.MaxZoom = 24;

            // draw this layer first
            kmlpolygonsoverlay = new GMapOverlay("kmlpolygons");
            Joysticknutton.Overlays.Add(kmlpolygonsoverlay);

            geofenceoverlay = new GMapOverlay("geofence");
            Joysticknutton.Overlays.Add(geofenceoverlay);

            rallypointoverlay = new GMapOverlay("rallypoints");
            Joysticknutton.Overlays.Add(rallypointoverlay);

            routesoverlay = new GMapOverlay("routes");
            Joysticknutton.Overlays.Add(routesoverlay);



            polygonsoverlay = new GMapOverlay("polygons");
            Joysticknutton.Overlays.Add(polygonsoverlay);

            trackoverlay = new GMapOverlay("tracking routes");
            Joysticknutton.Overlays.Add(trackoverlay);

            trackoverlay2 = new GMapOverlay("tracking routes2");
            Joysticknutton.Overlays.Add(trackoverlay2);

            airportsoverlay = new GMapOverlay("airports");
            Joysticknutton.Overlays.Add(airportsoverlay);

            objectsoverlay = new GMapOverlay("objects");
            Joysticknutton.Overlays.Add(objectsoverlay);

            drawnpolygonsoverlay = new GMapOverlay("drawnpolygons");
            Joysticknutton.Overlays.Add(drawnpolygonsoverlay);
            //drawnpolygonsoverlay.Markers.Add(PolgonToolTipMarker);

            currentwp = new GMapOverlay("currentwp");
            Joysticknutton.Overlays.Add(currentwp);
            MeasurePts = new GMapOverlay("MeasurePts");
            Joysticknutton.Overlays.Add(MeasurePts);

            MeasureRoute = new GMapRoute("MeasureRoute");

            MeasureRoute.Stroke = new Pen(Color.Orange, 3);
            MeasureRoute.Stroke.DashStyle = DashStyle.Custom;
            Joysticknutton.Overlays.Add(poioverlay);

            top = new GMapOverlay("top");
            //MainMap.Overlays.Add(top);

            objectsoverlay.Markers.Clear();

            // set current marker
            currentMarker = new GMarkerGoogle(Joysticknutton.Position, GMarkerGoogleType.red);
            //top.Markers.Add(currentMarker);

            // map center
            center = new GMarkerGoogle(Joysticknutton.Position, GMarkerGoogleType.none);
            top.Markers.Add(center);

            Joysticknutton.Zoom = 3;

            CMB_altmode.DisplayMember = "Value";
            CMB_altmode.ValueMember = "Key";
            CMB_altmode.DataSource = EnumTranslator.EnumToList<altmode>();

            //set default
            CMB_altmode.SelectedItem = altmode.Relative;

            RegeneratePolygon();

            if (MainV2.getConfig("MapType") != "")
            {
                try
                {
                    var index = GMapProviders.List.FindIndex(x => (x.Name == MainV2.getConfig("MapType")));

                    if (index != -1)
                        comboBoxMapType.SelectedIndex = index;
                }
                catch { }
            }

            updateCMDParams();

            Up.Image = Resources.up;
            Down.Image = Resources.down;

            // hide the map to prevent redraws when its loaded
            panelMap.Visible = false;
        }

        void updateCMDParams()
        {
            cmdParamNames = readCMDXML();

            List<string> cmds = new List<string>();

            foreach (string item in cmdParamNames.Keys)
            {
                cmds.Add(item);
            }

            cmds.Add("UNKNOWN");

            Command.DataSource = cmds;
        }

        Dictionary<string, string[]> readCMDXML()
        {
            Dictionary<string, string[]> cmd = new Dictionary<string, string[]>();

            // do lang stuff here

            string file = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + "mavcmd.xml";

            if (!File.Exists(file))
            {
                CustomMessageBox.Show("Missing mavcmd.xml file");
                return cmd;
            }

            using (XmlReader reader = XmlReader.Create(file))
            {
                reader.Read();
                reader.ReadStartElement("CMD");
                if (MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.ArduPlane || MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.Ateryx)
                {
                    reader.ReadToFollowing("APM");
                }
                else if (MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.ArduRover)
                {
                    reader.ReadToFollowing("APRover");
                }
                else
                {
                    reader.ReadToFollowing("AC2");
                }

                XmlReader inner = reader.ReadSubtree();

                inner.Read();

                inner.MoveToElement();

                inner.Read();

                while (inner.Read())
                {
                    inner.MoveToElement();
                    if (inner.IsStartElement())
                    {
                        string cmdname = inner.Name;
                        string[] cmdarray = new string[7];
                        int b = 0;

                        XmlReader inner2 = inner.ReadSubtree();

                        inner2.Read();

                        while (inner2.Read())
                        {
                            inner2.MoveToElement();
                            if (inner2.IsStartElement())
                            {
                                cmdarray[b] = inner2.ReadString();
                                b++;
                            }
                        }

                        cmd[cmdname] = cmdarray;
                    }
                }
            }

            return cmd;
        }

        void Commands_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            log.Info(e.Exception + " " + e.Context + " col " + e.ColumnIndex);
            e.Cancel = false;
            e.ThrowException = false;
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Adds a new row to the datagrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BUT_Add_Click(object sender, EventArgs e)
        {
            if (Commands.CurrentRow == null)
            {
                selectedrow = 0;
            }
            else
            {
                selectedrow = Commands.CurrentRow.Index;
            }

            if (Commands.RowCount <= 1)
            {
                selectedrow = Commands.Rows.Add();
            }
            else
            {
                if (Commands.RowCount == selectedrow + 1)
                {
                    DataGridViewRow temp = Commands.Rows[selectedrow];
                    selectedrow = Commands.Rows.Add();
                }
                else
                {
                    Commands.Rows.Insert(selectedrow + 1, 1);
                }
            }
            writeKML();
        }

        private void FlightPlanner_Load(object sender, EventArgs e)
        {
            
            quickadd = true;

            config(false);

            quickadd = false;

            POI.POIModified += POI_POIModified;

            if (MainV2.config["WMSserver"] != null)
                WMSProvider.CustomWMSURL = MainV2.config["WMSserver"].ToString();

            trackBar1.Value = (int)Joysticknutton.Zoom;

            // check for net and set offline if needed
            try
            {
                IPAddress[] addresslist = Dns.GetHostAddresses("www.google.com");
            }
            catch (Exception)
            { // here if dns failed
                isonline = false;
            }

            // setup geofence
            List<PointLatLng> polygonPoints = new List<PointLatLng>();
            geofencepolygon = new GMapPolygon(polygonPoints, "geofence");
            geofencepolygon.Stroke = new Pen(Color.Pink, 5);
            geofencepolygon.Fill = Brushes.Transparent;

            //setup drawnpolgon
            List<PointLatLng> polygonPoints2 = new List<PointLatLng>();
            drawnpolygon = new GMapPolygon(polygonPoints2, "drawnpoly");
            drawnpolygon.Stroke = new Pen(Color.Red, 2);
            drawnpolygon.Fill = Brushes.Transparent;
            

            updateCMDParams();

            panelMap.Visible = false;

            // mono
            panelMap.Dock = DockStyle.None;
            panelMap.Dock = DockStyle.Fill;
            panelMap_Resize(null, null);

            //set home
            try
            {
                if (TXT_homelat.Text != "")
                {
                    Joysticknutton.Position = new PointLatLng(double.Parse(TXT_homelat.Text), double.Parse(TXT_homelng.Text));
                    Joysticknutton.Zoom = 16;
                }

            }
            catch (Exception) { }

            panelMap.Refresh();

            panelMap.Visible = true;

            writeKML();

            // switch the action and wp table
            if (MainV2.getConfig("FP_docking") == "Bottom")
            {
                switchDockingToolStripMenuItem_Click(null, null);
            }

            timer1.Start();
            /////////////////////////////////////////////////
            MissionPlanner.Swarm.FormationControl objForm = new MissionPlanner.Swarm.FormationControl();
            objForm.TopLevel = false;
            SwarmPanel.Controls.Add(objForm);
            objForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            objForm.Dock = DockStyle.Fill;
            objForm.Show();
            //////////////////////////////////////////////////
            /////////////////////////////////////////////////
            Form joy = new JoystickSetup();
            ThemeManager.ApplyThemeTo(joy);
            joy.Show();
            joy.TopLevel = false;
            Joystick_Panel.Controls.Add(joy);
            joy.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            joy.Dock = DockStyle.Fill;
            joy.Show();
            //////////////////////////////////////////////////
            ////////////////////////////////////////////////////
            Form fllowmeform = new FollowMe();
            ThemeManager.ApplyThemeTo(fllowmeform);
            fllowmeform.Show();
            fllowmeform.TopLevel = false;
            FollowMePanel.Controls.Add(fllowmeform);
            fllowmeform.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            fllowmeform.Dock = DockStyle.Fill;
            fllowmeform.Show();
            //////////////////////////////////////////////////
            thisthread = new Thread(this.mainloop);
            thisthread.Name = "FP Mainloop";
            thisthread.IsBackground = true;
            this.threadrun = true;
            thisthread.Start();
            thisthread.Priority = ThreadPriority.Normal;
            ////////////
            ///
            MainV2.PolygonsOverlay = new GMapOverlay("MainMapPolys");
        }


        void POI_POIModified(object sender, EventArgs e)
        {
            POI.UpdateOverlay(poioverlay);
        }

        void parser_ElementAdded(object sender, ElementEventArgs e)
        {
            processKML(e.Element);
        }

        private void processKML(Element Element)
        {
            try
            {
                //  log.Info(Element.ToString() + " " + Element.Parent);
            }
            catch { }

            Document doc = Element as Document;
            Placemark pm = Element as Placemark;
            Folder folder = Element as Folder;
            Polygon polygon = Element as Polygon;
            LineString ls = Element as LineString;

            if (doc != null)
            {
                foreach (var feat in doc.Features)
                {
                    //Console.WriteLine("feat " + feat.GetType());
                    //processKML((Element)feat);
                }
            }
            else
                if (folder != null)
                {
                    foreach (Feature feat in folder.Features)
                    {
                        //Console.WriteLine("feat "+feat.GetType());
                        //processKML(feat);
                    }
                }
                else if (pm != null)
                {

                }
                else if (polygon != null)
                {
                    GMapPolygon kmlpolygon = new GMapPolygon(new List<PointLatLng>(), "kmlpolygon");

                    kmlpolygon.Stroke.Color = Color.Purple;

                    foreach (var loc in polygon.OuterBoundary.LinearRing.Coordinates)
                    {
                        kmlpolygon.Points.Add(new PointLatLng(loc.Latitude, loc.Longitude));
                    }

                    kmlpolygonsoverlay.Polygons.Add(kmlpolygon);
                }
                else if (ls != null)
                {
                    GMapRoute kmlroute = new GMapRoute(new List<PointLatLng>(), "kmlroute");

                    kmlroute.Stroke.Color = Color.Purple;

                    foreach (var loc in ls.Coordinates)
                    {
                        kmlroute.Points.Add(new PointLatLng(loc.Latitude, loc.Longitude));
                    }

                    kmlpolygonsoverlay.Routes.Add(kmlroute);
                }
        }

        private void ChangeColumnHeader(string command)
        {
            try
            {
                if (cmdParamNames.ContainsKey(command))
                    for (int i = 1; i <= 7; i++)
                        Commands.Columns[i].HeaderText = cmdParamNames[command][i - 1];
                else
                    for (int i = 1; i <= 7; i++)
                        Commands.Columns[i].HeaderText = "setme";
            }
            catch (Exception ex) { CustomMessageBox.Show(ex.ToString()); }
        }

        /// <summary>
        /// Used to update column headers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Commands_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (quickadd)
                return;
            try
            {
                selectedrow = e.RowIndex;
                string option = Commands[Command.Index, selectedrow].EditedFormattedValue.ToString();
                string cmd;
                try
                {
                    cmd = Commands[Command.Index, selectedrow].Value.ToString();
                }
                catch { cmd = option; }
                //Console.WriteLine("editformat " + option + " value " + cmd);
                ChangeColumnHeader(cmd);

                setgradanddistandaz();

                if (cmd == "WAYPOINT")
                {

                }

                //  writeKML();
            }
            catch (Exception ex) { CustomMessageBox.Show(ex.ToString()); }
        }

        private void Commands_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            for (int i = 0; i < Commands.ColumnCount; i++)
            {
                DataGridViewCell tcell = Commands.Rows[e.RowIndex].Cells[i];
                if (tcell.GetType() == typeof(DataGridViewTextBoxCell))
                {
                    if (tcell.Value == null)
                        tcell.Value = "0";
                }
            }

            DataGridViewComboBoxCell cell = Commands.Rows[e.RowIndex].Cells[Command.Index] as DataGridViewComboBoxCell;
            if (cell.Value == null)
            {
                cell.Value = "WAYPOINT";
                cell.DropDownWidth = 200;
                Commands.Rows[e.RowIndex].Cells[Delete.Index].Value = "X";
                if (!quickadd)
                {
                    Commands_RowEnter(sender, new DataGridViewCellEventArgs(0, e.RowIndex - 0)); // do header labels
                    Commands_RowValidating(sender, new DataGridViewCellCancelEventArgs(0, e.RowIndex)); // do default values
                }
            }

            if (quickadd)
                return;

            try
            {
                Commands.CurrentCell = Commands.Rows[e.RowIndex].Cells[0];

                if (Commands.Rows.Count > 1)
                {

                    if (Commands.Rows[e.RowIndex - 1].Cells[Command.Index].Value.ToString() == "WAYPOINT")
                    {
                        Commands.Rows[e.RowIndex].Selected = true; // highlight row
                    }
                    else
                    {
                        Commands.CurrentCell = Commands[1, e.RowIndex - 1];
                        //Commands_RowEnter(sender, new DataGridViewCellEventArgs(0, e.RowIndex-1));
                    }
                }
            }
            catch (Exception) { }
            // Commands.EndEdit();
        }
        private void Commands_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            selectedrow = e.RowIndex;
            Commands_RowEnter(sender, new DataGridViewCellEventArgs(0, e.RowIndex - 0)); // do header labels - encure we dont 0 out valid colums
            int cols = Commands.Columns.Count;
            for (int a = 1; a < cols; a++)
            {
                DataGridViewTextBoxCell cell;
                cell = Commands.Rows[selectedrow].Cells[a] as DataGridViewTextBoxCell;

                if (Commands.Columns[a].HeaderText.Equals("") && cell != null && cell.Value == null)
                {
                    cell.Value = "0";
                }
                else
                {
                    if (cell != null && (cell.Value == null || cell.Value.ToString() == ""))
                    {
                        cell.Value = "?";
                    }
                }
            }
        }

        /// <summary>
        /// used to add a marker to the map display
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="lng"></param>
        /// <param name="lat"></param>
        /// <param name="alt"></param>
        private void addpolygonmarker(string tag, double lng, double lat, int alt, Color? color)
        {
            try
            {
                PointLatLng point = new PointLatLng(lat, lng);

                GMapMarkerWP m = new GMapMarkerWP(point, tag);
                //m.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                //m.ToolTipText = "Alt: " + alt.ToString("0");
                m.Tag = tag;

                try
                {
                    // preselect groupmarker
                    if (groupmarkers.Contains(int.Parse(tag)))
                        m.selected = true;
                }
                catch { }

                //MissionPlanner.GMapMarkerRectWPRad mBorders = new MissionPlanner.GMapMarkerRectWPRad(point, (int)float.Parse(TXT_WPRad.Text), MainMap);
                GMapMarkerRect mBorders = new GMapMarkerRect(point);
                {
                    mBorders.InnerMarker = m;
                    mBorders.Tag = tag;
                    mBorders.wprad = (int)(float.Parse(TXT_WPRad.Text) / CurrentState.multiplierdist);
                    if (color.HasValue)
                    {
                        mBorders.Color = color.Value;
                    }
                }
               // if (alt != 10)
                //{
                    objectsoverlay.Markers.Add(m);
                    objectsoverlay.Markers.Add(mBorders);
               
               // }
            }
            catch (Exception) { }
        }

        private void addcurrwpmarker(string tag, double lng, double lat, int alt, Color? color)
        {
            try
            {
                PointLatLng point = new PointLatLng(lat, lng);

                GMapMarkerWP m = new GMapMarkerWP(point, tag);
                //m.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                //m.ToolTipText = "Alt: " + alt.ToString("0");
                m.Tag = tag;


                //MissionPlanner.GMapMarkerRectWPRad mBorders = new MissionPlanner.GMapMarkerRectWPRad(point, (int)float.Parse(TXT_WPRad.Text), MainMap);
                GMapMarkerRect mBorders = new GMapMarkerRect(point);
                {
                    mBorders.InnerMarker = m;
                    mBorders.Tag = tag;
                    mBorders.wprad = (int)(float.Parse(TXT_WPRad.Text) / CurrentState.multiplierdist);
                    if (color.HasValue)
                    {
                        mBorders.Color = color.Value;
                    }
                }
                // if (alt != 10)
                //{
                currentwp.Markers.Add(m);
                currentwp.Markers.Add(mBorders);
                // }
            }
            catch (Exception) { }
        }

        private void addpolygonmarkergrid(string tag, double lng, double lat, int alt)
        {
            try
            {

                PointLatLng point = new PointLatLng(lat, lng);
                GMarkerGoogle m = new GMarkerGoogle(point, GMarkerGoogleType.red);
                m.ToolTipMode = MarkerTooltipMode.Never;
                m.ToolTipText = "grid" + tag;
                m.Tag = "grid" + tag;

                //MissionPlanner.GMapMarkerRectWPRad mBorders = new MissionPlanner.GMapMarkerRectWPRad(point, (int)float.Parse(TXT_WPRad.Text), MainMap);
                GMapMarkerRect mBorders = new GMapMarkerRect(point);
                {
                    mBorders.InnerMarker = m;
                }
                //if (alt != 10)
               // {


                    drawnpolygonsoverlay.Markers.Add(m);
                    drawnpolygonsoverlay.Markers.Add(mBorders);
               // }

            }
            catch (Exception ex) { log.Info(ex.ToString()); }
        }
        private delegate void SafeCallDelegate(string text);

        private void ChangeVisibilitySpeedPanel(string text)
        {
            if (SpeedPanel.InvokeRequired)
            {
                var d = new SafeCallDelegate(ChangeVisibilitySpeedPanel);
                Invoke(d, new object[] { text });
            }
            else
            {
                SpeedPanel.Enabled = Boolean.Parse(text);
            }
        }
        private void ChangeVisibilityPanel6(string text)
        {
            if (panel6.InvokeRequired)
            {
                var d = new SafeCallDelegate(ChangeVisibilityPanel6);
                Invoke(d, new object[] { text });
            }
            else
            {
                panel6.Enabled = Boolean.Parse(text);
            }
        }
        void updateRowNumbers()
        {
            // number rows 
            Thread t1 = new Thread(delegate()
            {
            // thread for updateing row numbers
            if (Commands.Rows.Count == 0) { ChangeVisibilityPanel6("false"); ChangeVisibilitySpeedPanel("false"); }
            else {
                    if (SpeedPanel.Visible == false) { ChangeVisibilityPanel6("true"); }
                    ChangeVisibilitySpeedPanel("true");
                }

            

                for (int a = 0; a < Commands.Rows.Count - 0; a++)
                {
                    // this.BeginInvoke((MethodInvoker)delegate
                    {
                        try
                        {
                           
                            if (Commands.Rows[a].HeaderCell.Value == null)
                            {
                                Commands.Rows[a].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                                Commands.Rows[a].HeaderCell.Value = (a + 1).ToString();
                            }
                            // skip rows with the correct number
                            string rowno = Commands.Rows[a].HeaderCell.Value.ToString();
                            if (!rowno.Equals((a + 1).ToString()))
                            {
                                // this code is where the delay is when deleting.
                                Commands.Rows[a].HeaderCell.Value = (a + 1).ToString();
                            }
                        }
                        catch (Exception) { }
                    }//);
                }
               
            });
            t1.Name = "Row number updater";
            t1.IsBackground = true;
            t1.Start();
        }

        /// <summary>
        /// used to write a KML, update the Map view polygon, and update the row headers
        /// </summary>
        public void writeKML()
        {
            // quickadd is for when loading wps from eeprom or file, to prevent slow, loading times
            if (quickadd)
                return;

            // this is to share the current mission with the data tab
            pointlist = new List<PointLatLngAlt>();

            fullpointlist.Clear();

            Debug.WriteLine(DateTime.Now);
           
            try
            {
                
                if (objectsoverlay != null) // hasnt been created yet
                {
                    objectsoverlay.Markers.Clear();
                }

                // process and add home to the list
                string home;
                if (TXT_homealt.Text != "" && TXT_homelat.Text != "" && TXT_homelng.Text != "")
                {
                    home = string.Format("{0},{1},{2}\r\n", TXT_homelng.Text, TXT_homelat.Text, TXT_DefaultAlt.Text);
                    if (objectsoverlay != null) // during startup
                    {
                        pointlist.Add(new PointLatLngAlt(double.Parse(TXT_homelat.Text), double.Parse(TXT_homelng.Text), (int)double.Parse(TXT_homealt.Text), "H"));
                        fullpointlist.Add(pointlist[pointlist.Count - 1]);
                        addpolygonmarker("H", double.Parse(TXT_homelng.Text), double.Parse(TXT_homelat.Text), 0, null);
                    }
                }
                else
                {
                    home = "";
                    pointlist.Add(null);
                    fullpointlist.Add(pointlist[pointlist.Count - 1]);
                }

                // setup for centerpoint calc etc.
                double avglat = 0;
                double avglong = 0;
                double maxlat = -180;
                double maxlong = -180;
                double minlat = 180;
                double minlong = 180;
                double homealt = 0;
                try
                {
                    if (!String.IsNullOrEmpty(TXT_homealt.Text))
                        homealt = (int)double.Parse(TXT_homealt.Text);
                }
                catch { }
                if ((altmode)CMB_altmode.SelectedValue == altmode.Absolute)
                {
                    homealt = 0; // for absolute we dont need to add homealt
                }

                int usable = 0;

                updateRowNumbers();

                long temp = Stopwatch.GetTimestamp();

                string lookat = "";
                for (int a = 0; a < Commands.Rows.Count - 0; a++)
                {
                    try
                    {
                       
                        if (Commands.Rows[a].Cells[Command.Index].Value.ToString().Contains("UNKNOWN"))
                            continue;

                        int command = (byte)(int)Enum.Parse(typeof(MAVLink.MAV_CMD), Commands.Rows[a].Cells[Command.Index].Value.ToString(), false);
                        if (command < (byte)MAVLink.MAV_CMD.LAST &&
                            command != (byte)MAVLink.MAV_CMD.TAKEOFF &&
                            command != (byte)MAVLink.MAV_CMD.RETURN_TO_LAUNCH &&
                            command != (byte)MAVLink.MAV_CMD.CONTINUE_AND_CHANGE_ALT &&
                            command != (byte)MAVLink.MAV_CMD.GUIDED_ENABLE
                            || command == (byte)MAVLink.MAV_CMD.DO_SET_ROI || command == (byte)MAVLink.MAV_CMD.DO_CHANGE_SPEED)
                        {
                            string cell2 = Commands.Rows[a].Cells[Alt.Index].Value.ToString(); // alt
                            string cell3 = Commands.Rows[a].Cells[Lat.Index].Value.ToString(); // lat
                            string cell4 = Commands.Rows[a].Cells[Lon.Index].Value.ToString(); // lng

                            // land can be 0,0 or a lat,lng
                            if (command == (byte)MAVLink.MAV_CMD.LAND && cell3 == "0" && cell4 == "0")
                                continue;

                            if (cell4 == "?" || cell3 == "?")
                                continue;

                            if (command == (byte)MAVLink.MAV_CMD.DO_SET_ROI)
                            {
                                pointlist.Add(new PointLatLngAlt(double.Parse(cell3), double.Parse(cell4), (int)double.Parse(cell2) + homealt, "ROI" + (a + 1)) { color = Color.Red });
                                // do set roi is not a nav command. so we dont route through it
                                //fullpointlist.Add(pointlist[pointlist.Count - 1]);
                                GMarkerGoogle m = new GMarkerGoogle(new PointLatLng(double.Parse(cell3), double.Parse(cell4)), GMarkerGoogleType.red);
                                m.ToolTipMode = MarkerTooltipMode.Never;
                                m.ToolTipText = (a + 1).ToString();
                                m.Tag = (a + 1).ToString();

                                GMapMarkerRect mBorders = new GMapMarkerRect(m.Position);
                                {
                                    mBorders.InnerMarker = m;
                                    mBorders.Tag = "Dont draw line";
                                }

                                // check for clear roi, and hide it
                                if (m.Position.Lat != 0 && m.Position.Lng != 0)
                                {
                                    // order matters
                                    objectsoverlay.Markers.Add(m);
                                    objectsoverlay.Markers.Add(mBorders);
                                }
                            }
                            else if (command == (byte)MAVLink.MAV_CMD.LOITER_TIME || command == (byte)MAVLink.MAV_CMD.LOITER_TURNS || command == (byte)MAVLink.MAV_CMD.LOITER_UNLIM)
                            {
                                pointlist.Add(new PointLatLngAlt(double.Parse(cell3), double.Parse(cell4), (int)double.Parse(cell2) + homealt, (a + 1).ToString()) { color = Color.LightBlue });
                                fullpointlist.Add(pointlist[pointlist.Count - 1]);
                                addpolygonmarker((a + 1).ToString(), double.Parse(cell4), double.Parse(cell3), (int)double.Parse(cell2), Color.LightBlue);
                            }
                            else if (command == (byte)MAVLink.MAV_CMD.SPLINE_WAYPOINT)
                            {
                                pointlist.Add(new PointLatLngAlt(double.Parse(cell3), double.Parse(cell4), (int)double.Parse(cell2) + homealt, (a + 1).ToString()) { Tag2 = "spline" });
                                fullpointlist.Add(pointlist[pointlist.Count - 1]);
                                addpolygonmarker((a + 1).ToString(), double.Parse(cell4), double.Parse(cell3), (int)double.Parse(cell2), Color.Green);
                            }
                            else
                            {
                                pointlist.Add(new PointLatLngAlt(double.Parse(cell3), double.Parse(cell4), (int)double.Parse(cell2) + homealt, (a + 1).ToString()));
                                fullpointlist.Add(pointlist[pointlist.Count - 1]);
                                addpolygonmarker((a + 1).ToString(), double.Parse(cell4), double.Parse(cell3), (int)double.Parse(cell2), Color.Black);
                            }

                            avglong += double.Parse(Commands.Rows[a].Cells[Lon.Index].Value.ToString());
                            avglat += double.Parse(Commands.Rows[a].Cells[Lat.Index].Value.ToString());
                            usable++;

                            maxlong = Math.Max(double.Parse(Commands.Rows[a].Cells[Lon.Index].Value.ToString()), maxlong);
                            maxlat = Math.Max(double.Parse(Commands.Rows[a].Cells[Lat.Index].Value.ToString()), maxlat);
                            minlong = Math.Min(double.Parse(Commands.Rows[a].Cells[Lon.Index].Value.ToString()), minlong);
                            minlat = Math.Min(double.Parse(Commands.Rows[a].Cells[Lat.Index].Value.ToString()), minlat);

                            Debug.WriteLine(temp - Stopwatch.GetTimestamp());
                        }
                        else if (command == (byte)MAVLink.MAV_CMD.DO_JUMP) // fix do jumps into the future
                        {
                            pointlist.Add(null);

                            int wpno = int.Parse(Commands.Rows[a].Cells[Param1.Index].Value.ToString());
                            int repeat = int.Parse(Commands.Rows[a].Cells[Param2.Index].Value.ToString());

                            List<PointLatLngAlt> list = new List<PointLatLngAlt>();

                            // cycle through reps
                            for (int repno = repeat; repno > 0; repno--)
                            {
                                // cycle through wps
                                for (int no = wpno; no <= a; no++)
                                {
                                    if (pointlist[no] != null)
                                        list.Add(pointlist[no]);
                                }
                            }

                            fullpointlist.AddRange(list);
                        }
                        else
                        {
                            pointlist.Add(null);
                        }
                    }
                    catch (Exception e) { log.Info("writekml - bad wp data " + e); }
                }

                if (usable > 0)
                {
                    avglat = avglat / usable;
                    avglong = avglong / usable;
                    double latdiff = maxlat - minlat;
                    double longdiff = maxlong - minlong;
                    float range = 4000;

                    Locationwp loc1 = new Locationwp();
                    loc1.lat = (minlat);
                    loc1.lng = (minlong);
                    Locationwp loc2 = new Locationwp();
                    loc2.lat = (maxlat);
                    loc2.lng = (maxlong);

                    //double distance = getDistance(loc1, loc2);  // same code as ardupilot
                    double distance = 2000;

                    if (usable > 1)
                    {
                        range = (float)(distance * 2);
                    }
                    else
                    {
                        range = 4000;
                    }

                    if (avglong != 0 && usable < 3)
                    {
                        // no autozoom
                        lookat = "<LookAt>     <longitude>" + (minlong + longdiff / 2).ToString(new CultureInfo("en-US")) + "</longitude>     <latitude>" + (minlat + latdiff / 2).ToString(new CultureInfo("en-US")) + "</latitude> <range>" + range + "</range> </LookAt>";
                        //MainMap.ZoomAndCenterMarkers("objects");
                        //MainMap.Zoom -= 1;
                        //MainMap_OnMapZoomChanged();
                    }
                }
                else if (home.Length > 5 && usable == 0)
                {
                    lookat = "<LookAt>     <longitude>" + TXT_homelng.Text.ToString(new CultureInfo("en-US")) + "</longitude>     <latitude>" + TXT_homelat.Text.ToString(new CultureInfo("en-US")) + "</latitude> <range>4000</range> </LookAt>";

                    RectLatLng? rect = Joysticknutton.GetRectOfAllMarkers("objects");
                    if (rect.HasValue)
                    {
                        Joysticknutton.Position = rect.Value.LocationMiddle;
                    }

                    //MainMap.Zoom = 17;

                    MainMap_OnMapZoomChanged();
                }

                //RegeneratePolygon();

                RegenerateWPRoute(fullpointlist);
                //
                DrawMyTracks(fullpointlist);
                //
                if (fullpointlist.Count > 0)
                {
                    double homedist = 0;

                    if (home.Length > 5)
                    {
                        homedist = Joysticknutton.MapProvider.Projection.GetDistance(fullpointlist[fullpointlist.Count - 1], fullpointlist[0]);
                    }

                    double dist = 0;

                    for (int a = 1; a < fullpointlist.Count; a++)
                    {
                        if (fullpointlist[a - 1] == null)
                            continue;

                        if (fullpointlist[a] == null)
                            continue;

                        dist += Joysticknutton.MapProvider.Projection.GetDistance(fullpointlist[a - 1], fullpointlist[a]);
                    }

                    lbl_distance.Text = rm.GetString("lbl_distance.Text") + ": " + FormatDistance(dist + homedist, false);
                }

                setgradanddistandaz();
            }
            catch (Exception ex)
            {
                log.Info(ex.ToString());
            }

            Debug.WriteLine(DateTime.Now);
            WPNumberUpdate();
        }
        private void DrawMyTracks(List<PointLatLngAlt> fullpointlist)
        {

            //trackoverlay.Routes.Clear();
            //trackoverlay2.Routes.Clear();
            //int count = fullpointlist.Count;
            //count = count - 2;
            //int counter=0;
            //tracks.Clear();
            //tracks2.Clear();
            //List<PointLatLngAlt> wproute = new List<PointLatLngAlt>();
            //PointLatLngAlt temp_point = new PointLatLngAlt();
            //PointLatLngAlt oldpoint = new PointLatLngAlt();
            //PointLatLngAlt newpoint = new PointLatLngAlt();
            //double bearing;
            //int flag = 0,secondpt=0,flag2=0,firstpt=1,lastpt=0;
            //// newpos(double bearing, double distance)
            //// GetBearing(PointLatLngAlt p2)
            //if (count >= 2)
            //{
                
            //    for (counter = 2; counter <= count; counter++)
            //    {

            //        if (flag == 0 && fullpointlist[counter].Alt!=10 )
                    
            //        {
            //            secondpt = counter;
            //            flag2 = secondpt;
            //            bearing = fullpointlist[1].GetBearing(fullpointlist[secondpt]) + 90;
            //            temp_point = fullpointlist[1].newpos(bearing, 10);
            //            tracks.Points.Add(temp_point);
            //            temp_point = fullpointlist[1].newpos(bearing, -10);
            //            tracks2.Points.Add(temp_point);
            //            flag = 1;
 
            //        }
            //        else
            //        {
            //            if (fullpointlist[counter].Alt != 10)
            //            {
            //                lastpt = counter;
            //                bearing = fullpointlist[firstpt].GetBearing(fullpointlist[secondpt])+90;
            //                temp_point = fullpointlist[secondpt].newpos(bearing, 10);
            //                tracks.Points.Add(temp_point);
            //                temp_point = fullpointlist[secondpt].newpos(bearing, -10);
            //                tracks2.Points.Add(temp_point);
            //                bearing = fullpointlist[secondpt].GetBearing(fullpointlist[lastpt])+90;
            //                temp_point = fullpointlist[secondpt].newpos(bearing, 10);
            //                tracks.Points.Add(temp_point);
            //                temp_point = fullpointlist[secondpt].newpos(bearing, -10);
            //                tracks2.Points.Add(temp_point);
            //                firstpt = secondpt;
            //                secondpt = lastpt;
            //            }
                        
            //        }
                    
            //    }

            //    if (count == 2)
            //    {
            //        lastpt = 2;
            //        secondpt = 1;
            //        bearing = fullpointlist[secondpt].GetBearing(fullpointlist[lastpt]) + 90;
            //        temp_point = fullpointlist[lastpt].newpos(bearing, 10);
            //        tracks.Points.Add(temp_point);
            //        temp_point = fullpointlist[lastpt].newpos(bearing, -10);
            //        tracks2.Points.Add(temp_point);
            //    }
            //    else
            //    {

            //        secondpt = firstpt;
            //        bearing = fullpointlist[secondpt].GetBearing(fullpointlist[lastpt]) + 90;
            //        temp_point = fullpointlist[lastpt].newpos(bearing, 10);
            //        tracks.Points.Add(temp_point);
            //        temp_point = fullpointlist[lastpt].newpos(bearing, -10);
            //        tracks2.Points.Add(temp_point);
            //    }
       
            //    tracks.Stroke = new Pen(Color.Red, 2);
            //    tracks.Stroke.DashStyle = DashStyle.Dash;
            //    trackoverlay.Routes.Add(tracks);
            //    tracks2.Stroke = new Pen(Color.Red, 2);
            //    tracks2.Stroke.DashStyle = DashStyle.Dash;
            //    trackoverlay.Routes.Add(tracks2);
            //    MainMap.Overlays.Add(trackoverlay);
            //    MainMap.Overlays.Add(trackoverlay2);
            //}
        }
        private void RegenerateWPRoute(List<PointLatLngAlt> fullpointlist)
        {


            route.Clear();
            homeroute.Clear();

            polygonsoverlay.Routes.Clear();

            PointLatLngAlt lastpnt = fullpointlist[0];
            PointLatLngAlt lastpnt2 = fullpointlist[0];
            PointLatLngAlt lastnonspline = fullpointlist[0];
            List<PointLatLngAlt> splinepnts = new List<PointLatLngAlt>();
            List<PointLatLngAlt> wproute = new List<PointLatLngAlt>();

            // add home - this causeszx the spline to always have a straight finish
            fullpointlist.Add(fullpointlist[0]);

            for (int a = 0; a < fullpointlist.Count; a++)
            {
                if (fullpointlist[a] == null)
                    continue;

                if (fullpointlist[a].Tag2 == "spline")
                {
                    if (splinepnts.Count == 0)
                        splinepnts.Add(lastpnt);

                    splinepnts.Add(fullpointlist[a]);
                }
                else
                {
                    if (splinepnts.Count > 0)
                    {
                        List<PointLatLng> list = new List<PointLatLng>();

                        splinepnts.Add(fullpointlist[a]);

                        Spline2 sp = new Spline2();

                        //sp._flags.segment_type = MissionPlanner.Controls.Waypoints.Spline2.SegmentType.SEGMENT_STRAIGHT;
                        //sp._flags.reached_destination = true;
                        //sp._origin = sp.pv_location_to_vector(lastpnt);
                        //sp._destination = sp.pv_location_to_vector(fullpointlist[0]);

                        // sp._spline_origin_vel = sp.pv_location_to_vector(lastpnt) - sp.pv_location_to_vector(lastnonspline);

                        sp.set_wp_origin_and_destination(sp.pv_location_to_vector(lastpnt2), sp.pv_location_to_vector(lastpnt));

                        sp._flags.reached_destination = true;

                        for (int no = 1; no < (splinepnts.Count - 1); no++)
                        {
                            Spline2.spline_segment_end_type segtype = Spline2.spline_segment_end_type.SEGMENT_END_STRAIGHT;

                            if (no < (splinepnts.Count - 2))
                            {
                                segtype = Spline2.spline_segment_end_type.SEGMENT_END_SPLINE;
                            }

                            sp.set_spline_destination(sp.pv_location_to_vector(splinepnts[no]), false, segtype, sp.pv_location_to_vector(splinepnts[no + 1]));

                            //sp.update_spline();

                            while (sp._flags.reached_destination == false)
                            {
                                float t = 1f;
                                //sp.update_spline();
                                sp.advance_spline_target_along_track(t);
                                // Console.WriteLine(sp.pv_vector_to_location(sp.target_pos).ToString());
                                list.Add(sp.pv_vector_to_location(sp.target_pos));
                            }

                            list.Add(splinepnts[no]);

                        }

                        list.ForEach(x =>
                        {
                            wproute.Add(x);
                        });


                        splinepnts.Clear();

                        /*
                        MissionPlanner.Controls.Waypoints.Spline sp = new Controls.Waypoints.Spline();
                        
                        var spline = sp.doit(splinepnts, 20, lastlastpnt.GetBearing(splinepnts[0]),false);

                  
                         */

                        lastnonspline = fullpointlist[a];
                    }

                    wproute.Add(fullpointlist[a]);

                    lastpnt2 = lastpnt;
                    lastpnt = fullpointlist[a];
                }
            }
            /*

           List<PointLatLng> list = new List<PointLatLng>();
           fullpointlist.ForEach(x => { list.Add(x); });
           route.Points.AddRange(list);
           */
            // route is full need to get 1, 2 and last point as "HOME" route

            int count = wproute.Count;
            int counter = 0;
            PointLatLngAlt homepoint = new PointLatLngAlt();
            PointLatLngAlt firstpoint = new PointLatLngAlt();
            PointLatLngAlt lastpoint = new PointLatLngAlt();
            GMapRoute tracks = new GMapRoute("track1");
            tracks.Stroke = new Pen(Color.Red, 2);
            tracks.Stroke.DashStyle = DashStyle.Dash;
            GMapRoute tracks1 = new GMapRoute("track2");
            tracks1.Stroke = new Pen(Color.Red, 2);
            tracks1.Stroke.DashStyle = DashStyle.Dash;
            PointLatLngAlt temp_point = new PointLatLngAlt();
            PointLatLngAlt old_point = new PointLatLngAlt();
            PointLatLngAlt new_point = new PointLatLngAlt();
            double bearing;
            if (count >= 2)
            {

                for (int a = 2; a < (count-1); a++)
                {
                    old_point = wproute[a - 1];
                    new_point = wproute[a];

                    bearing = old_point.GetBearing(new_point) + 90;
                    temp_point = old_point.newpos(bearing, float.Parse(TXT_WPRad.Text));
                    tracks.Points.Add(temp_point);

                    bearing = old_point.GetBearing(new_point) + 90;
                    temp_point = new_point.newpos(bearing, float.Parse(TXT_WPRad.Text));
                    tracks.Points.Add(temp_point);

                    bearing = old_point.GetBearing(new_point) + 90;
                    temp_point = old_point.newpos(bearing, -float.Parse(TXT_WPRad.Text));
                    tracks1.Points.Add(temp_point);

                    bearing = old_point.GetBearing(new_point) + 90;
                    temp_point = new_point.newpos(bearing, -float.Parse(TXT_WPRad.Text));
                    tracks1.Points.Add(temp_point);

                }

            }
            if (count > 2)
            {
                // homeroute = last, home, first
                wproute.ForEach(x =>
                {
                    counter++;
                    if (counter == 1) { homepoint = x; return; }
                    if (counter == 2) { firstpoint = x; }
                    if (counter == count - 1) { lastpoint = x; }
                    if (counter == count) { homeroute.Points.Add(lastpoint); homeroute.Points.Add(homepoint); homeroute.Points.Add(firstpoint); return; }
                    route.Points.Add(x);
                });
 
                homeroute.Stroke = new Pen(Color.Black, 1);
                // if we have a large distance between home and the first/last point, it hangs on the draw of a the dashed line.
                if (homepoint.GetDistance(lastpoint) < 5000 && homepoint.GetDistance(firstpoint) < 5000)
                    homeroute.Stroke.DashStyle = DashStyle.Dash;

                polygonsoverlay.Routes.Add(homeroute);

                route.Stroke = new Pen(Color.Black, 3);
                route.Stroke.DashStyle = DashStyle.Custom;
                polygonsoverlay.Routes.Add(route);
                polygonsoverlay.Routes.Add(route);
                polygonsoverlay.Routes.Add(tracks);
                polygonsoverlay.Routes.Add(tracks1);
            }
        }

        /// <summary>
        /// used to redraw the polygon
        /// </summary>
        void RegeneratePolygon()
        {
            List<PointLatLng> polygonPoints = new List<PointLatLng>();

            if (objectsoverlay == null)
                return;

            foreach (GMapMarker m in objectsoverlay.Markers)
            {
                if (m is GMapMarkerRect)
                {
                    if (m.Tag == null)
                    {
                        m.Tag = polygonPoints.Count;
                        polygonPoints.Add(m.Position);
                    }
                }
            }

            if (wppolygon == null)
            {
                wppolygon = new GMapPolygon(polygonPoints, "polygon test");
                polygonsoverlay.Polygons.Add(wppolygon);
            }
            else
            {
                wppolygon.Points.Clear();
                wppolygon.Points.AddRange(polygonPoints);

                wppolygon.Stroke = new Pen(Color.Red, 4);
                wppolygon.Stroke.DashStyle = DashStyle.Custom;
                wppolygon.Fill = Brushes.Transparent;

                if (polygonsoverlay.Polygons.Count == 0)
                {
                    polygonsoverlay.Polygons.Add(wppolygon);
                }
                else
                {
                    lock (thisLock)
                    {
                        Joysticknutton.UpdatePolygonLocalPosition(wppolygon);
                    }
                }
            }
        }

        void setgradanddistandaz()
        {
            int a = 0;
            PointLatLngAlt last = MainV2.comPort.MAV.cs.HomeLocation;
            foreach (var lla in pointlist)
            {
                if (lla == null)
                    continue;
                try
                {
                    if (lla.Tag != null && lla.Tag != "H" && !lla.Tag.Contains("ROI"))
                    {
                        Commands.Rows[int.Parse(lla.Tag) - 1].Cells[Grad.Index].Value = (((lla.Alt - last.Alt) / (lla.GetDistance(last) * CurrentState.multiplierdist)) * 100).ToString("0.0");

                        Commands.Rows[int.Parse(lla.Tag) - 1].Cells[Dist.Index].Value = (lla.GetDistance(last) * CurrentState.multiplierdist).ToString("0.0");

                        Commands.Rows[int.Parse(lla.Tag) - 1].Cells[AZ.Index].Value = ((lla.GetBearing(last) + 180) % 360).ToString("0");
                    }
                }
                catch { }
                a++;
                last = lla;
            }
        }
        /// <summary>
        /// Saves a waypoint writer file
        /// </summary>
        private void savewaypoints()
        {
            using (SaveFileDialog fd = new SaveFileDialog())
            {
                fd.Filter = "Ardupilot Mission (*.txt)|*.*";
                fd.DefaultExt = ".txt";
                fd.FileName = wpfilename;
                DialogResult result = fd.ShowDialog();
                string file = fd.FileName;
                if (file != "")
                {
                    try
                    {
                        StreamWriter sw = new StreamWriter(file);
                        sw.WriteLine("QGC WPL 110");
                        try
                        {
                            sw.WriteLine("0\t1\t0\t16\t0\t0\t0\t0\t" + double.Parse(TXT_homelat.Text).ToString("0.000000", new CultureInfo("en-US")) + "\t" + double.Parse(TXT_homelng.Text).ToString("0.000000", new CultureInfo("en-US")) + "\t" + double.Parse(TXT_homealt.Text).ToString("0.000000", new CultureInfo("en-US")) + "\t1");
                        }
                        catch
                        {
                            sw.WriteLine("0\t1\t0\t0\t0\t0\t0\t0\t0\t0\t0\t1");
                        }
                        for (int a = 0; a < Commands.Rows.Count - 0; a++)
                        {
                            byte mode = (byte)(MAVLink.MAV_CMD)Enum.Parse(typeof(MAVLink.MAV_CMD), Commands.Rows[a].Cells[0].Value.ToString());

                            sw.Write((a + 1)); // seq
                            sw.Write("\t" + 0); // current
                            sw.Write("\t" + CMB_altmode.SelectedValue); //frame 
                            sw.Write("\t" + mode);
                            sw.Write("\t" + double.Parse(Commands.Rows[a].Cells[Param1.Index].Value.ToString()).ToString("0.000000", new CultureInfo("en-US")));
                            sw.Write("\t" + double.Parse(Commands.Rows[a].Cells[Param2.Index].Value.ToString()).ToString("0.000000", new CultureInfo("en-US")));
                            sw.Write("\t" + double.Parse(Commands.Rows[a].Cells[Param3.Index].Value.ToString()).ToString("0.000000", new CultureInfo("en-US")));
                            sw.Write("\t" + double.Parse(Commands.Rows[a].Cells[Param4.Index].Value.ToString()).ToString("0.000000", new CultureInfo("en-US")));
                            sw.Write("\t" + double.Parse(Commands.Rows[a].Cells[Lat.Index].Value.ToString()).ToString("0.000000", new CultureInfo("en-US")));
                            sw.Write("\t" + double.Parse(Commands.Rows[a].Cells[Lon.Index].Value.ToString()).ToString("0.000000", new CultureInfo("en-US")));
                            sw.Write("\t" + (double.Parse(Commands.Rows[a].Cells[Alt.Index].Value.ToString()) / CurrentState.multiplierdist).ToString("0.000000", new CultureInfo("en-US")));
                            sw.Write("\t" + 1);
                            sw.WriteLine("");
                        }
                        sw.Close();

                        lbl_wpfile.Text = "Saved " + Path.GetFileName(file);
                    }
                    catch (Exception) { CustomMessageBox.Show(Strings.ERROR); }
                }
            }
        }

        private void SaveFile_Click(object sender, EventArgs e)
        {
            savewaypoints();
            writeKML();
        }

        /// <summary>
        /// Reads the EEPROM from a com port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void BUT_read_Click(object sender, EventArgs e)
        {
            readwps();
        }
        private void readwps()
        {
            if (Commands.Rows.Count > 0)
            {
                if (CustomMessageBox.Show("This will clear your existing planned mission, Continue?", "Confirm", MessageBoxButtons.OKCancel) != DialogResult.OK)
                {
                    return;
                }
            }

            ProgressReporterDialogue frmProgressReporter = new ProgressReporterDialogue
            {
                StartPosition = FormStartPosition.CenterScreen,
                Text = "Receiving WP's"
            };

            frmProgressReporter.DoWork += getWPs;
            frmProgressReporter.UpdateProgressAndStatus(-1, "Receiving WP's");

            ThemeManager.ApplyThemeTo(frmProgressReporter);

            frmProgressReporter.RunBackgroundOperationAsync();

            frmProgressReporter.Dispose();
        }
        void getWPs(object sender, ProgressWorkerEventArgs e, object passdata = null)
        {
            List<Locationwp> cmds = new List<Locationwp>();

            try
            {
                MAVLinkInterface port = MainV2.comPort;

                if (!port.BaseStream.IsOpen)
                {
                    throw new Exception("Please Connect First!");
                }

                MainV2.comPort.giveComport = true;

                param = port.MAV.param;

                log.Info("Getting WP #");

                ((ProgressReporterDialogue)sender).UpdateProgressAndStatus(0, "Getting WP count");

                int cmdcount = port.getWPCount();

                for (ushort a = 0; a < cmdcount; a++)
                {
                    if (((ProgressReporterDialogue)sender).doWorkArgs.CancelRequested)
                    {
                        ((ProgressReporterDialogue)sender).doWorkArgs.CancelAcknowledged = true;
                        throw new Exception("Cancel Requested");
                    }

                    log.Info("Getting WP" + a);
                    ((ProgressReporterDialogue)sender).UpdateProgressAndStatus(a * 100 / cmdcount, "Getting WP " + a);
                    cmds.Add(port.getWP(a));
                }

                port.setWPACK();

                ((ProgressReporterDialogue)sender).UpdateProgressAndStatus(100, "Done");

                log.Info("Done");
            }
            catch { throw; }

            WPtoScreen(cmds);
        }

        public void WPtoScreen(List<Locationwp> cmds, bool withrally = true)
        {
            try
            {
                Invoke((MethodInvoker)delegate
                {
                    try
                    {
                        log.Info("Process " + cmds.Count);
                        processToScreen(cmds);
                    }
                    catch (Exception exx) { log.Info(exx.ToString()); }

                    try
                    {
                        if (withrally && MainV2.comPort.MAV.param.ContainsKey("RALLY_TOTAL") && int.Parse(MainV2.comPort.MAV.param["RALLY_TOTAL"].ToString()) >= 1)
                            getRallyPointsToolStripMenuItem_Click(null, null);
                    }
                    catch { }

                    MainV2.comPort.giveComport = false;

                    BUT_read.Enabled = true;

                    writeKML();

                });
            }
            catch (Exception exx) { log.Info(exx.ToString()); }
        }

        /// <summary>
        /// Writes the mission from the datagrid and values to the EEPROM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BUT_write_Click(object sender, EventArgs e)
        {
            uploadwps();

        }
        private void uploadwps()
        {
            if ((altmode)CMB_altmode.SelectedValue == altmode.Absolute)
            {
                if (DialogResult.No == CustomMessageBox.Show("Absolute Alt is selected are you sure?", "Alt Mode", MessageBoxButtons.YesNo))
                {
                    CMB_altmode.SelectedValue = (int)altmode.Relative;
                }
            }
            double torad = 3.1415926535897932384626433832795 / 180.0;
            double pi = 3.1415926535897932384626433832795;
            double lat1, lat2, lon1, lon2, bear, lat3, lon3, bear1, distance, north, east;
            double R = 6371000; // metres
            double safe = 1;
            int extrapts, newcounter, overallcounter = 0;
            int counter_of_points = Commands.Rows.Count;
            // CustomMessageBox.Show(Commands.Rows[0].Cells[5].Value.ToString());
            //   CustomMessageBox.Show("Total number of pts is " + counter_of_points);

            // CustomMessageBox.Show(overallcounter.ToString());
            if (Commands.Rows.Count >= 3)
            {
                for (int a = 2; a < Commands.Rows.Count - 0; a++)
                {
                    lat1 = Convert.ToDouble(Commands.Rows[a - 2].Cells[5].Value) * torad;
                    lon1 = Convert.ToDouble(Commands.Rows[a - 2].Cells[6].Value) * torad;
                    lat2 = Convert.ToDouble(Commands.Rows[a - 1].Cells[5].Value) * torad;
                    lon2 = Convert.ToDouble(Commands.Rows[a - 1].Cells[6].Value) * torad;
                    lat3 = Convert.ToDouble(Commands.Rows[a].Cells[5].Value) * torad;
                    lon3 = Convert.ToDouble(Commands.Rows[a].Cells[6].Value) * torad;
                    bear = Math.Atan2(Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1), Math.Sin(lon2 - lon1) * Math.Cos(lat2)) / torad;
                    bear1 = Math.Atan2(Math.Cos(lat2) * Math.Sin(lat3) - Math.Sin(lat2) * Math.Cos(lat3) * Math.Cos(lon3 - lon2), Math.Sin(lon3 - lon2) * Math.Cos(lat3)) / torad;
                    bear = Math.Abs(bear1 - bear);
                    if (bear > 180) { bear = Math.Abs(bear - 360); }
                    //CustomMessageBox.Show(bear.ToString());
                    if (bear > 1000) { CustomMessageBox.Show("WARNING! Heading change at WP# " + (a) + " exceeds 65 degrees"); safe = 0; }
                    //CustomMessageBox.Show(Commands.Rows[a].Cells[6].Value.ToString());
                    //CustomMessageBox.Show(Commands.Rows.Count.ToString());
                }
            }
            if (safe == 1)
            {
                if (Commands.Rows.Count >= 2)
                {
                    // Refresh_my_PTs();
                }
                // check for invalid grid data
                for (int a = 0; a < Commands.Rows.Count - 0; a++)
                {
                    for (int b = 0; b < Commands.ColumnCount - 0; b++)
                    {
                        double answer;
                        if (b >= 1 && b <= 7)
                        {
                            if (!double.TryParse(Commands[b, a].Value.ToString(), out answer))
                            {
                                CustomMessageBox.Show("There are errors in your mission");
                                return;
                            }
                        }

                        if (TXT_altwarn.Text == "")
                            TXT_altwarn.Text = (0).ToString();

                        if (Commands.Rows[a].Cells[Command.Index].Value.ToString().Contains("UNKNOWN"))
                            continue;

                        byte cmd = (byte)(int)Enum.Parse(typeof(MAVLink.MAV_CMD), Commands.Rows[a].Cells[Command.Index].Value.ToString(), false);

                        if (cmd < (byte)MAVLink.MAV_CMD.LAST && double.Parse(Commands[Alt.Index, a].Value.ToString()) < double.Parse(TXT_altwarn.Text))
                        {
                            if (cmd != (byte)MAVLink.MAV_CMD.TAKEOFF &&
                                cmd != (byte)MAVLink.MAV_CMD.LAND &&
                                cmd != (byte)MAVLink.MAV_CMD.RETURN_TO_LAUNCH)
                            {
                                // CustomMessageBox.Show("Low alt on WP#" + (a + 1) + "\nPlease reduce the alt warning, or increase the altitude");
                                //return;
                            }
                        }
                    }
                }

                ProgressReporterDialogue frmProgressReporter = new ProgressReporterDialogue
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    Text = "Sending WP's"
                };

                frmProgressReporter.DoWork += saveWPs;
                frmProgressReporter.UpdateProgressAndStatus(-1, "Sending WP's");

                ThemeManager.ApplyThemeTo(frmProgressReporter);

                frmProgressReporter.RunBackgroundOperationAsync();

                frmProgressReporter.Dispose();

                Joysticknutton.Focus();
            }
        }

        Locationwp DataViewtoLocationwp(int a)
        {
            Locationwp temp = new Locationwp();
            if (Commands.Rows[a].Cells[Command.Index].Value.ToString().Contains("UNKNOWN"))
            {
                temp.id = (byte)Commands.Rows[a].Cells[Command.Index].Tag;
            }
            else
            {
                temp.id = (byte)(int)Enum.Parse(typeof(MAVLink.MAV_CMD), Commands.Rows[a].Cells[Command.Index].Value.ToString(), false);
            }
            temp.p1 = float.Parse(Commands.Rows[a].Cells[Param1.Index].Value.ToString());

            temp.alt = (float)(double.Parse(Commands.Rows[a].Cells[Alt.Index].Value.ToString()) / CurrentState.multiplierdist);
            temp.lat = (double.Parse(Commands.Rows[a].Cells[Lat.Index].Value.ToString()));
            temp.lng = (double.Parse(Commands.Rows[a].Cells[Lon.Index].Value.ToString()));

            temp.p2 = (float)(double.Parse(Commands.Rows[a].Cells[Param2.Index].Value.ToString()));
            temp.p3 = (float)(double.Parse(Commands.Rows[a].Cells[Param3.Index].Value.ToString()));
            temp.p4 = (float)(double.Parse(Commands.Rows[a].Cells[Param4.Index].Value.ToString()));

            return temp;
        }

        List<Locationwp> GetCommandList()
        {
            List<Locationwp> commands = new List<Locationwp>();

            for (int a = 0; a < Commands.Rows.Count - 0; a++)
            {
                var temp = DataViewtoLocationwp(a);

                commands.Add(temp);
            }

            return commands;
        }

        void saveWPs(object sender, ProgressWorkerEventArgs e, object passdata = null)
        {
            try
            {
                MAVLinkInterface port = MainV2.comPort;

                if (!port.BaseStream.IsOpen)
                {
                    throw new Exception("Please connect first!");
                }

                MainV2.comPort.giveComport = true;
                int a = 0;

                // define the home point
                Locationwp home = new Locationwp();
                try
                {
                    home.id = (byte)MAVLink.MAV_CMD.WAYPOINT;
                    home.lat = (double.Parse(TXT_homelat.Text));
                    home.lng = (double.Parse(TXT_homelng.Text));
                    home.alt = (float.Parse(TXT_homealt.Text) / CurrentState.multiplierdist); // use saved home
                }
                catch { throw new Exception("Your home location is invalid"); }

                // log
                log.Info("wps values " + MainV2.comPort.MAV.wps.Values.Count);
                log.Info("cmd rows " + (Commands.Rows.Count + 1)); // + home

                // check for changes / future mod to send just changed wp's
                if (MainV2.comPort.MAV.wps.Values.Count == (Commands.Rows.Count + 1))
                {
                    Hashtable wpstoupload = new Hashtable();

                    a = -1;
                    foreach (var item in MainV2.comPort.MAV.wps.Values)
                    {
                        // skip home
                        if (a == -1)
                        {
                            a++;
                            continue;
                        }

                        MAVLink.mavlink_mission_item_t temp = DataViewtoLocationwp(a);

                        if (temp.command == item.command &&
                            temp.x == item.x &&
                            temp.y == item.y &&
                            temp.z == item.z &&
                            temp.param1 == item.param1 &&
                            temp.param2 == item.param2 &&
                            temp.param3 == item.param3 &&
                            temp.param4 == item.param4
                            )
                        {
                            log.Info("wp match " + (a + 1));
                        }
                        else
                        {
                            log.Info("wp no match" + (a + 1));
                            wpstoupload[a] = "";
                        }

                        a++;
                    }
                }

                // set wp total
                ((ProgressReporterDialogue)sender).UpdateProgressAndStatus(0, "Set total wps ");

                ushort totalwpcountforupload = (ushort)(Commands.Rows.Count + 1);

                port.setWPTotal(totalwpcountforupload); // + home

                // set home location - overwritten/ignored depending on firmware.
                ((ProgressReporterDialogue)sender).UpdateProgressAndStatus(0, "Set home");

                var homeans = port.setWP(home, 0, MAVLink.MAV_FRAME.GLOBAL, 0);

                if (homeans != MAVLink.MAV_MISSION_RESULT.MAV_MISSION_ACCEPTED)
                {
                    CustomMessageBox.Show(Strings.ErrorRejectedByMAV, Strings.ERROR);
                    return;
                }

                // define the default frame.
                MAVLink.MAV_FRAME frame = MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT;

                // get the command list from the datagrid
                var commandlist = GetCommandList();

                // upload from wp1, as home is alreadey sent
                a = 1;
                // process commandlist to the mav
                foreach (var temp in commandlist)
                {
                    // this code below fails
                    //a = commandlist.IndexOf(temp) + 1;

                    ((ProgressReporterDialogue)sender).UpdateProgressAndStatus(a * 100 / Commands.Rows.Count, "Setting WP " + a);

                    // make sure we are using the correct frame for these commands
                    if (temp.id < (byte)MAVLink.MAV_CMD.LAST || temp.id == (byte)MAVLink.MAV_CMD.DO_SET_HOME)
                    {
                        var mode = currentaltmode;

                        if (mode == altmode.Terrain)
                        {
                            frame = MAVLink.MAV_FRAME.GLOBAL_TERRAIN_ALT;
                        }
                        else if (mode == altmode.Absolute)
                        {
                            frame = MAVLink.MAV_FRAME.GLOBAL;
                        }
                        else
                        {
                            frame = MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT;
                        }
                    }

                    // try send the wp
                    MAVLink.MAV_MISSION_RESULT ans = port.setWP(temp, (ushort)(a), frame, 0);

                    // we timed out while uploading wps/ command wasnt replaced/ command wasnt added
                    if (ans == MAVLink.MAV_MISSION_RESULT.MAV_MISSION_ERROR)
                    {
                        // resend for partial upload
                        port.setWPPartialUpdate((ushort)(a), totalwpcountforupload);
                        // reupload this point.
                        ans = port.setWP(temp, (ushort)(a), frame, 0);
                    }

                    if (ans == MAVLink.MAV_MISSION_RESULT.MAV_MISSION_NO_SPACE)
                    {
                        e.ErrorMessage = "Upload failed, please reduce the number of wp's";
                        return;
                    }
                    if (ans == MAVLink.MAV_MISSION_RESULT.MAV_MISSION_INVALID)
                    {
                        e.ErrorMessage = "Upload failed, mission was rejected byt the Mav,\n item had a bad option wp# " + a + " " + ans;
                        return;
                    }
                    if (ans == MAVLink.MAV_MISSION_RESULT.MAV_MISSION_INVALID_SEQUENCE)
                    {
                        // invalid sequence can only occur if we failed to see a response from the apm when we sent the request.
                        // therefore it did see the request and has moved on that step, and so do we.
                        a++;
                        continue;
                    }
                    if (ans != MAVLink.MAV_MISSION_RESULT.MAV_MISSION_ACCEPTED)
                    {
                        e.ErrorMessage = "Upload wps failed " + Enum.Parse(typeof(MAVLink.MAV_CMD), temp.id.ToString()) + " " + Enum.Parse(typeof(MAVLink.MAV_MISSION_RESULT), ans.ToString());
                        return;
                    }

                    a++;
                }

                port.setWPACK();

                ((ProgressReporterDialogue)sender).UpdateProgressAndStatus(95, "Setting params");

                // m
               // port.setParam("WP_RADIUS", (byte)int.Parse(TXT_WPRad.Text) / CurrentState.multiplierdist);

                // cm's
                //port.setParam("WPNAV_RADIUS", (byte)int.Parse(TXT_WPRad.Text) / CurrentState.multiplierdist * 100);

                try
                {
                  //  port.setParam(new[] { "LOITER_RAD", "WP_LOITER_RAD" }, int.Parse(TXT_loiterrad.Text) / CurrentState.multiplierdist);
                }
                catch
                {

                }

                ((ProgressReporterDialogue)sender).UpdateProgressAndStatus(100, "Done.");
            }
            catch (Exception ex) { log.Error(ex); MainV2.comPort.giveComport = false; throw; }

            MainV2.comPort.giveComport = false;
        }

        /// <summary>
        /// Processes a loaded EEPROM to the map and datagrid
        /// </summary>
        void processToScreen(List<Locationwp> cmds, bool append = false)
        {
            quickadd = true;


            // mono fix
            Commands.CurrentCell = null;

            while (Commands.Rows.Count > 0 && !append)
                Commands.Rows.Clear();

            if (cmds.Count == 0)
            {
                quickadd = false;
                return;
            }

            int i = Commands.Rows.Count - 1;
            foreach (Locationwp temp in cmds)
            {
                i++;
                //Console.WriteLine("FP processToScreen " + i);
                if (temp.id == 0 && i != 0) // 0 and not home
                    break;
                if (temp.id == 255 && i != 0) // bad record - never loaded any WP's - but have started the board up.
                    break;
                if (i + 1 >= Commands.Rows.Count)
                {
                    selectedrow = Commands.Rows.Add();
                }
                //if (i == 0 && temp.alt == 0) // skip 0 home
                //  continue;
                DataGridViewTextBoxCell cell;
                DataGridViewComboBoxCell cellcmd;
                cellcmd = Commands.Rows[i].Cells[Command.Index] as DataGridViewComboBoxCell;
                cellcmd.Value = "UNKNOWN";
                cellcmd.Tag = temp.id;

                foreach (object value in Enum.GetValues(typeof(MAVLink.MAV_CMD)))
                {
                    if ((int)value == temp.id)
                    {
                        cellcmd.Value = value.ToString();
                        break;
                    }
                }

                // from ap_common.h
                if (temp.id < (byte)MAVLink.MAV_CMD.LAST || temp.id == (byte)MAVLink.MAV_CMD.DO_SET_HOME)
                {
                    // check ralatice and terrain flags
                    if ((temp.options & 0x9) == 0 && i != 0)
                    {
                        CMB_altmode.SelectedValue = (int)altmode.Absolute;
                    }// check terrain flag
                    else if ((temp.options & 0x8) != 0 && i != 0)
                    {
                        CMB_altmode.SelectedValue = (int)altmode.Terrain;
                    }// check relative flag
                    else if ((temp.options & 0x1) != 0 && i != 0)
                    {
                        CMB_altmode.SelectedValue = (int)altmode.Relative;
                    }
                }

                cell = Commands.Rows[i].Cells[Alt.Index] as DataGridViewTextBoxCell;
                cell.Value = Math.Round((temp.alt * CurrentState.multiplierdist), 6);
                cell = Commands.Rows[i].Cells[Lat.Index] as DataGridViewTextBoxCell;
                cell.Value = temp.lat;
                cell = Commands.Rows[i].Cells[Lon.Index] as DataGridViewTextBoxCell;
                cell.Value = temp.lng;

                cell = Commands.Rows[i].Cells[Param1.Index] as DataGridViewTextBoxCell;
                cell.Value = temp.p1;
                cell = Commands.Rows[i].Cells[Param2.Index] as DataGridViewTextBoxCell;
                cell.Value = temp.p2;
                cell = Commands.Rows[i].Cells[Param3.Index] as DataGridViewTextBoxCell;
                cell.Value = temp.p3;
                cell = Commands.Rows[i].Cells[Param4.Index] as DataGridViewTextBoxCell;
                cell.Value = temp.p4;
            }

            setWPParams();

            try
            {

                DataGridViewTextBoxCell cellhome;
                cellhome = Commands.Rows[0].Cells[Lat.Index] as DataGridViewTextBoxCell;
                if (cellhome.Value != null)
                {
                    if (cellhome.Value.ToString() != TXT_homelat.Text && cellhome.Value.ToString() != "0")
                    {
                        DialogResult dr = CustomMessageBox.Show("Reset Home to loaded coords", "Reset Home Coords", MessageBoxButtons.YesNo);

                        if (dr == DialogResult.Yes)
                        {
                            TXT_homelat.Text = (double.Parse(cellhome.Value.ToString())).ToString();
                            cellhome = Commands.Rows[0].Cells[Lon.Index] as DataGridViewTextBoxCell;
                            TXT_homelng.Text = (double.Parse(cellhome.Value.ToString())).ToString();
                            cellhome = Commands.Rows[0].Cells[Alt.Index] as DataGridViewTextBoxCell;
                            TXT_homealt.Text = (double.Parse(cellhome.Value.ToString()) * CurrentState.multiplierdist).ToString();
                        }
                    }
                }
            }
            catch (Exception ex) { log.Error(ex.ToString()); } // if there is no valid home

            if (Commands.RowCount > 0)
            {
                log.Info("remove home from list");
                Commands.Rows.Remove(Commands.Rows[0]); // remove home row
            }

            quickadd = false;

            writeKML();

            Joysticknutton.ZoomAndCenterMarkers("objects");

            MainMap_OnMapZoomChanged();
        }

        void setWPParams()
        {
            try
            {
                log.Info("Loading wp params");

                Hashtable param = new Hashtable((Hashtable)MainV2.comPort.MAV.param);

                if (param["WP_RADIUS"] != null)
                {
                    TXT_WPRad.Text = ((int)((float)param["WP_RADIUS"] * CurrentState.multiplierdist)).ToString();
                }
                if (param["WPNAV_RADIUS"] != null)
                {
                    TXT_WPRad.Text = ((int)((float)param["WPNAV_RADIUS"] * CurrentState.multiplierdist / 100)).ToString();
                }

                log.Info("param WP_RADIUS " + TXT_WPRad.Text);

                try
                {
                    TXT_loiterrad.Enabled = false;
                    if (param["LOITER_RADIUS"] != null)
                    {
                        TXT_loiterrad.Text = ((int)((float)param["LOITER_RADIUS"] * CurrentState.multiplierdist)).ToString();
                        TXT_loiterrad.Enabled = true;
                    }
                    else if (param["WP_LOITER_RAD"] != null)
                    {
                        TXT_loiterrad.Text = ((int)((float)param["WP_LOITER_RAD"] * CurrentState.multiplierdist)).ToString();
                        TXT_loiterrad.Enabled = true;
                    }

                    log.Info("param LOITER_RADIUS " + TXT_loiterrad.Text);
                }
                catch
                {

                }
            }
            catch (Exception ex) { log.Error(ex); }
        }

        /// <summary>
        /// Saves this forms config to MAIN, where it is written in a global config
        /// </summary>
        /// <param name="write">true/false</param>
        private void config(bool write)
        {
            if (write)
            {
                MainV2.config["TXT_homelat"] = TXT_homelat.Text;
                MainV2.config["TXT_homelng"] = TXT_homelng.Text;
                MainV2.config["TXT_homealt"] = TXT_homealt.Text;


              MainV2.config["TXT_WPRad"] = TXT_WPRad.Text;

               MainV2.config["TXT_loiterrad"] = TXT_loiterrad.Text;

                MainV2.config["TXT_DefaultAlt"] = TXT_DefaultAlt.Text;

                MainV2.config["CMB_altmode"] = CMB_altmode.Text;

                MainV2.config["fpminaltwarning"] = TXT_altwarn.Text;

                MainV2.config["fpcoordmouse"] = coords1.System;
            }
            else
            {
                Hashtable temp = new Hashtable((Hashtable)MainV2.config.Clone());

                foreach (string key in temp.Keys)
                {
                    switch (key)
                    {
                        case "TXT_WPRad":
                            TXT_WPRad.Text = MainV2.config[key].ToString();
                            break;
                        case "TXT_loiterrad":
                            TXT_loiterrad.Text = MainV2.config[key].ToString();
                            break;
                        case "TXT_DefaultAlt":
                            TXT_DefaultAlt.Text = MainV2.config[key].ToString();
                            break;
                        case "CMB_altmode":
                            CMB_altmode.Text = MainV2.config[key].ToString();
                            break;
                        case "fpminaltwarning":
                            TXT_altwarn.Text = MainV2.getConfig("fpminaltwarning");
                            break;
                        case "fpcoordmouse":
                            coords1.System = MainV2.config[key].ToString();
                            break;
                        default:
                            break;
                    }
                }

            }
        }

        private void TXT_WPRad_KeyPress(object sender, KeyPressEventArgs e)
        {
            int isNumber = 0;
            if (e.KeyChar.ToString() == "\b")
                return;
            e.Handled = !int.TryParse(e.KeyChar.ToString(), out isNumber);
        }

        private void TXT_WPRad_Leave(object sender, EventArgs e)
        {
            int isNumber = 0;
            if (!int.TryParse(TXT_WPRad.Text, out isNumber))
            {
                TXT_WPRad.Text = "30";
            }
            if (isNumber > (127 * CurrentState.multiplierdist))
            {
                //CustomMessageBox.Show("The value can only be between 0 and 127 m");
                //TXT_WPRad.Text = (127 * CurrentState.multiplierdist).ToString();
            }
            writeKML();
        }

        private void TXT_loiterrad_KeyPress(object sender, KeyPressEventArgs e)
        {
            int isNumber = 0;
            if (e.KeyChar.ToString() == "\b")
                return;

            if (e.KeyChar == '-')
                return;

            e.Handled = !int.TryParse(e.KeyChar.ToString(), out isNumber);
        }

        private void TXT_loiterrad_Leave(object sender, EventArgs e)
        {
            int isNumber = 0;
            if (!int.TryParse(TXT_loiterrad.Text, out isNumber))
            {
                TXT_loiterrad.Text = "45";
            }
        }

        private void TXT_DefaultAlt_KeyPress(object sender, KeyPressEventArgs e)
        {
            int isNumber = 0;
            if (e.KeyChar.ToString() == "\b")
                return;
            e.Handled = !int.TryParse(e.KeyChar.ToString(), out isNumber);
        }

        private void TXT_DefaultAlt_Leave(object sender, EventArgs e)
        {
            int isNumber = 0;
            if (!int.TryParse(TXT_DefaultAlt.Text, out isNumber))
            {
                TXT_DefaultAlt.Text = "100";
            }
        }


        /// <summary>
        /// used to control buttons in the datagrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Commands_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0)
                    return;
                if (e.ColumnIndex == Delete.Index && (e.RowIndex + 0) < Commands.RowCount) // delete
                {
                    quickadd = true;
                    Commands.Rows.RemoveAt(e.RowIndex);
                    quickadd = false;
                    
                    //Refresh_my_PTs();
                    writeKML();
                }
                if (e.ColumnIndex == Up.Index && e.RowIndex != 0) // up
                {
                    DataGridViewRow myrow = Commands.CurrentRow;
                    Commands.Rows.Remove(myrow);
                    Commands.Rows.Insert(e.RowIndex - 1, myrow);
                    writeKML();
                }
                if (e.ColumnIndex == Down.Index && e.RowIndex < Commands.RowCount - 1) // down
                {
                    DataGridViewRow myrow = Commands.CurrentRow;
                    Commands.Rows.Remove(myrow);
                    Commands.Rows.Insert(e.RowIndex + 1, myrow);
                    writeKML();
                }
                setgradanddistandaz();
            }
            catch (Exception) { CustomMessageBox.Show("Row error"); }
        }

        private void Commands_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells[Delete.Index].Value = "X";
            e.Row.Cells[Up.Index].Value = Resources.up;
            e.Row.Cells[Down.Index].Value = Resources.down;
        }

        private void TXT_homelat_TextChanged(object sender, EventArgs e)
        {
            sethome = false;
            try
            {
                MainV2.comPort.MAV.cs.HomeLocation.Lat = double.Parse(TXT_homelat.Text);
            }
            catch { }
            writeKML();

        }

        private void TXT_homelng_TextChanged(object sender, EventArgs e)
        {
            sethome = false;
            try
            {
                MainV2.comPort.MAV.cs.HomeLocation.Lng = double.Parse(TXT_homelng.Text);
            }
            catch { }
            writeKML();
        }

        private void TXT_homealt_TextChanged(object sender, EventArgs e)
        {
            sethome = false;
            try
            {
                MainV2.comPort.MAV.cs.HomeLocation.Alt = double.Parse(TXT_homealt.Text);
            }
            catch { }
            writeKML();
        }

        private void BUT_loadwpfile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.Filter = "All Supported Types|*.txt;*.shp|Ardupilot Mission (*.txt)|*.*|Shape file|*.shp";
                DialogResult result = fd.ShowDialog();
                string file = fd.FileName;

                if (File.Exists(file))
                {
                    if (file.ToLower().EndsWith(".shp"))
                    {
                        LoadSHPFile(file);
                    }
                    else
                    {
                        wpfilename = file;
                        readQGC110wpfile(file);
                    }

                    lbl_wpfile.Text = "Loaded " + Path.GetFileName(file);
                }
            }
        }

        public void readQGC110wpfile(string file, bool append = false)
        {
            int wp_count = 0;
            bool error = false;
            List<Locationwp> cmds = new List<Locationwp>();

            try
            {
                StreamReader sr = new StreamReader(file); //"defines.h"
                string header = sr.ReadLine();
                if (header == null || !header.Contains("QGC WPL"))
                {
                    CustomMessageBox.Show("Invalid Waypoint file");
                    return;
                }

                while (!error && !sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    // waypoints

                    if (line.StartsWith("#"))
                        continue;

                    string[] items = line.Split(new[] { '\t', ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

                    if (items.Length <= 9)
                        continue;

                    try
                    {

                        Locationwp temp = new Locationwp();
                        if (items[2] == "3")
                        { // abs MAV_FRAME_GLOBAL_RELATIVE_ALT=3
                            temp.options = 1;
                        }
                        else
                        {
                            temp.options = 0;
                        }
                        temp.id = (byte)(int)Enum.Parse(typeof(MAVLink.MAV_CMD), items[3], false);
                        temp.p1 = float.Parse(items[4], new CultureInfo("en-US"));

                        if (temp.id == 99)
                            temp.id = 0;

                        temp.alt = (float)(double.Parse(items[10], new CultureInfo("en-US")));
                        temp.lat = (double.Parse(items[8], new CultureInfo("en-US")));
                        temp.lng = (double.Parse(items[9], new CultureInfo("en-US")));

                        temp.p2 = (float)(double.Parse(items[5], new CultureInfo("en-US")));
                        temp.p3 = (float)(double.Parse(items[6], new CultureInfo("en-US")));
                        temp.p4 = (float)(double.Parse(items[7], new CultureInfo("en-US")));

                        cmds.Add(temp);

                        wp_count++;

                    }
                    catch { CustomMessageBox.Show("Line invalid\n" + line); }
                }

                sr.Close();

                processToScreen(cmds, append);

                writeKML();

                Joysticknutton.ZoomAndCenterMarkers("objects");
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Can't open file! " + ex);
            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                lock (thisLock)
                {
                    Joysticknutton.Zoom = trackBar1.Value;
                }
            }
            catch { }
        }

        double calcpolygonarea(List<PointLatLng> polygon)
        {
            // should be a closed polygon
            // coords are in lat long
            // need utm to calc area

            if (polygon.Count == 0)
            {
                CustomMessageBox.Show("Please define a polygon!");
                return 0;
            }

            // close the polygon
            if (polygon[0] != polygon[polygon.Count - 1])
                polygon.Add(polygon[0]); // make a full loop

            CoordinateTransformationFactory ctfac = new CoordinateTransformationFactory();

            GeographicCoordinateSystem wgs84 = GeographicCoordinateSystem.WGS84;

            int utmzone = (int)((polygon[0].Lng - -186.0) / 6.0);

            IProjectedCoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(utmzone, polygon[0].Lat < 0 ? false : true);

            ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(wgs84, utm);

            double prod1 = 0;
            double prod2 = 0;

            for (int a = 0; a < (polygon.Count - 1); a++)
            {
                double[] pll1 = { polygon[a].Lng, polygon[a].Lat };
                double[] pll2 = { polygon[a + 1].Lng, polygon[a + 1].Lat };

                double[] p1 = trans.MathTransform.Transform(pll1);
                double[] p2 = trans.MathTransform.Transform(pll2);

                prod1 += p1[0] * p2[1];
                prod2 += p1[1] * p2[0];
            }

            double answer = (prod1 - prod2) / 2;

            if (polygon[0] == polygon[polygon.Count - 1])
                polygon.RemoveAt(polygon.Count - 1); // unmake a full loop

            return answer;
        }

        // marker
        GMapMarker currentMarker;
        GMapMarker center = new GMarkerGoogle(new PointLatLng(0.0, 0.0), GMarkerGoogleType.none);

        // polygons
        GMapPolygon wppolygon;
        internal GMapPolygon drawnpolygon;
        GMapPolygon geofencepolygon;
        // setup geofence
        List<GMapPolygon> DrawingPolyList = new List<GMapPolygon>();


        // layers
        GMapOverlay top; // not currently used
        public static GMapOverlay objectsoverlay; // where the markers a drawn
        public static GMapOverlay routesoverlay;// static so can update from gcs
        public static GMapOverlay currentwp; //Current wp marker
        public static GMapOverlay DrawingsofPolys; //Current wp marker
        public static GMapOverlay polygonsoverlay; // where the track is drawn
        public static GMapOverlay trackoverlay; // where the  error track is drawn
        public static GMapOverlay trackoverlay2; // where the  error track is drawn
        public static GMapOverlay airportsoverlay;
        public static GMapOverlay poioverlay = new GMapOverlay("POI"); // poi layer
        GMapOverlay drawnpolygonsoverlay;
        GMapOverlay kmlpolygonsoverlay;
        GMapOverlay geofenceoverlay;
        static GMapOverlay rallypointoverlay;

        // etc
        readonly Random rnd = new Random();
        string mobileGpsLog = string.Empty;
        GMapMarkerRect CurentRectMarker;
        GMapMarkerRect clickedmarkerobj;
        GMapMarkerRallyPt CurrentRallyPt;
        GMapMarker CurrentGMapMarker;
        bool isMouseDown;
        bool isMouseDraging;
        bool isMouseClickOffMenu;
        PointLatLng MouseDownStart;
        internal PointLatLng MouseDownEnd;
        public int clickedmarker = 0;
        //public long ElapsedMilliseconds;

        #region -- map events --
        void MainMap_OnMarkerLeave(GMapMarker item)
        {
            if (!isMouseDown)
            {
                try
                {
                    if (item is GMapMarkerRect)
                    {
                        CurentRectMarker = null;
                        GMapMarkerRect rc = item as GMapMarkerRect;
                        markerjustleft = rc;

                        int answer = int.Parse(rc.InnerMarker.Tag.ToString());

                        rc.ResetColor();

                        Joysticknutton.Invalidate(false);
                    }
                    if (item is GMapMarkerRallyPt)
                    {
                        CurrentRallyPt = null;
                    }
                    if (item is GMapMarker)
                    {
                        // when you click the context menu this triggers and causes problems
                        CurrentGMapMarker = null;
                    }

                }
                catch { }
            }
            
        }

        void MainMap_OnMarkerEnter(GMapMarker item)
        {
            if (!isMouseDown)
            {
                if (item is GMapMarkerWP)
                    {
                    GMapMarkerRect rc = item as GMapMarkerRect;
                   
                    try
                    {

                        rc.Pen.Color = Color.Red;

                        Joysticknutton.Invalidate(false);
                    }
                    catch { }
                }
                if (item is GMapMarkerRect )
                {
                    GMapMarkerRect rc = item as GMapMarkerRect;
                    try
                    {
                        
                        int answer1 = int.Parse(rc.InnerMarker.Tag.ToString());
                        rc.Pen.Color = Color.Red;

                        
                        Joysticknutton.Invalidate(false);
                    }
                    catch { }

                    int answer;
                    if (item.Tag != null && rc.InnerMarker != null && int.TryParse(rc.InnerMarker.Tag.ToString(), out answer))
                    {
                        try
                        {
                            Commands.CurrentCell = Commands[0, answer - 1];
                            item.ToolTipText = "Alt: " + Commands[Alt.Index, answer - 1].Value;
                            item.ToolTipMode = MarkerTooltipMode.Never;
                        }
                        catch { }
                    }

                    CurentRectMarker = rc;
                }
                if (item is GMapMarkerRallyPt)
                {
                    CurrentRallyPt = item as GMapMarkerRallyPt;
                }
                if (item is GMapMarkerAirport)
                {
                    // do nothing - readonly
                    return;
                }
                if (item is GMapMarker)
                {
                    CurrentGMapMarker = item;
                }
            }
        }

        // click on some marker
        void MainMap_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            int answer;
       

            //CustomMessageBox.Show(item.ToString());

            if (item is GMapMarkerRect)
                {
                    try
                    {
                        if (clickedmarker != 0)
                        {
                            clickedmarkerobj.ResetColor();
                        }
                    }
                    catch { }
                    GMapMarkerRect rc = item as GMapMarkerRect;
                   // rc.Pen.Color = Color.Red;
                    // clickedmarkerobj.Pen.Color = Color.Blue;
                    try
                    {
                        answer = int.Parse(rc.InnerMarker.Tag.ToString());
                    if (PlaceAfterPanel.Visible == false)
                    {
                        WPNumber.Value = int.Parse(rc.InnerMarker.Tag.ToString());
                    }
                    else
                    {
                        numericUpDown3.Value = int.Parse(rc.InnerMarker.Tag.ToString());
                    }
                    clickedmarker = answer;

                        clickedmarkerobj = rc;
                        //CustomMessageBox.Show(answer.ToString());
                        
                        panel6.Visible = true;
                        
                }
                    catch { }


                }
                else if (item is GMapMarkerWP)
                {

                    GMapMarkerWP rctemp = item as GMapMarkerWP;
                    try
                    {
                        if (clickedmarker != 0)
                        {
                            clickedmarkerobj.ResetColor();
                        }
                        answer = int.Parse(rctemp.Tag.ToString());
                        clickedmarker = answer;
                       // CustomMessageBox.Show(answer.ToString());
                        
                        panel6.Visible = true;
                    

                    }
                    catch { }
                    
                    
                }
            
            
            try // when dragging item can sometimes be null
            {
                
                if (item.Tag == null)
                {
                    // home.. etc
                    return;
                }

                if (ModifierKeys == Keys.Control)
                {
                    try
                    {
                        groupmarkeradd(item);

                        log.Info("add marker to group");
                    }
                    catch { }
                }
                if (int.TryParse(item.Tag.ToString(), out answer))
                {

                    Commands.CurrentCell = Commands[0, answer - 1];
                    if (PlaceAfterPanel.Visible == false)
                    {
                        WPNumber.Value = answer;
                    }
                    else
                    {
                        numericUpDown3.Value = answer;
                    }
                }
            }
            catch { }
            try { WPNumberUpdate(); }
            catch { }
        }

        void MainMap_OnMapTypeChanged(GMapProvider type)
        {
            comboBoxMapType.SelectedItem = Joysticknutton.MapProvider;

            Joysticknutton.ZoomAndCenterMarkers("objects");

            if (type == WMSProvider.Instance)
            {
                string url = "";
                if (MainV2.config["WMSserver"] != null)
                    url = MainV2.config["WMSserver"].ToString();
                if (DialogResult.Cancel == InputBox.Show("WMS Server", "Enter the WMS server URL", ref url))
                    return;

                string szCapabilityRequest = url + "?version=1.1.0&Request=GetCapabilities&service=WMS";

                XmlDocument xCapabilityResponse = MakeRequest(szCapabilityRequest);
                ProcessWmsCapabilitesRequest(xCapabilityResponse);

                MainV2.config["WMSserver"] = url;
                WMSProvider.CustomWMSURL = url;
            }
        }

        /**
        * This function requests an XML document from a webserver.
        * @param requestUrl The request url as a string including. Example: http://129.206.228.72/cached/hillshade?Request=GetCapabilities
        * @return An XML document containing the response.
        */
        private XmlDocument MakeRequest(string requestUrl)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;


                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(response.GetResponseStream());
                return (xmlDoc);


            }
            catch (Exception e)
            {


                CustomMessageBox.Show("Failed to make WMS Server request: " + e.Message);
                return null;
            }
        }


        /**
         * This function parses a WMS server capabilites response.
         */
        private void ProcessWmsCapabilitesRequest(XmlDocument xCapabilitesResponse)
        {
            //Create namespace manager
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xCapabilitesResponse.NameTable);

            //check if the response is a valid xml document - if not, the server might still be able to serve us but all the checks below would fail. example: http://tiles.kartat.kapsi.fi/peruskartta
            //best sign is that there is no node WMT_MS_Capabilities
            if (xCapabilitesResponse.SelectNodes("//WMT_MS_Capabilities", nsmgr).Count == 0)
                return;


            //first, we have to make sure that the server is able to send us png imagery
            bool bPngCapable = false;
            XmlNodeList getMapElements = xCapabilitesResponse.SelectNodes("//GetMap", nsmgr);
            if (getMapElements.Count != 1)
                CustomMessageBox.Show("Invalid WMS Server response: Invalid number of GetMap elements.");
            else
            {
                XmlNode getMapNode = getMapElements.Item(0);
                //search through all format nodes for image/png
                foreach (XmlNode formatNode in getMapNode.SelectNodes("//Format", nsmgr))
                {
                    if (formatNode.InnerText.Contains("image/png"))
                    {
                        bPngCapable = true;
                        break;
                    }
                }
            }


            if (!bPngCapable)
            {
                CustomMessageBox.Show("Invalid WMS Server response: Server unable to return PNG images.");
                return;
            }


            //now search through all layer -> srs nodes for EPSG:4326 compatibility
            bool bEpsgCapable = false;
            XmlNodeList srsELements = xCapabilitesResponse.SelectNodes("//SRS", nsmgr);
            foreach (XmlNode srsNode in srsELements)
            {
                if (srsNode.InnerText.Contains("EPSG:4326"))
                {
                    bEpsgCapable = true;
                    break;
                }
            }


            if (!bEpsgCapable)
            {
                CustomMessageBox.Show("Invalid WMS Server response: Server unable to return EPSG:4326 / WGS84 compatible images.");
                return;
            }


            //the server is capable of serving our requests - now check if there is a layer to be selected
            //format: layer -> layer -> name
            string szLayerSelection = "";
            int iSelect = 0;
            List<string> szListLayerName = new List<string>();
            XmlNodeList layerELements = xCapabilitesResponse.SelectNodes("//Layer/Layer/Name", nsmgr);
            foreach (XmlNode nameNode in layerELements)
            {
                szLayerSelection += string.Format("{0}: " + nameNode.InnerText + ", ", iSelect); //mixing control and formatting is not optimal...
                szListLayerName.Add(nameNode.InnerText);
                iSelect++;
            }


            //only select layer if there is one
            if (szListLayerName.Count != 0)
            {
                //now let the user select a layer
                string szUserSelection = "";
                if (DialogResult.Cancel == InputBox.Show("WMS Server", "The following layers were detected: " + szLayerSelection + "please choose one by typing the associated number.", ref szUserSelection))
                    return;
                int iUserSelection = 0;
                try
                {
                    iUserSelection = Convert.ToInt32(szUserSelection);
                }
                catch
                {
                    iUserSelection = 0; //ignore all errors and default to first layer
                }

                WMSProvider.szWmsLayer = szListLayerName[iUserSelection];
            }
        }

        void groupmarkeradd(GMapMarker marker)
        {
            groupmarkers.Add(int.Parse(marker.Tag.ToString()));
            if (marker is GMapMarkerWP)
            {
                ((GMapMarkerWP)marker).selected = true;
            }
            if (marker is GMapMarkerRect)
            {
                ((GMapMarkerWP)((GMapMarkerRect)marker).InnerMarker).selected = true;
            }
        }
        PointLatLng start_point;
        double total_measurement = 0;
        bool first_measurementpt = false;
        int measure_mode = 0;
        int measure_unit_flag = 0; //0: km  1: m    2: Nm 
        int new_poly_index = 0;
        
        void drawpolys()
        {
            drawnpolygonsoverlay.Clear();
            List<PointLatLng> TempPointHolder = new List<PointLatLng>();
            for (int i=0; i< list_of_polys.Count; i++)
            {
                list_of_polys[i].IsHitTestVisible = true;
                drawnpolygonsoverlay.Polygons.Add(list_of_polys[i]);
                TempPointHolder=list_of_polys[i].Points;
                //GMapMarker Marker_Measure = new GMarkerGoogle(new PointLatLng(MouseDownStart.Lat, MouseDownStart.Lng), GMarkerGoogleType.orange);
                for (int j = 0; j < TempPointHolder.Count; j++)
                {
                    //CustomMessageBox.Show(TempPointHolder.Count.ToString());
                    drawnpolygonsoverlay.Markers.Add(new GMarkerCross(TempPointHolder[j]));
                    
                }

            }
            //Joysticknutton.Overlays.Add(drawnpolygonsoverlay);
        }
        int new_poly_entered_flag = 0;
        int selected_poly_index = 0;
        int first_selection_of_polygon = 0;
        void MainMap_PolygonSelected(object item, MouseEventArgs e)
        {
            if (new_polygon_flag == false && DrawPanel.Visible == true)
            {
                GMapPolygon poly_temp = item as GMapPolygon;
                selected_poly_index = list_of_polys.IndexOf(poly_temp);
                PolyNameBox.Text = list_of_polys[selected_poly_index].Name;
                SolidBrush PolyColor = (SolidBrush)list_of_polys[selected_poly_index].Fill;
                ColorBTN.BackColor = Color.FromArgb(255, PolyColor.Color);
                // CustomMessageBox.Show(selected_poly_index.ToString());
                polygon_edit_flag = true;
                PolyNameBox.Enabled = true;
                ApplyPoly.Enabled = true;
                DeletePoly.Enabled = true;
                ColorBTN.Enabled = true;
            }

        }
        void MainMap_ShowPolygonName(object item)
        {
            if (new_poly_entered_flag == 0)
            {
                PolgonToolTipMarker = new GMapMarkerRect(Joysticknutton.FromLocalToLatLng(MousePosition.X, MousePosition.Y));
                PolgonToolTipMarker.ToolTip = new GMapToolTip(PolgonToolTipMarker);
                PolgonToolTipMarker.ToolTipMode = MarkerTooltipMode.Always;
                //CustomMessageBox.Show("Test");
                new_poly_entered_flag = 1;
                drawnpolygonsoverlay.Markers.Add(PolgonToolTipMarker);
                PolgonToolTipMarker.Position = (Joysticknutton.FromLocalToLatLng(MousePosition.X, MousePosition.Y));
                GMapPolygon poly_temp = item as GMapPolygon;
                PolgonToolTipMarker.ToolTipText = poly_temp.Name;
            }
            


        }
        void MainMap_HidePolygonName(object sender)
        {
            PolgonToolTipMarker.ToolTipMode = MarkerTooltipMode.Never;
            new_poly_entered_flag = 0;
            //DeletePoly.Text = "Outside";
        }
        void MainMap_MouseUp(object sender, MouseEventArgs e)
        {
            if (isMouseClickOffMenu)
            {
                isMouseClickOffMenu = false;
                return;
            }

            MouseDownEnd = Joysticknutton.FromLocalToLatLng(e.X, e.Y);

            // Console.WriteLine("MainMap MU");

            if (e.Button == MouseButtons.Right) // ignore right clicks
            {
                return;
            }
            if (isMouseDown == true && new_polygon_flag == true)
            {
           
                new_polygon_list.Add(new PointLatLng(MouseDownStart.Lat, MouseDownStart.Lng));
                //CustomMessageBox.Show(new_poly_index.ToString());

                if (new_poly_index >= list_of_polys.Count)
                {
                    if (PolyNameBox.Text != null)
                    {
                            String poly_name = PolyNameBox.Text;
                            list_of_polys.Add(new GMapPolygon(new_polygon_list, poly_name));
                    }
                    else
                    {
                        String poly_name = "Area Unnammed" + new_poly_index.ToString("0");
                        list_of_polys.Add(new GMapPolygon(new_polygon_list, poly_name));
                    }
                }
                else
                {
                    if (PolyNameBox.Text != null)
                    {
                        String poly_name = PolyNameBox.Text;
                        list_of_polys[new_poly_index] = new GMapPolygon(new_polygon_list, poly_name);
                    }
                    else
                    {
                        String poly_name = "Area Unnammed" + new_poly_index.ToString("0");
                        list_of_polys[new_poly_index] = new GMapPolygon(new_polygon_list, poly_name);
                    }
                }
                list_of_polys[new_poly_index].Fill = new SolidBrush(Color.FromArgb(20, ColorBTN.BackColor));
                list_of_polys[new_poly_index].Stroke = new Pen(Color.Black, 1);
                drawpolys();

                return;
            }
            else if (isMouseDown == true && polygon_edit_flag == true)
            {
               /* if (first_selection_of_polygon == 0)
                {
                    first_selection_of_polygon = first_selection_of_polygon + 1;
                }
                else
                {
                    String poly_name = PolyNameBox.Text;
                    list_of_polys[selected_poly_index].Points.Add(new PointLatLng(MouseDownStart.Lat, MouseDownStart.Lng));
                    list_of_polys[selected_poly_index].Name = poly_name;
                    list_of_polys[selected_poly_index].Fill = new SolidBrush(Color.FromArgb(20, ColorBTN.BackColor));
                    list_of_polys[selected_poly_index].Stroke = new Pen(Color.Black, 1);
                    drawpolys();
                }*/

                return;
            }
                if (isMouseDown == true && measure_mode == 1)
            {
                // Add Pt.
                GMapMarker Marker_Measure = new GMarkerGoogle(new PointLatLng(MouseDownStart.Lat, MouseDownStart.Lng), GMarkerGoogleType.orange);
                if (first_measurementpt == false)
                {
                    Marker_Measure.ToolTip = new GMapToolTip(Marker_Measure);
                    Marker_Measure.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                    total_measurement = total_measurement + Joysticknutton.MapProvider.Projection.GetDistance(start_point, new PointLatLng(MouseDownStart.Lat, MouseDownStart.Lng)) * 1000;
                    Marker_Measure.ToolTipText = "Dist: " + ((Joysticknutton.MapProvider.Projection.GetDistance(start_point, new PointLatLng(MouseDownStart.Lat, MouseDownStart.Lng)) * 1000)).ToString("0") + "\nBearing: " + ((Joysticknutton.MapProvider.Projection.GetBearing(start_point, new PointLatLng(MouseDownStart.Lat, MouseDownStart.Lng)))).ToString("0");
                    if (measure_unit_flag == 0)
                    {
                        measureLabel.Text = total_measurement.ToString("0");
                    }
                    else if (measure_unit_flag == 1)
                    {
                        measureLabel.Text = (total_measurement / 1000.0).ToString("F2");
                    }
                    else if (measure_unit_flag == 2)
                    {
                        measureLabel.Text = (total_measurement / 1852).ToString("F2");
                    }
                }
                MeasurePts.Markers.Add(Marker_Measure);
                MeasureRoute.Points.Add(new PointLatLng(MouseDownStart.Lat, MouseDownStart.Lng));
                MeasurePts.Routes.Add(MeasureRoute);
                first_measurementpt = false;
                start_point = new PointLatLng(MouseDownStart.Lat, MouseDownStart.Lng);
                isMouseDown = false;
                return;
            }
            if (isMouseDown) // mouse down on some other object and dragged to here.
            {
                if (e.Button == MouseButtons.Left)
                {
                    isMouseDown = false;
                }
                if (ModifierKeys == Keys.Control)
                {
                    // group select wps
                    GMapPolygon poly = new GMapPolygon(new List<PointLatLng>(), "temp");

                    poly.Points.Add(MouseDownStart);
                    poly.Points.Add(new PointLatLng(MouseDownStart.Lat, MouseDownEnd.Lng));
                    poly.Points.Add(MouseDownEnd);
                    poly.Points.Add(new PointLatLng(MouseDownEnd.Lat, MouseDownStart.Lng));

                    foreach (var marker in objectsoverlay.Markers)
                    {
                        if (poly.IsInside(marker.Position))
                        {
                            try
                            {
                                if (marker.Tag != null)
                                {
                                    groupmarkeradd(marker);
                                }
                            }
                            catch { }
                        }
                    }

                    isMouseDraging = false;
                    return;
                }
                if (!isMouseDraging)
                {
                    if (CurentRectMarker != null)
                    {
                        // cant add WP in existing rect
                    }
                    else
                    {
                        AddWPToMap(currentMarker.Position.Lat, currentMarker.Position.Lng, 0);
                    }
                }
                else
                {
                  
                    if (groupmarkers.Count > 0)
                    {
                        Dictionary<string, PointLatLng> dest = new Dictionary<string, PointLatLng>();

                        foreach (var markerid in groupmarkers)
                        {
                            for (int a = 0; a < objectsoverlay.Markers.Count; a++)
                            {
                                var marker = objectsoverlay.Markers[a];

                                if (marker.Tag != null && marker.Tag.ToString() == markerid.ToString())
                                {
                                    dest[marker.Tag.ToString()] = marker.Position;
                                    break;
                                }
                            }
                        }

                        foreach (KeyValuePair<string, PointLatLng> item in dest)
                        {
                            var value = item.Value;
                            callMeDrag(item.Key, value.Lat, value.Lng, -1);
                        }

                        Joysticknutton.SelectedArea = RectLatLng.Empty;
                        groupmarkers.Clear();
                        // redraw to remove selection
                        writeKML();

                        CurentRectMarker = null;
                    }

                    if (CurentRectMarker != null)
                    {
                        if (CurentRectMarker.InnerMarker.Tag.ToString().Contains("grid"))
                        {
                            try
                            {
                                drawnpolygon.Points[int.Parse(CurentRectMarker.InnerMarker.Tag.ToString().Replace("grid", "")) - 1] = new PointLatLng(MouseDownEnd.Lat, MouseDownEnd.Lng);
                                Joysticknutton.UpdatePolygonLocalPosition(drawnpolygon);
                                Joysticknutton.Invalidate();
                            }
                            catch { }
                        }
                        else
                        {
                            callMeDrag(CurentRectMarker.InnerMarker.Tag.ToString(), currentMarker.Position.Lat, currentMarker.Position.Lng, -1);
                        }
                        CurentRectMarker = null;
                    }
                }
            }

            isMouseDraging = false;
        }

        void MainMap_MouseDown(object sender, MouseEventArgs e)
        {
            if (isMouseClickOffMenu)
                return;

            MouseDownStart = Joysticknutton.FromLocalToLatLng(e.X, e.Y);

            //   Console.WriteLine("MainMap MD");

            if (e.Button == MouseButtons.Left && (groupmarkers.Count > 0 || ModifierKeys == Keys.Control))
            {
                // group move
                isMouseDown = true;
                isMouseDraging = false;

                return;
            }

            if (e.Button == MouseButtons.Left && ModifierKeys != Keys.Alt && ModifierKeys != Keys.Control)
            {
                isMouseDown = true;
                isMouseDraging = false;

                if (currentMarker.IsVisible)
                {
                    currentMarker.Position = Joysticknutton.FromLocalToLatLng(e.X, e.Y);
                }
            }
        }

        // move current marker with left holding
        void MainMap_MouseMove(object sender, MouseEventArgs e)
        {
            //myButton1.Text = MainV2.comPort.MAV.cs.mode.ToUpper();
            //setWPParams();
            
            PointLatLng point = Joysticknutton.FromLocalToLatLng(e.X, e.Y);

            if (MouseDownStart == point)
                return;

            //  Console.WriteLine("MainMap MM " + point);

            currentMarker.Position = point;

            if (!isMouseDown)
            {
                // update mouse pos display
                SetMouseDisplay(point.Lat, point.Lng, 0);
            }

            //draging
            if (e.Button == MouseButtons.Left && isMouseDown)
            {
                isMouseDraging = true;
                currentwp.Clear();
                if (CurrentRallyPt != null)
                {
                    PointLatLng pnew = Joysticknutton.FromLocalToLatLng(e.X, e.Y);

                    CurrentRallyPt.Position = pnew;
                }
                else if (groupmarkers.Count > 0)
                {
                    // group drag

                    double latdif = MouseDownStart.Lat - point.Lat;
                    double lngdif = MouseDownStart.Lng - point.Lng;

                    MouseDownStart = point;

                    Hashtable seen = new Hashtable();

                    foreach (var markerid in groupmarkers)
                    {
                        if (seen.ContainsKey(markerid))
                            continue;

                        seen[markerid] = 1;
                        for (int a = 0; a < objectsoverlay.Markers.Count; a++)
                        {
                            var marker = objectsoverlay.Markers[a];

                            if (marker.Tag != null && marker.Tag.ToString() == markerid.ToString())
                            {
                                var temp = new PointLatLng(marker.Position.Lat, marker.Position.Lng);
                                temp.Offset(latdif, -lngdif);

                                marker.Position = temp;
                            }
                        }
                    }
                }
                else if (CurentRectMarker != null) // left click pan
                {
                    try
                    {
                        // check if this is a grid point
                        if (CurentRectMarker.InnerMarker.Tag.ToString().Contains("grid"))
                        {
                            drawnpolygon.Points[int.Parse(CurentRectMarker.InnerMarker.Tag.ToString().Replace("grid", "")) - 1] = new PointLatLng(point.Lat, point.Lng);
                            Joysticknutton.UpdatePolygonLocalPosition(drawnpolygon);
                            Joysticknutton.Invalidate();

                        }
                    }
                    catch { }

                    PointLatLng pnew = Joysticknutton.FromLocalToLatLng(e.X, e.Y);

                    // adjust polyline point while we drag
                    try
                    {
                        if (CurrentGMapMarker != null && CurrentGMapMarker.Tag is int)
                        {

                            int? pIndex = (int?)CurentRectMarker.Tag;
                            if (pIndex.HasValue)
                            {
                                if (pIndex < wppolygon.Points.Count)
                                {
                                    wppolygon.Points[pIndex.Value] = pnew;
                                    lock (thisLock)
                                    {
                                        Joysticknutton.UpdatePolygonLocalPosition(wppolygon);
                                    }
                                }
                            }
                        }
                    }
                    catch { }

                    // update rect and marker pos.
                    if (currentMarker.IsVisible)
                    {
                        currentMarker.Position = pnew;
                        
                        
                    }
                    CurentRectMarker.Position = pnew;

                    if (CurentRectMarker.InnerMarker != null)
                    {
                        CurentRectMarker.InnerMarker.Position = pnew;
                    }
                }
                else if (CurrentGMapMarker != null)
                {
                    PointLatLng pnew = Joysticknutton.FromLocalToLatLng(e.X, e.Y);

                    CurrentGMapMarker.Position = pnew;
                }
                else if (ModifierKeys == Keys.Control)
                {
                    // draw selection box
                    double latdif = MouseDownStart.Lat - point.Lat;
                    double lngdif = MouseDownStart.Lng - point.Lng;

                    Joysticknutton.SelectedArea = new RectLatLng(Math.Max(MouseDownStart.Lat, point.Lat), Math.Min(MouseDownStart.Lng, point.Lng), Math.Abs(lngdif), Math.Abs(latdif));
                }
                else // left click pan
                {
                    double latdif = MouseDownStart.Lat - point.Lat;
                    double lngdif = MouseDownStart.Lng - point.Lng;

                    try
                    {
                        lock (thisLock)
                        {
                            Joysticknutton.Position = new PointLatLng(center.Position.Lat + latdif, center.Position.Lng + lngdif);
                        }
                    }
                    catch { }
                }

            }
        }

        // MapZoomChanged
        void MainMap_OnMapZoomChanged()
        {
            if (Joysticknutton.Zoom > 0)
            {
                try
                {
                    trackBar1.Value = (int)(Joysticknutton.Zoom);
                }
                catch { }
                //textBoxZoomCurrent.Text = MainMap.Zoom.ToString();
                center.Position = Joysticknutton.Position;
             
            }
        }



        // loader start loading tiles
        void MainMap_OnTileLoadStart()
        {
            MethodInvoker m = delegate
            {
                lbl_status.Text = "Status: loading tiles...";
            };
            try
            {
                BeginInvoke(m);
            }
            catch
            {
            }
        }

        // loader end loading tiles
        void MainMap_OnTileLoadComplete(long ElapsedMilliseconds)
        {

            //MainMap.ElapsedMilliseconds = ElapsedMilliseconds;

            MethodInvoker m = delegate
            {
                lbl_status.Text = "Status: loaded tiles";

                //panelMenu.Text = "Menu, last load in " + MainMap.ElapsedMilliseconds + "ms";

                //textBoxMemory.Text = string.Format(CultureInfo.InvariantCulture, "{0:0.00}MB of {1:0.00}MB", MainMap.Manager.MemoryCacheSize, MainMap.Manager.MemoryCacheCapacity);
            };
            try
            {
                if (!IsDisposed)
                    BeginInvoke(m);
            }
            catch
            {
            }

        }

        // current point changed
        void MainMap_OnCurrentPositionChanged(PointLatLng point)
        {
            if (point.Lat > 90) { point.Lat = 90; }
            if (point.Lat < -90) { point.Lat = -90; }
            if (point.Lng > 180) { point.Lng = 180; }
            if (point.Lng < -180) { point.Lng = -180; }
            center.Position = point;

            coords1.Lat = point.Lat;
            coords1.Lng = point.Lng;

            // always show on planner view
            if (MainV2.ShowAirports)
            {
                airportsoverlay.Clear();
                foreach (var item in Airports.getAirports(Joysticknutton.Position))
                {
                    airportsoverlay.Markers.Add(new GMapMarkerAirport(item) { ToolTipText = item.Tag, ToolTipMode = MarkerTooltipMode.Never });
                }
            }
        }

        // center markers on start
        private void MainForm_Load(object sender, EventArgs e)
        {
            if (objectsoverlay.Markers.Count > 0)
            {
                Joysticknutton.ZoomAndCenterMarkers(null);
            }
            trackBar1.Value = (int)Joysticknutton.Zoom;
        }

        // ensure focus on map, trackbar can have it too
        private void MainMap_MouseEnter(object sender, EventArgs e)
        {
            // MainMap.Focus();
        }
        #endregion


        private void comboBoxMapType_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                Joysticknutton.MapProvider = (GMapProvider)BingSatelliteMapProvider.Instance; //OpenStreetMapProvider.Instance; //BingSatelliteMapProvider.Instance;// OpenStreetMapProvider.Instance;
                FlightData.mymap.MapProvider = (GMapProvider)comboBoxMapType.SelectedItem;
                MainV2.config["MapType"] = comboBoxMapType.Text;
            }
            catch { CustomMessageBox.Show("Map change failed. try zooming out first."); }
        }

        private void Commands_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control.GetType() == typeof(DataGridViewComboBoxEditingControl))
            {
                var temp = ((ComboBox)e.Control);
                ((ComboBox)e.Control).SelectionChangeCommitted -= Commands_SelectionChangeCommitted;
                ((ComboBox)e.Control).SelectionChangeCommitted += Commands_SelectionChangeCommitted;
                ((ComboBox)e.Control).ForeColor = Color.White;
                ((ComboBox)e.Control).BackColor = Color.FromArgb(0x43, 0x44, 0x45);
                Debug.WriteLine("Setting event handle");
            }
        }

        void Commands_SelectionChangeCommitted(object sender, EventArgs e)
        {
            // update row headers
            ((ComboBox)sender).ForeColor = Color.White;
            ChangeColumnHeader(((ComboBox)sender).Text);
            try
            {
                // default takeoff to non 0 alt
                if (((ComboBox)sender).Text == "TAKEOFF")
                {
                    if (Commands.Rows[selectedrow].Cells[Alt.Index].Value != null && Commands.Rows[selectedrow].Cells[Alt.Index].Value.ToString() == "0")
                        Commands.Rows[selectedrow].Cells[Alt.Index].Value = TXT_DefaultAlt.Text;
                }

                for (int i = 0; i < Commands.ColumnCount; i++)
                {
                    DataGridViewCell tcell = Commands.Rows[selectedrow].Cells[i];
                    if (tcell.GetType() == typeof(DataGridViewTextBoxCell))
                    {
                        if (tcell.Value.ToString() == "?")
                            tcell.Value = "0";
                    }
                }
            }
            catch { }
        }
        /// <summary>
        /// Get the Google earth ALT for a given coord
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <returns>Altitude</returns>
        double getGEAlt(double lat, double lng)
        {
            double alt = 0;
            //http://maps.google.com/maps/api/elevation/xml

            try
            {
                using (XmlTextReader xmlreader = new XmlTextReader("http://maps.google.com/maps/api/elevation/xml?locations=" + lat.ToString(new CultureInfo("en-US")) + "," + lng.ToString(new CultureInfo("en-US")) + "&sensor=true"))
                {
                    while (xmlreader.Read())
                    {
                        xmlreader.MoveToElement();
                        switch (xmlreader.Name)
                        {
                            case "elevation":
                                alt = double.Parse(xmlreader.ReadString(), new CultureInfo("en-US"));
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch { }

            return alt * CurrentState.multiplierdist;
        }

        private void TXT_homelat_Enter(object sender, EventArgs e)
        {
            sethome = true;
            CustomMessageBox.Show("Click on the Map to set Home ");
        }

        private void Planner_Resize(object sender, EventArgs e)
        {
            Joysticknutton.Zoom = trackBar1.Value;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            try
            {
                base.OnPaint(pe);
            }
            catch (Exception)
            {
            }
        }

        private void Commands_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            Commands_RowEnter(null, new DataGridViewCellEventArgs(Commands.CurrentCell.ColumnIndex, Commands.CurrentCell.RowIndex));

            writeKML();
        }

        private void MainMap_Resize(object sender, EventArgs e)
        {
            Joysticknutton.Zoom = Joysticknutton.Zoom + 0.01;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            try
            {
                lock (thisLock)
                {
                    Joysticknutton.Zoom = trackBar1.Value;
                }
            }
            catch { }
        }


        private void BUT_Prefetch_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// from http://stackoverflow.com/questions/1119451/how-to-tell-if-a-line-intersects-a-polygon-in-c
        /// </summary>
        /// <param name="start1"></param>
        /// <param name="end1"></param>
        /// <param name="start2"></param>
        /// <param name="end2"></param>
        /// <returns></returns>
        public PointLatLng FindLineIntersection(PointLatLng start1, PointLatLng end1, PointLatLng start2, PointLatLng end2)
        {
            double denom = ((end1.Lng - start1.Lng) * (end2.Lat - start2.Lat)) - ((end1.Lat - start1.Lat) * (end2.Lng - start2.Lng));
            //  AB & CD are parallel         
            if (denom == 0)
                return PointLatLng.Empty;
            double numer = ((start1.Lat - start2.Lat) * (end2.Lng - start2.Lng)) - ((start1.Lng - start2.Lng) * (end2.Lat - start2.Lat));
            double r = numer / denom;
            double numer2 = ((start1.Lat - start2.Lat) * (end1.Lng - start1.Lng)) - ((start1.Lng - start2.Lng) * (end1.Lat - start1.Lat));
            double s = numer2 / denom;
            if ((r < 0 || r > 1) || (s < 0 || s > 1))
                return PointLatLng.Empty;
            // Find intersection point      
            PointLatLng result = new PointLatLng();
            result.Lng = start1.Lng + (r * (end1.Lng - start1.Lng));
            result.Lat = start1.Lat + (r * (end1.Lat - start1.Lat));
            return result;
        }

        RectLatLng getPolyMinMax(GMapPolygon poly)
        {
            if (poly.Points.Count == 0)
                return new RectLatLng();

            double minx, miny, maxx, maxy;

            minx = maxx = poly.Points[0].Lng;
            miny = maxy = poly.Points[0].Lat;

            foreach (PointLatLng pnt in poly.Points)
            {
                //Console.WriteLine(pnt.ToString());
                minx = Math.Min(minx, pnt.Lng);
                maxx = Math.Max(maxx, pnt.Lng);

                miny = Math.Min(miny, pnt.Lat);
                maxy = Math.Max(maxy, pnt.Lat);
            }

            return new RectLatLng(maxy, minx, Math.Abs(maxx - minx), Math.Abs(miny - maxy));
        }

        const float rad2deg = (float)(180 / Math.PI);
        const float deg2rad = (float)(1.0 / rad2deg);

        private void BUT_grid_Click(object sender, EventArgs e)
        {

        }

        private void label4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (MainV2.comPort.MAV.cs.lat != 0)
            {
                TXT_homealt.Text = (MainV2.comPort.MAV.cs.altasl).ToString("0");
                TXT_homelat.Text = MainV2.comPort.MAV.cs.lat.ToString();
                TXT_homelng.Text = MainV2.comPort.MAV.cs.lng.ToString();
            }
            else
            {
                CustomMessageBox.Show("If you're at the field, connect to your APM and wait for GPS lock. Then click 'Home Location' link to set home to your location");
            }
        }


        /// <summary>
        /// Format distance according to prefer distance unit
        /// </summary>
        /// <param name="distInKM">distance in kilometers</param>
        /// <param name="toMeterOrFeet">convert distance to meter or feet if true, covert to km or miles if false</param>
        /// <returns>formatted distance with unit</returns>
        private string FormatDistance(double distInKM, bool toMeterOrFeet)
        {
            string sunits = MainV2.getConfig("distunits");
            Common.distances units = Common.distances.Meters;

            if (sunits != "")
                try
                {
                    units = (Common.distances)Enum.Parse(typeof(Common.distances), sunits);
                }
                catch (Exception) { }

            switch (units)
            {
                case Common.distances.Feet:
                    return toMeterOrFeet ? string.Format((distInKM * 3280.8399).ToString("0.00 ft")) :
                        string.Format((distInKM * 0.621371).ToString("0.0000 miles"));
                case Common.distances.Meters:
                default:
                    return toMeterOrFeet ? string.Format((distInKM * 1000).ToString("0.00 m")) :
                        string.Format(distInKM.ToString("0.0000 km"));
            }
        }

        PointLatLng startmeasure;

        private void ContextMeasure_Click(object sender, EventArgs e)
        {
            if (startmeasure.IsEmpty)
            {
                startmeasure = MouseDownStart;
                polygonsoverlay.Markers.Add(new GMarkerGoogle(MouseDownStart, GMarkerGoogleType.red));
                Joysticknutton.Invalidate();
                Common.MessageShowAgain("Measure Dist", "You can now pan/zoom around.\nClick this option again to get the distance.");
            }
            else
            {
                List<PointLatLng> polygonPoints = new List<PointLatLng>();
                polygonPoints.Add(startmeasure);
                polygonPoints.Add(MouseDownStart);

                GMapPolygon line = new GMapPolygon(polygonPoints, "measure dist");
                line.Stroke.Color = Color.Green;

                polygonsoverlay.Polygons.Add(line);

                polygonsoverlay.Markers.Add(new GMarkerGoogle(MouseDownStart, GMarkerGoogleType.red));
                Joysticknutton.Invalidate();
                CustomMessageBox.Show("Distance: " + FormatDistance(Joysticknutton.MapProvider.Projection.GetDistance(startmeasure, MouseDownStart), true) + " AZ: " + (Joysticknutton.MapProvider.Projection.GetBearing(startmeasure, MouseDownStart).ToString("0")));
                polygonsoverlay.Polygons.Remove(line);
                polygonsoverlay.Markers.Clear();
                startmeasure = new PointLatLng();
            }
        }

        private void rotateMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string heading = "0";
            if (DialogResult.Cancel == InputBox.Show("Rotate map to heading", "Enter new UP heading", ref heading))
                return;
            float ans = 0;
            if (float.TryParse(heading, out ans))
            {
                Joysticknutton.Bearing = ans;
                FlightData.mymap.Bearing = ans;
            }
        }

        private void addPolygonPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (polygongridmode == false)
            {
                CustomMessageBox.Show("You will remain in polygon mode until you clear the polygon or create a grid/upload a fence");
            }

            polygongridmode = true;

            List<PointLatLng> polygonPoints = new List<PointLatLng>();
            if (drawnpolygonsoverlay.Polygons.Count == 0)
            {
                drawnpolygon.Points.Clear();
                drawnpolygonsoverlay.Polygons.Add(drawnpolygon);
            }

            drawnpolygon.Fill = Brushes.Transparent;

            // remove full loop is exists
            if (drawnpolygon.Points.Count > 1 && drawnpolygon.Points[0] == drawnpolygon.Points[drawnpolygon.Points.Count - 1])
                drawnpolygon.Points.RemoveAt(drawnpolygon.Points.Count - 1); // unmake a full loop

            drawnpolygon.Points.Add(new PointLatLng(MouseDownStart.Lat, MouseDownStart.Lng));

            addpolygonmarkergrid(drawnpolygon.Points.Count.ToString(), MouseDownStart.Lng, MouseDownStart.Lat, 0);

            Joysticknutton.UpdatePolygonLocalPosition(drawnpolygon);

            Joysticknutton.Invalidate();

        }

        public void redrawPolygonSurvey(List<PointLatLngAlt> list)
        {
            drawnpolygon.Points.Clear();
            drawnpolygonsoverlay.Clear();

            int tag = 0;
            list.ForEach(x =>
            {
                tag++;
                drawnpolygon.Points.Add(x);
                addpolygonmarkergrid(tag.ToString(), x.Lng, x.Lat, 0);
            });

            drawnpolygonsoverlay.Polygons.Add(drawnpolygon);
            Joysticknutton.UpdatePolygonLocalPosition(drawnpolygon);
            Joysticknutton.Invalidate();
        }

        private void clearPolygonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            polygongridmode = false;
            if (drawnpolygon == null)
                return;
            drawnpolygon.Points.Clear();
            drawnpolygonsoverlay.Markers.Clear();
            Joysticknutton.Invalidate();

            writeKML();
        }

        private void clearMissionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            quickadd = true;

            // mono fix
            Commands.CurrentCell = null;

            Commands.Rows.Clear();

            selectedrow = 0;
            quickadd = false;
            writeKML();
        }

        private void loiterForeverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectedrow = Commands.Rows.Add();

            Commands.Rows[selectedrow].Cells[Command.Index].Value = MAVLink.MAV_CMD.LOITER_UNLIM.ToString();

            ChangeColumnHeader(MAVLink.MAV_CMD.LOITER_UNLIM.ToString());

            setfromMap(MouseDownEnd.Lat, MouseDownEnd.Lng, (int)float.Parse(TXT_DefaultAlt.Text));

            writeKML();
        }

        private void jumpstartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string repeat = "5";
            if (DialogResult.Cancel == InputBox.Show("Jump repeat", "Number of times to Repeat", ref repeat))
                return;

            selectedrow = Commands.Rows.Add();

            Commands.Rows[selectedrow].Cells[Command.Index].Value = MAVLink.MAV_CMD.DO_JUMP.ToString();

            Commands.Rows[selectedrow].Cells[Param1.Index].Value = 1;

            Commands.Rows[selectedrow].Cells[Param2.Index].Value = repeat;

            writeKML();
        }

        private void jumpwPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string wp = "1";
            if (DialogResult.Cancel == InputBox.Show("WP No", "Jump to WP no?", ref wp))
                return;
            string repeat = "5";
            if (DialogResult.Cancel == InputBox.Show("Jump repeat", "Number of times to Repeat", ref repeat))
                return;

            selectedrow = Commands.Rows.Add();

            Commands.Rows[selectedrow].Cells[Command.Index].Value = MAVLink.MAV_CMD.DO_JUMP.ToString();

            Commands.Rows[selectedrow].Cells[Param1.Index].Value = wp;

            Commands.Rows[selectedrow].Cells[Param2.Index].Value = repeat;

            writeKML();
        }

        private void deleteWPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int no = 0;
            if (CurentRectMarker != null)
            {
                if (int.TryParse(CurentRectMarker.InnerMarker.Tag.ToString(), out no))
                {
                    try
                    {
                        Commands.Rows.RemoveAt(no - 1); // home is 0
                    }
                    catch { CustomMessageBox.Show("error selecting wp, please try again."); }
                }
                else if (int.TryParse(CurentRectMarker.InnerMarker.Tag.ToString().Replace("grid", ""), out no))
                {
                    try
                    {
                        drawnpolygon.Points.RemoveAt(no - 1);
                        drawnpolygonsoverlay.Markers.Clear();

                        int a = 1;
                        foreach (PointLatLng pnt in drawnpolygon.Points)
                        {
                            addpolygonmarkergrid(a.ToString(), pnt.Lng, pnt.Lat, 0);
                            a++;
                        }

                        Joysticknutton.UpdatePolygonLocalPosition(drawnpolygon);

                        Joysticknutton.Invalidate();
                    }
                    catch
                    {
                        CustomMessageBox.Show("Remove point Failed. Please try again.");
                    }
                }
            }
            else if (CurrentRallyPt != null)
            {
                rallypointoverlay.Markers.Remove(CurrentRallyPt);
                Joysticknutton.Invalidate(true);

                CurrentRallyPt = null;
            }


            if (currentMarker != null)
                CurentRectMarker = null;
            //Refresh_my_PTs();
            writeKML();
        }

        private void loitertimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string time = "5";
            if (DialogResult.Cancel == InputBox.Show("Loiter Time", "Loiter Time", ref time))
                return;

            selectedrow = Commands.Rows.Add();

            Commands.Rows[selectedrow].Cells[Command.Index].Value = MAVLink.MAV_CMD.LOITER_TIME.ToString();

            Commands.Rows[selectedrow].Cells[Param1.Index].Value = time;

            ChangeColumnHeader(MAVLink.MAV_CMD.LOITER_TIME.ToString());

            setfromMap(MouseDownEnd.Lat, MouseDownEnd.Lng, (int)float.Parse(TXT_DefaultAlt.Text));

            writeKML();
        }

        private void loitercirclesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string turns = "3";
            if (DialogResult.Cancel == InputBox.Show("Loiter Turns", "Loiter Turns", ref turns))
                return;

            selectedrow = Commands.Rows.Add();

            Commands.Rows[selectedrow].Cells[Command.Index].Value = MAVLink.MAV_CMD.LOITER_TURNS.ToString();

            Commands.Rows[selectedrow].Cells[Param1.Index].Value = turns;

            ChangeColumnHeader(MAVLink.MAV_CMD.LOITER_TURNS.ToString());

            setfromMap(MouseDownEnd.Lat, MouseDownEnd.Lng, (int)float.Parse(TXT_DefaultAlt.Text));

            writeKML();
        }

        private void panelMap_Resize(object sender, EventArgs e)
        {
            // this is a mono fix for the zoom bar
            //Console.WriteLine("panelmap "+panelMap.Size.ToString());
            Joysticknutton.Size = new Size(panelMap.Size.Width - 50, panelMap.Size.Height);
            trackBar1.Location = new Point(panelMap.Size.Width - 50, trackBar1.Location.Y);
            trackBar1.Size = new Size(trackBar1.Size.Width, panelMap.Size.Height - trackBar1.Location.Y);
            label11.Location = new Point(panelMap.Size.Width - 50, label11.Location.Y);
        }

        DateTime mapupdate = DateTime.MinValue;

        /// <summary>
        /// Draw an mav icon, and update tracker location icon and guided mode wp on FP screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (isMouseDown)
                    return;

                routesoverlay.Markers.Clear();

                if (MainV2.comPort.MAV.cs.TrackerLocation != MainV2.comPort.MAV.cs.HomeLocation && MainV2.comPort.MAV.cs.TrackerLocation.Lng != 0)
                {
                    addpolygonmarker("Tracker Home", MainV2.comPort.MAV.cs.TrackerLocation.Lng, MainV2.comPort.MAV.cs.TrackerLocation.Lat, (int)MainV2.comPort.MAV.cs.TrackerLocation.Alt, Color.Blue, routesoverlay);
                }

                if (MainV2.comPort.MAV.cs.lat == 0 || MainV2.comPort.MAV.cs.lng == 0)
                    return;

                PointLatLng currentloc = new PointLatLng(MainV2.comPort.MAV.cs.lat, MainV2.comPort.MAV.cs.lng);

                if (MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.ArduPlane || MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.Ateryx)
                {
                    routesoverlay.Markers.Add(new GMapMarkerPlane(currentloc, MainV2.comPort.MAV.cs.yaw, MainV2.comPort.MAV.cs.groundcourse, MainV2.comPort.MAV.cs.nav_bearing, MainV2.comPort.MAV.cs.target_bearing));
                }
                else if (MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.ArduRover)
                {
                    routesoverlay.Markers.Add(new GMapMarkerRover(currentloc, MainV2.comPort.MAV.cs.yaw, MainV2.comPort.MAV.cs.groundcourse, MainV2.comPort.MAV.cs.nav_bearing, MainV2.comPort.MAV.cs.target_bearing));
                }
                else if (MainV2.comPort.MAV.aptype == MAVLink.MAV_TYPE.HELICOPTER)
                {
                    routesoverlay.Markers.Add((new GMapMarkerHeli(currentloc, MainV2.comPort.MAV.cs.yaw, MainV2.comPort.MAV.cs.groundcourse, MainV2.comPort.MAV.cs.nav_bearing)));
                }
                else if (MainV2.comPort.MAV.aptype == MAVLink.MAV_TYPE.ANTENNA_TRACKER)
                {
                    routesoverlay.Markers.Add(new GMapMarkerAntennaTracker(currentloc, MainV2.comPort.MAV.cs.yaw, MainV2.comPort.MAV.cs.target_bearing));
                }
                else
                {
                    routesoverlay.Markers.Add(new GMapMarkerQuad(currentloc, MainV2.comPort.MAV.cs.yaw, MainV2.comPort.MAV.cs.groundcourse, MainV2.comPort.MAV.cs.nav_bearing, MainV2.comPort.MAV.sysid));
                }

                if (MainV2.comPort.MAV.cs.mode.ToLower() == "guided" && MainV2.comPort.MAV.GuidedMode.x != 0)
                {
                    addpolygonmarker("Guided Mode", MainV2.comPort.MAV.GuidedMode.y, MainV2.comPort.MAV.GuidedMode.x, (int)MainV2.comPort.MAV.GuidedMode.z, Color.Blue, routesoverlay);
                }

                //autopan
                if (autopan)
                {
                    if (route.Points[route.Points.Count - 1].Lat != 0 && (mapupdate.AddSeconds(3) < DateTime.Now))
                    {
                        updateMapPosition(currentloc);
                        mapupdate = DateTime.Now;
                    }
                }
            }
            catch (Exception ex) { log.Warn(ex); }
        }

        /// <summary>
        /// Try to reduce the number of map position changes generated by the code
        /// </summary>
        DateTime lastmapposchange = DateTime.MinValue;

        private void updateMapPosition(PointLatLng currentloc)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                try
                {
                    if (lastmapposchange.Second != DateTime.Now.Second)
                    {
                        Joysticknutton.Position = currentloc;
                        lastmapposchange = DateTime.Now;
                    }
                }
                catch { }
            });
        }

        private void addpolygonmarker(string tag, double lng, double lat, int alt, Color? color, GMapOverlay overlay)
        {
            try
            {

                PointLatLng point = new PointLatLng(lat, lng);
                GMarkerGoogle m = new GMarkerGoogle(point, GMarkerGoogleType.green);

                m.ToolTipMode = MarkerTooltipMode.Always;
                m.ToolTipText = tag;
                m.Tag = tag;
                m.ToolTip.Stroke = Pens.Azure;


                GMapMarkerRect mBorders = new GMapMarkerRect(point);
                {
                    mBorders.InnerMarker = m;
                    try
                    {
                        mBorders.wprad = (int)(float.Parse(MainV2.config["TXT_WPRad"].ToString()) / CurrentState.multiplierdist);
                    }
                    catch { }
                    if (color.HasValue)
                    {
                        mBorders.Color = color.Value;
                    }
                }
                //if (alt != 10)
               // {

                    overlay.Markers.Add(m);
                    overlay.Markers.Add(mBorders);
                //}
            }
            catch (Exception) { }
        }

        private void GeoFenceuploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            polygongridmode = false;
            //FENCE_TOTAL
            if (MainV2.comPort.MAV.param["FENCE_ACTION"] == null)
            {
                CustomMessageBox.Show("Not Supported");
                return;
            }

            if (drawnpolygon == null)
            {
                CustomMessageBox.Show("No polygon to upload");
                return;
            }

            if (geofenceoverlay.Markers.Count == 0)
            {
                CustomMessageBox.Show("No return location set");
                return;
            }

            if (drawnpolygon.Points.Count == 0)
            {
                CustomMessageBox.Show("No polygon drawn");
                return;
            }

            // check if return is inside polygon
            List<PointLatLng> plll = new List<PointLatLng>(drawnpolygon.Points.ToArray());
            // close it
            plll.Add(plll[0]);
            // check it
            if (!pnpoly(plll.ToArray(), geofenceoverlay.Markers[0].Position.Lat, geofenceoverlay.Markers[0].Position.Lng))
            {
                CustomMessageBox.Show("Your return location is outside the polygon");
                return;
            }

            string minalts = (int.Parse(MainV2.comPort.MAV.param["FENCE_MINALT"].ToString()) * CurrentState.multiplierdist).ToString("0");
            if (DialogResult.Cancel == InputBox.Show("Min Alt", "Box Minimum Altitude?", ref minalts))
                return;

            string maxalts = (int.Parse(MainV2.comPort.MAV.param["FENCE_MAXALT"].ToString()) * CurrentState.multiplierdist).ToString("0");
            if (DialogResult.Cancel == InputBox.Show("Max Alt", "Box Maximum Altitude?", ref maxalts))
                return;

            int minalt = 0;
            int maxalt = 0;

            if (!int.TryParse(minalts, out minalt))
            {
                CustomMessageBox.Show("Bad Min Alt");
                return;
            }

            if (!int.TryParse(maxalts, out maxalt))
            {
                CustomMessageBox.Show("Bad Max Alt");
                return;
            }

            try
            {
                MainV2.comPort.setParam("FENCE_MINALT", minalt);
                MainV2.comPort.setParam("FENCE_MAXALT", maxalt);
            }
            catch
            {
                CustomMessageBox.Show("Failed to set min/max fence alt");
                return;
            }

            float oldaction = (float)MainV2.comPort.MAV.param["FENCE_ACTION"];

            try
            {
                MainV2.comPort.setParam("FENCE_ACTION", 0);
            }
            catch
            {
                CustomMessageBox.Show("Failed to set FENCE_ACTION");
                return;
            }

            // points + return + close
            byte pointcount = (byte)(drawnpolygon.Points.Count + 2);


            try
            {
                MainV2.comPort.setParam("FENCE_TOTAL", pointcount);
            }
            catch
            {
                CustomMessageBox.Show("Failed to set FENCE_TOTAL");
                return;
            }

            try
            {
                byte a = 0;
                // add return loc
                MainV2.comPort.setFencePoint(a, new PointLatLngAlt(geofenceoverlay.Markers[0].Position), pointcount);
                a++;
                // add points
                foreach (var pll in drawnpolygon.Points)
                {
                    MainV2.comPort.setFencePoint(a, new PointLatLngAlt(pll), pointcount);
                    a++;
                }

                // add polygon close
                MainV2.comPort.setFencePoint(a, new PointLatLngAlt(drawnpolygon.Points[0]), pointcount);

                try
                {
                    MainV2.comPort.setParam("FENCE_ACTION", oldaction);
                }
                catch
                {
                    CustomMessageBox.Show("Failed to restore FENCE_ACTION");
                    return;
                }

                // clear everything
                drawnpolygonsoverlay.Polygons.Clear();
                drawnpolygonsoverlay.Markers.Clear();
                geofenceoverlay.Polygons.Clear();
                geofencepolygon.Points.Clear();

                // add polygon
                geofencepolygon.Points.AddRange(drawnpolygon.Points.ToArray());

                drawnpolygon.Points.Clear();

                geofenceoverlay.Polygons.Add(geofencepolygon);

                // update flightdata
                FlightData.geofence.Markers.Clear();
                FlightData.geofence.Polygons.Clear();
                FlightData.geofence.Polygons.Add(new GMapPolygon(geofencepolygon.Points, "gf fd") { Stroke = geofencepolygon.Stroke });
                FlightData.geofence.Markers.Add(new GMarkerGoogle(geofenceoverlay.Markers[0].Position, GMarkerGoogleType.red) { ToolTipText = geofenceoverlay.Markers[0].ToolTipText, ToolTipMode = geofenceoverlay.Markers[0].ToolTipMode });

                Joysticknutton.UpdatePolygonLocalPosition(geofencepolygon);
                Joysticknutton.UpdateMarkerLocalPosition(geofenceoverlay.Markers[0]);

                Joysticknutton.Invalidate();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Failed to send new fence points " + ex, Strings.ERROR);
            }
        }

        private void GeoFencedownloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            polygongridmode = false;
            int count = 1;

            if (MainV2.comPort.MAV.param["FENCE_ACTION"] == null || MainV2.comPort.MAV.param["FENCE_TOTAL"] == null)
            {
                CustomMessageBox.Show("Not Supported");
                return;
            }

            if (int.Parse(MainV2.comPort.MAV.param["FENCE_TOTAL"].ToString()) <= 1)
            {
                CustomMessageBox.Show("Nothing to download");
                return;
            }

            geofenceoverlay.Polygons.Clear();
            geofenceoverlay.Markers.Clear();
            geofencepolygon.Points.Clear();


            for (int a = 0; a < count; a++)
            {
                try
                {
                    PointLatLngAlt plla = MainV2.comPort.getFencePoint(a, ref count);
                    geofencepolygon.Points.Add(new PointLatLng(plla.Lat, plla.Lng));
                }
                catch { CustomMessageBox.Show("Failed to get fence point", Strings.ERROR); return; }
            }

            // do return location
            geofenceoverlay.Markers.Add(new GMarkerGoogle(new PointLatLng(geofencepolygon.Points[0].Lat, geofencepolygon.Points[0].Lng), GMarkerGoogleType.red) { ToolTipMode = MarkerTooltipMode.Never, ToolTipText = "GeoFence Return" });
            geofencepolygon.Points.RemoveAt(0);

            // add now - so local points are calced
            geofenceoverlay.Polygons.Add(geofencepolygon);

            // update flight data
            FlightData.geofence.Markers.Clear();
            FlightData.geofence.Polygons.Clear();
            FlightData.geofence.Polygons.Add(new GMapPolygon(geofencepolygon.Points, "gf fd") { Stroke = geofencepolygon.Stroke, Fill = Brushes.Transparent });
            FlightData.geofence.Markers.Add(new GMarkerGoogle(geofenceoverlay.Markers[0].Position, GMarkerGoogleType.red) { ToolTipText = geofenceoverlay.Markers[0].ToolTipText, ToolTipMode = geofenceoverlay.Markers[0].ToolTipMode });

            Joysticknutton.UpdatePolygonLocalPosition(geofencepolygon);
            Joysticknutton.UpdateMarkerLocalPosition(geofenceoverlay.Markers[0]);

            Joysticknutton.Invalidate();
        }

        private void setReturnLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            geofenceoverlay.Markers.Clear();
            geofenceoverlay.Markers.Add(new GMarkerGoogle(new PointLatLng(MouseDownStart.Lat, MouseDownStart.Lng), GMarkerGoogleType.red) { ToolTipMode = MarkerTooltipMode.Never, ToolTipText = "GeoFence Return" });

            Joysticknutton.Invalidate();
        }

        /// <summary>
        /// from http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html
        /// </summary>
        /// <param name="array"> a closed polygon</param>
        /// <param name="testx"></param>
        /// <param name="testy"></param>
        /// <returns> true = outside</returns>
        bool pnpoly(PointLatLng[] array, double testx, double testy)
        {
            int nvert = array.Length;
            int i, j = 0;
            bool c = false;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if (((array[i].Lng > testy) != (array[j].Lng > testy)) &&
                 (testx < (array[j].Lat - array[i].Lat) * (testy - array[i].Lng) / (array[j].Lng - array[i].Lng) + array[i].Lat))
                    c = !c;
            }
            return c;
        }

        private void loadFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.Filter = "Fence (*.fen)|*.fen";
                fd.ShowDialog();
                if (File.Exists(fd.FileName))
                {
                    StreamReader sr = new StreamReader(fd.OpenFile());

                    drawnpolygonsoverlay.Markers.Clear();
                    drawnpolygonsoverlay.Polygons.Clear();
                    drawnpolygon.Points.Clear();

                    int a = 0;

                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (line.StartsWith("#"))
                        {
                        }
                        else
                        {
                            string[] items = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                            if (a == 0)
                            {
                                geofenceoverlay.Markers.Clear();
                                geofenceoverlay.Markers.Add(new GMarkerGoogle(new PointLatLng(double.Parse(items[0]), double.Parse(items[1])), GMarkerGoogleType.red) { ToolTipMode = MarkerTooltipMode.Never, ToolTipText = "GeoFence Return" });
                                Joysticknutton.UpdateMarkerLocalPosition(geofenceoverlay.Markers[0]);
                            }
                            else
                            {
                                drawnpolygon.Points.Add(new PointLatLng(double.Parse(items[0]), double.Parse(items[1])));
                                addpolygonmarkergrid(drawnpolygon.Points.Count.ToString(), double.Parse(items[1]), double.Parse(items[0]), 0);
                            }
                            a++;
                        }
                    }

                    // remove loop close
                    if (drawnpolygon.Points.Count > 1 && drawnpolygon.Points[0] == drawnpolygon.Points[drawnpolygon.Points.Count - 1])
                    {
                        drawnpolygon.Points.RemoveAt(drawnpolygon.Points.Count - 1);
                    }

                    drawnpolygonsoverlay.Polygons.Add(drawnpolygon);

                    Joysticknutton.UpdatePolygonLocalPosition(drawnpolygon);

                    Joysticknutton.Invalidate();
                }
            }
        }

        private void saveToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (geofenceoverlay.Markers.Count == 0)
            {
                CustomMessageBox.Show("Please set a return location");
                return;
            }


            using (SaveFileDialog sf = new SaveFileDialog())
            {
                sf.Filter = "Fence (*.fen)|*.fen";
                sf.ShowDialog();
                if (sf.FileName != "")
                {
                    try
                    {
                        StreamWriter sw = new StreamWriter(sf.OpenFile());

                        sw.WriteLine("#saved by APM Planner " + Application.ProductVersion);

                        sw.WriteLine(geofenceoverlay.Markers[0].Position.Lat + " " + geofenceoverlay.Markers[0].Position.Lng);
                        if (drawnpolygon.Points.Count > 0)
                        {
                            foreach (var pll in drawnpolygon.Points)
                            {
                                sw.WriteLine(pll.Lat + " " + pll.Lng);
                            }

                            PointLatLng pll2 = drawnpolygon.Points[0];

                            sw.WriteLine(pll2.Lat + " " + pll2.Lng);
                        }
                        else
                        {
                            foreach (var pll in geofencepolygon.Points)
                            {
                                sw.WriteLine(pll.Lat + " " + pll.Lng);
                            }

                            PointLatLng pll2 = geofencepolygon.Points[0];

                            sw.WriteLine(pll2.Lat + " " + pll2.Lng);
                        }

                        sw.Close();
                    }
                    catch { CustomMessageBox.Show("Failed to write fence file"); }
                }
            }
        }

        public T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();

                formatter.Serialize(ms, obj);

                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

        private void createWpCircleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string RadiusIn = "50";
            if (DialogResult.Cancel == InputBox.Show("Radius", "Radius", ref RadiusIn))
                return;

            string Pointsin = "20";
            if (DialogResult.Cancel == InputBox.Show("Points", "Number of points to generate Circle", ref Pointsin))
                return;

            string Directionin = "1";
            if (DialogResult.Cancel == InputBox.Show("Points", "Direction of circle (-1 or 1)", ref Directionin))
                return;

            string startanglein = "0";
            if (DialogResult.Cancel == InputBox.Show("angle", "Angle of first point (whole degrees)", ref startanglein))
                return;

            int Points = 0;
            int Radius = 0;
            int Direction = 1;
            int startangle = 0;

            if (!int.TryParse(RadiusIn, out Radius))
            {
                CustomMessageBox.Show("Bad Radius");
                return;
            }

            if (!int.TryParse(Pointsin, out Points))
            {
                CustomMessageBox.Show("Bad Point value");
                return;
            }

            if (!int.TryParse(Directionin, out Direction))
            {
                CustomMessageBox.Show("Bad Direction value");
                return;
            }

            if (!int.TryParse(startanglein, out startangle))
            {
                CustomMessageBox.Show("Bad start angle value");
                return;
            }

            double a = startangle;
            double step = 360.0f / Points;
            if (Direction == -1)
            {
                a += 360;
                step *= -1;
            }

            quickadd = true;

            for (; a <= (startangle + 360) && a >= 0; a += step)
            {

                selectedrow = Commands.Rows.Add();

                Commands.Rows[selectedrow].Cells[Command.Index].Value = MAVLink.MAV_CMD.WAYPOINT.ToString();

                ChangeColumnHeader(MAVLink.MAV_CMD.WAYPOINT.ToString());

                float d = Radius;
                float R = 6371000;

                var lat2 = Math.Asin(Math.Sin(MouseDownEnd.Lat * deg2rad) * Math.Cos(d / R) +
              Math.Cos(MouseDownEnd.Lat * deg2rad) * Math.Sin(d / R) * Math.Cos(a * deg2rad));
                var lon2 = MouseDownEnd.Lng * deg2rad + Math.Atan2(Math.Sin(a * deg2rad) * Math.Sin(d / R) * Math.Cos(MouseDownEnd.Lat * deg2rad),
                                     Math.Cos(d / R) - Math.Sin(MouseDownEnd.Lat * deg2rad) * Math.Sin(lat2));

                PointLatLng pll = new PointLatLng(lat2 * rad2deg, lon2 * rad2deg);

                setfromMap(pll.Lat, pll.Lng, (int)float.Parse(TXT_DefaultAlt.Text));

            }

            quickadd = false;
            writeKML();

            //drawnpolygon.Points.Add(new PointLatLng(start.Lat, start.Lng));
        }

        public void Activate()
        {
            timer1.Start();

            if (MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.ArduCopter2)
            {
                CMB_altmode.Visible = false;
            }
            else
            {
                CMB_altmode.Visible = true;
            }

            //switchDockingToolStripMenuItem_Click(null, null);

            updateHome();

            setWPParams();

            updateCMDParams();

            try
            {
                int.Parse(TXT_DefaultAlt.Text);
            }
            catch { CustomMessageBox.Show("Please fix your default alt value"); TXT_DefaultAlt.Text = (50 * CurrentState.multiplierdist).ToString("0"); }
        }

        public void updateHome()
        {
            quickadd = true;
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate { updateHomeText(); });
            }
            else
            {
                updateHomeText();
            }
            quickadd = false;
        }

        private void updateHomeText()
        {
            // set home location
            if (MainV2.comPort.MAV.cs.HomeLocation.Lat != 0 && MainV2.comPort.MAV.cs.HomeLocation.Lng != 0)
            {
                TXT_homelat.Text = MainV2.comPort.MAV.cs.HomeLocation.Lat.ToString();

                TXT_homelng.Text = MainV2.comPort.MAV.cs.HomeLocation.Lng.ToString();

                TXT_homealt.Text = MainV2.comPort.MAV.cs.HomeLocation.Alt.ToString();

                writeKML();
            }

        }

        public void Deactivate()
        {
            config(true);
            timer1.Stop();
        }

        private void FlightPlanner_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
            this.threadrun = false;
        }

        private void setROIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!cmdParamNames.ContainsKey("DO_SET_ROI"))
            {
                CustomMessageBox.Show(Strings.ErrorFeatureNotEnabled, Strings.ERROR);
                return;
            }

            selectedrow = Commands.Rows.Add();

            Commands.Rows[selectedrow].Cells[Command.Index].Value = MAVLink.MAV_CMD.DO_SET_ROI.ToString();

            //Commands.Rows[selectedrow].Cells[Param1.Index].Value = time;

            ChangeColumnHeader(MAVLink.MAV_CMD.DO_SET_ROI.ToString());

            setfromMap(MouseDownEnd.Lat, MouseDownEnd.Lng, (int)float.Parse(TXT_DefaultAlt.Text));

            writeKML();
        }

        private void zoomToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string place = "Perth Airport, Australia";
            if (DialogResult.OK == InputBox.Show("Location", "Enter your location", ref place))
            {

                GeoCoderStatusCode status = Joysticknutton.SetPositionByKeywords(place);
                if (status != GeoCoderStatusCode.G_GEO_SUCCESS)
                {
                    CustomMessageBox.Show("Google Maps Geocoder can't find: '" + place + "', reason: " + status, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    Joysticknutton.Zoom = 15;
                }
            }
        }

        bool fetchpathrip;

        private void FetchPath()
        {
            PointLatLngAlt lastpnt = null;

            DialogResult res = CustomMessageBox.Show("Ready ripp WP Path at Zoom = " + (int)Joysticknutton.Zoom + " ?", "GMap.NET", MessageBoxButtons.YesNo);

            fetchpathrip = true;

            foreach (var pnt in pointlist)
            {
                if (pnt == null)
                    continue;

                // exit if reqested
                if (!fetchpathrip)
                    break;

                // setup initial enviroment
                if (lastpnt == null)
                {
                    lastpnt = pnt;
                    continue;
                }

                RectLatLng area = new RectLatLng();
                double top = Math.Max(lastpnt.Lat, pnt.Lat);
                double left = Math.Min(lastpnt.Lng, pnt.Lng);
                double bottom = Math.Min(lastpnt.Lat, pnt.Lat);
                double right = Math.Max(lastpnt.Lng, pnt.Lng);

                area.LocationTopLeft = new PointLatLng(top, left);
                area.HeightLat = top - bottom;
                area.WidthLng = right - left;



                int todo;
                // todo
                // split up pull area to smaller chunks

                for (int i = 1; i <= Joysticknutton.MaxZoom; i++)
                {
                    if (res == DialogResult.Yes)
                    {
                        TilePrefetcher obj = new TilePrefetcher();
                        obj.KeyDown += obj_KeyDown;
                        obj.ShowCompleteMessage = false;
                        obj.Start(area, i, Joysticknutton.MapProvider, 100, 0);

                    }
                    else if (res == DialogResult.No)
                    {
                    }
                    else
                    {
                        break;
                    }
                }

                if (res == DialogResult.Cancel || res == DialogResult.None)
                {
                    break;
                }

                lastpnt = pnt;
            }
        }

        void obj_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                fetchpathrip = false;
            }
        }

        private void prefetchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RectLatLng area = Joysticknutton.SelectedArea;
            if (area.IsEmpty)
            {
                DialogResult res = CustomMessageBox.Show("No ripp area defined, ripp displayed on screen?", "Rip", MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes)
                {
                    area = Joysticknutton.ViewArea;
                }
            }

            if (!area.IsEmpty)
            {
                DialogResult res = CustomMessageBox.Show("Ready ripp at Zoom = " + (int)Joysticknutton.Zoom + " ?", "GMap.NET", MessageBoxButtons.YesNo);

                for (int i = 1; i <= Joysticknutton.MaxZoom; i++)
                {
                    if (res == DialogResult.Yes)
                    {
                        TilePrefetcher obj = new TilePrefetcher();
                        obj.ShowCompleteMessage = false;
                        obj.Start(area, i, Joysticknutton.MapProvider, 100, 0);
                    }
                    else if (res == DialogResult.No)
                    {
                    }
                    else if (res == DialogResult.Cancel)
                    {
                        break;
                    }
                }
            }
            else
            {
                CustomMessageBox.Show("Select map area holding ALT", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void kMLOverlayToolStripMenuItem_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.Filter = "Google Earth KML |*.kml;*.kmz";
                DialogResult result = fd.ShowDialog();
                string file = fd.FileName;
                if (file != "")
                {
                    try
                    {
                        kmlpolygonsoverlay.Polygons.Clear();
                        kmlpolygonsoverlay.Routes.Clear();

                        FlightData.kmlpolygons.Routes.Clear();
                        FlightData.kmlpolygons.Polygons.Clear();

                        string kml = "";
                        string tempdir = "";
                        if (file.ToLower().EndsWith("kmz"))
                        {
                            ZipFile input = new ZipFile(file);

                            tempdir = Path.GetTempPath() + Path.DirectorySeparatorChar + Path.GetRandomFileName();
                            input.ExtractAll(tempdir, ExtractExistingFileAction.OverwriteSilently);

                            string[] kmls = Directory.GetFiles(tempdir, "*.kml");

                            if (kmls.Length > 0)
                            {
                                file = kmls[0];

                                input.Dispose();
                            }
                            else
                            {
                                input.Dispose();
                                return;
                            }
                        }

                        var sr = new StreamReader(File.OpenRead(file));
                        kml = sr.ReadToEnd();
                        sr.Close();

                        // cleanup after out
                        if (tempdir != "")
                            Directory.Delete(tempdir, true);

                        kml = kml.Replace("<Snippet/>", "");

                        var parser = new Parser();

                        parser.ElementAdded += parser_ElementAdded;
                        parser.ParseString(kml, false);

                        if (DialogResult.Yes == CustomMessageBox.Show("Do you want to load this into the flight data screen?", "Load data", MessageBoxButtons.YesNo))
                        {
                            foreach (var temp in kmlpolygonsoverlay.Polygons)
                            {
                                FlightData.kmlpolygons.Polygons.Add(temp);
                            }
                            foreach (var temp in kmlpolygonsoverlay.Routes)
                            {
                                FlightData.kmlpolygons.Routes.Add(temp);
                            }
                        }

                    }
                    catch (Exception ex) { CustomMessageBox.Show("Bad KML File :" + ex); }
                }
            }
        }

        private void elevationGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            writeKML();
            double homealt = MainV2.comPort.MAV.cs.HomeAlt;
            Form temp = new ElevationProfile(pointlist, homealt, (altmode)CMB_altmode.SelectedValue);
            ThemeManager.ApplyThemeTo(temp);
            temp.ShowDialog();
        }

        private void rTLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectedrow = Commands.Rows.Add();

            Commands.Rows[selectedrow].Cells[Command.Index].Value = MAVLink.MAV_CMD.RETURN_TO_LAUNCH.ToString();

            //Commands.Rows[selectedrow].Cells[Param1.Index].Value = time;

            ChangeColumnHeader(MAVLink.MAV_CMD.RETURN_TO_LAUNCH.ToString());

            writeKML();
        }

        private void landToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectedrow = Commands.Rows.Add();

            Commands.Rows[selectedrow].Cells[Command.Index].Value = MAVLink.MAV_CMD.LAND.ToString();

            //Commands.Rows[selectedrow].Cells[Param1.Index].Value = time;

            ChangeColumnHeader(MAVLink.MAV_CMD.LAND.ToString());

            setfromMap(MouseDownEnd.Lat, MouseDownEnd.Lng, 1);

            writeKML();
        }

        private void AddDigicamControlPhoto()
        {
            selectedrow = Commands.Rows.Add();

            Commands.Rows[selectedrow].Cells[Command.Index].Value = MAVLink.MAV_CMD.DO_DIGICAM_CONTROL.ToString();

            ChangeColumnHeader(MAVLink.MAV_CMD.DO_DIGICAM_CONTROL.ToString());

            writeKML();
        }

        public void AddCommand(MAVLink.MAV_CMD cmd, double p1, double p2, double p3, double p4, double x, double y, double z)
        {
            selectedrow = Commands.Rows.Add();

            Commands.Rows[selectedrow].Cells[Command.Index].Value = cmd.ToString();
            ChangeColumnHeader(cmd.ToString());

            // switch wp to spline if spline checked
            if (splinemode && cmd == MAVLink.MAV_CMD.WAYPOINT)
            {
                Commands.Rows[selectedrow].Cells[Command.Index].Value = MAVLink.MAV_CMD.SPLINE_WAYPOINT.ToString();
                ChangeColumnHeader(MAVLink.MAV_CMD.SPLINE_WAYPOINT.ToString());
            }

            if (cmd == MAVLink.MAV_CMD.WAYPOINT)
            {
                setfromMap(y, x, (int)z, Math.Round(p1, 1));
            }
            else
            {
                Commands.Rows[selectedrow].Cells[Param1.Index].Value = p1;
                Commands.Rows[selectedrow].Cells[Param2.Index].Value = p2;
                Commands.Rows[selectedrow].Cells[Param3.Index].Value = p3;
                Commands.Rows[selectedrow].Cells[Param4.Index].Value = p4;
                Commands.Rows[selectedrow].Cells[Lat.Index].Value = y;
                Commands.Rows[selectedrow].Cells[Lon.Index].Value = x;
                Commands.Rows[selectedrow].Cells[Alt.Index].Value = z;
            }

            writeKML();
        }

        private void takeoffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // altitude
            string alt = "10";

            if (DialogResult.Cancel == InputBox.Show("Altitude", "Please enter your takeoff altitude", ref alt))
                return;

            int alti = -1;

            if (!int.TryParse(alt, out alti))
            {
                MessageBox.Show("Bad Alt");
                return;
            }

            // take off pitch
            int topi = 0;

            if (MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.ArduPlane || MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.Ateryx)
            {
                string top = "15";

                if (DialogResult.Cancel == InputBox.Show("Takeoff Pitch", "Please enter your takeoff pitch", ref top))
                    return;

                if (!int.TryParse(top, out topi))
                {
                    MessageBox.Show("Bad Takeoff pitch");
                    return;
                }
            }

            selectedrow = Commands.Rows.Add();

            Commands.Rows[selectedrow].Cells[Command.Index].Value = MAVLink.MAV_CMD.TAKEOFF.ToString();

            Commands.Rows[selectedrow].Cells[Param1.Index].Value = topi;

            Commands.Rows[selectedrow].Cells[Alt.Index].Value = alti;

            ChangeColumnHeader(MAVLink.MAV_CMD.TAKEOFF.ToString());

            writeKML();
        }

        internal string wpfilename;

        private void loadWPFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BUT_loadwpfile_Click(null, null);
        }

        private void saveWPFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile_Click(null, null);
        }

        private void trackerHomeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainV2.comPort.MAV.cs.TrackerLocation = new PointLatLngAlt(MouseDownEnd) { Alt = MainV2.comPort.MAV.cs.HomeAlt };
        }

        private void reverseWPsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridViewRowCollection rows = Commands.Rows;
            //Commands.Rows.Clear();

            int count = rows.Count;

            quickadd = true;

            for (int a = count; a > 0; a--)
            {
                DataGridViewRow row = Commands.Rows[a - 1];
                Commands.Rows.Remove(row);
                Commands.Rows.Add(row);
            }

            quickadd = false;

            writeKML();
        }

        private void loadAndAppendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.Filter = "Ardupilot Mission (*.txt)|*.*";
                fd.DefaultExt = ".txt";
                DialogResult result = fd.ShowDialog();
                string file = fd.FileName;
                if (file != "")
                {
                    readQGC110wpfile(file, true);
                }
            }
        }
        private void savepolys()
        {
            if (list_of_polys.Count == 0)
            {
                return;
            }


            using (SaveFileDialog sf = new SaveFileDialog())
            {
                sf.Filter = "Zones (*.MAPZn)|*.MAPZn";
                sf.ShowDialog();
                if (sf.FileName != "")
                {
                    try
                    {
                        StreamWriter sw = new StreamWriter(sf.OpenFile());

                        sw.WriteLine("#MAP Polygons ");
                        for (int i = 0; i < list_of_polys.Count; i++)
                        {
                            sw.WriteLine("Poly: " + list_of_polys[i].Name);
                            SolidBrush PolyColor = (SolidBrush)list_of_polys[i].Fill;
                            string Color_name = Color.FromArgb(20, PolyColor.Color).ToString();
                            sw.WriteLine("Poly Color: " + Color_name);
                            if (list_of_polys[i].Points.Count > 0)
                            {
                                foreach (var pll in list_of_polys[i].Points)
                                {
                                    sw.WriteLine(pll.Lat + " " + pll.Lng);
                                }

                                PointLatLng pll2 = list_of_polys[i].Points[0];

                                sw.WriteLine(pll2.Lat + " " + pll2.Lng);
                            }

                        }
                        sw.Close();
                    }
                    catch { CustomMessageBox.Show("Failed to write fence file"); }
                }
            }
        }
        private void savePolygonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            savepolys();
        }


        private void loadPolygonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadpolys();
        }
        private void loadpolys()
        {
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.Filter = "Zones (*.MAPZn)|*.MAPZn";
                fd.ShowDialog();
                if (File.Exists(fd.FileName))
                {
                    StreamReader sr = new StreamReader(fd.OpenFile());

                    drawnpolygonsoverlay.Markers.Clear();
                    drawnpolygonsoverlay.Polygons.Clear();
                    drawnpolygon.Points.Clear();

                    new_polygon_list.Clear();
                    GMapPolygon TempPoly = new GMapPolygon(new_polygon_list, "Temp");
                    int end_poly = 0;
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        string polyname;

                        if (line.StartsWith("#"))
                        {
                        }
                        else if (line.StartsWith("Poly:"))
                        {

                            if (end_poly == 1)
                            {
                                list_of_polys.Add(TempPoly);
                                end_poly = 0;
                            }
                            string new_line;
                            new_line = String.Copy(line.Replace(" ", ""));
                            polyname = String.Copy(new_line.Replace("Poly:", ""));
                            new_polygon_list.Clear();
                            TempPoly = new GMapPolygon(new_polygon_list, polyname);
                        }
                        else if (line.StartsWith("Poly Color:"))
                        {
                            line.Replace(" ", "");
                            line.Replace("Poly Color:", "");
                            int start_index_al = line.IndexOf("A=") + 2;
                            int end_index_al = line.IndexOf(",", line.IndexOf("A="));
                            //CustomMessageBox.Show(line+"   "+ end_index_al.ToString());
                            int alpha_color = Convert.ToInt32(line.Substring(start_index_al, end_index_al - start_index_al));

                            int start_index_red = line.IndexOf("R=") + 2;
                            int end_index_red = line.IndexOf(",", line.IndexOf("R="));
                            int red_color = Convert.ToInt32(line.Substring(start_index_red, end_index_red - start_index_red));

                            int start_index_blue = line.IndexOf("B=") + 2;
                            int end_index_blue = line.IndexOf("]", line.IndexOf("B="));
                            int blue_color = Convert.ToInt32(line.Substring(start_index_blue, end_index_blue - start_index_blue));

                            int start_index_green = line.IndexOf("G=") + 2;
                            int end_index_green = line.IndexOf(",", line.IndexOf("G="));
                            int green_color = Convert.ToInt32(line.Substring(start_index_green, end_index_green - start_index_green));
                            TempPoly.Stroke = new Pen(Color.Black, 1);
                            TempPoly.Fill = new SolidBrush(Color.FromArgb(alpha_color, red_color, green_color, blue_color));
                        }
                        else
                        {
                            string[] items = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                            TempPoly.Points.Add(new PointLatLng(double.Parse(items[0]), double.Parse(items[1])));
                            end_poly = 1;
                        }
                    }
                    if (end_poly == 1)
                    {
                        list_of_polys.Add(TempPoly);
                        end_poly = 0;
                    }
                    drawpolys();
                    /* // remove loop close
                     if (drawnpolygon.Points.Count > 1 && drawnpolygon.Points[0] == drawnpolygon.Points[drawnpolygon.Points.Count - 1])
                     {
                         drawnpolygon.Points.RemoveAt(drawnpolygon.Points.Count - 1);
                     }

                     drawnpolygonsoverlay.Polygons.Add(drawnpolygon);

                     Joysticknutton.UpdatePolygonLocalPosition(drawnpolygon);

                     Joysticknutton.Invalidate();

                     Joysticknutton.ZoomAndCenterMarkers(drawnpolygonsoverlay.Id);*/
                }
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (MainV2.comPort.MAV.cs.firmware != MainV2.Firmwares.ArduPlane)
            {
                //geoFenceToolStripMenuItem.Enabled = false;
            }
            else
            {
                //geoFenceToolStripMenuItem.Enabled = true;
            }
            isMouseClickOffMenu = false; // Just incase
        }

        private void contextMenuStrip1_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            if (e.CloseReason.ToString() == "AppClicked")
                isMouseClickOffMenu = true;
        }

        private void areaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double aream2 = Math.Abs(calcpolygonarea(drawnpolygon.Points));

            double areaa = aream2 * 0.000247105;

            double areaha = aream2 * 1e-4;

            double areasqf = aream2 * 10.7639;

            CustomMessageBox.Show("Area: " + aream2.ToString("0") + " m2\n\t" + areaa.ToString("0.00") + " Acre\n\t" + areaha.ToString("0.00") + " Hectare\n\t" + areasqf.ToString("0") + " sqf", "Area");
        }

        private void MainMap_Paint(object sender, PaintEventArgs e)
        {
            // draw utm grid
            {
                if (!grid)
                    return;

                if (Joysticknutton.Zoom < 10)
                    return;

                var rect = Joysticknutton.ViewArea;

                var plla1 = new PointLatLngAlt(rect.LocationTopLeft);
                var plla2 = new PointLatLngAlt(rect.LocationRightBottom);

                var zone = plla1.GetUTMZone();

                var utm1 = plla1.ToUTM(zone);
                var utm2 = plla2.ToUTM(zone);

                var deltax = utm1[0] - utm2[0];
                var deltay = utm1[1] - utm2[1];

                //if (deltax)

                var gridsize = 1000.0;


                if (Math.Abs(deltax) / 100000 < 40)
                    gridsize = 100000;

                if (Math.Abs(deltax) / 10000 < 40)
                    gridsize = 10000;

                if (Math.Abs(deltax) / 1000 < 40)
                    gridsize = 1000;

                if (Math.Abs(deltax) / 100 < 40)
                    gridsize = 100;



                // round it - x
                utm1[0] = utm1[0] - (utm1[0] % gridsize);
                // y
                utm2[1] = utm2[1] - (utm2[1] % gridsize);

                // x's
                for (double x = utm1[0]; x < utm2[0]; x += gridsize)
                {
                    var p1 = Joysticknutton.FromLatLngToLocal(PointLatLngAlt.FromUTM(zone, x, utm1[1]));
                    var p2 = Joysticknutton.FromLatLngToLocal(PointLatLngAlt.FromUTM(zone, x, utm2[1]));

                    int x1 = (int)p1.X;
                    int y1 = (int)p1.Y;
                    int x2 = (int)p2.X;
                    int y2 = (int)p2.Y;

                    e.Graphics.DrawLine(new Pen(Joysticknutton.SelectionPen.Color, 1), x1, y1, x2, y2);
                }

                // y's
                for (double y = utm2[1]; y < utm1[1]; y += gridsize)
                {
                    var p1 = Joysticknutton.FromLatLngToLocal(PointLatLngAlt.FromUTM(zone, utm1[0], y));
                    var p2 = Joysticknutton.FromLatLngToLocal(PointLatLngAlt.FromUTM(zone, utm2[0], y));

                    int x1 = (int)p1.X;
                    int y1 = (int)p1.Y;
                    int x2 = (int)p2.X;
                    int y2 = (int)p2.Y;

                    e.Graphics.DrawLine(new Pen(Joysticknutton.SelectionPen.Color, 1), x1, y1, x2, y2);
                }
            }
        }

        private void chk_grid_CheckedChanged(object sender, EventArgs e)
        {
            grid = chk_grid.Checked;
        }

        private void insertWpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string wpno = (selectedrow + 1).ToString("0");
            if (InputBox.Show("Insert WP", "Insert WP after wp#", ref wpno) == DialogResult.OK)
            {
                try
                {
                    Commands.Rows.Insert(int.Parse(wpno), 1);
                }
                catch { CustomMessageBox.Show("Invalid insert position", Strings.ERROR); return; }

                selectedrow = int.Parse(wpno);

                ChangeColumnHeader(MAVLink.MAV_CMD.WAYPOINT.ToString());

                setfromMap(MouseDownStart.Lat, MouseDownStart.Lng, (int)float.Parse(TXT_DefaultAlt.Text));
            }
        }

        public void getRallyPointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MainV2.comPort.MAV.param["RALLY_TOTAL"] == null)
            {
                CustomMessageBox.Show("Not Supported");
                return;
            }

            if (int.Parse(MainV2.comPort.MAV.param["RALLY_TOTAL"].ToString()) < 1)
            {
                CustomMessageBox.Show("Rally points - Nothing to download");
                return;
            }

            rallypointoverlay.Markers.Clear();

            int count = int.Parse(MainV2.comPort.MAV.param["RALLY_TOTAL"].ToString());

            for (int a = 0; a < (count); a++)
            {
                try
                {
                    PointLatLngAlt plla = MainV2.comPort.getRallyPoint(a, ref count);
                    rallypointoverlay.Markers.Add(new GMapMarkerRallyPt(new PointLatLng(plla.Lat, plla.Lng)) { Alt = (int)plla.Alt, ToolTipMode = MarkerTooltipMode.Never, ToolTipText = "Rally Point" + "\nAlt: " + (plla.Alt * CurrentState.multiplierdist) });
                }
                catch { CustomMessageBox.Show("Failed to get rally point", Strings.ERROR); return; }
            }

            Joysticknutton.UpdateMarkerLocalPosition(rallypointoverlay.Markers[0]);

            Joysticknutton.Invalidate();
        }

        private void saveRallyPointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte count = 0;

            MainV2.comPort.setParam("RALLY_TOTAL", rallypointoverlay.Markers.Count);

            foreach (GMapMarkerRallyPt pnt in rallypointoverlay.Markers)
            {
                try
                {
                    MainV2.comPort.setRallyPoint(count, new PointLatLngAlt(pnt.Position) { Alt = pnt.Alt }, 0, 0, 0, (byte)(float)MainV2.comPort.MAV.param["RALLY_TOTAL"]);
                    count++;
                }
                catch { CustomMessageBox.Show("Failed to save rally point", Strings.ERROR); return; }
            }
        }

        private void setRallyPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string altstring = TXT_DefaultAlt.Text;

            if (InputBox.Show("Altitude", "Altitude", ref altstring) == DialogResult.Cancel)
                return;

            int alt = 0;

            if (int.TryParse(altstring, out alt))
            {
                PointLatLngAlt rallypt = new PointLatLngAlt(MouseDownStart.Lat, MouseDownStart.Lng, alt / CurrentState.multiplierdist, "Rally Point");
                rallypointoverlay.Markers.Add(
                        new GMapMarkerRallyPt(rallypt)
                        {
                            ToolTipMode = MarkerTooltipMode.Never,
                            ToolTipText = "Rally Point" + "\nAlt: " + alt,
                            Tag = rallypointoverlay.Markers.Count,
                            Alt = (int)rallypt.Alt
                        }
                );
            }
            else
            {
                CustomMessageBox.Show(Strings.InvalidAlt, Strings.ERROR);
            }
        }

        private void clearRallyPointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MainV2.comPort.setParam("RALLY_TOTAL", 0);
            }
            catch { }
            rallypointoverlay.Markers.Clear();
            MainV2.comPort.MAV.rallypoints.Clear();
        }

        private void loadKMLFileToolStripMenuItem_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.Filter = "Google Earth KML |*.kml;*.kmz";
                DialogResult result = fd.ShowDialog();
                string file = fd.FileName;
                if (file != "")
                {
                    try
                    {
                        string kml = "";
                        string tempdir = "";
                        if (file.ToLower().EndsWith("kmz"))
                        {
                            ZipFile input = new ZipFile(file);

                            tempdir = Path.GetTempPath() + Path.DirectorySeparatorChar + Path.GetRandomFileName();
                            input.ExtractAll(tempdir, ExtractExistingFileAction.OverwriteSilently);

                            string[] kmls = Directory.GetFiles(tempdir, "*.kml");

                            if (kmls.Length > 0)
                            {
                                file = kmls[0];

                                input.Dispose();
                            }
                            else
                            {
                                input.Dispose();
                                return;
                            }
                        }

                        var sr = new StreamReader(File.OpenRead(file));
                        kml = sr.ReadToEnd();
                        sr.Close();

                        // cleanup after out
                        if (tempdir != "")
                            Directory.Delete(tempdir, true);

                        kml = kml.Replace("<Snippet/>", "");

                        var parser = new Parser();

                        parser.ElementAdded += processKMLMission;
                        parser.ParseString(kml, false);

                    }
                    catch (Exception ex) { CustomMessageBox.Show("Bad KML File :" + ex); }
                }
            }
        }

        private void processKMLMission(object sender, ElementEventArgs e)
        {
            Element element = e.Element;
            try
            {
                //  log.Info(Element.ToString() + " " + Element.Parent);
            }
            catch { }

            Document doc = element as Document;
            Placemark pm = element as Placemark;
            Folder folder = element as Folder;
            Polygon polygon = element as Polygon;
            LineString ls = element as LineString;

            if (doc != null)
            {
                foreach (var feat in doc.Features)
                {
                    //Console.WriteLine("feat " + feat.GetType());
                    //processKML((Element)feat);
                }
            }
            else
                if (folder != null)
                {
                    foreach (Feature feat in folder.Features)
                    {
                        //Console.WriteLine("feat "+feat.GetType());
                        //processKML(feat);
                    }
                }
                else if (pm != null)
                {

                }
                else if (polygon != null)
                {

                }
                else if (ls != null)
                {
                    foreach (var loc in ls.Coordinates)
                    {
                        selectedrow = Commands.Rows.Add();
                        setfromMap(loc.Latitude, loc.Longitude, (int)loc.Altitude);
                    }
                }
        }

        private void lnk_kml_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("http://127.0.0.1:56781/network.kml");
            }
            catch { CustomMessageBox.Show("Failed to open url http://127.0.0.1:56781/network.kml"); }
        }

        private void modifyAltToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string altdif = "0";
            InputBox.Show("Alt Change", "Please enter the alitude change you require.\n(20 = up 20, *2 = up by alt * 2)", ref altdif);

            int altchange = 0;
            float multiplyer = 1;

            try
            {
                if (altdif.Contains("*"))
                {
                    multiplyer = float.Parse(altdif.Replace('*', ' '));
                }
                else
                {
                    altchange = int.Parse(altdif);
                }
            }
            catch
            {
                CustomMessageBox.Show(Strings.InvalidNumberEntered, Strings.ERROR);
                return;
            }


            foreach (DataGridViewRow line in Commands.Rows)
            {
                line.Cells[Alt.Index].Value = (int)(float.Parse(line.Cells[Alt.Index].Value.ToString()) * multiplyer + altchange);
            }
        }

        private void saveToFileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (rallypointoverlay.Markers.Count == 0)
            {
                CustomMessageBox.Show("Please set some rally points");
                return;
            }
            /*
Column 1: Field type (RALLY is the only one at the moment -- may have RALLY_LAND in the future)
 Column 2,3: Lat, lon
 Column 4: Loiter altitude
 Column 5: Break altitude (when landing from rally is implemented, this is the altitude to break out of loiter from)
 Column 6: Landing heading (also for future when landing from rally is implemented)
 Column 7: Flags (just 0 for now, also future use).
             */

            using (SaveFileDialog sf = new SaveFileDialog())
            {
                sf.Filter = "Rally (*.ral)|*.ral";
                sf.ShowDialog();
                if (sf.FileName != "")
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(sf.OpenFile()))
                        {

                            sw.WriteLine("#saved by Mission Planner " + Application.ProductVersion);


                            foreach (GMapMarkerRallyPt mark in rallypointoverlay.Markers)
                            {
                                sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", "RALLY", mark.Position.Lat, mark.Position.Lng, mark.Alt, 0, 0, 0);
                            }
                        }
                    }
                    catch { CustomMessageBox.Show("Failed to write rally file"); }
                }
            }
        }

        private void loadFromFileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.Filter = "Rally (*.ral)|*.ral";
                fd.ShowDialog();
                if (File.Exists(fd.FileName))
                {
                    StreamReader sr = new StreamReader(fd.OpenFile());

                    int a = 0;

                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (line.StartsWith("#"))
                        {
                        }
                        else
                        {
                            string[] items = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                            MAVLink.mavlink_rally_point_t rally = new MAVLink.mavlink_rally_point_t();

                            rally.lat = (int)(float.Parse(items[1]) * 1e7);
                            rally.lng = (int)(float.Parse(items[2]) * 1e7);
                            rally.alt = (short)float.Parse(items[3]);
                            rally.break_alt = (short)float.Parse(items[4]);
                            rally.land_dir = (ushort)float.Parse(items[5]);
                            rally.flags = byte.Parse(items[6]);

                            if (a == 0)
                            {
                                rallypointoverlay.Markers.Clear();

                                rallypointoverlay.Markers.Add(new GMapMarkerRallyPt(rally));
                            }
                            else
                            {
                                rallypointoverlay.Markers.Add(new GMapMarkerRallyPt(rally));
                            }
                            a++;
                        }
                    }
                    Joysticknutton.Invalidate();
                }
            }
        }

        private void prefetchWPPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FetchPath();
        }

        static string zone = "50s";

        private void enterUTMCoordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string easting = "578994";
            string northing = "6126244";

            if (InputBox.Show("Zone", "Enter Zone. (eg 50S, 11N)", ref zone) != DialogResult.OK)
                return;
            if (InputBox.Show("Easting", "Easting", ref easting) != DialogResult.OK)
                return;
            if (InputBox.Show("Northing", "Northing", ref northing) != DialogResult.OK)
                return;

            string newzone = zone.ToLower().Replace('s', ' ');
            newzone = newzone.ToLower().Replace('n', ' ');

            int zoneint = int.Parse(newzone);

            UTM utm = new UTM(zoneint, double.Parse(easting), double.Parse(northing), zone.ToLower().Contains("N") ? Geocentric.Hemisphere.North : Geocentric.Hemisphere.South);

            PointLatLngAlt ans = ((Geographic)utm);

            selectedrow = Commands.Rows.Add();

            ChangeColumnHeader(MAVLink.MAV_CMD.WAYPOINT.ToString());

            setfromMap(ans.Lat, ans.Lng, (int)ans.Alt);
        }

        private void loadSHPFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.Filter = "Shape file|*.shp";
                DialogResult result = fd.ShowDialog();
                string file = fd.FileName;

                LoadSHPFile(file);
            }
        }

        private void LoadSHPFile(string file)
        {
            ProjectionInfo pStart = new ProjectionInfo();
            ProjectionInfo pESRIEnd = KnownCoordinateSystems.Geographic.World.WGS1984;
            bool reproject = false;

            if (File.Exists(file))
            {
                string prjfile = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(file) + ".prj";
                if (File.Exists(prjfile))
                {

                    using (StreamReader re = File.OpenText(Path.GetDirectoryName(file) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(file) + ".prj"))
                    {
                        pStart.ParseEsriString(re.ReadLine());

                        reproject = true;
                    }
                }

                IFeatureSet fs = FeatureSet.Open(file);

                fs.FillAttributes();

                int rows = fs.NumRows();

                DataTable dtOriginal = fs.DataTable;
                for (int row = 0; row < dtOriginal.Rows.Count; row++)
                {
                    object[] original = dtOriginal.Rows[row].ItemArray;
                }

                foreach (DataColumn col in dtOriginal.Columns)
                {
                    Console.WriteLine(col.ColumnName + " " + col.DataType);
                }

                quickadd = true;

                bool dosort = false;

                List<PointLatLngAlt> wplist = new List<PointLatLngAlt>();

                for (int row = 0; row < dtOriginal.Rows.Count; row++)
                {
                    double x = fs.Vertex[row * 2];
                    double y = fs.Vertex[row * 2 + 1];

                    double z = -1;
                    float wp = 0;

                    try
                    {
                        if (dtOriginal.Columns.Contains("ELEVATION"))
                            z = (float)Convert.ChangeType(dtOriginal.Rows[row]["ELEVATION"], TypeCode.Single);
                    }
                    catch { }

                    try
                    {
                        if (z == -1 && dtOriginal.Columns.Contains("alt"))
                            z = (float)Convert.ChangeType(dtOriginal.Rows[row]["alt"], TypeCode.Single);
                    }
                    catch { }

                    try
                    {
                        if (z == -1)
                            z = fs.Z[row];
                    }
                    catch { }


                    try
                    {
                        if (dtOriginal.Columns.Contains("wp"))
                        {
                            wp = (float)Convert.ChangeType(dtOriginal.Rows[row]["wp"], TypeCode.Single);
                            dosort = true;
                        }
                    }
                    catch { }

                    if (reproject)
                    {
                        double[] xyarray = { x, y };
                        double[] zarray = { z };

                        Reproject.ReprojectPoints(xyarray, zarray, pStart, pESRIEnd, 0, 1);


                        x = xyarray[0];
                        y = xyarray[1];
                        z = zarray[0];
                    }

                    PointLatLngAlt pnt = new PointLatLngAlt(x, y, z, wp.ToString());

                    wplist.Add(pnt);
                }

                if (dosort)
                    wplist.Sort();

                foreach (var item in wplist)
                {
                    AddCommand(MAVLink.MAV_CMD.WAYPOINT, 0, 0, 0, 0, item.Lat, item.Lng, item.Alt);
                }

                quickadd = false;

                writeKML();

                Joysticknutton.ZoomAndCenterMarkers("objects");
            }
        }

        private void BUT_saveWPFile_Click(object sender, EventArgs e)
        {
            SaveFile_Click(null, null);
        }

        private void switchDockingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (panelAction.Dock == DockStyle.Bottom)
            {
                panelAction.Dock = DockStyle.Right;
                panelWaypoints.Dock = DockStyle.Bottom;
            }
            else
            {
                panelAction.Dock = DockStyle.Bottom;
                panelAction.Height = 120;
                panelWaypoints.Dock = DockStyle.Right;
                panelWaypoints.Width = Width / 2;
            }

            MainV2.config["FP_docking"] = panelAction.Dock;
        }

        private void insertSplineWPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string wpno = (selectedrow + 1).ToString("0");
            if (InputBox.Show("Insert WP", "Insert WP after wp#", ref wpno) == DialogResult.OK)
            {
                try
                {
                    Commands.Rows.Insert(int.Parse(wpno), 1);
                }
                catch { CustomMessageBox.Show(Strings.InvalidNumberEntered, Strings.ERROR); return; }

                selectedrow = int.Parse(wpno);

                try
                {
                    Commands.Rows[selectedrow].Cells[Command.Index].Value = MAVLink.MAV_CMD.SPLINE_WAYPOINT.ToString();
                }
                catch { CustomMessageBox.Show("SPLINE_WAYPOINT command not supported."); Commands.Rows.RemoveAt(selectedrow); return; }

                ChangeColumnHeader(MAVLink.MAV_CMD.SPLINE_WAYPOINT.ToString());

                setfromMap(MouseDownStart.Lat, MouseDownStart.Lng, (int)float.Parse(TXT_DefaultAlt.Text));
            }
        }

        private void CHK_splinedefault_CheckedChanged(object sender, EventArgs e)
        {
            splinemode = CHK_splinedefault.Checked;
        }

        private void createSplineCircleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string RadiusIn = "50";
            if (DialogResult.Cancel == InputBox.Show("Radius", "Radius", ref RadiusIn))
                return;

            string minaltin = "5";
            if (DialogResult.Cancel == InputBox.Show("min alt", "Min Alt", ref minaltin))
                return;

            string maxaltin = "20";
            if (DialogResult.Cancel == InputBox.Show("max alt", "Max Alt", ref maxaltin))
                return;

            string altstepin = "5";
            if (DialogResult.Cancel == InputBox.Show("alt step", "alt step", ref altstepin))
                return;


            string startanglein = "0";
            if (DialogResult.Cancel == InputBox.Show("angle", "Angle of first point (whole degrees)", ref startanglein))
                return;

            int Points = 4;
            int Radius = 0;
            int Direction = 1;
            int startangle = 0;
            int minalt = 5;
            int maxalt = 20;
            int altstep = 5;
            if (!int.TryParse(RadiusIn, out Radius))
            {
                CustomMessageBox.Show("Bad Radius");
                return;
            }

            if (!int.TryParse(minaltin, out minalt))
            {
                CustomMessageBox.Show("Bad min alt");
                return;
            }
            if (!int.TryParse(maxaltin, out maxalt))
            {
                CustomMessageBox.Show("Bad maxalt");
                return;
            }
            if (!int.TryParse(altstepin, out altstep))
            {
                CustomMessageBox.Show("Bad alt step");
                return;
            }

            double a = startangle;
            double step = 360.0f / Points;

            quickadd = true;

            AddCommand(MAVLink.MAV_CMD.DO_SET_ROI, 0, 0, 0, 0, MouseDownStart.Lng, MouseDownStart.Lat, 0);

            bool startup = true;

            for (int stepalt = minalt; stepalt <= maxalt; )
            {

                for (a = 0; a <= (startangle + 360) && a >= 0; a += step)
                {

                    selectedrow = Commands.Rows.Add();

                    Commands.Rows[selectedrow].Cells[Command.Index].Value = MAVLink.MAV_CMD.SPLINE_WAYPOINT.ToString();

                    ChangeColumnHeader(MAVLink.MAV_CMD.SPLINE_WAYPOINT.ToString());

                    float d = Radius;
                    float R = 6371000;

                    var lat2 = Math.Asin(Math.Sin(MouseDownEnd.Lat * deg2rad) * Math.Cos(d / R) +
                  Math.Cos(MouseDownEnd.Lat * deg2rad) * Math.Sin(d / R) * Math.Cos(a * deg2rad));
                    var lon2 = MouseDownEnd.Lng * deg2rad + Math.Atan2(Math.Sin(a * deg2rad) * Math.Sin(d / R) * Math.Cos(MouseDownEnd.Lat * deg2rad),
                                         Math.Cos(d / R) - Math.Sin(MouseDownEnd.Lat * deg2rad) * Math.Sin(lat2));

                    PointLatLng pll = new PointLatLng(lat2 * rad2deg, lon2 * rad2deg);

                    setfromMap(pll.Lat, pll.Lng, stepalt);

                    if (!startup)
                        stepalt += altstep / Points;

                }

                // reset back to the start
                if (startup)
                    stepalt = minalt;

                // we have finsihed the first run
                startup = false;
            }

            quickadd = false;
            writeKML();

        }

        private void CMB_altmode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CMB_altmode.SelectedValue == null)
            {
                CMB_altmode.SelectedIndex = 0;
            }
            else
            {
                currentaltmode = (altmode)CMB_altmode.SelectedValue;
            }
        }

        private void fromSHPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.Filter = "Shape file|*.shp";
                DialogResult result = fd.ShowDialog();
                string file = fd.FileName;
                ProjectionInfo pStart = new ProjectionInfo();
                ProjectionInfo pESRIEnd = KnownCoordinateSystems.Geographic.World.WGS1984;
                bool reproject = false;
                // Poly Clear
                drawnpolygonsoverlay.Markers.Clear();
                drawnpolygonsoverlay.Polygons.Clear();
                drawnpolygon.Points.Clear();
                if (File.Exists(file))
                {
                    string prjfile = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(file) + ".prj";
                    if (File.Exists(prjfile))
                    {
                        using (StreamReader re = File.OpenText(Path.GetDirectoryName(file) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(file) + ".prj"))
                        {
                            pStart.ParseEsriString(re.ReadLine());
                            reproject = true;
                        }
                    }
                    try
                    {
                        IFeatureSet fs = FeatureSet.Open(file);
                        fs.FillAttributes();
                        int rows = fs.NumRows();
                        DataTable dtOriginal = fs.DataTable;
                        for (int row = 0; row < dtOriginal.Rows.Count; row++)
                        {
                            object[] original = dtOriginal.Rows[row].ItemArray;
                        }
                        string path = Path.GetDirectoryName(file);
                        foreach (var feature in fs.Features)
                        {
                            foreach (var point in feature.Coordinates)
                            {
                                if (reproject)
                                {
                                    double[] xyarray = { point.X, point.Y };
                                    double[] zarray = { point.Z };
                                    Reproject.ReprojectPoints(xyarray, zarray, pStart, pESRIEnd, 0, 1);
                                    point.X = xyarray[0];
                                    point.Y = xyarray[1];
                                    point.Z = zarray[0];
                                }
                                drawnpolygon.Points.Add(new PointLatLng(point.Y, point.X));
                                addpolygonmarkergrid(drawnpolygon.Points.Count.ToString(), point.X, point.Y, 0);
                            }
                            // remove loop close
                            if (drawnpolygon.Points.Count > 1 && drawnpolygon.Points[0] == drawnpolygon.Points[drawnpolygon.Points.Count - 1])
                            {
                                drawnpolygon.Points.RemoveAt(drawnpolygon.Points.Count - 1);
                            }
                            drawnpolygonsoverlay.Polygons.Add(drawnpolygon);
                            Joysticknutton.UpdatePolygonLocalPosition(drawnpolygon);
                            Joysticknutton.Invalidate();
                            Joysticknutton.ZoomAndCenterMarkers(drawnpolygonsoverlay.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show(Strings.ERROR + "\n" + ex, Strings.ERROR);
                    }
                }
            }
        }

        private void panelWaypoints_ExpandClick(object sender, EventArgs e)
        {
            Commands.AutoResizeColumns();
        }

        private void panelWaypoints_CloseClick(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
 
            //CustomMessageBox.Show("HI");
           //Refresh_my_PTs();

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Reverse_my_PTs();

            
           /* myButton1.Text = MainV2.comPort.MAV.cs.mode.ToUpper();
           // CustomMessageBox.Show("HI");
            try
            {
                ((Button)sender).Enabled = false;

                MainV2.comPort.setWPCurrent(0); // set nav to

            }
            catch { CustomMessageBox.Show(Strings.CommandFailed, Strings.ERROR); }
            ((Button)sender).Enabled = true;*/
        }

        private void AUTOQUICK_Click(object sender, EventArgs e)
        {
            // myButton1.Text = MainV2.comPort.MAV.cs.mode.ToUpper();
            //MyView.ShowScreen("FlightData3");
            try
            {
                ///////////////
                MAVLink.mavlink_rc_channels_override_t rc = new MAVLink.mavlink_rc_channels_override_t();

                rc.target_component = MainV2.comPort.MAV.compid;
                rc.target_system = MainV2.comPort.MAV.sysid;
                rc.chan1_raw = 1500;
                rc.chan3_raw = 1500;
                MainV2.comPort.sendPacket(rc);
                MainV2.comPort.sendPacket(rc);
                ///////////////////
                ((Button)sender).Enabled = false;
                MainV2.comPort.setMode("HOLD");
            }
            catch { CustomMessageBox.Show(Strings.CommandFailed, Strings.ERROR); }
            ((Button)sender).Enabled = true;
            
        }


        private void HOLDQUICK_Click(object sender, EventArgs e)
        {
            if (Joystick_Panel.Visible == true)
            { 
                Joystick_Panel.Visible = false; 
            }
            else
            {
                Joystick_Panel.Visible = true;
                FollowMePanel.Visible = false;
            }
        }

        private void HOMECLICK_Click(object sender, EventArgs e)
        {
            if (MainV2.comPort.MAV.cs.lat != 0)
            {
                TXT_homealt.Text = (MainV2.comPort.MAV.cs.altasl).ToString("0");
                TXT_homelat.Text = MainV2.comPort.MAV.cs.lat.ToString();
                TXT_homelng.Text = MainV2.comPort.MAV.cs.lng.ToString();
            }
            else
            {
                CustomMessageBox.Show("If you're at the field, connect to your APM and wait for GPS lock. Then click 'Home Location' link to set home to your location");
            }
        }

        private void ClearClick_Click(object sender, EventArgs e)
        {
            DialogResult Reply;
            Reply = CustomMessageBox.Show("You are about to clear your route. Click OK to confirm","Warning",MessageBoxButtons.OKCancel,MessageBoxIcon.Warning);
            if (Reply == DialogResult.OK)
            {
                quickadd = true;

                // mono fix
                Commands.CurrentCell = null;

                Commands.Rows.Clear();

                selectedrow = 0;
                quickadd = false;
                writeKML();
                currentwp.Markers.Clear();
            
                ///////////////////////////////////////////
            }
        }

        private void myButton1_Click(object sender, EventArgs e)
        {
            //CustomMessageBox.Show(MainV2.comPort.MAV.cs.mode.ToUpper());
          //  myButton1.Text = MainV2.comPort.MAV.cs.mode.ToUpper();
        }

        private void Deletebu_Click(object sender, EventArgs e)
        {

            
           
           
               
            
           
            try
            {
                change_wp_key = true;
                CurentRectMarker = null;
                GMapMarkerRect rc = markerjustleft;
                //int answer = int.Parse(rc.InnerMarker.Tag.ToString());

                rc.ResetColor();

                Joysticknutton.Invalidate(false);

                int no = clickedmarker;
                no = (int)WPNumber.Value;
         if (no > 0 && no <= Commands.Rows.Count)
            {
                quickadd = true;
                Commands.Rows.RemoveAt(no - 1); // home is 0
                quickadd = false;
                writeKML();
                setgradanddistandaz();
                if (no > Commands.Rows.Count)
                { no = Commands.Rows.Count;
                  WPNumber.Value = no;
                }
                    WPNumberUpdate();

              }
            }
            catch { CustomMessageBox.Show("error selecting wp, please try again."); }
            Deletebu.Text = "Delete WP";
            
            //Refresh_my_PTs();
            
            
  
        }

        private void mytest_Click(object sender, EventArgs e)
        {

        }

        private void panelMap_Paint(object sender, PaintEventArgs e)
        {

        }

        private void TXT_WPRad_TextChanged(object sender, EventArgs e)
        {

        }

        private void switchview_Click(object sender, EventArgs e)
        {
            if (tableLayoutPanel6.Visible == true)
            { tableLayoutPanel6.Visible = false; }
            else
            {
                tableLayoutPanel6.Visible = true;
                Hide_BTNs();
            }
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void newhome_Click(object sender, EventArgs e)
        {
            /*if (MainV2.comPort.MAV.cs.lat != 0)
            {
                TXT_homealt.Text = (MainV2.comPort.MAV.cs.altasl).ToString("0");
                TXT_homelat.Text = MainV2.comPort.MAV.cs.lat.ToString();
                TXT_homelng.Text = MainV2.comPort.MAV.cs.lng.ToString();
            }
            else
            {
                CustomMessageBox.Show("If you're at the field, connect to your APM and wait for GPS lock. Then click 'Home Location' link to set home to your location");
            }*/
            if (panel8.Visible == true)
            { panel8.Visible = false; }
            else
            { panel8.Visible = true; }
            
        }

        private void Reversewpbutton_Click(object sender, EventArgs e)
        {
            Reverse_my_PTs();
        }

        private void myButton2_Click(object sender, EventArgs e)
        {
           // CurentRectMarker = null;
            //GMapMarkerRect rc = markerjustleft;
            //int answer = int.Parse(rc.InnerMarker.Tag.ToString());

           //rc.ResetColor();

            //MainMap.Invalidate(false);

            int no = clickedmarker;
            //Commands.Rows[no].Cells[5].Value;
            if (panel6.Visible == true && SpeedPanel.Visible == false)
            { panel6.Visible = false;
                FormatPanel.Visible = false;
            }
            else
            { panel6.Visible = true; }
            WPNumber.Value = 1;
            /*if (no > 0 && panel6.Visible == true)
            {
                LatDeg.Value = Math.Floor(Convert.ToDecimal(Commands.Rows[no - 1].Cells[5].Value));
                LatMin.Value = ((Convert.ToDecimal(Commands.Rows[no - 1].Cells[5].Value) * 60) % 60);


                LongDeg.Value = Math.Floor(Convert.ToDecimal(Commands.Rows[no - 1].Cells[6].Value));
                LongMin.Value = ((Convert.ToDecimal(Commands.Rows[no - 1].Cells[6].Value) * 60) % 60);
            }*/

            //Refresh_my_PTs();

        }

        private void panel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void myButton2_Click_1(object sender, EventArgs e)
        {
            double Lat;
            double Latd=(double)LatDeg.Value;
            double Latm = (double)LatMin.Value;
            double Lats = (double)LatSec.Value;
            Lat = Latd + Latm / 60.0 + Lats / 60.0 / 60.0;
            double Long;
            double Lond = (double)LongDeg.Value;
            double Lonm = (double)LongMin.Value;
            double Lons = (double)LongSec.Value;
            Long = Lond + Lonm / 60.0 + Lons / 60.0 / 60.0;
            if (NS.Text == "S")
            { Lat = Lat * -1; }
            if (NS.Text == "W")
            { Long = Long * -1; }
            
            CurentRectMarker = null;
            GMapMarkerRect rc = markerjustleft;
            //int answer = int.Parse(rc.InnerMarker.Tag.ToString());

           // rc.ResetColor();

            //MainMap.Invalidate(false);

            int no = clickedmarker;
            no = (int) WPNumber.Value;
            if (no > 0 && no <= Commands.Rows.Count)
            {
                Commands.Rows[no-1].Cells[5].Value = Lat;
                Commands.Rows[no-1].Cells[6].Value = Long;
               // CustomMessageBox.Show(Lat.ToString());
                writeKML();
            }
            Show_BTNs();

        }

        private void NS_Click(object sender, EventArgs e)
        {
            if (NS.Text == "N")
            { NS.Text = "S"; }
            else
            { NS.Text = "N"; }
        }

        private void EW_Click(object sender, EventArgs e)
        {
            if (EW.Text == "E")
            { EW.Text = "W"; }
            else
            { EW.Text = "E"; }
        }

        private void MainMap_Load(object sender, EventArgs e)
        {

        }
        private void Show_BTNs()
        {
            WPNumber.Enabled = true;
            BackWP.Enabled = true;
            NextWP.Enabled = true;
            BUT_write.Enabled = true;
            BUT_read.Enabled = true;
            switchview.Enabled = true;
            EditButton.Enabled = true;
            myButton13.Enabled = true;
            Deletebu.Enabled = true;
            myButton14.Enabled = true;
            BUT_saveWPFile.Enabled = true;
            BUT_loadwpfile.Enabled = true;
            ClearClick.Enabled = true;
            myButton12.Enabled = false;
            Reversewpbutton.Enabled = true;
        }
        private void Hide_BTNs()
        {
            if (change_wp_key == false)
            {
                WPNumber.Enabled = false;
                BackWP.Enabled = false;
                NextWP.Enabled = false;
                BUT_write.Enabled = false;
                BUT_read.Enabled = false;
                switchview.Enabled = false;
                myButton13.Enabled = false;
                FormatPanel.Visible = false;
                EditButton.Enabled = false;
                Deletebu.Enabled = false;
                myButton14.Enabled = false;
                BUT_saveWPFile.Enabled = false;
                BUT_loadwpfile.Enabled = false;
                ClearClick.Enabled = false;
                myButton12.Enabled = true;
                Reversewpbutton.Enabled = false;
            }
        }
        private void NextWP_Click(object sender, EventArgs e)
        {
            WPNumber.Value = WPNumber.Value + 1;
            int no = (int)WPNumber.Value;
            /*if (no > 0 && no <= Commands.Rows.Count)
            {
                LatDeg.Value = Math.Floor(Convert.ToDecimal(Commands.Rows[no - 1].Cells[5].Value));
                LatMin.Value = ((Convert.ToDecimal(Commands.Rows[no - 1].Cells[5].Value) * 60) % 60);
                WPSp.Value = (Convert.ToDecimal((Convert.ToDouble(Commands.Rows[no - 1].Cells[Command.Index + 2].Value) / 0.514444444)));
                WPTh.Value = Math.Floor(Convert.ToDecimal(Commands.Rows[no - 1].Cells[Command.Index + 3].Value));
                LongDeg.Value = Math.Floor(Convert.ToDecimal(Commands.Rows[no - 1].Cells[6].Value));
                LongMin.Value = ((Convert.ToDecimal(Commands.Rows[no - 1].Cells[6].Value) * 60) % 60);
            }*/

        }

        private void BackWP_Click(object sender, EventArgs e)
        {
            if (WPNumber.Value > 1) { WPNumber.Value = WPNumber.Value - 1; }
            else { WPNumber.Value = Commands.Rows.Count; }
            int no = (int)WPNumber.Value;
           /* if (no > 0 && no <= Commands.Rows.Count)
            {
                LatDeg.Value = Math.Floor(Convert.ToDecimal(Commands.Rows[no - 1].Cells[5].Value));
                LatMin.Value = ((Convert.ToDecimal(Commands.Rows[no - 1].Cells[5].Value)*60)%60);
                WPSp.Value = (Convert.ToDecimal((Convert.ToDouble(Commands.Rows[no - 1].Cells[Command.Index + 2].Value) / 0.514444444)));
                WPTh.Value = Math.Floor(Convert.ToDecimal(Commands.Rows[no - 1].Cells[Command.Index + 3].Value));

                LongDeg.Value = Math.Floor(Convert.ToDecimal(Commands.Rows[no - 1].Cells[6].Value));
                LongMin.Value = ((Convert.ToDecimal(Commands.Rows[no - 1].Cells[6].Value) * 60) % 60);


                
            }*/
        }

        private void myButton3_Click(object sender, EventArgs e)
        {
            MAVLinkInterface port = MainV2.comPort;

            if (!port.BaseStream.IsOpen)
            {
                return;
            }
            try
                {

            // m
             port.setParam("WP_RADIUS", (byte)int.Parse(TXT_WPRad.Text) / CurrentState.multiplierdist);

            // cm's
            port.setParam("WPNAV_RADIUS", (byte)int.Parse(TXT_WPRad.Text) / CurrentState.multiplierdist * 100);
                }
                catch
            {}
        }

        private void myButton4_Click(object sender, EventArgs e)
        {
            SpeedPanel.Visible = true;
            SpeedPanel.Enabled = true;
            panel6.Enabled = false;
        }
        private void myButton15_Click(object sender, EventArgs e)
        {
            LatDeg.Enabled = true;
            LatMin.Enabled = false;
            LatSec.Enabled = false;

            LongDeg.Enabled = true;
            LongMin.Enabled = false;
            LongSec.Enabled = false;

            LatDeg.DecimalPlaces = 5;
            LongDeg.DecimalPlaces = 5;

            format_flag = 0;
            WPNumberUpdate();
            myButton15.Font = new Font(myButton17.Font, FontStyle.Bold);
            myButton16.Font = new Font(myButton17.Font, FontStyle.Regular);
            myButton17.Font = new Font(myButton17.Font, FontStyle.Regular);
        }
        private void SetSpeedBTN(object sender, EventArgs e)
        {
            int no = (int)WPNumber.Value;

            CurentRectMarker = null;
            GMapMarkerRect rc = markerjustleft;
            if (no > 0 && no <= Commands.Rows.Count)
            {

                Commands.Rows[no - 1].Cells[(Alt.Index)].Value = (Convert.ToDecimal(Convert.ToDouble(WPSp.Value) * -0.514444444)).ToString();
                //CustomMessageBox.Show((Convert.ToDecimal(Convert.ToDouble(WPSp.Value) * -0.514444444)).ToString());
                writeKML();
            }
        }
            private void myButton6_Click(object sender, EventArgs e)
        {
            int row_index = (int)numericUpDown3.Value-1;
            if (row_index < Commands.RowCount+1 && row_index >=-1) // down

            {
             
                DataGridViewRow myrow = Commands.Rows[(int)WPNumber.Value-1];
                int curr_index = myrow.Index;
                Commands.Rows.Remove(myrow);
                if (curr_index < row_index)
                {
                    Commands.Rows.Insert(row_index, myrow);
                }
                else
                {
                    Commands.Rows.Insert(row_index+1, myrow);
                }
                writeKML();
            }
            PlaceAfterPanel.Visible = false;
            Show_BTNs();
        }

        private void myButton7_Click(object sender, EventArgs e)
        {
            PlaceAfterPanel.Visible = false;
            Show_BTNs();
        }
        private void BackSpeed_Click(object sender, EventArgs e)
        {
            SpeedPanel.Visible = false;
            SpeedPanel.Enabled = false;
            panel6.Enabled = true;
        }
        private void WPSpeed_ValueChanged(object sender, EventArgs e)
        {
            if (WPSp.Value == 0) { WPSp.BackColor = Color.White; }
            else { WPSp.BackColor = Color.Black; }
        }
        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown3.Value < 0) { numericUpDown3.Value = Commands.Rows.Count; }
            if (numericUpDown3.Value > Commands.Rows.Count) { numericUpDown3.Value = 1; }
        }
        private void WPNumberUpdate()
        {
            change_wp_key = true;
            if (WPNumber.Value > Commands.Rows.Count)
            {
                WPNumber.Value = 1;
            }
            if (WPNumber.Value > 0 && Commands.Rows.Count>0)
            {
                int wpnum = Decimal.ToInt32(WPNumber.Value);
                double currwp_lat = double.Parse(Commands.Rows[wpnum - 1].Cells[Lat.Index].Value.ToString()); // lat
                double currwp_lng = double.Parse(Commands.Rows[wpnum - 1].Cells[Lon.Index].Value.ToString()); // lng
                currentwp.Markers.Clear();
                addcurrwpmarker(" ", currwp_lng, currwp_lat, 0, Color.Yellow);
            }
            int no = (int)WPNumber.Value;
            if (no > 0 && no <= Commands.Rows.Count)
            {
                
                if (Math.Floor(Convert.ToDecimal(Commands.Rows[no - 1].Cells[5].Value)) < 0)
                { NS.Text = "S"; }
                else
                { NS.Text = "N"; }
                float wp_speed = (float)(Convert.ToDecimal((Convert.ToDouble(Commands.Rows[no - 1].Cells[Alt.Index].Value) / -0.514444444)));
                if (wp_speed > 0)
                {
                    WPSp.Value = (Convert.ToDecimal((Convert.ToDouble(Commands.Rows[no - 1].Cells[Alt.Index].Value) / -0.514444444)));
                    //CustomMessageBox.Show(Commands.Rows[no - 1].Cells[Alt.Index].Value.ToString());
                    WPSp.ForeColor = Color.White;
                    WPSp.BackColor = Color.Gray;
                }
                else
                {
                    WPSp.Value = 0;
                    WPSp.ForeColor = Color.White;
                    WPSp.BackColor = Color.White;
                }
                WPTh.Value = Math.Floor(Convert.ToDecimal(Commands.Rows[no - 1].Cells[Command.Index + 3].Value));
                if (format_flag == 0)
                {
                    LatDeg.Value = Math.Abs((Convert.ToDecimal(Commands.Rows[no - 1].Cells[5].Value)));
                    LongDeg.Value = Math.Abs((Convert.ToDecimal(Commands.Rows[no - 1].Cells[6].Value)));
                    LatMin.Value = 0;
                    LongMin.Value = 0;
                    LatSec.Value = 0;
                    LongSec.Value = 0;
                }
                else if (format_flag == 1)
                {
                    LatDeg.Value = Math.Abs(Math.Floor(Convert.ToDecimal(Commands.Rows[no - 1].Cells[5].Value)));
                    LatMin.Value = Math.Abs(((Convert.ToDecimal(Commands.Rows[no - 1].Cells[5].Value) * 60) % 60));
                    LatSec.Value = 0;
                    LongDeg.Value = Math.Abs(Math.Floor(Convert.ToDecimal(Commands.Rows[no - 1].Cells[6].Value)));
                    LongMin.Value = Math.Abs(((Convert.ToDecimal(Commands.Rows[no - 1].Cells[6].Value) * 60) % 60));
                    LongSec.Value = 0;
                }
                else
                {
                    LatDeg.Value = Math.Abs(Math.Floor(Convert.ToDecimal(Commands.Rows[no - 1].Cells[5].Value)));
                    LatMin.Value = Math.Abs(Math.Floor((Convert.ToDecimal(Commands.Rows[no - 1].Cells[5].Value) * 60) % 60));
                    LatSec.Value = ((Math.Abs((Convert.ToDecimal(Commands.Rows[no - 1].Cells[5].Value) * 60) % 60)- LatMin.Value)*60)%60;
                    LongDeg.Value = Math.Abs(Math.Floor(Convert.ToDecimal(Commands.Rows[no - 1].Cells[6].Value)));
                    LongMin.Value = Math.Abs(Math.Floor((Convert.ToDecimal(Commands.Rows[no - 1].Cells[6].Value) * 60) % 60));
                    LongSec.Value = ((Math.Abs(((Convert.ToDecimal(Commands.Rows[no - 1].Cells[6].Value) * 60) % 60)) - LongMin.Value) * 60) % 60;

                }
                if (Math.Floor(Convert.ToDecimal(Commands.Rows[no - 1].Cells[6].Value)) < 0)
                { EW.Text = "W"; }
                else
                { EW.Text = "E"; }


                
            }
            change_wp_key = false;
        }
        private void WPNumber_ValueChanged(object sender, EventArgs e)
        {
            WPNumberUpdate();
            
        }

        private void SwarmPanel_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void myButton5_Click(object sender, EventArgs e)
        {
            if (SwarmPanel.Visible == true)
            { SwarmPanel.Visible = false;}
            else { SwarmPanel.Visible = true; 
            }

        }

        private void Joystickbutton_Click(object sender, EventArgs e)
        {
            /* Form joy = new JoystickSetup();
              ThemeManager.ApplyThemeTo(joy);
              joy.Show();*/
            if (Joystick_Panel.Visible == true)
            { Joystick_Panel.Visible = false; }
            else
            {
                Joystick_Panel.Visible = true;
            }

        }

        private void myButton1_Click_1(object sender, EventArgs e)
        {
            if (panel9.Visible == true)
            { panel9.Visible = false; }
            else
            {
                panel9.Visible = true;
            }
            //CustomMessageBox.Show(thisthread.IsAlive.ToString());
            
        }

        private void myButton11_Click(object sender, EventArgs e)
        {
            panel9.Visible = false;
        }

        private void myButton10_Click(object sender, EventArgs e)
        {
            try
            {
                ((Button)sender).Enabled = false;
                MainV2.comPort.setMode("ANCHOR");
            }
            catch { CustomMessageBox.Show(Strings.CommandFailed, Strings.ERROR); }
            ((Button)sender).Enabled = true;
        }
        private void FollowMeBTN_Click(object sender, EventArgs e)
        {

        }

        private void myButton9_Click(object sender, EventArgs e)
        {
            try
            {
                ((Button)sender).Enabled = false;
                MainV2.comPort.setMode("MANUAL");
            }
            catch { CustomMessageBox.Show(Strings.CommandFailed, Strings.ERROR); }
            ((Button)sender).Enabled = true;
        }

        private void myButton8_Click(object sender, EventArgs e)
        {
            try
            {
                ///////////////
                MAVLink.mavlink_rc_channels_override_t rc = new MAVLink.mavlink_rc_channels_override_t();

                rc.target_component = MainV2.comPort.MAV.compid;
                rc.target_system = MainV2.comPort.MAV.sysid;
                rc.chan1_raw = 1500;
                rc.chan3_raw = 1500;
                MainV2.comPort.sendPacket(rc);
                MainV2.comPort.sendPacket(rc);
                ///////////////////
                ((Button)sender).Enabled = false;
                MainV2.comPort.setMode("HOLD");
            }
            catch { CustomMessageBox.Show(Strings.CommandFailed, Strings.ERROR); }
            ((Button)sender).Enabled = true;
            
        }

        private void myButton13_Click(object sender, EventArgs e)
        {
            if (FormatPanel.Visible == true)
            { FormatPanel.Visible = false; }
            else
            { FormatPanel.Visible = true; }


        }

        private void myButton12_Click(object sender, EventArgs e)
        {

            int row_index = (int)WPNumber.Value - 1;
            if (row_index < Commands.RowCount - 1 && row_index >= 0) // down
            {
                DataGridViewRow myrow = Commands.Rows[row_index];
                Commands.Rows.RemoveAt(row_index);
                Commands.Rows.Insert(row_index + 1, myrow);
                writeKML();
            }
            
        }

        private void LatDeg_ValueChanged(object sender, EventArgs e)
        {
            Hide_BTNs();
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void myButton16_Click(object sender, EventArgs e)
        {
            LatDeg.Enabled = true;
            LatMin.Enabled = true;
            LatSec.Enabled = false;

            LongDeg.Enabled = true;
            LongMin.Enabled = true;
            LongSec.Enabled = false;

            LatDeg.DecimalPlaces = 0;
            LongDeg.DecimalPlaces = 0;

            LatMin.DecimalPlaces = 3;
            LongMin.DecimalPlaces = 3;
            format_flag = 1;
            WPNumberUpdate();
            myButton15.Font = new Font(myButton17.Font, FontStyle.Regular);
            myButton16.Font = new Font(myButton17.Font, FontStyle.Bold);
            myButton17.Font = new Font(myButton17.Font, FontStyle.Regular);

        }

        private void myButton17_Click(object sender, EventArgs e)
        {
            LatDeg.Enabled = true;
            LatMin.Enabled = true;
            LatSec.Enabled = true;

            LongDeg.Enabled = true;
            LongMin.Enabled = true;
            LongSec.Enabled = true;

            LatDeg.DecimalPlaces = 0;
            LongDeg.DecimalPlaces = 0;

            LatMin.DecimalPlaces = 0;
            LongMin.DecimalPlaces = 0;

            LatSec.DecimalPlaces = 2;
            LongSec.DecimalPlaces = 2;
            myButton15.Font = new Font(myButton17.Font, FontStyle.Regular);
            myButton16.Font = new Font(myButton17.Font, FontStyle.Regular);
            myButton17.Font = new Font(myButton17.Font, FontStyle.Bold);

            format_flag = 3;
            WPNumberUpdate();

        }

        private void LatMin_ValueChanged(object sender, EventArgs e)
        {
            Hide_BTNs();
        }

        private void LatSec_ValueChanged(object sender, EventArgs e)
        {
            Hide_BTNs();
        }

        private void LongDeg_ValueChanged(object sender, EventArgs e)
        {
            Hide_BTNs();
        }

        private void LongMin_ValueChanged(object sender, EventArgs e)
        {
            Hide_BTNs();
        }

        private void LongSec_ValueChanged(object sender, EventArgs e)
        {
            Hide_BTNs();
        }

        private void myButton12_Click_1(object sender, EventArgs e)
        {
            Show_BTNs();
        }

        private void myButton14_Click(object sender, EventArgs e)
        {
            if (PlaceAfterPanel.Visible == true)
            { PlaceAfterPanel.Visible = false; }
            else
            { PlaceAfterPanel.Visible = true; }
            Hide_BTNs();
        }

        private void Joystick_Panel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void myButton20_Click(object sender, EventArgs e)
        {
            tableLayoutPanel6.Visible = false;
            Show_BTNs();
        }

        private void myButton18_Click(object sender, EventArgs e)
        {
            readwps();
            update_main_list_of_polys();
            MainV2.MyView.ShowScreen("FlightData3");
            tableLayoutPanel6.Visible = false;
            Show_BTNs();
        }

        private void myButton21_Click(object sender, EventArgs e)
        {
            uploadwps();


            update_main_list_of_polys();

            MainV2.MyView.ShowScreen("FlightData3");
            tableLayoutPanel6.Visible = false;
            Show_BTNs();
        }
        private void update_main_list_of_polys()
        {

            MainV2.PolygonsOverlay.Clear();
            List<PointLatLng> TempPointHolder = new List<PointLatLng>();
            for (int i = 0; i < list_of_polys.Count; i++)
            {
                list_of_polys[i].IsHitTestVisible = true;
                MainV2.PolygonsOverlay.Polygons.Add(list_of_polys[i]);
                TempPointHolder = list_of_polys[i].Points;
                //GMapMarker Marker_Measure = new GMarkerGoogle(new PointLatLng(MouseDownStart.Lat, MouseDownStart.Lng), GMarkerGoogleType.orange);
                for (int j = 0; j < TempPointHolder.Count; j++)
                {
                    //CustomMessageBox.Show(TempPointHolder.Count.ToString());
                    MainV2.PolygonsOverlay.Markers.Add(new GMarkerCross(TempPointHolder[j]));

                }

            }

        }

        private void label19_Click(object sender, EventArgs e)
        {

        }

        private void ToolsTable_Paint(object sender, PaintEventArgs e)
        {

        }
        
        private void MeasureBTN_Click(object sender, EventArgs e)
        {
            if (measurePanel.Visible == true)
            {
                measurePanel.Visible = false;
                DrawBTN.Enabled = true;
                measure_mode = 0;
                MeasureBTN.Text = "Measure";
                MeasurePts.Clear();
                MeasureRoute.Clear();
                total_measurement = 0;
            }
            else
            {
                measureLabel.Text = "0";
                DrawBTN.Enabled = false;
                measurePanel.Visible = true;
                //tableLayoutPanel1.Enabled = false;
                //MeasureBTN.Enabled = true;
                measure_mode = 1;
                first_measurementpt = true;
                MeasureBTN.Text = "End";
            }
        }

        private void UnitsMeasureBTN_Click(object sender, EventArgs e)
        {
            measure_unit_flag = (measure_unit_flag + 1) % 3;
            if (measure_unit_flag == 0)
            {
                UnitsMeasureBTN.Text = "m";
                measureLabel.Text = total_measurement.ToString("0");
            }
            else if (measure_unit_flag == 1)
            {
                UnitsMeasureBTN.Text = "km";
                measureLabel.Text = (total_measurement / 1000.0).ToString("F2");
            }
            else if (measure_unit_flag == 2)
            {
                UnitsMeasureBTN.Text = "Nm";
                measureLabel.Text = (total_measurement / 1852).ToString("F2");
            }
        }

        private void ToolsBTN_Click(object sender, EventArgs e)
        {
            if (ToolsTable.Visible == true)
            { ToolsTable.Visible = false; }
            else { ToolsTable.Visible = true; }
        }

        private void panel10_Paint(object sender, PaintEventArgs e)
        {

        }

        private void DrawBTN_Click(object sender, EventArgs e)
        {
            if (DrawPanel.Visible == true)
            {
                DrawPanel.Visible = false;
                MeasureBTN.Enabled = true;
                ToolsBTN.Enabled = true;
                DrawBTN.Text = "Draw";
            }
            else
            {
                MeasureBTN.Enabled = false;
                DrawPanel.Visible = true;
                ToolsBTN.Enabled = false;
                DrawBTN.Text = "Close";
            }
        }

        private void NewDrawPoly_Click(object sender, EventArgs e)
        {
            new_polygon_flag = true;
            new_polygon_list.Clear();
            new_poly_index = list_of_polys.Count;
            if (NewDrawPoly.Text == "New")
            {
                NewDrawPoly.Text = "End";
                
                PolyNameBox.Enabled = true;
                ColorBTN.Enabled = true;

            }
            else
            {
                NewDrawPoly.Text = "New";
                new_polygon_flag = false;
             
                PolyNameBox.Enabled = false;
                ApplyPoly.Enabled = false;
                DeletePoly.Enabled = false;
                ColorBTN.Enabled = false;

            }




        }

        private void ApplyPoly_Click(object sender, EventArgs e)
        {
            String poly_name = PolyNameBox.Text;
            polygon_edit_flag = false;
            new_polygon_flag = false;

            PolyNameBox.Enabled = false;
            ApplyPoly.Enabled = false;
            DeletePoly.Enabled = false;
            ColorBTN.Enabled = false;
            list_of_polys[selected_poly_index].Name = poly_name;
            list_of_polys[selected_poly_index].Fill = new SolidBrush(Color.FromArgb(20, ColorBTN.BackColor));
            list_of_polys[selected_poly_index].Stroke = new Pen(Color.Black, 1);
            
            drawpolys();
            first_selection_of_polygon = 0;

        }

        private void tableLayoutPanel8_Paint(object sender, PaintEventArgs e)
        {


        }

        private void ColorBTN_Click(object sender, EventArgs e)
        {
            ColorDialog colorDlg = new ColorDialog();
            colorDlg.AllowFullOpen = false;
            colorDlg.AnyColor = true;
            colorDlg.SolidColorOnly = false;
            colorDlg.Color = Color.Red;

            if (colorDlg.ShowDialog() == DialogResult.OK)
            {
                ColorBTN.BackColor = colorDlg.Color;
            }
        }

        private void DeletePoly_Click(object sender, EventArgs e)
        {
            polygon_edit_flag = false;
            new_polygon_flag = false;
            PolyNameBox.Enabled = false;
            ApplyPoly.Enabled = false;
            DeletePoly.Enabled = false;
            ColorBTN.Enabled = false;
            list_of_polys.RemoveAt(selected_poly_index);
            drawpolys();
        }

        private void SavePolysBTN_Click(object sender, EventArgs e)
        {
            savepolys();
        }

        private void ImportPolysBTN_Click(object sender, EventArgs e)
        {
            loadpolys();
        }

        private void myButton22_Click(object sender, EventArgs e)
        {

            update_main_list_of_polys();

            MainV2.MyView.ShowScreen("FlightData3");
            tableLayoutPanel6.Visible = false;
            Show_BTNs();
        }

        private void zoom_in_Click(object sender, EventArgs e)
        {
            Joysticknutton.Zoom += 1;
        }

        private void zoom_out_Click(object sender, EventArgs e)
        {
            Joysticknutton.Zoom -= 1;
        }
    }
}
