using NeUWP.Controls;
using NeUWP.Frameworks;
using NeUWP.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NeUWP.Demo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TestPivot : Page
    {
        public TestPivot()
        {
            this.InitializeComponent();
            init();
        }

        private void init()
        {

            ObservableCollection<BData> source = new ObservableCollection<BData>();
            source.Add(new BData() { TargetCover = "http://img5.cache.netease.com/news/2016/10/19/20161019113906d67cb.jpg", Title = " 1" });
            source.Add(new BData() { TargetCover = "http://img2.cache.netease.com/news/2016/11/18/2016111813354234329.jpg", Title = "picture 2" });
            source.Add(new BData() { TargetCover = "http://img5.cache.netease.com/news/2016/11/3/20161103153210077e7.jpg", Title = "ddddddddddddd" });
            source.Add(new BData() { TargetCover = "http://img5.cache.netease.com/news/2016/10/19/20161019113906d67cb.jpg", Title = "bbbbbb" });

            source.Add(new BData() { TargetCover = "http://img5.cache.netease.com/news/2016/10/19/20161019113906d67cb.jpg", Title = "picture 555555555" });
            this.DataContext = source;
        }

        class BData : ViewModelBase
        {
            private string title;
            public string Title
            {
                get { return title; }
                set { SetProperty(ref title, value); }
            }

            private string targetCover;
            public string TargetCover
            {
                get { return targetCover; }
                set { SetProperty(ref targetCover, value); }
            }
        }

    }
}
