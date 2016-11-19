using NeUWP.Controls.ATest;
using NeUWP.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NeUWP.Controls
{
    public class TestDialog
    {
        public static async void TestNormalDialog()
        {
            var _value = await UIHelper.ShowDilog(new TestDialog1(), false, true, true);
            UIHelper.ShowToast( _value as string, ToastIconType.Ok, "title");
        }

        static int i = 0;
        public static  void TestToast()
        {
            UIHelper.ShowToast("content"+i++,ToastIconType.Ok, "title" + i);
        }

        public static async void TestMenu(object sender) {
            List<string> _data = new List<string>() { "aaa", "bbbb", "ccc", MMenuItem.SplitorTag, "ddd" };
            var _value =await UIHelper.ShowMenu(_data, sender as FrameworkElement);
            if(_value!=null)
                UIHelper.ShowToast( (_value as MMenuItem).Tag, ToastIconType.Ok, "title");
            else
                UIHelper.ShowToast("空", ToastIconType.Ok, "title");
        }

        public static async void TestMenu(Point position)
        {
            List<string> _data = new List<string>() { "aaa", "bbbb", "ccc", MMenuItem.SplitorTag, "ddd" };
            var _value = await UIHelper.ShowMenuAtPosition(_data, position);
            if (_value != null)
                UIHelper.ShowToast( (_value as MMenuItem).Tag, ToastIconType.Ok, "title");
            else
                UIHelper.ShowToast( "空", ToastIconType.Ok, "title");
        }

        public static void Test()
        {
            App.PageContainer.PageFrame.Navigate(typeof(TestPage));
        }
    }
}
