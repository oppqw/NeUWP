using NeUWP.Frameworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudMusic.Collection
{
    public class CMCollectionAdaper<T> : ViewModelBase, IList<T>
    {

        public event NotifyCollectionChangedEventHandler OnUICollectionChanged;

        public event Func<bool> IsHasMore;
        public event Func<CancellationToken, Task<bool>> OnLoadMore;
        public event Action<T, T> RepleaceHandler;


        private IncrementalCollection<T> _uiDatas = null;
        public IncrementalCollection<T> UIDatas
        {
            set { SetProperty(ref _uiDatas, value); }
            get { return _uiDatas; }

        }
 
        private ObservableCollection<T> _source = null;

        public CMCollectionAdaper(ObservableCollection<T> source, Func<bool> isHasMore, Func<CancellationToken, Task<bool>> onLoadMore,Action<T,T> replaceHanler)
        {
            IsHasMore += isHasMore;
            OnLoadMore += onLoadMore;
            RepleaceHandler += replaceHanler;
            _source = source;
            GenerateUIDatas();
        }



        object lockObj = new object();
        public bool NotifyDatasChanged()
        {
            lock (lockObj) {
                if (_source == null)
                {
                    ClearRecoder();
                    return false;
                }
                if (UIDatas == null)
                    GenerateUIDatas();
                var _isLoadAll = !HasMore();
                foreach (var action in _actionRecoder)
                {
                    switch (action.Item1)
                    {
                        case ActionType.Add:
                            var _addData = (T)action.Item2;
                            if (_isLoadAll && !UIDatas.Contains(_addData))
                                UIDatas.Add(_addData);
                            break;
                        case ActionType.Delete:
                            var _data = (T)action.Item2;
                            if (UIDatas.Contains(_data))
                                UIDatas.Remove(_data);
                            break;
                        case ActionType.Move:
                        case ActionType.Insert:
                            var _tp = (Tuple<int, T>)action.Item2;
                            var _iData = (T)_tp.Item2;
                            if (UIDatas.Contains(_iData))
                                UIDatas.Remove(_iData);
                            if (_tp.Item1 <= UIDatas.Count)
                                UIDatas.Insert(_tp.Item1, _iData);
                            break;
                        case ActionType.Repleace:
                            if (RepleaceHandler != null)
                            {
                                var _rpData = (Tuple<T, T>)action.Item2;
                                RepleaceHandler(_rpData.Item1, _rpData.Item2);
                            }

                            break;
                    }
                }
                ClearRecoder();
            }
         
            return true;
        }

        private void GenerateUIDatas()
        {
            UIDatas = new IncrementalCollection<T>(HasMore, LoadMore);
            UIDatas.CollectionChanged += UIDatas_CollectionChanged;
        }

        private void UIDatas_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (OnUICollectionChanged != null)
                OnUICollectionChanged(sender,e);
        }

        private bool HasMore()
        {
            if (_source == null || _uiDatas == null)
                return false;
            return _uiDatas.Count < _source.Count() || (IsHasMore != null && IsHasMore());
        }

        private async Task<IEnumerable<T>> LoadMore(CancellationToken c, uint count)
        {
            try
            {
                if (c.IsCancellationRequested)
                    return null;
                int sourceCount = 0;
                if (HasMore())
                {
                    sourceCount = _source.Count();
                    if (_uiDatas.Count >= sourceCount)
                    {
                        await OnLoadMore(c);
                        sourceCount = _source.Count();
                        if (_uiDatas.Count >= sourceCount || c.IsCancellationRequested)
                        {
                            return null;
                        }
                    }
                    else
                        await Task.Delay(50);
                    List<T> _datas = new List<T>();
                    for (int i = _uiDatas.Count; i < sourceCount; i++)
                    {
                        var _item = _source.ElementAt(i);
                        if (!_datas.Contains(_item))
                            _datas.Add(_item);
                        if (_datas.Count >= 20)
                            break;
                    }
                    return _datas;
                }
            }
            catch (Exception e){
                Debug.Write(e.Message);
            }
            return null;
        }


        //List<T> addDatas = new List<T>();
        //List<T> removeDatas = new List<T>();
        //List<Tuple<int, T>> _insertDatas = new List<Tuple<int, T>>();
        //List<Tuple<T, T>> repleaceDatas = new List<Tuple<T, T>>();
        //List<Tuple<int, T>> moveDatas = new List<Tuple<int, T>>();

        enum ActionType { Add,Delete,Insert,Repleace,Move}

        private List<Tuple<ActionType, object>> _actionRecoder = new List<Tuple<ActionType, object>>();
        private void AddRecoder(ActionType type ,object data) {
            _actionRecoder.Add(new Tuple<ActionType, object>(type,data));
        }

        private void ClearRecoder() {
            _actionRecoder.Clear();
        }

        #region interface override

        public int Count
        {
            get
            {
                return _source == null ? 0 : _source.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public T this[int index]
        {
            get
            {
                if (_source == null)
                    return default(T);
                return index < _source.Count && index >= 0 ? _source[index] : default(T);
            }

            set
            {
                if (_source == null)
                    throw new Exception();
                _source[index] = value;
            }
        }

        public int IndexOf(T item)
        {
            if (_source == null)
                return -1;
            return _source.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (_source == null)
                return;
            _source.Insert(index, item);
            AddRecoder(ActionType.Insert, new Tuple<int, T>(index, item));
        }

        public void RemoveAt(int index)
        {
            if (_source == null)
                return;
            AddRecoder(ActionType.Delete, _source[index]);
            _source.RemoveAt(index);

        }

        public void Add(T item)
        {
            if (_source == null)
                return;
            _source.Add(item);
            AddRecoder(ActionType.Add, item);
        }

        public void Clear()
        {
            if (_source == null)
                return;
            _source.Clear();
            if (UIDatas != null)
                UIDatas.Clear();
            ClearRecoder();
        }

        public bool Contains(T item)
        {
            if (_source == null)
                return false;
            return _source.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            if (_source == null)
                return false;
            AddRecoder(ActionType.Delete, item);
            return _source.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_source == null)
                return null;
            return _source.GetEnumerator();
        }

        public  void Move(int oldIndex, int newIndex) {
            if (_source == null)
                return;
            AddRecoder(ActionType.Move, new Tuple<int,T>(newIndex,_source[oldIndex]));
            _source.Move(oldIndex,newIndex);
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion


    }
}
