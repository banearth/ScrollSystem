using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{

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

}