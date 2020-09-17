using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{
	public class GameObjectPool
	{

		private PoolWithObjectParam<GameObject, GameObject> pool;

		public GameObjectPool(GameObject originPrefab, Transform parent)
		{
			originPrefab.SetActive(false);
			this.pool = new PoolWithObjectParam<GameObject, GameObject>(
				prefab =>
				{
					var createGo = GameObject.Instantiate(prefab, parent) as GameObject;
					createGo.name = prefab.name;
					return createGo;
				},
				(prefab, element) =>
				{
					element.SetActive(true);
				},
				(prefab, element) =>
				{
					if (element.name != prefab.name)
					{
						element.name = prefab.name;
					}
					element.SetActive(false);
				},
				originPrefab
			);
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
		public int countAll { get { return pool.countAll; } }

		public int countActive { get { return pool.countActive; } }

		public int countInactive { get { return pool.countInactive; } }

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

	public class PoolWithObjectParam<T,ParamT>
	{
		private readonly List<T> m_Storage = new List<T>();
		private readonly Func<ParamT, T> m_ActionOnCreate;
		private readonly Action<ParamT,T> m_ActionOnGet;
		private readonly Action<ParamT,T> m_ActionOnRelease;
		public int countAll { get; private set; }
		public int countActive { get { return countAll - countInactive; } }
		public int countInactive { get { return m_Storage.Count; } }

		private ParamT param;

		public PoolWithObjectParam(Func<ParamT, T> actionOnCreate, Action<ParamT,T> actionOnGet, Action<ParamT,T> actionOnRelease, ParamT param)
		{
			this.param = param;
			m_ActionOnCreate = actionOnCreate;
			m_ActionOnGet = actionOnGet;
			m_ActionOnRelease = actionOnRelease;
		}

		public T Get()
		{
			T element;
			if (m_Storage.Count == 0)
			{
				element = m_ActionOnCreate(this.param);
				countAll++;
			}
			else
			{
				element = m_Storage[0];
				m_Storage.RemoveAt(0);
			}
			if (m_ActionOnGet != null)
			{
				m_ActionOnGet(this.param, element);
			}
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
			{
				m_ActionOnRelease(this.param, element);
			}
			m_Storage.Add(element);
		}

		public string GetState()
		{
			string result = typeof(T).ToString() + " Pool(WithObjectParam)\n";
			result += "countAll:" + this.countAll + "\n";
			result += "countActive:" + this.countActive + "\n";
			result += "countInactive:" + this.countInactive + "\n";
			return result;
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