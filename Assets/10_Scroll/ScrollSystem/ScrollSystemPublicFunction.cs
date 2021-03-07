using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BanSupport
{
	public partial class ScrollSystem
	{

		#region -----------------------外部事件-----------------------

		/// <summary>
		/// 设置打开回调（物体从无到有的时候）
		/// 参数依次为 （预制体名字，实例化物体，数据）
		/// </summary>
		public void SetItemRefresh(Action<string, GameObject, object> onItemRefresh)
		{
			this.onItemRefresh = onItemRefresh;
		}

		/// <summary>
		/// 设置关闭回调（物体从有到无的时候）
		/// 参数依次为 （预制体名字，实例化物体，数据）
		/// </summary>
		public void SetItemClose(Action<string, GameObject, object> onItemClose)
		{
			this.onItemClose = onItemClose;
		}

		/// <summary>
		/// 刷新回调（内容变化的时候）
		/// 参数依次为 （预制体名字，实例化物体，数据）
		/// </summary>
		public void SetItemOpen(Action<string, GameObject, object> onItemOpen)
		{
			this.onItemOpen = onItemOpen;
		}

		public void SetBeginDrag(Action<PointerEventData> onBeginDrag)
		{
			this.onBeginDrag = onBeginDrag;
		}

		public void SetEndDrag(Action<PointerEventData> onEndDrag)
		{
			this.onEndDrag = onEndDrag;
		}

		public void SetDrag(Action<PointerEventData> onDrag)
		{
			this.onDrag = onDrag;
		}

		#endregion

		#region -----------------------外部调用的方法-----------------------

		/// <summary>
		/// 更新元素显示
		/// </summary>
		public void Set(object dataSource)
		{
			if (dic_DataSource_ScrollData.ContainsKey(dataSource))
			{
				var scrollData = dic_DataSource_ScrollData[dataSource];
				if (scrollData.CalculateSize(true))
				{
					dataAddOrRemove = DataAddOrRemove.Removed;
				}
				else
				{
					scrollData.Show(DataChange.BothPositionAndContent);
				}
			}
			else
			{
				Debug.LogWarning("无法找到该dataSource:" + dataSource.ToString());
			}
		}

		/// <summary>
		/// 替换
		/// </summary>
		public void Replace(object fromDataSource, object toDataSource)
		{
			if (dic_DataSource_ScrollData.ContainsKey(fromDataSource))
			{
				var referScrollData = dic_DataSource_ScrollData[fromDataSource];
				dic_DataSource_ScrollData.Remove(fromDataSource);
				referScrollData.dataSource = toDataSource;
				dic_DataSource_ScrollData.Add(toDataSource, referScrollData);
				referScrollData.Show(DataChange.BothPositionAndContent);
			}
			else
			{
				Debug.LogWarning("无法找到该dataSource:" + toDataSource.ToString());
			}
		}

		/// <summary>
		/// 刷新可见元素
		/// </summary>
		public void Refresh()
		{
			if (listData.Count == 0)
			{
				return;
			}
			dataAddOrRemove = DataAddOrRemove.Removed;
		}

		/// <summary>
		/// 删除全部
		/// </summary>
		public void Clear()
		{
			bool removedAny = false;
			while (listData.Count > 0)
			{
				if (Remove(0))
				{
					removedAny = true;
				}
			}
			if (removedAny)
			{
				dataAddOrRemove = DataAddOrRemove.Removed;
				Jump(this.resetNormalizedPosition);
			}
			this.addDataStartIndex = 0;
		}

		/// <summary>
		/// 删最后一个
		/// </summary>
		public bool RemoveLast()
		{
			return Remove(listData.Count - 1);
		}

		/// <summary>
		/// 删第一个
		/// </summary>
		public bool RemoveFirst()
		{
			return Remove(0);
		}

		/// <summary>
		/// 删，根据索引
		/// </summary>
		public bool Remove(int index)
		{
			if (index >= 0 && index < listData.Count)
			{
				var removedScrollData = listData[index];
				//自身删除
				removedScrollData.Hide();
				//从listData中删除
				listData.RemoveAt(index);
				//从dic_DataSource_ScrollData中删除
				if (removedScrollData.dataSource != null)
				{
					dic_DataSource_ScrollData.Remove(removedScrollData.dataSource);
				}
				//从listVisibleScrollData删除
				if (listVisibleScrollData.Contains(removedScrollData))
				{
					listVisibleScrollData.Remove(removedScrollData);
				}
				//标记当前数据更改过
				if (dataAddOrRemove < DataAddOrRemove.Removed)
				{
					dataAddOrRemove = DataAddOrRemove.Removed;
				}
				return true;
			}
			else
			{
				Debug.LogWarning("无法找到index:" + index);
				return false;
			}
		}

		/// <summary>
		/// 删，根据data
		/// </summary>
		public bool Remove(object dataSource)
		{
			if (dic_DataSource_ScrollData.ContainsKey(dataSource))
			{
				var removedScrollData = dic_DataSource_ScrollData[dataSource];
				//自身删除
				removedScrollData.Hide();
				//从listData中删除
				listData.Remove(removedScrollData);
				//从dic_DataSource_ScrollData中删除
				dic_DataSource_ScrollData.Remove(dataSource);
				//从listVisibleScrollData删除
				if (listVisibleScrollData.Contains(removedScrollData))
				{
					listVisibleScrollData.Remove(removedScrollData);
				}
				//标记当前数据更改过
				if (this.dataAddOrRemove < DataAddOrRemove.Removed)
				{
					dataAddOrRemove = DataAddOrRemove.Removed;
				}
				return true;
			}
			else
			{
				Debug.LogWarning("无法找到该dataSource:" + dataSource.ToString());
				return false;
			}
		}

		/// <summary>
		/// 倒序排列
		/// </summary>
		public void Reverse()
		{
			if (listData.Count <= 0)
			{
				return;
			}
			listData.Reverse();
			if (this.dataAddOrRemove < DataAddOrRemove.Removed)
			{
				this.dataAddOrRemove = DataAddOrRemove.Removed;
			}
		}

		/// <summary>
		/// 通过一个索引来跳转倒某个位置
		/// </summary>
		public void Jump(int index, bool animated = false)
		{
			if (index >= 0 && index < listData.Count)
			{
				Jump(listData[index].dataSource, animated);
			}
			else
			{
				Debug.LogWarning("无法找到该Index:" + index);
			}
		}

		/// <summary>
		/// 跳转到某个数据
		/// </summary>
		public void Jump(object dataSource, bool animated = false)
		{
			if (!dic_DataSource_ScrollData.ContainsKey(dataSource))
			{
				Debug.LogWarning("无法找到该dataSource:" + dataSource?.ToString());
				return;
			}
			var scrollData = this.dic_DataSource_ScrollData[dataSource];
			jumpState.Do(scrollData, animated);
		}

		/// <summary>
		/// 跳转
		/// 垂直滚动时 0表示最上方，1表示最下方
		/// 水平滚动时 0表示最左方，1表示最右方
		/// animated 是否保留动画
		/// </summary>
		public void Jump(float normalizedPosition, bool animated = false)
		{
			jumpState.Do(normalizedPosition, animated);
		}

		/// <summary>
		/// 遍历预制体，用于结合lua代码
		/// </summary>
		public void ForeachOriginPrefabs(Action<string, GameObject> foreachFunc)
		{
			Init();
			foreach (var prefabName in objectPoolDic.Keys)
			{
				var curGroup = objectPoolDic[prefabName];
				curGroup.BindScript(foreachFunc);
			}
		}

		/// <summary>
		/// 根据名字获得原始模板预制体
		/// </summary>
		public GameObject GetOriginPrefab(string prefabName)
		{
			if (objectPoolDic.ContainsKey(prefabName))
			{
				return objectPoolDic[prefabName].origin;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// 全部的元素数量
		/// </summary>
		public int GetDataCount()
		{
			return listData.Count;
		}

		/// <summary>
		/// 某种或多种元素的总数量
		/// </summary>
		public int GetDataCount(params string[] prefabNames)
		{
			int count = 0;
			foreach (var curData in listData)
			{
				if (prefabNames.Contains(curData.objectPool.prefabName))
				{
					count++;
				}
			}
			return count;
		}

		/// <summary>
		/// 除了某种或者多种元素的总数量
		/// </summary>
		public int GetDataCountExcept(params string[] prefabNames)
		{
			int count = 0;
			foreach (var curData in listData)
			{
				if (!prefabNames.Contains(curData.objectPool.prefabName))
				{
					count++;
				}
			}
			return count;
		}

		/// <summary>
		/// 增
		/// </summary>
		/// <param name="getSize">具体参数含义 Vector2  GetSize(object data) </param>
		public void Add(string prefabName, object dataSource, Func<object, Vector2> getSize = null)
		{
			Init();
			if (!objectPoolDic.ContainsKey(prefabName))
			{
				Debug.LogWarning("要增加的预制体没有注册 prefabName:" + prefabName);
				return;
			}
			ScrollData scrollData = new ScrollData(this, prefabName, dataSource, getSize);
			//如果之前什么都没有，那么不需要一个个添加进去
			if (listData.Count <= 0)
			{
				dataAddOrRemove = DataAddOrRemove.Removed;
				this.Jump(this.resetNormalizedPosition);
			}
			listData.Add(scrollData);
			if (dataAddOrRemove < DataAddOrRemove.Added)
			{
				addDataStartIndex = listData.Count - 1;
				dataAddOrRemove = DataAddOrRemove.Added;
			}
			if (dataSource != null)
			{
				dic_DataSource_ScrollData.Add(dataSource, scrollData);
			}
		}

		/// <summary>
		/// 插入在谁的前面
		/// 第一个参数插在谁的前面
		/// 第二个是真正插入的新数据
		/// </summary>
		public void Insert(string prefabName, object insertedDataSource, object newDataSource, Func<object, Vector2> onResize = null)
		{
			if (!objectPoolDic.ContainsKey(prefabName))
			{
				Debug.LogWarning("要增加的预制体没有注册 prefabName:" + prefabName);
				return;
			}
			if (!dic_DataSource_ScrollData.ContainsKey(insertedDataSource))
			{
				Debug.LogWarning("无法找到该insertedDataSource:" + insertedDataSource.ToString());
				return;
			}
			var insertedScrollData = dic_DataSource_ScrollData[insertedDataSource];
			int insertIndex = listData.IndexOf(insertedScrollData);
			if (insertIndex < 0)
			{
				Debug.LogWarning("无法找到需要插入的Index:" + insertIndex);
				return;
			}
			ScrollData newScrollData = new ScrollData(this, prefabName, newDataSource, onResize);
			listData.Insert(insertIndex, newScrollData);
			if (this.dataAddOrRemove < DataAddOrRemove.Removed)
			{
				this.dataAddOrRemove = DataAddOrRemove.Removed;
				this.Jump(this.resetNormalizedPosition);
			}
			if (newDataSource != null)
			{
				dic_DataSource_ScrollData.Add(newDataSource, newScrollData);
			}
		}

		/// <summary>
		/// 最后一个元素是否可见
		/// </summary>
		public bool IsVisible(object dataSource)
		{
			if (dic_DataSource_ScrollData.ContainsKey(dataSource))
			{
				var scrollData = dic_DataSource_ScrollData[dataSource];
				return scrollData.isVisible;
			}
			else
			{
				Debug.LogWarning("无法找到该dataSource:" + dataSource.ToString());
				return false;
			}
		}

		/// <summary>
		/// 第一个元素是否可见
		/// </summary>
		public bool IsFirstVisible()
		{
			if (listData.Count > 0)
			{
				return IsVisible(listData[0].dataSource);
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// 最后一个元素可否可见
		/// </summary>
		public bool IsLastVisible()
		{
			if (listData.Count > 0)
			{
				return IsVisible(listData[listData.Count - 1].dataSource);
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// 是否超出边界
		/// </summary>
		public bool IsContentOutOfBounds()
		{
			if (scrollDirection == ScrollDirection.Vertical)
			{
				if (this.ContentTrans.Value.sizeDelta.y > Height)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				if (this.ContentTrans.Value.sizeDelta.x > Width)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		#endregion

		#region -----------------------一些辅助方法-----------------------

		/// <summary>
		/// 不区分大小写
		/// </summary>
		private static bool IsPrefabNameIgnored(string prefabName)
		{
			foreach (var ignoreName in ignorePrefabNames)
			{
				if (ignoreName == prefabName.ToLower())
				{
					return true;
				}
			}
			return false;
		}

		private Action<PointerEventData> onBeginDrag = null;
		private Action<PointerEventData> onEndDrag = null;
		private Action<PointerEventData> onDrag = null;

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (onBeginDrag != null)
			{
				onBeginDrag(eventData);
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (onEndDrag != null)
			{
				onEndDrag(eventData);
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (onDrag != null)
			{
				onDrag(eventData);
			}
		}

		public void SetScrollDirection(bool isHorOrVer)
		{
			this.ScrollRect.horizontal = isHorOrVer;
			this.ScrollRect.vertical = !isHorOrVer;
		}

		public void DisableMovement()
		{
			this.jumpState.Stop();
			this.ScrollRect.StopMovement();
			this.ScrollRect.enabled = false;
		}

		public void EnableMovement()
		{
			this.ScrollRect.enabled = true;
		}

		#endregion

	}

}