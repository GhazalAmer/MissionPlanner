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
using MissionPlanner.ArduPilot;
using MissionPlanner.Controls;
using MissionPlanner.Grid;
using MissionPlanner.Maps;
using MissionPlanner.Plugin;
using MissionPlanner.Properties;
using MissionPlanner.Utilities;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using SharpKml.Base;
using SharpKml.Dom;
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
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
#if !LIB
using GDAL;
#endif
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using Feature = SharpKml.Dom.Feature;
using Formatting = Newtonsoft.Json.Formatting;
using ILog = log4net.ILog;
using Placemark = SharpKml.Dom.Placemark;
using Point = System.Drawing.Point;
using Resources = MissionPlanner.Properties.Resources;
using Newtonsoft.Json;
using MissionPlanner.ArduPilot.Mavlink;

namespace MissionPlanner.GCSViews
{
    public partial class FlightPlannerMarakeb : MyUserControl, IDeactivate, IActivate
    {
        public FlightPlannerMarakeb()
        {
            InitializeComponent();
        }

        public void Activate()
        {
            
        }
        public void Deactivate()
        {
        
        }

        private void FlightPlannerMarakeb_Load(object sender, EventArgs e)
        {

        }

        private void MainMap_Load(object sender, EventArgs e)
        {

        }
    }
}
