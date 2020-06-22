using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{
	public class GameObjectPool
	{

		private Pool<GameObject> pool;
		public GameObjectPool(GameObject prefab, Transform parent)
		{
			prefab.SetActive(false);
			this.pool = new Pool<GameObject>(() => GameObject.Instantiate(prefab, parent) as GameObject,
				element => element.gameObject.SetActive(true),
				element => element.gameObject.SetActive(false));
		}

		public GameObjectPool(GameObject prefab, Transform parent, int poolCount) : this(prefab, parent)
		{
			var list = ListPool<GameObject>.Get();
			for (int i = 0; i < poolCount; i++)
			{
				list.Add(pool.Get());
			}
			for (int i = 0; i < poolCount; i++)
			{
				pool.Release(list[i]);
			}
			ListPool<GameObject>.Release(list);
		}
		public GameObject Get()
		{
			return pool.Get();
		}

		public void Release(GameObject element)
		{
			pool.Release(element);
		}

	}

	/// <summary>
	/// 这是一个工厂类
	/// </summary>
	public static class ListPool<T>
	{

		private static readonly Pool<List<T>> s_ListPool = new Pool<List<T>>(() => new List<T>(), null, l => l.Clear());

		public static List<T> Get()
		{
			return s_ListPool.Get();
		}

		public static void Release(List<T> toRelease)
		{
			s_ListPool.Release(toRelease);
		}

	}

	/// <summary>
	/// 这是一个工厂类
	/// </summary>
	public static class ObjectPool<T> where T : new()
	{

		private static readonly Pool<T> s_ObjectPool = new Pool<T>(() => new T(), null, null);

		public static T Get()
		{
			return s_ObjectPool.Get();
		}

		public static void Release(T element)
		{
			s_ObjectPool.Release(element);
		}

		public static string GetState()
		{
			return s_ObjectPool.GetState();
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
				element = m_Storage[0];
				m_Storage.RemoveAt(0);
			}
			if (m_ActionOnGet != null)
				m_ActionOnGet(element);
			return element;
		}

		public void Release(T element)
		{
			if (m_Storage.Count > 0 && m_Storage.Contains(element))
			{
				Debug.LogWarning("Trying to destroy object that is already released to pool.");
				return;
			}
			if (m_ActionOnRelease != null)
				m_ActionOnRelease(element);
			m_Storage.Add(element);
		}

		public string GetState()
		{
			string result = typeof(T).ToString() + " Pool\n";
			result += "countAll:" + this.countAll + "\n";
			result += "countActive:" + this.countActive + "\n";
			result += "countInactive:" + this.countInactive + "\n";
			return result;
		}

	}

}