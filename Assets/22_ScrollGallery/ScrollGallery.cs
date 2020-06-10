using System;
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

		#endregion


		public RectTransform[] splits;

		public int splitCount = 5;

		public RectBounds scrollBounds
		{
			get
			{
				return _scrollRange;
			}
		}
		private RectBounds _scrollRange = new RectBounds();

		public int mainIndex = 0;

		

		public enum State { None, Drag, Move, Return }
		private State state = State.None;
		private PointerEventData pressData = null;
		private List<GalleryData> listData = new List<GalleryData>();

		public enum DataChange
		{
			None,
			Changed,
		}
		private DataChange dataChanged = DataChange.None;

		public void OnPointerDown(PointerEventData eventData)
		{
			Debug.Log("OnPointerDown:" + eventData.position);
			if (pressData != null) { return; }
			pressData = eventData;
			state = State.Drag;
			listData.ForEach(temp => temp.RecordPos());

		}

		public void OnPointerUp(PointerEventData eventData)
		{
			Debug.Log("OnPointerUp:" + eventData.position);
			if (pressData != eventData) { return; }
			state = State.None;
			pressData = null;
		}

		public void OnDrag(PointerEventData eventData)
		{
			Debug.Log("OnDrag:" + eventData.position);
			if (pressData != eventData) { return; }
			//Debug.Log(pressData == eventData);
			float scrollDelta;
			if (scrollDirection == ScrollDirection.Vertical)
			{
				scrollDelta = (eventData.pressPosition.y - eventData.position.y) / height * splitCount;
				Debug.Log("scrollDelta:" + scrollDelta + " pressPosition:"+ eventData.pressPosition.y+ " eventData.position.y:"+ eventData.position.y);
				listData.ForEach(temp => temp.Move(scrollDelta));
			}
			else
			{

			}
		}

		private void OnDisable()
		{
			state = State.None;
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
				dataChanged = DataChange.None;
			}
			Show();
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

		private bool inited = false;
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
			if (findOrigin == null) {
				Debug.LogError("无法找到模板预制体");
				return;
			}
			this.objectPool = new ObjectPool(findOrigin.gameObject, this);
		}

		#region Get Pos Scale Size

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

		public ObjectPool objectPool { private set; get; }

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

		#region Public

		public Action<GameObject, GalleryData> onItemOpen { get; private set; }
		public Action<GameObject, GalleryData> onItemClose { get; private set; }
		public Action<GameObject, GalleryData> onItemRefresh { get; private set; }

		public void SetOnItemOpen(Action<GameObject, GalleryData> onItemOpen)
		{
			this.onItemOpen = onItemOpen;
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

		}

		public void Set(object dataSource) { 
			
		}

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