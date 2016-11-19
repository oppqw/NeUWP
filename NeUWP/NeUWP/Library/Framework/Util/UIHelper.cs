using NeUWP.Controls;
using NeUWP.Frameworks;
using NeUWP.Utilities;
using NeUWP.ViewModels;
using NeUWP.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace NeUWP.Helpers
{
    public class UIHelper
    {

        private static object _lockObj = new object();
        public static async void TryCloseDrawers()
        {
            await MDrawer.TryCloseCurrent();
        }



        public static void Show(string content, ToastIconType iconType = ToastIconType.None,bool isUsePopup=false)
        {
            if (!string.IsNullOrEmpty(content))
            {

                DispatcherUtil.Run(() =>
                {


                    try
                    {
                        ToastView.ShowToast(content, iconType);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                }, Windows.UI.Core.CoreDispatcherPriority.High);
            }
        }

        public static void ShowNoNetToast()
        {
            Show("无网络。。。");
        }

        //该dilog控件已经自带了点击旁边关闭，以及关闭按钮，
        //如果在View内部需要触发关闭Dialog，可以让View实现IDialogChild接口，show的时候会将Dialog容器注入,支持关闭及向外传值
        //view需要设置好width 和height，
        //居中显示
        public static Task<object> ShowDilog(FrameworkElement view, bool isBackgroundTransparent = true, bool isHasCloseButton = true, bool isTapDismiss = true)
        {
            return MDilog.Show(view, isBackgroundTransparent, isHasCloseButton, isTapDismiss);
        }

        public static void ShowToast(string content, ToastIconType iconType = ToastIconType.None, string title = null)
        {
            ToastView.ShowToast(content, iconType, title);
        }

        public static Task<object> ShowMenu(IEnumerable<string> _data, FrameworkElement attachView, string title = null)
        {
            return MenuView.ShowFromView(_data, attachView, title);
        }

        public static Task<object> ShowMenu(IEnumerable<string> _data, string title = null)
        {
            return MenuView.Show(_data, title);
        }

        public static Task<object> ShowMenuAtPosition(IEnumerable<string> _data, Point position, string title = null)
        {
            return MenuView.ShowAtPosition(_data, position, title);
        }

    }
}
