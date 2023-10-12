using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RestrictR
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProcessWindow : Window
    {
        public ProcessWindow()
        {
            this.InitializeComponent();
        }
    }

    // The ItemsSource used is a list of custom-class Bar objects called BarItems

    public class ProcessInfo
    {
        public Bar(string in)
        {
            Length = length;
            MaxLength = max;

            Height = length / 4;
            MaxHeight = max / 4;

            Diameter = length / 6;
            MaxDiameter = max / 6;
        }


        public double Length { get; set; }
        public int MaxLength { get; set; }

        public double Height { get; set; }
        public double MaxHeight { get; set; }

        public double Diameter { get; set; }
        public double MaxDiameter { get; set; }
    }

    public ObservableCollection<Bar> BarItems;
    private int MaxLength = 425;

    private void InitializeData()
    {
        if (BarItems == null)
        {
            BarItems = new ObservableCollection<Bar>();
        }
        BarItems.Add(new Bar(300, this.MaxLength));
        BarItems.Add(new Bar(25, this.MaxLength));
        BarItems.Add(new Bar(175, this.MaxLength));
    }


}
