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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NeUWP.Controls
{
    public sealed partial class TestDialog1 : UserControl,IDialogChild
    {
        public TestDialog1()
        {
            this.InitializeComponent();
        }

        //实现IDialogChild接口,会在显示之前，将父控件给注入进来，在这主要用来船值出去给父控件，从而发送给调用方
        public MDilog ParentDialog
        {
            set; get;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ParentDialog.ChangeChildView(new TestDialog2(this));
        }

        //用户选择了bbb
        private void OnSetValue(object sender, RoutedEventArgs e)
        {
            ParentDialog.HideWithResult("bbb");
        }
    }
}
