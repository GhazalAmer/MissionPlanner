using MissionPlanner.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MissionPlanner.GCSViews
{
    public partial class UserControl1 : MyUserControl, IDeactivate, IActivate
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        public void Activate()
        {

        }

        public void Deactivate()
        {

        }
    }
}