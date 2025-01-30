using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using SignalLabelingApp.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Primitives;
using Avalonia;
using Avalonia.Media.Immutable;
using Avalonia.Collections;
using Avalonia.Interactivity;
using Avalonia.Layout;
using System.Collections.ObjectModel;
using Label = SignalLabelingApp.Classes.Label;

namespace SignalLabelingApp.Views
{
    public partial class OyControl: UserControl 
    {
        public TextBlock maxValue { 
            get;
            set;
        }
        public TextBlock middleValue { get;  set; }
        public TextBlock minValue { get;  set; }

        public OyControl()
        {
            InitializeComponent();
            maxValue = MaxValue;
            middleValue = MiddleValue;
            minValue = MinValue;
        }

    }

}