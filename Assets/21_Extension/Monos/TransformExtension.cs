using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{
	public static class TransformExtension
	{
		public static Transform AddChild(this Transform trans, string name, bool isRectTransform)
		{
			Transform result;
			if (isRectTransform)
			{
				result = new GameObject(name, typeof(RectTransform)).transform;
			}
			else
			{
				result = new GameObject(name).transform;
			}
			result.SetParent(trans);
			result.localScale = Vector3.one;
			result.localPosition = Vector3.zero;
			return result;
		}

	}
}