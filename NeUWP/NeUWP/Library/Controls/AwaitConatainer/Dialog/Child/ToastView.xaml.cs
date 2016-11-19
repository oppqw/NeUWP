using NeUWP.Controls;
using NeUWP.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NeUWP.Controls
{
    public enum ToastIconType { None , Ok }
    public sealed partial class ToastView : UserControl,IDialogChild
    {
        private static ToastView _instance = new ToastView();

        public ToastView()
        {
            this.InitializeComponent();
        }

        public MDilog ParentDialog
        {
            set; get;
        }

        private DispatcherTimer _timer = null;
        private void TryStartTimer() {
            if (_timer != null)
            {
                _timer.Stop();
            }
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(1500);
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }


        private void _timer_Tick(object sender, object e)
        {
            (sender as DispatcherTimer).Stop();
            if (_timer == sender)
            {
                _timer = null;
                Hide();
            }
        }

        private void Hide() {
            VisualStateManager.GoToState(this, "HideState", true);
        }

        private void Show(ToastData data,DataTemplate template=null)
        {
            if (template == null)
                template = this.Resources["iconToastTemplate"] as DataTemplate;
            container.ContentTemplate = template;
            container.Content = data;
            if (DevUtil.DeviceFamily == DeviceFamilyType.Desktop)
            {
                if (data.Type == ToastIconType.None)
                {
                    root.Height = 50;
                    root.Width = 280;
                }
                else
                {
                    root.Height = 170;
                    root.Width = 270;
                }
            }
            if (ParentDialog == null)
                MDilog.Show(this, true, false, false,true);
            DispatcherUtil.Run(() => {
                VisualStateManager.GoToState(this, "ShowState", true);
                TryStartTimer();
            });
        }

        public static void ShowToast(string description, ToastIconType type, string title=null) {
          var _data =  new ToastData() { Title = title, Description = description, Type = type };
          _instance.Show(_data);
        }

        private void Storyboard_Completed(object sender, object e)
        {
            container.Content = null;
            ParentDialog.Hide();
        }
    }
    class ToastData
    {
        public string Title { set; get; }
        public string Description { set; get; }
        public ToastIconType Type { set; get; }
    }
}
