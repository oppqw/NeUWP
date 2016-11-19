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
using System.Threading.Tasks;
using NeUWP.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NeUWP.Demo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TestListView : PageBase
    {
        public TestListView()
        {
            this.InitializeComponent();
            this.DataContext= new SampleViewModel();
        }

    }

    class SampleViewModel : ViewModels.ListPageViewModelBase<String>, IPullRefresh
    {

        public SampleViewModel() :base(true){

        }

        int _currentPos = 0;

        public async Task Onrefresh()
        {
           bool res = await Pull(true);
            ToastView.ShowToast("刷新成功",ToastIconType.None);
            return;
        }

        protected override bool HasMore()
        {
            return true;
        }

        protected override async Task<IEnumerable<string>> GetDatas(bool isFirst)
        {
            
            await Task.Delay(3000);
            var _result = new List<String>();
            int start = isFirst ? 0 : (_currentPos + 1);
            for (int i = 0; i < 20; i++)
            {
                _result.Add("item  " + (start + i));
            }
            _currentPos = start + 20;
            if (!isFirst) {
                ToastView.ShowToast("加载成功20条", ToastIconType.None);
            }
            return _result;
        }
    }
}
