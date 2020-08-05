using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{

	public static partial class Tools
	{
		public static void Select(this GameObject[] gameObjectArray, GameObject target)
		{
			foreach (var aGo in gameObjectArray)
			{
				aGo?.SetActive(aGo == target);
			}
		}

		public static void Select(this GameObject[] gameObjectArray, int index)
		{
			for (int i = 0; i < gameObjectArray.Length; i++)
			{
				gameObjectArray[i]?.SetActive(i == index);
			}
		}

		public static Vector3 CastXZ(this Vector3 vector3)
		{
			vector3.y = 0;
			return vector3;
		}

		public static Vector3 CastXY(this Vector3 vector3)
		{
			vector3.z = 0;
			return vector3;
		}

		public static Vector3 CastYZ(this Vector3 vector3)
		{
			vector3.x = 0;
			return vector3;
		}

		public static bool Contains<T>(this IEnumerable<T> set, T t)
		{
			foreach (var aT in set)
			{
				if (aT.Equals(t))
				{
					return true;
				}
			}
			return false;
		}

		public static bool Contains<T>(this IEnumerable<T> set, Func<T, bool> conditionFunc)
		{
			foreach (var aT in set)
			{
				if (conditionFunc(aT))
				{
					return true;
				}
			}
			return false;
		}

	}

}


