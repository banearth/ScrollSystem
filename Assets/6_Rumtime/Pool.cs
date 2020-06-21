using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{

	public static class ListObjectPool<T>
	{
		// Object pool to avoid allocations.
		private static readonly Pool<List<T>> s_ListPool = new Pool<List<T>>(null, l => l.Clear());

		public static List<T> Get()
		{
			return s_ListPool.Get();
		}

		public static void Release(List<T> toRelease)
		{
			s_ListPool.Release(toRelease);
		}
	}

	public class Pool<T>
	{
		private readonly List<T> m_Storage = new List<T>();
		private readonly Func<T> m_ActionOnCreate;
		private readonly Action<T> m_ActionOnGet;
		private readonly Action<T> m_ActionOnRelease;

		public int countAll { get; private set; }
		public int countActive { get { return countAll - countInactive; } }
		public int countInactive { get { return m_Storage.Count; } }

		public Pool(Func<T> actionOnCreate, Action<T> actionOnGet, Action<T> actionOnRelease)
		{
			m_ActionOnCreate = actionOnCreate;
			m_ActionOnGet = actionOnGet;
			m_ActionOnRelease = actionOnRelease;
		}

		public T Get()
		{
			T element;
			if (m_Storage.Count == 0)
			{
				element = m_ActionOnCreate();
				countAll++;
			}
			else
			{
				element = m_Storage.Pop();
			}
			if (m_ActionOnGet != null)
				m_ActionOnGet(element);
			return element;
		}

		public void Release(T element)
		{
			if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
				Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
			if (m_ActionOnRelease != null)
				m_ActionOnRelease(element);
			m_Stack.Push(element);
		}

	}

	/*
	public interface IPoolObject
	{
		//入库
		void EnterStorage();
		//出库
		void ExitStorage();
		//删除释放
		void Release();
	}

	public class ObjectPool<T> where T : IPoolObject
	{

		//仓库
		private List<T> storage = new List<T>();
		//创建方法
		private readonly Func<T> createFunction;
		//仓库总数量
		public int Count
		{
			get
			{
				return storage.Count;
			}
		}
		//类型
		private Type eventType;

		public ObjectPool(Func<T> createFunction, int capacity = 10)
		{
			this.eventType = typeof(T);
			this.createFunction = createFunction;
			for (int i = 0; i < capacity; i++)
			{
				var newT = createFunction();
				newT.EnterStorage();
				storage.Add(newT);
			}
		}

		/// <summary>
		/// 主方法获得一个实例，缓存池里面有多余的话会从缓存池里面拿，如果没有的话，会自动创建
		/// </summary>
		public T Get()
		{
			if (storage.Count > 0)
			{
				var t = storage[0];
				t.ExitStorage();
				storage.RemoveAt(0);
				return t;
			}
			else
			{
				var t = createFunction();
				return t;
			}
		}

		/// <summary>
		/// 回收一个实例
		/// </summary>
		public void Recycle(T t)
		{
			t.EnterStorage();
			storage.Add(t);
		}

		/// <summary>
		/// 释放所有的实例
		/// </summary>
		public void Release()
		{
			while (storage.Count > 0)
			{
				var t = storage[0];
				if (t != null) {
					t.Release();
				}
				storage.RemoveAt(0);
			}
		}

		public override string ToString()
		{
			string colorStr = "#C60600";
			string returnStr = "class name:" + Tools.GetRichText(eventType.FullName, colorStr);
			returnStr += " count is " + Tools.GetRichText(storage.Count.ToString(), colorStr);
			return returnStr;
		}

	}
	*/

}