using System;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Pieces.Supply
{
    public class PieceSupplyController : MonoBehaviour
    {
        private readonly List<IPlaceable> _items = new();

        public IReadOnlyList<IPlaceable> Items => _items;

        public event Action<IPlaceable> OnItemAdded;
        public event Action<IPlaceable> OnItemRemoved;
        public event Action<List<IPlaceable>> OnItemsReplaced;

        public void AddItem(IPlaceable item)
        {
            _items.Add(item);
            OnItemAdded?.Invoke(item);
        }

        public void RemoveItem(IPlaceable item)
        {
            if (_items.Remove(item))
                OnItemRemoved?.Invoke(item);
        }

        public void ReplaceItems(List<IPlaceable> items)
        {
            _items.Clear();
            _items.AddRange(items);
            OnItemsReplaced?.Invoke(_items);
        }

        public void DeleteAllItems()
        {
            _items.Clear();
            OnItemsReplaced?.Invoke(_items);
        }
    }
}
