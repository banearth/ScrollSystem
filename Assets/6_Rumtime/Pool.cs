using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{

	public static class GameObjectPool<T> where T : MonoBehaviour
	{

		private static readonly Pool<GameObject> s_GameObjectPool = new Pool<GameObject>(() => GameObject.Instantiate(originPrefab, generateParent), null, null);

		private static GameObject originPrefab;

		private static Transform generateParent;
		public static void Set(GameObject prefab, Transform parent)
		{
			originPrefab = prefab;
			generateParent = parent;
		}

	}

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

		public static int countAll { get { return s_ObjectPool.countAll; } }

		public static int countActive { get { return s_ObjectPool.countActive; } }

		public static int countInactive { get { return s_ObjectPool.countInactive; } }

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

	}

}