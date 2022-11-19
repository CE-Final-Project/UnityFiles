using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survival.Game.Infrastructure.SubscriptableObjectArchitecture
{
    public class RuntimeCollection<T> : ScriptableObject
    {
        public List<T> Items = new List<T>();

        public event Action<T> ItemsAdded;

        public event Action<T> ItemsRemoved;

        public void Add(T item)
        {
            if (Items.Contains(item)) return;
            
            Items.Add(item);
            ItemsAdded?.Invoke(item);
        }

        public void Remove(T item)
        {
            if (!Items.Contains(item)) return;
            
            Items.Remove(item);
            ItemsRemoved?.Invoke(item);
        }
    }
}