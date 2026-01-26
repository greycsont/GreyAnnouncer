using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace GreyAnnouncer.Base;

public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
    where TValue : INotifyPropertyChanged
{
    private readonly Dictionary<TKey, TValue> _dict = new();
    
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    public event PropertyChangedEventHandler PropertyChanged;
    

    public TValue this[TKey key]
    {
        get => _dict[key];
        set
        {
            if (_dict.ContainsKey(key))
            {
                var oldValue = _dict[key];
                oldValue.PropertyChanged -= OnItemPropertyChanged;
                _dict[key] = value;
                value.PropertyChanged += OnItemPropertyChanged;

                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace, value, oldValue));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            }
            else
            {
                _dict[key] = value;
                value.PropertyChanged += OnItemPropertyChanged;

                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, value));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            }
        }
    }

    public ICollection<TKey> Keys => _dict.Keys;
    public ICollection<TValue> Values => _dict.Values;
    public int Count => _dict.Count;
    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value) => this[key] = value;

    public bool Remove(TKey key)
    {
        if (_dict.TryGetValue(key, out var value))
        {
            value.PropertyChanged -= OnItemPropertyChanged;
            var removed = _dict.Remove(key);
            if (removed)
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove, value));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            }
            return removed;
        }
        return false;
    }

    public bool ContainsKey(TKey key) => _dict.ContainsKey(key);
    public bool TryGetValue(TKey key, out TValue value) => _dict.TryGetValue(key, out value);
    public void Clear()
    {
        foreach (var item in _dict.Values)
            item.PropertyChanged -= OnItemPropertyChanged;

        _dict.Clear();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
    }

    private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        // 内部属性变化，统一触发字典 PropertyChanged
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item.{e.PropertyName}"));

    #region IDictionary & IEnumerable implementation
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dict.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _dict.GetEnumerator();
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public bool Contains(KeyValuePair<TKey, TValue> item) => _dict.ContainsKey(item.Key) && _dict[item.Key].Equals(item.Value);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
    #endregion
}

