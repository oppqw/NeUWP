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
    public sealed partial class TestDialog2 : UserControl,IDialogChild
    {
        public TestDialog2(FrameworkElement back)
        {
            this.InitializeComponent();
            _back = back;
        }

        public MDilog ParentDialog
        {
            set; get;
        }
        FrameworkElement _back;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_back != null)
                ParentDialog.ChangeChildView(_back);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ParentDialog.HideWithResult("aaa");
        }
    }
}
