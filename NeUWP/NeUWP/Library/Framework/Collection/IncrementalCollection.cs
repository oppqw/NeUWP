//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace CloudMusic.Collection
{
    // This class can used as a jumpstart for implementing ISupportIncrementalLoading. 
    // Implementing the ISupportIncrementalLoading interfaces allows you to create a list that loads
    //  more data automatically when the user scrolls to the end of of a GridView or ListView.
    public  class IncrementalCollection<T>: ObservableCollection<T>, ISupportIncrementalLoading
    {
        public IncrementalCollection(IEnumerable<T> _data):base(_data){

        }
        public IncrementalCollection() :base()
        {

        }
        public IncrementalCollection(IEnumerable<T> _data, HasMoreItemsDelegate _isHasMore,LoadMoreItemsAsyncDelegate _loadMore) : base(_data)
        {
            this.IsHasMoreItems += _isHasMore;
            this.OnLoadMoreItems += _loadMore;
        }

        public IncrementalCollection( HasMoreItemsDelegate _isHasMore, LoadMoreItemsAsyncDelegate _loadMore)
        {
            this.IsHasMoreItems += _isHasMore;
            this.OnLoadMoreItems += _loadMore;
        }
        #region IList
        public void AddRange(IEnumerable<T> _data) {
            if (_data == null)
                return;
            foreach (T item in _data)
                this.Add(item);
        }
       
        #endregion 
    
        #region ISupportIncrementalLoading

        public bool HasMoreItems
        {
            get {

                if (IsHasMoreItems == null)
                    return false;
                return IsHasMoreItems();
            }
        }

        public Windows.Foundation.IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if (_busy)
            {
                throw new InvalidOperationException("Only one operation in flight at a time");
            }

            _busy = true;

            return AsyncInfo.Run((c) => LoadMoreItemsAsync(c, count));
        }

        #endregion 


        #region Private methods

        private async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken c, uint count)
        {
            try
            {
                if (OnLoadMoreItems == null)
                    return new LoadMoreItemsResult { Count = 0 };
                var items = await OnLoadMoreItems(c, count);
                if (items == null)
                    return new LoadMoreItemsResult { Count = 0 };
                this.AddRange(items);
                return new LoadMoreItemsResult { Count =(uint)items.Count() };
            }
            finally
            {
                _busy = false;
            }
        }

        #endregion

        #region Overridable methods

        public delegate Task<IEnumerable<T>> LoadMoreItemsAsyncDelegate(CancellationToken c, uint count);
        public delegate bool HasMoreItemsDelegate();
        public event LoadMoreItemsAsyncDelegate OnLoadMoreItems;
        public event HasMoreItemsDelegate IsHasMoreItems;
        #endregion

        #region State

        bool _busy = false;

        #endregion 
    }
}
