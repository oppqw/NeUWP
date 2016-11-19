using NeUWP.Frameworks;
using NeUWP.Helpers;
using NeUWP.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeUWP.ViewModels
{

    public interface ISupportLoadMore
    {
        Task<bool> LoadMore();
    }
    public abstract class ListPageViewModelBase<T> : ViewModelPageBase, ISupportLoadMore
    {
        private string _title = null;
        public string Title { set { SetProperty(ref _title, value); } get { return _title; } }

        private bool _isAutoLoad = true;
        public ListPageViewModelBase(bool isAutoLoad = true)
        {
            _isAutoLoad = isAutoLoad;
            Init();
        }

        public override void Refresh()
        {
            Init();
        }

        private async void Init()
        {
            if (_isAutoLoad)
            {
                if (Datas == null || Datas.Count == 0)
                    await Pull(true);
            }
        }

     
        #region list datas
        private ObservableCollection<T> _datas = null;
        public ObservableCollection<T> Datas { set { SetProperty(ref _datas, value); } get { return _datas; } }

        private bool _isLoading = false;
        public bool IsLoading { set { SetProperty(ref _isLoading, value); } get { return _isLoading; } }

        private bool _isEmpty = true;
        public bool IsEmpty { set { SetProperty(ref _isEmpty, value); } get { return _isEmpty; } }


        private void UpdateEmptyTag()
        {
            IsEmpty = Datas == null || Datas.Count == 0;
        }

        public override void Cleanup()
        {
            base.Cleanup();
            _datas = null;
            _hasMore = false;
        }

        private long _requestNewSongsTicks = -1;

        public async Task<bool> Pull(bool isFirst)
        {
            try
            {
                if (IsLoading)
                    return false;
                IsLoading = true;
                if (isFirst)
                {
                    UpdateEmptyTag();
                }
                var ticks = _requestNewSongsTicks = DateTime.Now.Ticks;
                IEnumerable<T> _result = null;
                try
                {
                    _result = await GetDatas(isFirst);
                }
                catch { }

                if (_result == null || _result.Count() == 0)
                {
                    _hasMore = false;
                    IsLoading = false;
                    return false;
                }
                _hasMore = HasMore();
                if (ticks != _requestNewSongsTicks)
                    return false;
                PrepareDatasBeforeShow(_result);
                if (ticks != _requestNewSongsTicks)
                    return false;
                if (isFirst)
                    Datas = new ObservableCollection<T>(_result);
                else
                {
                    foreach (T data in _result)
                        Datas.Add(data);

                }
                UpdateEmptyTag();
                IsLoading = false;
                return true;
            }
            catch { }
            return false;
        }

        public Task<bool> LoadMore()
        {
            if (!IsEmpty && _hasMore && !IsLoading)
                return Pull(false);
            return Task.FromResult(false);
        }


        bool _hasMore = false;

        protected virtual bool HasMore()
        {
            return false;
        }

        protected abstract Task<IEnumerable<T>> GetDatas(bool isFirst);

        protected virtual void PrepareDatasBeforeShow(IEnumerable<T> datas)
        {

        }
        #endregion

        #region 多选
        private bool _isMutiSelectMode = false;
        public bool IsMutiSelectMode { private set { SetProperty(ref _isMutiSelectMode, value); } get { return _isMutiSelectMode; } }

        private bool _isAllSelected = false;
        //是否已经全选标记
        public bool IsAllSelected
        {
            set { SetProperty(ref _isAllSelected, value); }
            get { return _isAllSelected; }
        }

        private bool _isHasSelected = false;
        //是否已经全选标记
        public bool IsHasSelected
        {
            set { SetProperty(ref _isHasSelected, value); }
            get { return _isHasSelected; }
        }


        //多选模式开关按钮
        public void ToogleSelectMode()
        {
            var mode = !IsMutiSelectMode;
            if (mode)
            {
                //if (!IsNetworkAvailiable)
                //{
                //    UIHelper.ShowToast("无网络，不可操作");
                //    return;
                //}
            }
            IsAllSelected = false;
            IsHasSelected = false;
            IsMutiSelectMode = mode;
            if (Datas != null)
            {
                foreach (var item in Datas)
                {
                    if (item is ISupportMutiSelect)
                    {
                        var _data = item as ISupportMutiSelect;
                        _data.MultiMode = mode;
                        _data.IsSelected = false;
                    }
                }
            }
        }

        //全选或者全部选
        public void SelectAllAction(bool isSelect)
        {
            if (Datas == null)
                return;
            foreach (var data in Datas)
            {
                if (data is ISupportMutiSelect)
                {
                    (data as ISupportMutiSelect).IsSelected = isSelect;
                }
            }
            IsAllSelected = isSelect;
            IsHasSelected = isSelect;
        }

        //注册在listviewbase 的item点击事件上，点击会变化item的IsSelected选择及IsAllSelected
        public void ToogleItemSelected(T item)
        {
            if (item is ISupportMutiSelect)
            {
                var _data = item as ISupportMutiSelect;
                _data.IsSelected = !_data.IsSelected;
                if (!_data.IsSelected)
                {
                    IsAllSelected = false;
                    if (IsHasSelected)
                    {
                        if (Datas != null && Datas.FirstOrDefault(s => (s is ISupportMutiSelect) && (s as ISupportMutiSelect).IsSelected) == null)
                        {
                            IsHasSelected = false;
                        }
                    }
                }
                else {
                    if (!IsAllSelected)
                    {
                        if (Datas != null && Datas.FirstOrDefault(s => (s is ISupportMutiSelect) && !(s as ISupportMutiSelect).IsSelected) == null)
                        {
                            IsAllSelected = true;
                        }
                    }
                    IsHasSelected = true;
                }

            }
        }

        //获取全选了的item
        public IEnumerable<T> GetAllSelected()
        {
            if (Datas == null)
                return null;
            return Datas.Where(s => (s is ISupportMutiSelect) && (s as ISupportMutiSelect).IsSelected);
        }
        #endregion
    }

    public interface ISupportMutiSelect
    {
        bool IsSelected
        {
            set; get;
        }

        bool MultiMode
        {
            set; get;
        }
    }

}
