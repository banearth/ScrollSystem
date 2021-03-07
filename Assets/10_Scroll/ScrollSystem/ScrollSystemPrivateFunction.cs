using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BanSupport
{
	public partial class ScrollSystem: IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
	{
		#region-----------------------内部方法-----------------------

		private void Start()
		{
			//只在运行时运行
			if (!Application.isPlaying) { return; }
			Init();
		}

		private void Update()
		{
			//只在运行时运行
			if (!Application.isPlaying) { return; }
			if (dataAddOrRemove != DataAddOrRemove.None)
			{
				switch (dataAddOrRemove)
				{
					case DataAddOrRemove.Added:
						for (int i = this.addDataStartIndex; i < this.listData.Count; i++)
						{
							this.onAlignScrollDataAction(this.listData[i]);
						}
						EndSetData();
						break;
					case DataAddOrRemove.Removed:
						SetAllData();
						break;
				}
				dataAddOrRemove = DataAddOrRemove.None;
				if (dataChange < DataChange.OnlyPosition)
				{
					dataChange = DataChange.OnlyPosition;
				}
			}

			//跳转相关
			if (jumpState.Update())
			{
				if (dataChange < DataChange.OnlyPosition)
				{
					dataChange = DataChange.OnlyPosition;
				}
			}
			if (dataChange != DataChange.None)
			{
				Show();
			}
		}

		private void OnDestroy()
		{
			//只在运行时运行
			if (!Application.isPlaying) { return; }
			this.listData.Clear();
		}

		private void Init()
		{
			if (!inited)
			{
				inited = true;
				//初始化
				InitDelegate();

				InitFormatPrefabRectTransform();
				InitCursor();
				InitContentTrans();
				//注册预制体对象池
				RegistObjectPool();
				//注册滚动监听
				ScrollRect.onValueChanged.AddListener(OnValueChanged);
				if (scrollDirection == ScrollDirection.Vertical)
				{
					onAlignScrollDataAction = AlignScrollDataWhenVertical;
					onGetDistanceToCenter = GetDistanceToCenterWhenVeritical;
					jumpState = new JumpState(this, SetNormalizedPosWhenVertical, GetNormalizedPosWhenVertical);
				}
				else if (scrollDirection == ScrollDirection.Horizontal)
				{
					onAlignScrollDataAction = SetSingleContentDataWhenHorizontal;
					onGetDistanceToCenter = GetDistanceToCenterWhenHorizontal;
					jumpState = new JumpState(this, SetNormalizedPosWhenHorizontal, GetNormalizedPosWhenHorizontal);
				}
			}
		}

		private void OnValueChanged(Vector2 newPos)
		{
			if (dataChange == DataChange.OnlyPosition)
			{
				dataChange = DataChange.OnlyPosition;
			}
		}

		private float GetDistanceToCenterWhenVeritical(Vector2 anchoredPosition)
		{
			return Mathf.Abs(anchoredPosition.y - contentCenterPosition.y);
		}

		private float GetDistanceToCenterWhenHorizontal(Vector2 anchoredPosition)
		{
			return Mathf.Abs(anchoredPosition.x - contentCenterPosition.x);
		}

		private void SearchListSort()
		{
			this.searchList.Sort((temp1, temp2) =>
			{
				return temp1.distance - temp2.distance > 0 ? 1 : -1;
			});
		}

		private void UpdateBounds()
		{
			switch (startCorner)
			{
				case 0:
					//Left Up
					bounds.left = -ContentTrans.Value.anchoredPosition.x;
					bounds.right = Width - ContentTrans.Value.anchoredPosition.x;
					bounds.up = -ContentTrans.Value.anchoredPosition.y;
					bounds.down = -Height - ContentTrans.Value.anchoredPosition.y;
					contentCenterPosition.x = Width / 2 - ContentTrans.Value.anchoredPosition.x;
					contentCenterPosition.y = -Height / 2 - ContentTrans.Value.anchoredPosition.y;
					break;
				case 1:
					//Right Up
					bounds.left = -Width - ContentTrans.Value.anchoredPosition.x;
					bounds.right = -ContentTrans.Value.anchoredPosition.x;
					bounds.up = -ContentTrans.Value.anchoredPosition.y;
					bounds.down = -Height - ContentTrans.Value.anchoredPosition.y;
					contentCenterPosition.x = Width / 2 - ContentTrans.Value.anchoredPosition.x;
					contentCenterPosition.y = -Height / 2 - ContentTrans.Value.anchoredPosition.y;
					break;
				case 2:
					//Left Down
					bounds.left = -ContentTrans.Value.anchoredPosition.x;
					bounds.right = Width - ContentTrans.Value.anchoredPosition.x;
					bounds.up = Height - ContentTrans.Value.anchoredPosition.y;
					bounds.down = -ContentTrans.Value.anchoredPosition.y;
					contentCenterPosition.x = Width / 2 - ContentTrans.Value.anchoredPosition.x;
					contentCenterPosition.y = Height / 2 - ContentTrans.Value.anchoredPosition.y;
					break;
				case 3:
					//Right Down
					bounds.left = -Width - ContentTrans.Value.anchoredPosition.x;
					bounds.right = -ContentTrans.Value.anchoredPosition.x;
					bounds.up = Height - ContentTrans.Value.anchoredPosition.y;
					bounds.down = -ContentTrans.Value.anchoredPosition.y;
					contentCenterPosition.x = Width / 2 - ContentTrans.Value.anchoredPosition.x;
					contentCenterPosition.y = Height / 2 - ContentTrans.Value.anchoredPosition.y;
					break;
			}
		}

		private float GetScrollDataLineStartPos(ScrollData aScrollData)
		{
			if (scrollDirection == ScrollDirection.Vertical)
			{
				switch (startCorner)
				{
					case 0:
					case 1:
						return aScrollData.Up;
					case 2:
					case 3:
						return aScrollData.Down;
				}
			}
			else
			{
				switch (startCorner)
				{
					case 0:
					case 2:
						return aScrollData.Left;
					case 1:
					case 3:
						return aScrollData.Right;
				}
			}
			return 0;
		}

		private void Show()
		{
			UpdateBounds();
			if (listData.Count <= 0)
			{
				return;
			}
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
				SearchListSort();
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
			//上下寻找下一帧视野内的所有ScrollData
			if (found)
			{
				var nextVisibleDatas = ListPool<ScrollData>.Get();
				nextVisibleDatas.Add(listData[foundIndex]);
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
								nextVisibleDatas.Add(curData);
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
							nextVisibleDatas.Add(curData);
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
								nextVisibleDatas.Add(curData);
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
							nextVisibleDatas.Add(curData);
							found = true;
						}
					}
				}

				//下次不再显示的Data隐藏
				foreach (var aData in nextVisibleDatas) { this.listVisibleScrollData.Remove(aData); }
				foreach (var tempData in listVisibleScrollData) { tempData.Hide(); }
				//设置当前显示的Data
				this.listVisibleScrollData.Clear();
				this.listVisibleScrollData.AddRange(nextVisibleDatas);
				ListPool<ScrollData>.Release(nextVisibleDatas);
				foreach (var visibleData in this.listVisibleScrollData)
				{
					visibleData.Show(dataChange);
				}

			}
		}

		private void InitCursor()
		{
			this.cursorPos = new Vector2(border.x, border.y);
			this.maxHeight = 0;
			this.maxWidth = 0;
			this.oldMaxWidth = 0;
		}

		/// <summary>
		/// 初始化内容面板
		/// </summary>
		private void InitContentTrans()
		{
			//同步父物体Layer
			ContentTrans.gameObject.layer = this.gameObject.layer;
			if (scrollDirection == ScrollDirection.Vertical)
			{
				switch (startCorner)
				{
					case 0://Left Up
					case 1://Right Up
						ContentTrans.Value.pivot = new Vector2(0.5f, 1);
						ContentTrans.Value.anchorMin = new Vector2(0, 1);
						ContentTrans.Value.anchorMax = new Vector2(1, 1);
						ContentTrans.Value.offsetMin = new Vector2(0, 0);
						ContentTrans.Value.offsetMax = new Vector2(0, 0);
						break;
					case 2://Left Down
					case 3://Right Down
						ContentTrans.Value.pivot = new Vector2(0.5f, 0);
						ContentTrans.Value.anchorMin = new Vector2(0, 0);
						ContentTrans.Value.anchorMax = new Vector2(1, 0);
						ContentTrans.Value.offsetMin = new Vector2(0, 0);
						ContentTrans.Value.offsetMax = new Vector2(0, 0);
						break;
				}
			}
			else if (scrollDirection == ScrollDirection.Horizontal)
			{
				switch (startCorner)
				{
					case 0://Left Up
					case 2://Left Down
						ContentTrans.Value.pivot = new Vector2(0, 0.5f);
						ContentTrans.Value.anchorMin = new Vector2(0, 0);
						ContentTrans.Value.anchorMax = new Vector2(0, 1);
						ContentTrans.Value.offsetMin = new Vector2(0, 0);
						ContentTrans.Value.offsetMax = new Vector2(0, 0);
						break;
					case 1://Right Up
					case 3://Right Down
						ContentTrans.Value.pivot = new Vector2(1, 0.5f);
						ContentTrans.Value.anchorMin = new Vector2(1, 0);
						ContentTrans.Value.anchorMax = new Vector2(1, 1);
						ContentTrans.Value.offsetMin = new Vector2(0, 0);
						ContentTrans.Value.offsetMax = new Vector2(0, 0);
						break;
				}
			}
		}

		private void UpdateCentered()
		{
			if (isCenter)
			{
				if (oldMaxWidth == maxWidth)
				{
					return;
				}
				oldMaxWidth = maxWidth;
				var centerOffset = onGetCenterOffset();
				foreach (var aScrollData in listData)
				{
					aScrollData.SetCenterOffset(centerOffset);
				}
			}
		}

		/// <summary>
		/// 更新内容面板大小
		/// </summary>
		private void UpdateContentSize()
		{
			if (scrollDirection == ScrollDirection.Vertical)
			{
				this.contentSize = cursorPos.y + maxHeight - (listData.Count > 0 ? spacing.y : 0) + border.y;
				ContentTrans.Value.sizeDelta = new Vector2(ContentTrans.Value.sizeDelta.x, this.contentSize);
			}
			else if (scrollDirection == ScrollDirection.Horizontal)
			{
				this.contentSize = cursorPos.x + maxHeight - (listData.Count > 0 ? spacing.x : 0) + border.x;
				ContentTrans.Value.sizeDelta = new Vector2(this.contentSize, ContentTrans.Value.sizeDelta.y);
			}
		}

		//haha
		/// <summary>
		/// 转化AnchoredPosition
		/// </summary>
		public Vector2 TransAnchoredPosition(Vector2 position)
		{
			var returnPosition = onGetAnchoredPosition(position);
			return returnPosition;
		}

		/// <summary>
		/// 格式化预制体
		/// </summary>
		private void InitFormatPrefabRectTransform()
		{
			switch (startCorner)
			{
				case 0:
					//Left Up
					onFormatPrefab = rectTransform =>
					{
						rectTransform.pivot = new Vector2(0.5f, 0.5f);
						rectTransform.anchorMin = new Vector2(0, 1);
						rectTransform.anchorMax = new Vector2(0, 1);
					};
					break;
				case 1:
					//Right Up
					onFormatPrefab = rectTransform =>
					{
						rectTransform.pivot = new Vector2(0.5f, 0.5f);
						rectTransform.anchorMin = new Vector2(1, 1);
						rectTransform.anchorMax = new Vector2(1, 1);
					};
					break;
				case 2:
					//Left Down
					onFormatPrefab = rectTransform =>
					{
						rectTransform.pivot = new Vector2(0.5f, 0.5f);
						rectTransform.anchorMin = new Vector2(0, 0);
						rectTransform.anchorMax = new Vector2(0, 0);
					};
					break;
				case 3:
					//Right Down
					onFormatPrefab = rectTransform =>
					{
						rectTransform.pivot = new Vector2(0.5f, 0.5f);
						rectTransform.anchorMin = new Vector2(1, 0);
						rectTransform.anchorMax = new Vector2(1, 0);
					};
					break;
			}
			if (Application.isPlaying)
			{
				switch (startCorner)
				{
					case 0:
						//Left Up
						prefabAnchor = new Vector2(0, 1);
						break;
					case 1:
						//Right Up
						prefabAnchor = new Vector2(1, 1);
						break;
					case 2:
						//Left Down
						prefabAnchor = new Vector2(0, 0);
						break;
					case 3:
						//Right Down
						prefabAnchor = new Vector2(1, 0);
						break;
				}
			}
		}

		/// <summary>
		/// applyLocate 表示保持界面固定不动
		/// </summary>
		private void SetAllData()
		{
			InitCursor();
			var dataCount = this.listData.Count;
			for (int i = 0; i < dataCount; i++)
			{
				var curData = this.listData[i];
				onAlignScrollDataAction(curData);
			}
			EndSetData();
		}

		/// <summary>
		/// 当内容确定下来之后需要调用这个
		/// </summary>
		private void EndSetData()
		{
			UpdateCentered();
			UpdateContentSize();

			//haha
			//UpdateEnableScroll();

			//Add重置
			this.addDataStartIndex = 0;
		}

		private float GetNormalizedPosWhenVertical()
		{
			return ScrollRect.verticalNormalizedPosition;
		}

		private float GetNormalizedPosWhenHorizontal()
		{
			return ScrollRect.horizontalNormalizedPosition;
		}

		private void SetNormalizedPosWhenVertical(float normalizedPos)
		{
			ScrollRect.verticalNormalizedPosition = normalizedPos;
		}

		private void SetNormalizedPosWhenHorizontal(float normalizedPos)
		{
			ScrollRect.horizontalNormalizedPosition = normalizedPos;
		}

		private void AlignScrollDataWhenVertical(ScrollData data)
		{
			data.CalculateSize();
			if (data.newLine == ScrollLayout.NewLine.None)
			{
				//发生过偏移，并且这次物体的右边界超过宽度
				if (cursorPos.x > border.x && cursorPos.x + data.width > Width - border.x)
				{
					//那么执行换行操作
					cursorPos.x = border.x;
					cursorPos.y += maxHeight;
					maxHeight = 0;
				}
				//设置位置
				data.SetAnchoredPosition(cursorPos + data.Size / 2);
				//更新光标
				cursorPos.x += data.width;
				//更新最大宽度
				if (maxWidth < cursorPos.x) { maxWidth = cursorPos.x; }
				//增加间隔
				if (data.width > 0)
				{
					cursorPos.x += spacing.x;
				}
				//更新最大高度
				float curMaxHeight = data.height > 0 ? (data.height + spacing.y) : 0;
				if (maxHeight < curMaxHeight)
				{
					maxHeight = curMaxHeight;
				}
			}
			else
			{
				//发生过偏移
				if (cursorPos.x > border.x)
				{
					//换行
					cursorPos.y += maxHeight;
					maxHeight = 0;
				}

				//指定使用新行
				switch (data.newLine)
				{
					case ScrollLayout.NewLine.Center:
						cursorPos.x = Width / 2;
						break;
					case ScrollLayout.NewLine.LeftOrUp:
						cursorPos.x = data.width / 2 + border.x;
						break;
					case ScrollLayout.NewLine.RightOrDown:
						cursorPos.x = Width - data.width / 2 - border.x;
						break;
				}

				//设置位置
				data.SetAnchoredPosition(cursorPos + data.height / 2 * Vector2.up);
				//换新行
				cursorPos.x = border.x;
				cursorPos.y += data.height + spacing.y;
			}
		}

		private void SetSingleContentDataWhenHorizontal(ScrollData data)
		{
			data.CalculateSize();
			//指定使用新行
			if (data.newLine == ScrollLayout.NewLine.None)
			{
				//发生过偏移，并且这次物体的右边界超过宽度
				if (cursorPos.y > border.y && cursorPos.y + data.height > Height - border.y)
				{
					//那么执行换行操作
					cursorPos.y = border.y;
					cursorPos.x += maxHeight;
					maxHeight = 0;
				}
				//设置位置
				data.SetAnchoredPosition(cursorPos + data.Size / 2);
				//更新光标
				cursorPos.y += data.height;
				//更新最大宽度
				if (maxWidth < cursorPos.y) { maxWidth = cursorPos.y; }
				//增加间隔
				if (data.height > 0)
				{
					cursorPos.y += spacing.y;
				}
				//更新最大高度
				var curMaxHeight = data.width > 0 ? (data.width + spacing.x) : 0;
				if (maxHeight < curMaxHeight)
				{
					maxHeight = curMaxHeight;
				}
			}
			else
			{
				//发生过偏移
				if (cursorPos.y > border.y)
				{
					//换行
					cursorPos.x += maxHeight;
					maxHeight = 0;
				}

				//指定使用新行
				switch (data.newLine)
				{
					case ScrollLayout.NewLine.Center:
						cursorPos.y = Width / 2;
						break;
					case ScrollLayout.NewLine.LeftOrUp:
						cursorPos.y = data.height / 2 + border.y;
						break;
					case ScrollLayout.NewLine.RightOrDown:
						cursorPos.y = Width - data.width / 2 - border.y;
						break;
				}
				//设置位置
				data.SetAnchoredPosition(cursorPos + data.width / 2 * Vector2.right);
				//换新行
				cursorPos.y = border.y;
				cursorPos.x += data.width + spacing.x;
			}
		}

		private void DisableScrollDirection()
		{
			ScrollRect.horizontal = false;
			ScrollRect.vertical = false;
			ScrollRect.enabled = false;
		}

		private void EnableScrollDirection()
		{
			ScrollRect.enabled = true;
			if (scrollDirection == ScrollDirection.Vertical)
			{
				ScrollRect.horizontal = false;
				ScrollRect.vertical = true;
			}
			else if (scrollDirection == ScrollDirection.Horizontal)
			{
				ScrollRect.horizontal = true;
				ScrollRect.vertical = false;
			}
		}

		/// <summary>
		/// 注册对象池
		/// </summary>
		/// <returns>返回是否注册成功</returns>
		private bool RegistObjectPool()
		{
			if (ContentTrans.Value.childCount == 0)
			{
				Debug.LogWarning("请把需要创建的对象置于contentTrans节点下！");
				return false;
			}
			List<RectTransform> allChildren = new List<RectTransform>();
			for (int i = 0; i < ContentTrans.Value.childCount; i++)
			{
				allChildren.Add(ContentTrans.Value.GetChild(i) as RectTransform);
			}
			foreach (var originRectTransform in allChildren)
			{
				if (IsPrefabNameIgnored(originRectTransform.name)) { continue; }
				//确保提前格式化
				onFormatPrefab(originRectTransform);
				if (objectPoolDic.ContainsKey(originRectTransform.name))
				{
					Debug.LogWarning("请确保缓存对象没有重名情况！");
					return false;
				}
				else
				{
					objectPoolDic.Add(originRectTransform.name, new PrefabGroup(originRectTransform.gameObject, this, registPoolCount));
				}
			}
			allChildren.Clear();
			allChildren = null;
			return true;
		}

		/// <summary>
		/// 当任何点击出现后，都应该停止跳转
		/// </summary>
		/// <param name="eventData"></param>
		public void OnInitializePotentialDrag(PointerEventData eventData)
		{
			jumpState.Stop();
		}

		#endregion

		#region -----------------------内部事件-----------------------

		private void InitDelegate()
		{
			//中心偏移量
			if (scrollDirection == ScrollDirection.Vertical)
			{
				onGetCenterOffset = () => { return new Vector2((Width - border.x - maxWidth) / 2, 0); };
			}
			else if (scrollDirection == ScrollDirection.Horizontal)
			{
				onGetCenterOffset = () => { return new Vector2(0, (Height - border.y - maxWidth) / 2); };
			}
			//根据起始点，确定坐标转化方式
			switch (startCorner)
			{
				case 0:
					//Left Up
					onGetAnchoredPosition = origin =>
					{
						origin.y = -origin.y;
						return origin;
					};
					break;
				case 1:
					//Right Up
					onGetAnchoredPosition = origin =>
					{
						origin.x = -origin.x;
						origin.y = -origin.y;
						return origin;
					};
					break;
				case 2:
					//Left Down
					onGetAnchoredPosition = origin =>
					{
						return origin;
					};
					break;
				case 3:
					//Right Down
					onGetAnchoredPosition = origin =>
					{
						origin.x = -origin.x;
						return origin;
					};
					break;
			}

		}

		private void CallItemRefresh(string prefabName, GameObject go, object data)
		{
			if (this.onItemRefresh != null)
			{
				this.onItemRefresh(prefabName, go, data);
			}
		}

		public void CallItemClose(string prefabName, GameObject go, object data)
		{
			if (this.onItemClose != null)
			{
				this.onItemClose(prefabName, go, data);
			}
		}

		public void CallItemOpen(string prefabName, GameObject go, object data)
		{
			if (this.onItemOpen != null)
			{
				this.onItemOpen(prefabName, go, data);
			}
		}

		#endregion
	}
}