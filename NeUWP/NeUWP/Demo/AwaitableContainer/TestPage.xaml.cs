using NeUWP.Controls.ATest;
using NeUWP.Controls.AwaitConatainer.Tset;
using NeUWP.Helpers;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace NeUWP.Controls
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TestPage : PageBase

    {
        public TestPage()
        {
            this.InitializeComponent();
        }

        private void ToastTest(object sender, RoutedEventArgs e)
        {
            TestDialog.TestToast();  
        }

        private void MenuTest(object sender, RoutedEventArgs e)
        {
            TestDialog.TestMenu(sender);
        }

        private void MenuPositionTap(object sender, TappedRoutedEventArgs e)
        {
            TestDialog.TestMenu(e.GetPosition(null));
        }

        private void DialogTest(object sender, RoutedEventArgs e)
        {
            TestDialog.TestNormalDialog();
        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private async void DrawerTest(object sender, RoutedEventArgs e)
        {
            // App.NavigationManager.Navigate(typeof(TestNDrawer));
          await  MDrawer.Show(new DrawerContent(), 100, true,MDrawer.Mode.Bottom);
        }

        private async void DrawerTest1(object sender, RoutedEventArgs e)
        {
            await MDrawer.Show(new DrawerContent(), 100, true, MDrawer.Mode.Right);
        }
    }
}
