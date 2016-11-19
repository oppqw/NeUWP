using NeUWP.Helpers;
using NeUWP.ViewModels;
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

namespace NeUWP.Controls.ATest
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TestNDrawer : PageBase
    {
        public TestNDrawer()
        {
            this.InitializeComponent();
            var _datas = new List<string>();
            for (int i = 0; i < 100; i++)
                _datas.Add("aaa" + i);

            list.ItemsSource = _datas;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //var _source = e.Parameter as MVDetailParameter;
            //if (_source != null)
            //{
            //    int br = 480;
            //    try
            //    {
            //        var data = await MvHelper.RequestMVPlayUrl(_source.id, br);
            //        player.SetSource(data.url);
            //    }
            //    catch { }
            //}
        }

        private void ScrollViewer_DirectManipulationCompleted(object sender, object e)
        {
            if (sc.HorizontalOffset < 50)
                sc.ChangeView(0, 0, 1, true);
            else
                drawer.Close();
        }

        private void drawer_OnClosed()
        {
            sc.ChangeView(0, 0, 1, false);
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            list.Height = e.NewSize.Height;
        }
    }
}
