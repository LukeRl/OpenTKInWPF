using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenTK.Wpf;


namespace OpenTK_Tutorial_in_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ExampleScene exampleScene;
        public MainWindow()
        {
            InitializeComponent();
            var settings = new GLWpfControlSettings
            {
                MajorVersion = 4,
                MinorVersion = 3
            };
            OpenTkControl.Start(settings);
            exampleScene = new ExampleScene();
        }

        private void OpenTkControl_OnRender(TimeSpan delta)
        {
            double width = OpenTkControl.ActualWidth;
            double height = OpenTkControl.ActualHeight;
            Point p = Mouse.GetPosition(OpenTkControl);
            if (p.X <= width && p.Y <= height && p.X >= 0 && p.Y >= 0)
            {
                Trace.WriteLine(p);
            }
            exampleScene.Render();
        }

        private void OpenTkControl_Loaded(object sender, RoutedEventArgs e)
        {
            exampleScene.Prepare();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            exampleScene.Close();
        }
    }
}
