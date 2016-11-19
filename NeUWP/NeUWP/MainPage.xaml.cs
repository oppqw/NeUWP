using NeUWP.Controls;
using NeUWP.Demo;
using NeUWP.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NeUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : PageBase
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void dialogTest(object sender, RoutedEventArgs e)
        {
            TestDialog.Test();
        }

    
        private void imageTest(object sender, RoutedEventArgs e)
        {
            App.PageContainer.PageFrame.Navigate(typeof(TestXImage));
        }

        private void pivotTest(object sender, RoutedEventArgs e)
        {
            App.PageContainer.PageFrame.Navigate(typeof(TestPivot));
        }

        private void listTest(object sender, RoutedEventArgs e)
        {
            App.PageContainer.PageFrame.Navigate(typeof(TestListView));
        }

        private void gridviewTest(object sender, RoutedEventArgs e)
        {
            App.PageContainer.PageFrame.Navigate(typeof(TestGridView));
        }
    }
}
