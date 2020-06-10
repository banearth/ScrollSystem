﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BanSupport
{
	[ExecuteInEditMode]
	public class ScrollGallery : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{

		public class ObjectPool
		{
			public GameObject origin;
			public List<GameObject> list;
			public ScrollGallery scrollGallery;

			public ObjectPool(GameObject origin, ScrollGallery scrollGallery)
			{
				this.origin = origin;
				this.list = new List<GameObject>();
				this.scrollGallery = scrollGallery;
				origin.SetActive(false);
				for (int i = 0; i < scrollGallery.registPoolCount; i++)
				{
					this.list.Add(GameObject.Instantiate(origin, scrollGallery.rectTransform) as GameObject);
				}
			}

			public GameObject Get()
			{
				GameObject getObject;
				if (this.list.Count > 0)
				{
					getObject = this.list[0];
					list.RemoveAt(0);
				}
				else
				{
					getObject = GameObject.Instantiate(this.origin, scrollGallery.transform);
					getObject.name = getObject.name.Substring(0, getObject.name.Length - 7);
				}
				getObject.SetActive(true);
				return getObject;
			}

			public void Recycle(GameObject obj)
			{
				if (obj == null)
				{
					Debug.LogWarning("回收的对象为空！");
					return;
				}
				obj.SetActive(false);
				this.list.Add(obj);
			}

		}

		#region 编辑器

		private Transform splitParent
		{
			get
			{
				var find = this.transform.Find("SplitParent") as RectTransform;
				if (find == null)
				{
					find = new GameObject("SplitParent", typeof(RectTransform)).transform as RectTransform;
					find.SetParent(this.transform);
					find.SetAsLastSibling();
					find.anchorMin = Vector2.zero;
					find.anchorMax = Vector2.one;
					find.offsetMin = Vector2.zero;
					find.offsetMax = Vector2.zero;
				}
				return find;
			}
		}

		[ContextMenu("Create Split")]
		public void CreateSplits()
		{

			mainIndex = splitCount / 2;

			foreach (var split in splits)
			{
				if (split != null)
				{
					DestroyImmediate(split.gameObject);
				}
			}
			if (splitParent.childCount > 0)
			{
				var temp = splitParent;
				while (temp.childCount > 0)
				{
					DestroyImmediate(temp.GetChild(0).gameObject);
				}
			}

			splits = new RectTransform[splitCount + 2];
			if (scrollDirection == ScrollDirection.Vertical)
			{
				var splitWidth = width;
				var splitHeight = height / 5;
				for (int i = -1; i < splits.Length; i++)
				{
					string name;
					if (i == -1)
					{
						name = "split_Start";
					}
					else if (i == splits.Length - 1)
					{
						name = "split_End";
					}
					else
					{
						name = "split_" + i;
					}
					var split = new GameObject(name, typeof(RectTransform)).transform as RectTransform;
					split.SetParent(this.splitParent);
					split.sizeDelta = new Vector2(splitWidth, splitHeight);
					split.anchoredPosition = new Vector2(0, -i * splitHeight + height / 2 - splitHeight / 2);
					splits[i + 1] = split;
				}
			}
			else
			{
				var splitWidth = width / 5;
				var splitHeight = height;
				for (int i = 0; i < splitCount; i++)
				{
					var split = new GameObject("split_" + i, typeof(RectTransform)).transform as RectTransform;
					split.SetParent(this.transform);
					split.sizeDelta = new Vector2(splitWidth, splitHeight);
					split.anchoredPosition = new Vector2(i * splitWidth - width / 2 + splitWidth / 2, 0);
					splits[i] = split;
				}
			}

		}

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(0, 1, 0, 0.2f);
			foreach (var split in splits)
			{
				if (split != null)
				{
					var size = new Vector3(split.lossyScale.x * split.sizeDelta.x, split.lossyScale.y * split.sizeDelta.y, 0);
					Gizmos.DrawWireCube(split.position, size);
				}
			}
		}

		private void Start()
		{
#if UNITY_EDITOR
			if (Application.isPlaying) { return; }

			//这里有一些创建初始裁切的操作，但我发现没有很大必要

			bool isNew;
			var image = Tools.AddComponentIfNotExist<Image>(this.gameObject, out isNew);
			if (isNew)
			{
				image.sprite = null;
				image.color = new Color(1, 1, 1, 0.2f);
			}
			//var mask = Tools.AddComponentIfNotExist<Mask>(this.gameObject, out isNew);
			//if (isNew)
			//{
			//	mask.showMaskGraphic = true;
			//}
#endif
		}

		#endregion

		#region 属性方法

		public float width
		{
			get
			{
				return rectTransform.sizeDelta.x;
			}
		}

		public float height
		{
			get
			{
				return rectTransform.sizeDelta.y;
			}
		}

		private RectTransform _rectTransform = null;
		public RectTransform rectTransform
		{
			get
			{
				if (_rectTransform == null)
				{
					_rectTransform = this.transform as RectTransform;
				}
				return _rectTransform;
			}
		}

		#endregion

		#region 成员变量

		/// <summary>
		/// 滚动方向
		/// </summary>
		public enum ScrollDirection { Vertical, Horizontal }
		[SerializeField]
		private ScrollDirection scrollDirection = ScrollDirection.Vertical;

		/// <summary>
		/// 跳转的速度
		/// </summary>
		[Tooltip("跳转的速度")]
		[SerializeField]
		private float jumpToSpeed = 10;

		/// <summary>
		/// 注册数量
		/// </summary>
		[Tooltip("注册数量")]
		[SerializeField]
		private int registPoolCount = 3;

		/// <summary>
		/// 分割的数量
		/// </summary>
		[Tooltip("分割的数量")]
		[SerializeField]
		private int splitCount = 5;

		/// <summary>
		/// 主Index，这个非常重要
		/// </summary>
		[Tooltip("主Index，这个非常重要")]
		[SerializeField]
		private int mainIndex = 0;

		/// <summary>
		/// 最多超出主index多少距离
		/// </summary>
		[Tooltip("最多超出主index多少距离")]
		[SerializeField]
		private float maxDistanceBeyondMainIndex = 0.2f;

		/// <summary>
		/// 滑动低于这个时间是return否则是move
		/// </summary>
		[Tooltip("滑动低于这个时间是return否则是move")]
		[SerializeField]
		private float releaseReturnOrMoveTime = 0.5f;

		/// <summary>
		/// 用这些来进行区域划分，这个不需要手动设置
		/// </summary>
		[Tooltip("用这些来进行区域划分，这个不需要手动设置")]
		[SerializeField]
		private RectTransform[] splits;

		public enum ScrollState { None, Drag, Move, Return }
		/// <summary>
		/// 当前的滚动状态
		/// </summary>
		[SerializeField]
		private ScrollState scrollState = ScrollState.None;

		/// <summary>
		/// 按下的事件数据
		/// </summary>
		private PointerEventData pressData = null;

		/// <summary>
		/// 按下的时间
		/// </summary>
		private float pressTime;

		/// <summary>
		/// 主数据
		/// </summary>
		private List<GalleryData> listData = new List<GalleryData>();

		public enum DataChange { None, Changed, }
		/// <summary>
		/// 数据是否发生改变
		/// </summary>
		private DataChange dataChanged = DataChange.None;

		/// <summary>
		/// 是否初始化过
		/// </summary>
		private bool inited = false;

		/// <summary>
		/// 对象池
		/// </summary>
		public ObjectPool objectPool { private set; get; }

		/// <summary>
		/// 创建打开回调
		/// </summary>
		public Action<GameObject, GalleryData> onItemOpen { get; private set; }

		/// <summary>
		/// 关闭回调
		/// </summary>
		public Action<GameObject, GalleryData> onItemClose { get; private set; }

		/// <summary>
		/// 刷新数据回调
		/// </summary>
		public Action<GameObject, GalleryData> onItemRefresh { get; private set; }


		/// <summary>
		/// 返回速度
		/// </summary>
		[Tooltip("主Index，这个非常重要")]
		[SerializeField]
		private float returnSpeed = 10;

		private float minRecordNormalizedPos;

		private float maxRecordNormalizedPos;

		private float moveSpeed;

		/// <summary>
		/// 摩擦力，在松手惯性滑动的时候生效
		/// </summary>
		[Tooltip("摩擦力，在松手惯性滑动的时候生效")]
		[SerializeField]
		private float moveFriction = 10;

		//haha
		//private 

		#endregion

		#region 系统方法
		public void OnPointerDown(PointerEventData eventData)
		{
			if (pressData != null) { return; }
			pressData = eventData;
			scrollState = ScrollState.Drag;
			listData.ForEach(temp => temp.RecordPos());
			minRecordNormalizedPos = GetMinRecordNormalizedPos();
			maxRecordNormalizedPos = GetMaxRecordNormalizedPos();
			pressTime = Time.time;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (pressData != eventData) { return; }
			pressData = null;
			//haha
			if (Time.time - pressTime > releaseReturnOrMoveTime)
			{
				scrollState = ScrollState.Return;
				listData.ForEach(temp => temp.SetReturnPos());
			}
			else
			{
				scrollState = ScrollState.Move;
				moveSpeed = (GetMinNormalizedPos() - minRecordNormalizedPos) / (Time.time - pressTime);
				Debug.Log("moveSpeed:"+ moveSpeed);
			}

		}

		public void OnDrag(PointerEventData eventData)
		{
			if (pressData != eventData) { return; }
			float scrollDelta;
			if (scrollDirection == ScrollDirection.Vertical)
			{
				scrollDelta = (eventData.pressPosition.y - eventData.position.y) / height * splitCount;
			}
			else
			{
				Debug.LogWarning("something need to do");
				scrollDelta = 0;
			}
			if (minRecordNormalizedPos + scrollDelta > mainIndex + maxDistanceBeyondMainIndex)
			{
				scrollDelta = mainIndex + maxDistanceBeyondMainIndex - minRecordNormalizedPos;
			}
			else if (maxRecordNormalizedPos + scrollDelta < mainIndex - maxDistanceBeyondMainIndex)
			{
				scrollDelta = mainIndex - maxDistanceBeyondMainIndex - maxRecordNormalizedPos;
			}
			listData.ForEach(temp => temp.MoveByRecord(scrollDelta));
		}

		private void OnDisable()
		{
			scrollState = ScrollState.None;
			pressData = null;
		}

		private void LateUpdate()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return;
			}
#endif
			if (dataChanged != DataChange.None)
			{
				switch (dataChanged)
				{
					case DataChange.Changed:
						SetAllData();
						break;
				}
				scrollState = ScrollState.None;
				dataChanged = DataChange.None;
			}

			if (scrollState != ScrollState.None) {
				switch (scrollState) {
					case ScrollState.Move:
						{
							var dir = Mathf.Sign(moveSpeed);
							moveSpeed -= dir * moveFriction * Time.deltaTime;
							bool willReturn = false;
							//haha
							if (dir * moveSpeed < 0 || )
							{
								willReturn = true;
							}
							else
							{
								float moveDelta = moveSpeed * Time.deltaTime;
								var minNormalizedPos = GetMinNormalizedPos();
								var maxNormalizedPos = GetMaxNormalizedPos();
								if (minNormalizedPos + moveDelta > mainIndex + maxDistanceBeyondMainIndex)
								{
									moveDelta = mainIndex + maxDistanceBeyondMainIndex - minNormalizedPos;
									willReturn = true;
								}
								else if (maxNormalizedPos + moveDelta < mainIndex - maxDistanceBeyondMainIndex)
								{
									moveDelta = mainIndex - maxDistanceBeyondMainIndex - maxNormalizedPos;
									willReturn = true;
								}
								listData.ForEach(temp => temp.Move(moveDelta));
							}
							
							if (willReturn)
							{
								listData.ForEach(temp=>temp.SetReturnPos());
								scrollState = ScrollState.Return;
							}
						}
						break;
					case ScrollState.Return:
						bool returnEnd = false;
						listData.ForEach(temp =>
						{
							returnEnd |= temp.DoReturn(Time.deltaTime * returnSpeed);
						});
						if (returnEnd)
						{
							Debug.Log("ScrollState.None");
							scrollState = ScrollState.None;
						}
						break;
				}
			}

			Show();
		}

		#endregion

		#region 内部方法
		
		private void Init()
		{
			if (inited) { return; }
			inited = true;
			//注册对象池（找到第一个名字不是SplitParent的子物体）
			Transform findOrigin = null;
			for (int i = 0; i < this.transform.childCount; i++)
			{
				var curChild = this.transform.GetChild(i);
				if (curChild.name != "SplitParent")
				{
					findOrigin = curChild;
					break;
				}
			}
			if (findOrigin == null)
			{
				Debug.LogError("无法找到模板预制体");
				return;
			}
			this.objectPool = new ObjectPool(findOrigin.gameObject, this);
		}

		private void SetAllData()
		{
			var totalCount = listData.Count;
			int curPos = 0;
			for (int i = 0; i < totalCount; i++)
			{
				var aData = listData[i];
				aData.normalizedPos = curPos++;
			}
		}

		private void Show()
		{
			var totalCount = listData.Count;
			//先处理Hide
			for (int i = 0; i < totalCount; i++)
			{
				var aData = listData[i];
				if (aData.normalizedPos <= -1 || aData.normalizedPos >= splitCount)
				{
					aData.isVisible = false;
				}
				aData.Update(true, true);
			}
			//在处理Show
			for (int i = 0; i < totalCount; i++)
			{
				var aData = listData[i];
				if (aData.normalizedPos > -1 && aData.normalizedPos < splitCount)
				{
					aData.isVisible = true;
					aData.worldPosition = GetPos(aData.normalizedPos);
					aData.sizeDelta = GetSize(aData.normalizedPos);
					aData.scale = GetScale(aData.normalizedPos);
				}
				aData.Update(true, true);
			}
		}


		#endregion

		#region Get Pos Scale Size

		public float GetMinNormalizedPos()
		{
			if (listData.Count > 0)
			{
				float minPos = float.MaxValue;
				listData.ForEach(temp => { if (minPos > temp.normalizedPos) { minPos = temp.normalizedPos; } });
				return minPos;
			}
			else
			{
				return 0;
			}
		}

		public float GetMaxNormalizedPos()
		{
			if (listData.Count > 0)
			{
				float maxPos = float.MinValue;
				listData.ForEach(temp => { if (maxPos < temp.normalizedPos) { maxPos = temp.normalizedPos; } });
				return maxPos;
			}
			else
			{
				return 0;
			}
		}

		public float GetMinRecordNormalizedPos()
		{
			if (listData.Count > 0)
			{
				float minPos = float.MaxValue;
				listData.ForEach(temp => { if (minPos > temp.recordNormalizedPos) { minPos = temp.recordNormalizedPos; } });
				return minPos;
			}
			else
			{
				return 0;
			}
		}

		public float GetMaxRecordNormalizedPos()
		{
			if (listData.Count > 0)
			{
				float maxPos = float.MinValue;
				listData.ForEach(temp => { if (maxPos < temp.recordNormalizedPos) { maxPos = temp.recordNormalizedPos; } });
				return maxPos;
			}
			else
			{
				return 0;
			}
		}

		public Vector3 GetPos(int pos)
		{
			return splits[Mathf.Clamp(pos + 1, 0, splits.Length - 1)].position;
		}

		public Vector3 GetPos(float pos)
		{
			var intPos = Mathf.RoundToInt(pos);
			var minus = pos - intPos;
			if (minus > 0)
			{
				return Vector3.Lerp(GetPos(intPos), GetPos(intPos + 1), minus);
			}
			else if (minus < 0)
			{
				return Vector3.Lerp(GetPos(intPos - 1), GetPos(intPos), minus + 1);
			}
			else
			{
				return GetPos(intPos);
			}
		}

		public Vector3 GetScale(int pos)
		{
			return splits[Mathf.Clamp(pos + 1, 0, splits.Length - 1)].localScale;
		}

		public Vector3 GetScale(float pos)
		{
			var intPos = Mathf.RoundToInt(pos);
			var minus = pos - intPos;
			if (minus > 0)
			{
				return Vector3.Lerp(GetScale(intPos), GetScale(intPos + 1), minus);
			}
			else if (minus < 0)
			{
				return Vector3.Lerp(GetScale(intPos - 1), GetScale(intPos), minus + 1);
			}
			else
			{
				return GetScale(intPos);
			}
		}

		public Vector2 GetSize(int pos)
		{
			return splits[Mathf.Clamp(pos + 1, 0, splits.Length - 1)].sizeDelta;
		}

		public Vector2 GetSize(float pos)
		{
			var intPos = Mathf.RoundToInt(pos);
			var minus = pos - intPos;
			if (minus > 0)
			{
				return Vector2.Lerp(GetSize(intPos), GetSize(intPos + 1), minus);
			}
			else if (minus < 0)
			{
				return Vector3.Lerp(GetSize(intPos - 1), GetSize(intPos), minus + 1);
			}
			else
			{
				return GetSize(intPos);
			}
		}

		#endregion

		#region 外部调用

		public void SetOnItemOpen(Action<GameObject, GalleryData> onItemOpen)
		{
			this.onItemOpen = onItemOpen;
		}

		public void SetOnItemClose(Action<GameObject, GalleryData> onItemClose)
		{
			this.onItemClose = onItemClose;
		}

		public void SetOnItemRefresh(Action<GameObject, GalleryData> onItemRefresh)
		{
			this.onItemRefresh = onItemRefresh;
		}

		public void Add(object dataSource)
		{
			Init();
			dataChanged = DataChange.Changed;
			listData.Add(new GalleryData(this, dataSource));
		}

		public void Remove(object dataSource)
		{
			//int
		}

		public void Set(object dataSource) { 
			
		}

		#endregion

	}
}


//private void ApplyFriction()
//{
//	var dir = Mathf.Sign(f_Speed);
//	float newSpeed = f_Speed - dir * (f_Friction + f_SpeedFriction * Mathf.Abs(f_Speed)) * Time.deltaTime;
//	if (newSpeed * f_Speed <= 0)
//	{
//		newSpeed = 0;
//	}
//	f_Speed = newSpeed;
//}

//private void ApplyEdge()
//{
//	if (list_Item.Count > 0)
//	{
//		var firstItem = list_Item[0];
//		float tempProcess = firstItem.GetProcess();
//		if (tempProcess > 0)
//		{
//			foreach (var item in list_Item)
//			{
//				item.MoveOffset(-tempProcess);
//			}
//		}
//		var lastItem = list_Item[list_Item.Count - 1];
//		tempProcess = lastItem.GetProcess();
//		if (tempProcess < 0)
//		{
//			foreach (var item in list_Item)
//			{
//				item.MoveOffset(-tempProcess);
//			}
//		}
//	}
//}

//private void ApplyLoop()
//{
//	int spaceCount = list_Item.Count - totalCount;
//	int rightLimit = spaceCount + extraCount;
//	int leftLimit = -spaceCount - extraCount;
//	foreach (var item in list_Item)
//	{
//		var tempProcess = item.GetProcess();
//		if (tempProcess > rightLimit)
//		{
//			item.MoveOffset(-list_Item.Count);
//		}
//		else if (tempProcess < leftLimit)
//		{
//			item.MoveOffset(list_Item.Count);
//		}
//	}
//}