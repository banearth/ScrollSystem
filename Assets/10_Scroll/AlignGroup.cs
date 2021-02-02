using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BanSupport.ScrollSystem
{
	public class AlignGroup
	{
		public ScrollSystem scrollSystem;
		/// <summary>
		/// 光标的位置
		/// </summary>
		public Vector2 cursorPos;
		/// <summary>
		/// 最后一行的最大高度，用于在换行的时候使用
		/// </summary>
		public float maxHeight;
		/// <summary>
		/// 所有行里面的最大宽度，用于在居中的时候计算偏移量
		/// </summary>
		public float maxWidth;
		public float oldMaxWidth;
		public RectBounds rectBounds;
		public List<ScrollData> listData;
		public Func<Vector2> getCenterOffset;

		public Dictionary<object, ScrollData> dic_DataSource_ScrollData = new Dictionary<object, ScrollData>();
		private List<ScrollData> listVisibleScrollData = new List<ScrollData>(8);
		private List<ScrollData> listNextVisibleScrollData = new List<ScrollData>(8);

		public float Width
		{
			get
			{
				return rectBounds.Width;
			}
		}

		public float Height
		{
			get
			{
				return rectBounds.Height;
			}
		}

		public AlignGroup(ScrollSystem scrollSystem, int index)
		{
			this.scrollSystem = scrollSystem;
			this.rectBounds = this.scrollSystem.GetGroupRectBounds(index);
			this.listData = new List<ScrollData>();

			if (scrollSystem.ScrollDirection_GET == ScrollSystem.ScrollDirection.Vertical)
			{
				getCenterOffset = () => { return new Vector2((Width - scrollSystem.Border.x - maxWidth) / 2, 0); };
			}
			else if (scrollSystem.ScrollDirection_GET == ScrollSystem.ScrollDirection.Horizontal)
			{
				getCenterOffset = () => { return new Vector2(0, (Height - scrollSystem.Border.y - maxHeight) / 2); };
			}

			InitCursor();
		}

		public void InitCursor()
		{
			this.cursorPos = new Vector2(scrollSystem.Border.x, scrollSystem.Border.y);
			this.maxHeight = 0;
			this.maxWidth = 0;
			this.oldMaxWidth = 0;
		}

		public void Show()
		{
			if (listData.Count <= 0)
			{
				return;
			}
			var searchList = ListPool<SearchGroup>.Get();
			searchList.Add(SearchGroup.Get(0, listData.Count - 1, this));
			bool found = false;
			int foundIndex = -1;
			uint maxSearchTimes = 1000;
			while (searchList.Count > 0 && (--maxSearchTimes > 0))
			{
				var curSearch = searchList[0];
				searchList.RemoveAt(0);
				ObjectPool<SearchGroup>.Release(curSearch);
				if (curSearch.found)
				{
					found = true;
					foundIndex = curSearch.middle;
					break;
				}
				else
				{
					curSearch.Expand(searchList, this);
				}
				SearchListSort(searchList);
			}

			//Debug.LogWarning("seachTimes:" + (1000 - maxSearchTimes));

			if (maxSearchTimes == 0)
			{
				Debug.LogWarning("maxSearchTimes == 0");
			}

			if (searchList.Count > 0)
			{
				foreach (var aSearch in searchList) { ObjectPool<SearchGroup>.Release(aSearch); }
				searchList.Clear();
			}

			//根据位置上下寻找
			if (found)
			{
				//curAlignGroup
				listNextVisibleScrollData.Add(listData[foundIndex]);
				float lastLineStartPos = GetScrollDataLineStartPos(listData[foundIndex]);
				//向上
				for (int i = foundIndex - 1; i >= 0; i--)
				{
					var curData = listData[i];
					var curLineStartPos = GetScrollDataLineStartPos(curData);
					if (lastLineStartPos != curLineStartPos)
					{
						if (found)
						{
							if (curData.IsVisible())
							{
								listNextVisibleScrollData.Add(curData);
							}
							found = curData.isVisible;
							lastLineStartPos = curLineStartPos;
						}
						else
						{
							break;
						}
					}
					else
					{
						if (curData.IsVisible())
						{
							listNextVisibleScrollData.Add(curData);
							found = true;
						}
					}
				}

				//向下
				lastLineStartPos = GetScrollDataLineStartPos(listData[foundIndex]);
				found = true;
				for (int i = foundIndex + 1; i < listData.Count; i++)
				{
					var curData = listData[i];
					var curLineStartPos = GetScrollDataLineStartPos(curData);
					if (lastLineStartPos != curLineStartPos)
					{
						if (found)
						{
							if (curData.IsVisible())
							{
								listNextVisibleScrollData.Add(curData);
							}
							found = curData.isVisible;
							lastLineStartPos = curLineStartPos;
						}
						else
						{
							break;
						}
					}
					else
					{
						if (curData.IsVisible())
						{
							listNextVisibleScrollData.Add(curData);
							found = true;
						}
					}
				}

				//方法一（这个效率更高一些）
				//var watch = Tools.StartWatch();
				foreach (var visibleData in listNextVisibleScrollData) { listVisibleScrollData.Remove(visibleData); }
				foreach (var tempData in listVisibleScrollData) { tempData.Hide(); }
				listVisibleScrollData.Clear();
				listVisibleScrollData.AddRange(listNextVisibleScrollData);
				listNextVisibleScrollData.Clear();
				foreach (var visibleData in listVisibleScrollData)
				{
					//haha
					visibleData.Update(willShowState);
				}
				//Debug.Log(Tools.StopWatch(watch));


				//方法二
				/*
				var watch = Tools.StartWatch();
				for (int i = 0;i<visibleStartIndex;i++) {
					listData[i].Hide();
				}
				for (int i = visibleEndIndex+1;i<listData.Count;i++) {
					listData[i].Hide();
				}
				for (int i = visibleStartIndex; i <= visibleEndIndex; i++)
				{
					listData[i].Update(false, refreshPosition);
				}
				Debug.Log(Tools.StopWatch(watch));
				*/
			}
		}

		private void SearchListSort(List<SearchGroup> list)
		{
			//haha
			list.Sort((temp1, temp2) =>
			{
				return temp1.distance - temp2.distance > 0 ? 1 : -1;
			});
		}


	}

}

//for (int groupIndex = 0; groupIndex<GroupCount; groupIndex++)
//			{
//				var curAlignGroup = alignList[groupIndex];
//var listData = curAlignGroup.listData;
//				if (listData.Count <= 0)
//				{
//					return;
//				}
//				searchList.Add(SearchGroup.Get(0, listData.Count - 1, curAlignGroup));
//				bool found = false;
//int foundIndex = -1;
//uint maxSearchTimes = 1000;

//				while (searchList.Count > 0 && (--maxSearchTimes > 0))
//				{
//					var curSearch = searchList[0];
//searchList.RemoveAt(0);
//					ObjectPool<SearchGroup>.Release(curSearch);
//					if (curSearch.found)
//					{
//						found = true;
//						foundIndex = curSearch.middle;
//						break;
//					}
//					else
//					{
//						curSearch.Expand(searchList, curAlignGroup);
//					}
//					SearchListSort();
//				}

//				//Debug.LogWarning("seachTimes:" + (1000 - maxSearchTimes));

//				if (maxSearchTimes == 0)
//				{
//					Debug.LogWarning("maxSearchTimes == 0");
//				}

//				if (searchList.Count > 0)
//				{
//					foreach (var aSearch in searchList) { ObjectPool<SearchGroup>.Release(aSearch); }
//					searchList.Clear();
//				}

//				//根据位置上下寻找
//				if (found)
//				{
//					//curAlignGroup
//					listNextVisibleScrollData.Add(listData[foundIndex]);
//					float lastLineStartPos = GetScrollDataLineStartPos(listData[foundIndex]);
//					//向上
//					for (int i = foundIndex - 1; i >= 0; i--)
//					{
//						var curData = listData[i];
//var curLineStartPos = GetScrollDataLineStartPos(curData);
//						if (lastLineStartPos != curLineStartPos)
//						{
//							if (found)
//							{
//								if (curData.IsVisible())
//								{
//									listNextVisibleScrollData.Add(curData);
//								}
//								found = curData.isVisible;
//								lastLineStartPos = curLineStartPos;
//							}
//							else
//							{
//								break;
//							}
//						}
//						else
//						{
//							if (curData.IsVisible())
//							{
//								listNextVisibleScrollData.Add(curData);
//								found = true;
//							}
//						}
//					}

//					//向下
//					lastLineStartPos = GetScrollDataLineStartPos(listData[foundIndex]);
//found = true;
//					for (int i = foundIndex + 1; i<listData.Count; i++)
//					{
//						var curData = listData[i];
//var curLineStartPos = GetScrollDataLineStartPos(curData);
//						if (lastLineStartPos != curLineStartPos)
//						{
//							if (found)
//							{
//								if (curData.IsVisible())
//								{
//									listNextVisibleScrollData.Add(curData);
//								}
//								found = curData.isVisible;
//								lastLineStartPos = curLineStartPos;
//							}
//							else
//							{
//								break;
//							}
//						}
//						else
//						{
//							if (curData.IsVisible())
//							{
//								listNextVisibleScrollData.Add(curData);
//								found = true;
//							}
//						}
//					}

//					//方法一（这个效率更高一些）
//					//var watch = Tools.StartWatch();
//					foreach (var visibleData in listNextVisibleScrollData) { listVisibleScrollData.Remove(visibleData); }
//					foreach (var tempData in listVisibleScrollData) { tempData.Hide(); }
//					listVisibleScrollData.Clear();
//					listVisibleScrollData.AddRange(listNextVisibleScrollData);
//					listNextVisibleScrollData.Clear();
//					foreach (var visibleData in listVisibleScrollData)
//					{
//						visibleData.Update(willShowState);
//					}
//					//Debug.Log(Tools.StopWatch(watch));


//					//方法二
//					/*
//					var watch = Tools.StartWatch();
//					for (int i = 0;i<visibleStartIndex;i++) {
//						listData[i].Hide();
//					}
//					for (int i = visibleEndIndex+1;i<listData.Count;i++) {
//						listData[i].Hide();
//					}
//					for (int i = visibleStartIndex; i <= visibleEndIndex; i++)
//					{
//						listData[i].Update(false, refreshPosition);
//					}
//					Debug.Log(Tools.StopWatch(watch));
//					*/
//				}

//			}