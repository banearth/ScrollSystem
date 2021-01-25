using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport.ScrollSystem
{
	public class SearchGroup
	{
		public int left;
		public int right;
		/// <summary>
		/// 距离scrollsystem的距离
		/// </summary>
		public float distance;
		public int middle;
		public bool found;

		public static SearchGroup Get(int left, int right, AlignGroup alignGroup)
		{
			var result = ObjectPool<SearchGroup>.Get();
			result.left = left;
			result.right = right;
			result.middle = (left + right) / 2;

			var curData = alignGroup.listData[result.middle];
			result.distance = alignGroup.scrollSystem.getDistanceToCenter(curData.anchoredPosition);
			result.found = curData.IsVisible();
			return result;
		}

		public void Expand(List<SearchGroup> list, AlignGroup alignGroup)
		{
			int minus = right - left;
			if (minus > 1)
			{
				list.Add(Get(left, middle - 1, alignGroup));
				list.Add(Get(middle + 1, right, alignGroup));
			}
			else if (minus > 0)
			{
				list.Add(Get(right, right, alignGroup));
			}
		}

		public void EnterStorage() { }

		public void ExitStorage() { }

		public void Release() { }

	}

}