using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CasualMeter.Tracker;

namespace CasualMeter.Common.UI.Controls
{
    public class PlayerInfoControl : Grid
    {
        public static readonly DependencyProperty PlayerInfoProperty =
        DependencyProperty.Register("PlayerInfo", typeof(PlayerInfo),
            typeof(PlayerInfoControl), new UIPropertyMetadata(null));

        public PlayerInfo PlayerInfo
        {
            get { return (PlayerInfo)GetValue(PlayerInfoProperty); }
            set { SetValue(PlayerInfoProperty, value); }
        }
    }
}
