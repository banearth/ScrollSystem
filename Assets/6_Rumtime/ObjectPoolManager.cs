using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{

	/// <summary>
	/// 对象池管理器
	/// </summary>
	public class ObjectPoolManager
	{
		private static Dictionary<Type, object> database = new Dictionary<Type, object>();

        /// <summary>
        /// 检查是否注册过
        /// </summary>
        public static bool IsRegisted<T>() where T : IPoolObject
        {
            var type = typeof(T);
            return database.ContainsKey(type);
        }

        /// <summary>
        /// 注册一种类型的对象池，需要指定创建方法和初始容器的数量
        /// </summary>
        public static void Regist<T>(Func<T> createFunction, int capacity = 10) where T : IPoolObject
		{
			var type = typeof(T);
			if (!database.ContainsKey(type))
			{
				var objectPool = new ObjectPool<T>(createFunction, capacity);
				database.Add(type, objectPool);
			}
			else
			{
				Debug.LogWarning("ObjectPoolManager has already registed:" + type.ToString());
			}
		}

		/// <summary>
		/// 卸载一种类型的对象池，同时会删除在缓存池中的对象
		/// </summary>
		public static void Release<T>() where T : IPoolObject
		{
			var type = typeof(T);
			if (database.ContainsKey(type))
			{
				(database[type] as ObjectPool<T>).Release();
				database.Remove(type);
			}
			else
			{
				Debug.LogWarning("ObjectPoolManager has nothing to unRegist:" + type.ToString());
			}
		}

		/// <summary>
		/// 回收一种类型的实例，将不用的对象缓存起来
		/// </summary>
		public static void Recycle<T>(T t) where T: IPoolObject
		{
			var type = typeof(T);
			if (database.ContainsKey(type))
			{
				//如果存在对应容器
				(database[type] as ObjectPool<T>).Recycle(t);
			}
			else
			{
				Debug.LogError("ObjectPoolManager can not Recycle:"+type.ToString());
				//没有容器直接删除
				t.Release();
			}
		}

        /// <summary>
        /// 获得对应的实例
        /// </summary>
        public static T Get<T>(bool logWarningIfNotRegist = true) where T : IPoolObject
        {
			var type = typeof(T);
			if (database.ContainsKey(type))
			{
				//如果存在对应容器
				return (database[type] as ObjectPool<T>).Get();
			}
			else
			{
                if (logWarningIfNotRegist) {
                    Debug.LogError("ObjectPoolManager Must Regist before Get:" + type.ToString());
                }
				return default(T);
			}
		}

		public static string GetString()
		{
			string returnStr = "ObjectPoolManager Detail:\n";
			returnStr += "Count:" + database.Keys.Count + "\n";
			foreach (var key in database.Keys)
			{
				returnStr += database[key] + "\n";
			}
			return returnStr;
		}

        public static string GetString<T>() where T : IPoolObject
        {
            var type = typeof(T);
            if (database.ContainsKey(type))
            {
                return database[type].ToString() + "\n";
            }
            else
            {
                return type.ToString()+" is null";
            }
        }

    }

}