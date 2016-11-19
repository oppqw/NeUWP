using NeUWP.Frameworks; 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
    public sealed partial class MenuView : UserControl, IDialogChild
    {
        public MenuView()
        {
            this.InitializeComponent();
        }

        public string Title{

            set {
                if (string.IsNullOrEmpty(value)) {
                    titleContainer.Visibility = Visibility.Collapsed;
                }
                else
                {
                    title.Text = value;
                    titleContainer.Visibility = Visibility.Visible;
                }
            } }

        public MDilog ParentDialog
        {
            set; get;
        }

        public static Task<object> ShowAtPosition(IEnumerable<string> menuitems, Point position,string title)
        {
            List<MMenuItem> _menus = new List<MMenuItem>();
            foreach (string tag in menuitems)
                _menus.Add(new MMenuItem() { Tag = tag });
            return ShowAtPosition(_menus, position,title);
        }

        public static Task<object> ShowFromView(IEnumerable<string> menuitems, FrameworkElement attachView, string title)
        {
            List<MMenuItem> _menus = new List<MMenuItem>();
            foreach (string tag in menuitems)
                _menus.Add(new MMenuItem() { Tag = tag });
            return ShowFromView(_menus, attachView,title);
        }

        public static Task<object> Show(IEnumerable<string> menuitems,  string title)
        {
            List<MMenuItem> _menus = new List<MMenuItem>();
            foreach (string tag in menuitems)
                _menus.Add(new MMenuItem() { Tag = tag });
            return Show(_menus, title);
        }

        public static Task<object> ShowAtPosition(IEnumerable<MMenuItem> menuitems,Point position, string title) {
           var _datas = new ObservableCollection<MMenuItem>(menuitems);
            return MDilog.ShowAtPosition(new MenuView() { DataContext = _datas ,Title=title}, position);
        }
        public static Task<object> ShowFromView(IEnumerable<MMenuItem> menuitems, FrameworkElement attachView, string title)
        {
           var _datas = new ObservableCollection<MMenuItem>(menuitems);
            return MDilog.ShowFromView(new MenuView() { DataContext = _datas, Title = title }, attachView);
        }

        public static Task<object> Show(IEnumerable<MMenuItem> menuitems,string title)
        {
            var _datas = new ObservableCollection<MMenuItem>(menuitems);
            return MDilog.Show(new MenuView() { DataContext = _datas, Title = title, Width = Window.Current.Bounds.Width-24},false,false,true);
        }

        private void list_ItemClick(object sender, ItemClickEventArgs e)
        {
            ParentDialog.HideWithResult(e.ClickedItem);
        }
    }
    public class MMenuItem: ViewModelBase
    {
        public const string SplitorTag = "---";
        private string _tag;
        public string Tag { set { SetProperty(ref _tag, value); } get{ return _tag; } }
    }

    public class MenuItemSelecter : DataTemplateSelector
    {
        
        public DataTemplate Splitor { set; get; }
        public DataTemplate Normal { set; get; }
        protected override DataTemplate SelectTemplateCore(System.Object item, DependencyObject container)
        {
            if(item is MMenuItem)
            {
              return MMenuItem.SplitorTag.Equals((item as MMenuItem).Tag) ? Splitor : Normal;
            }
            return null;
        }
    }
    
}
