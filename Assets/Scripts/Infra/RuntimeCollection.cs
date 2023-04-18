using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace disguys.Infrastructure
{
    /// <summary>
    /// 이 클래스는 ScriptableObject를 상속받은 클래스로, 특정 타입의 리스트를 포함하고 있습니다.
    /// 이 ScriptableObject의 인스턴스는 각 컴포넌트에서 하드 레퍼런스 없이 참조될 수 있습니다.
    /// </summary>
    public class RuntimeCollection<T> : ScriptableObject
    {
        public List<T> Items = new List<T>();
        
        public event Action<T> ItemAdded;

        public event Action<T> ItemRemoved;

        public void Add(T item)
        {
            if (!Items.Contains(item))
            {
                Items.Add(item);
                ItemAdded?.Invoke(item);
            }
        }

        public void Remove(T item)
        {
            if (Items.Contains(item))
            {
                Items.Remove(item);
                ItemRemoved?.Invoke(item);
            }
        }
    }

}
