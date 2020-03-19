using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BanSupport
{

	[ExecuteInEditMode]
	public class ScrollSystem : MonoBehaviour, IInitializePotentialDragHandler
	{

		#region 众多参数

		[Tooltip("边界")]
		[SerializeField]
		private Vector2 border = Vector2.zero;

		/// <summary>
		/// 当为负数的时候，表示无上限
		/// </summary>
		[Tooltip("当为负数的时候，表示无上限。否则的话，当添加新的元素的时候，超过上限会自动删掉最开始的")]
		[SerializeField]
		private int maxCount = -1;

		[Range(0, 1)]
		/// <summary>
		/// 当发生重置的时候，设置的位置
		/// </summary>
		[Tooltip("当发生重置的时候，设置的位置")]
		[SerializeField]
		private float resetNormalizedPosition = 0;

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
		/// 超出边界的时候才允许滚动
		/// </summary>
		[Tooltip("超出边界的时候才允许滚动")]
		[SerializeField]
		private bool enableScrollOnlyWhenOutOfBounds = false;

		/// <summary>
		/// 直接用于滚动
		/// </summary>
		public RectTransform contentTrans;

		/// <summary>
		/// 元素之间的间隔
		/// </summary>
		[Range(0, 200)]
		[SerializeField]
		private float gap = 10;

		/// <summary>
		/// Gizmos相关
		/// </summary>
		[SerializeField]
		private bool drawGizmos = true;

		public bool DrawGizmos { get { return drawGizmos; } }

		/// <summary>
		/// 滚动方向
		/// </summary>
		public enum ScrollDirection { Vertical, Horizontal }
		[SerializeField]
		private ScrollDirection scrollDirection = ScrollDirection.Vertical;

		/// <summary>
		/// 像素单位的宽度
		/// </summary>
		public float Width
		{
			get
			{
				return (this.transform as RectTransform).rect.width - border.x * 2;
			}
		}

		/// <summary>
		/// 像素单位的宽度
		/// </summary>
		public float Height
		{
			get
			{
				return (this.transform as RectTransform).rect.height;
			}
		}

		/// <summary>
		/// 本体
		/// </summary>
		private ScrollRect scrollRect
		{
			get
			{
				if (_scrollRect == null)
				{
					_scrollRect = GetComponent<ScrollRect>();
				}
				return _scrollRect;
			}
		}
		private ScrollRect _scrollRect = null;
		

		/// <summary>
		/// 这个scrollRect的范围，用于检测是否超出了范围
		/// 当位置发生改变的时候，这个值也需要更新
		/// </summary>
		public RectBounds scrollBounds
		{
			get
			{
				return _scrollRange;
			}
		}
		private RectBounds _scrollRange = new RectBounds();

		/// <summary>
		/// 帧数，scrollSystem帧数大于对应data的帧数，才会进行刷新
		/// </summary>
		private uint updateFrame = 0;

		/// <summary>
		/// 运行时用的Data，核心数据
		/// </summary>
		private List<ScrollData> listData = new List<ScrollData>();

		/// <summary>
		/// 用于内部的搜索
		/// </summary>
		private List<SearchGroup> searchList = new List<SearchGroup>();

		/// <summary>
		/// 用于缓存物体对象
		/// </summary>
		private Dictionary<string, ObjectPool> objectPoolDic = new Dictionary<string, ObjectPool>();

		/// <summary>
		/// 光标的位置
		/// </summary>
		private Vector2 cursorPos;

		/// <summary>
		/// 当前最大高度
		/// </summary>
		private float maxHeight;

		private Action<ScrollData> setSingleDataAction;

		private Coroutine jumpToCoroutine = null;

		private bool inited = false;

		public enum DataChange 
		{ 
			None,//表示无变化，不需要操作
			Added,//表示在末尾添加，不需要位置重构，只需要刷新显示
			Removed,//表示需要位置重构
			ResetRemoved //表示需要位置重构，并且重置滑动位置
		}

		/// <summary>
		/// 用于决定是否在show的时候进行数据操作
		/// </summary>
		private DataChange dataChanged = DataChange.None;

		/// <summary>
		/// 返回是否跳转结束
		/// </summary>
		private Func<float,bool> jumpToAction;

		/// <summary>
		/// 获得距离中心点的距离
		/// </summary>
		private Func<Vector2, float> getDistanceToCenter;

		/// <summary>
		/// 用于更直观展示剩余缓存数量
		/// </summary>
		public Dictionary<string, ObjectPool> ObjectPoolDic
		{
			get
			{
				return objectPoolDic;
			}
		}

		private int visibleStartIndex = 0;
		private int visibleEndIndex = 0;
		private Vector2 centerAnchoredPosition;

#if UNITY_EDITOR

		//----------------------------------用于记录编辑器环境下变化的值----------------------------------
		private Vector2 m_OldSizeDelta = Vector2.zero;
		private ScrollDirection m_OldScrollDirection = ScrollDirection.Vertical;
		private Vector2 m_OldBorder = Vector2.zero;
		private float m_OldGap = 10;
		private int m_OldChildCount = 0;
		private Action m_TurnSameAction;
#endif

		#endregion

		private float GetDistanceToCenterWhenVeritical(Vector2 anchoredPosition)
		{
			return Mathf.Abs(anchoredPosition.y - centerAnchoredPosition.y);
		}

		private float GetDistanceToCenterWhenHorizontal(Vector2 anchoredPosition)
		{
			return Mathf.Abs(anchoredPosition.x - centerAnchoredPosition.x);
		}

		#region 内部类

		internal class SearchGroup : IPoolObject
		{
			public int left;
			public int right;
			/// <summary>
			/// 距离scrollsystem的距离
			/// </summary>
			public float distance;
			public int middle;
			public bool found;

			public static SearchGroup Get(int left, int right,ScrollSystem scrollSystem)
			{
				var result = ObjectPoolManager.Get<SearchGroup>();
				result.left = left;
				result.right = right;
				result.middle = (left + right) / 2;
				var curData = scrollSystem.listData[result.middle];
				curData.CheckVisible(scrollSystem.updateFrame);
				result.distance = scrollSystem.getDistanceToCenter(curData.anchoredPosition);
				result.found = curData.isVisible;
				return result;
			}

			public void Expand(List<SearchGroup> list, ScrollSystem scrollSystem)
			{
				int minus = right - left;
				if (minus > 1)
				{
					list.Add(Get(left, middle - 1, scrollSystem));
					list.Add(Get(middle + 1, right, scrollSystem));
				}
				else if (minus > 0)
				{
					list.Add(Get(right, right, scrollSystem));
				}
			}

			public void EnterStorage() { }

			public void ExitStorage() { }

			public void Release() { }

		}

		public class ObjectPool
		{
			public string prefabName { get; private set; }
			public GameObject origin;
			public List<GameObject> list;
			public Transform contentTrans;
			public ScrollLayout.NewLine newLine { private set; get; }

			private Func<string,float> calculateHeightByFitString;
			
			public float prefabWidth { private set; get; }

			public float prefabHeight { private set; get; }

			public ObjectPool(GameObject origin,List<GameObject> list,Transform contentTrans)
			{
				this.prefabName = origin.name;
				this.origin = origin;
				this.list = list;
				this.contentTrans = contentTrans;
				var prefabRectTransform = origin.transform as RectTransform;
				this.prefabWidth = prefabRectTransform.sizeDelta.x;
				this.prefabHeight = prefabRectTransform.sizeDelta.y;
				var layout = origin.GetComponent<ScrollLayout>();
				if (layout != null)
				{
					this.newLine = layout.newLine;
				}
				else
				{
					this.newLine = ScrollLayout.NewLine.None;
				}
				if (layout != null && layout.fitLabel != null)
				{
					var height = (origin.transform as RectTransform).sizeDelta.y;
					var heightOffset = height - layout.fitLabel.preferredHeight;
					this.calculateHeightByFitString = tempStr =>
					{
						layout.fitLabel.text = tempStr;
						return layout.fitLabel.preferredHeight + heightOffset;
					};
				}
				else
				{
					this.calculateHeightByFitString = null;
				}
			}

			/// <summary>
			/// 根据要匹配的字符串，来计算所需的高度
			/// </summary>
			public float CalculateHeightByFitString(string str)
			{
				if (this.calculateHeightByFitString == null)
				{
					return (origin.transform as RectTransform).sizeDelta.y;
				}
				else
				{
					return this.calculateHeightByFitString(str);
				}
			}

			public GameObject Get()
			{
				GameObject getObject;
				if (this.list.Count > 0)
				{
					getObject = this.list[0];
					list.RemoveAt(0);
					getObject.transform.SetParent(this.contentTrans.transform);
				}
				else
				{
					//没有库存需要生成
					getObject = GameObject.Instantiate(this.origin, this.contentTrans.transform);
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
				obj.transform.SetParent(this.contentTrans.parent);
				obj.SetActive(false);
				this.list.Add(obj);
			}

		}

		#endregion

		#region 内部方法

		private void Start()
		{
			if (Application.isPlaying)
			{
				drawGizmos = false;
				Init();
			}
#if UNITY_EDITOR
			else
			{
				SetComponent();
				SetContentChildren();
			}
#endif
		}

		private void Init()
		{
			if (!inited)
			{
				inited = true;
				if (!ObjectPoolManager.IsRegisted<SearchGroup>()) { ObjectPoolManager.Regist<SearchGroup>(() => new SearchGroup()); }
				RegistObjectPool();
				scrollRect.onValueChanged.AddListener(OnValueChanged);
				InitCursor();
				InitContentTrans();
				if (scrollDirection == ScrollDirection.Vertical)
				{
					setSingleDataAction = SetSingleContentDataWhenVertical;
					jumpToAction = JumpToWhenVertical;
					getDistanceToCenter = GetDistanceToCenterWhenVeritical;
				}
				else if (scrollDirection == ScrollDirection.Horizontal)
				{
					setSingleDataAction = SetSingleContentDataWhenHorizontal;
					jumpToAction = JumpToWhenHorizontal;
					getDistanceToCenter = GetDistanceToCenterWhenHorizontal;
				}
			}
		}

		/// <summary>
		/// 当Content发生了滑动 
		/// </summary>
		private void OnValueChanged(Vector2 newPos)
		{
			Show();
		}

		/// <summary>
		/// 用于检测一些关键值得改变
		/// </summary>
		private void Update()
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
			{
				return;
			}
			if (CheckSetComponent())
			{
				SetComponent();
			}
			if (CheckSetContentChildren())
			{
				SetContentChildren();
			}
			if (m_TurnSameAction != null)
			{
				m_TurnSameAction();
				m_TurnSameAction = null;
			}
#endif
		}

		private void LateUpdate()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (dataChanged != DataChange.None)
			{
				switch (dataChanged)
				{
					case DataChange.Added:
						EndSetData();
						break;
					case DataChange.Removed:
						SetAllData(true);
						break;
					case DataChange.ResetRemoved:
						SetAllData(false);
						break;
				}
				Show();
				dataChanged = DataChange.None;
			}
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
			var rectTransfrom = this.contentTrans as RectTransform;
			scrollBounds.left = -rectTransfrom.anchoredPosition.x;
			scrollBounds.right = Width - rectTransfrom.anchoredPosition.x;
			scrollBounds.up = -rectTransfrom.anchoredPosition.y;
			scrollBounds.down = -Height - rectTransfrom.anchoredPosition.y;
			centerAnchoredPosition.x = Width / 2 - rectTransfrom.anchoredPosition.x;
			centerAnchoredPosition.y = - Height / 2 - rectTransfrom.anchoredPosition.y;

			//scrollSystem.forceCenterOffset

		}

		/// <summary>
		/// 根据ListData内容展示所需展示的，这里经过了优化以确保在运行时保持较高效率
		/// </summary>
		private void Show()
		{	
			updateFrame++;
			UpdateBounds();

			if (listData.Count <= 0)
			{
				return;
			}

			searchList.Add(SearchGroup.Get(0, listData.Count - 1,this));
			bool found = false;
			int foundIndex = -1;
			uint maxSearchTimes = 1000;

			while (searchList.Count > 0 && (--maxSearchTimes > 0)) {
				var curSearch = searchList[0];
				searchList.RemoveAt(0);
				ObjectPoolManager.Recycle(curSearch);
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

			Debug.LogWarning("seachTimes:" + (1000 - maxSearchTimes));

			if (maxSearchTimes == 0)
			{
				Debug.LogWarning("maxSearchTimes == 0");
			}

			if (searchList.Count > 0)
			{
				foreach (var aSearch in searchList) { ObjectPoolManager.Recycle(aSearch); }
				searchList.Clear();
			}

			//根据位置上下寻找
			if (found)
			{
				//向上
				visibleStartIndex = foundIndex;
				for (int i = foundIndex - 1; i >= 0; i--)
				{
					var curData = listData[i];
					curData.CheckVisible(updateFrame);
					if (curData.isVisible)
					{
						visibleStartIndex = i;
					}
					else
					{
						break;
					}
				}

				//向下
				visibleEndIndex = foundIndex;
				for (int i = foundIndex + 1; i < listData.Count; i++)
				{
					var curData = listData[i];
					curData.CheckVisible(updateFrame);
					if (curData.isVisible)
					{
						visibleEndIndex = i;
					}
					else
					{
						break;
					}
				}

				for (int i = 0; i < visibleStartIndex; i++)
				{
					listData[i].Hide();
				}

				for (int i = visibleEndIndex + 1; i < listData.Count; i++)
				{
					listData[i].Hide();
				}

				switch (dataChanged) {
					case DataChange.None:
					case DataChange.Added:
						for (int i = visibleStartIndex; i <= visibleEndIndex; i++)
						{
							listData[i].Update(false,false);
						}
						break;
					case DataChange.Removed:
					case DataChange.ResetRemoved:
						for (int i = visibleStartIndex; i <= visibleEndIndex; i++)
						{
							listData[i].Update(false, true);
						}
						break;
				}

			}
		}

		public void ShowGizmosBounds()
		{
			listData.ForEach(temp=> {
				if (temp.isVisible)
				{
					temp.ShowGizmosBounds();
				}
			});
		}

		/// <summary>
		/// 初始化当前光标以及最大高度
		/// </summary>
		private void InitCursor()
		{
			this.cursorPos = Vector2.zero;
			this.maxHeight = 0;
		}

		/// <summary>
		/// 初始化内容面板
		/// </summary>
		private void InitContentTrans()
		{
			if (scrollDirection == ScrollDirection.Vertical)
			{
				//设置conentTrans
				contentTrans.pivot = new Vector2(0.5f, 1);
				contentTrans.anchorMin = new Vector2(0, 1);
				contentTrans.anchorMax = new Vector2(1, 1);
				contentTrans.offsetMin = new Vector2(0, 0);
				contentTrans.offsetMax = new Vector2(0, 0);
			}
			else if (scrollDirection == ScrollDirection.Horizontal)
			{
				//设置conentTrans
				contentTrans.pivot = new Vector2(0, 0.5f);
				contentTrans.anchorMin = new Vector2(0, 0);
				contentTrans.anchorMax = new Vector2(0, 1);
				contentTrans.offsetMin = new Vector2(0, 0);
				contentTrans.offsetMax = new Vector2(0, 0);
			}
		}

		/// <summary>
		/// 更新内容面板大小
		/// </summary>
		private void UpdateContentSize()
		{
			if (scrollDirection == ScrollDirection.Vertical)
			{
				contentTrans.sizeDelta = new Vector2(contentTrans.sizeDelta.x, cursorPos.y + maxHeight - (listData.Count > 0 ? gap : 0));
			}
			else if (scrollDirection == ScrollDirection.Horizontal)
			{
				contentTrans.sizeDelta = new Vector2(cursorPos.x + maxHeight - (listData.Count > 0 ? gap : 0), contentTrans.sizeDelta.y);
			}
		}


		/// <summary>
		/// 根据子物体来排列内容，这个方法只在编辑器环境下使用
		/// </summary>
		public void SetContentChildren()
		{
#if UNITY_EDITOR
			if (this.contentTrans == null)
			{
				return;
			}

			//haha
			// 设置一个实体的位置
			Action<RectTransform,Vector2> setPostionFunc = (rectTransform,position) => {
				position.y = -position.y;
				rectTransform.anchoredPosition = position + new Vector2(border.x,-border.y);
			};

			//初始化
			InitCursor();
			InitContentTrans();

			var childCount = this.contentTrans.childCount;
			if (scrollDirection == ScrollDirection.Vertical)
			{	
				for (int i = 0; i < childCount; i++)
				{
					var rectTransform = this.contentTrans.GetChild(i) as RectTransform;
					rectTransform.pivot = new Vector2(0.5f, 0.5f);
					rectTransform.anchorMin = new Vector2(0, 1);
					rectTransform.anchorMax = new Vector2(0, 1);
					ScrollLayout.NewLine newLine = ScrollLayout.NewLine.None;
					var layout = this.contentTrans.GetChild(i).GetComponent<ScrollLayout>();
					if (layout != null)
					{
						newLine = layout.newLine;
					}
					if (newLine == ScrollLayout.NewLine.None)
					{
						//发生过偏移，并且这次物体的右边界超过宽度
						if (cursorPos.x > 0 && cursorPos.x + rectTransform.sizeDelta.x > Width)
						{
							//那么执行换行操作
							cursorPos.x = 0;
							cursorPos.y += maxHeight;
							maxHeight = 0;
						}
						//设置位置
						setPostionFunc(rectTransform, cursorPos + rectTransform.sizeDelta / 2);
						//更新光标
						cursorPos += Vector2.right * (rectTransform.sizeDelta.x + gap);
						//更新最大高度
						if (maxHeight < rectTransform.sizeDelta.y + gap)
						{
							maxHeight = rectTransform.sizeDelta.y + gap;
						}
					}
					else
					{
						//发生过偏移，换行
						if (cursorPos.x > 0)
						{
							cursorPos.y += maxHeight;
							maxHeight = 0;
						}
						switch (newLine)
						{
							case ScrollLayout.NewLine.Center:
								{
									cursorPos.x = Width / 2;
								}
								break;
							case ScrollLayout.NewLine.CenterFitSize:
								{
									rectTransform.sizeDelta = new Vector2(Width, rectTransform.sizeDelta.y);
									cursorPos.x = Width / 2;
								}
								break;
							case ScrollLayout.NewLine.LeftOrUp:
								{
									cursorPos.x = rectTransform.sizeDelta.x / 2;
								}
								break;
							case ScrollLayout.NewLine.RightOrDown:
								{
									cursorPos.x = Width - rectTransform.sizeDelta.x / 2;
								}
								break;
						}
						//设置位置
						setPostionFunc(rectTransform, cursorPos + rectTransform.sizeDelta.y / 2 * Vector2.up);
						//换新行
						cursorPos.x = 0;
						cursorPos.y += rectTransform.sizeDelta.y + gap;
					}
				}
				//设置content高度
				contentTrans.sizeDelta = new Vector2(contentTrans.sizeDelta.x, cursorPos.y + maxHeight - (childCount > 0 ? gap : 0));
			}
			else if (scrollDirection == ScrollDirection.Horizontal)
			{
				for (int i = 0; i < childCount; i++)
				{
					var rectTransform = this.contentTrans.GetChild(i) as RectTransform;
					rectTransform.pivot = new Vector2(0.5f, 0.5f);
					rectTransform.anchorMin = new Vector2(0, 1);
					rectTransform.anchorMax = new Vector2(0, 1);
					ScrollLayout.NewLine newLine = ScrollLayout.NewLine.None;
					var layout = this.contentTrans.GetChild(i).GetComponent<ScrollLayout>();
					if (layout != null)
					{
						newLine = layout.newLine;
					}

					if (newLine == ScrollLayout.NewLine.None)
					{
						//发生过偏移，并且这次物体的右边界超过宽度
						if (cursorPos.y > 0 && cursorPos.y + rectTransform.sizeDelta.y > Height)
						{
							//那么执行换行操作
							cursorPos.y = 0;
							cursorPos.x += maxHeight;
							maxHeight = 0;
						}
						//设置位置
						setPostionFunc(rectTransform, cursorPos + rectTransform.sizeDelta / 2);
						//更新光标
						cursorPos += Vector2.up * (rectTransform.sizeDelta.y + gap);
						//更新最大高度
						if (maxHeight < rectTransform.sizeDelta.x + gap)
						{
							maxHeight = rectTransform.sizeDelta.x + gap;
						}
					}
					else
					{
						//发生过偏移，换行
						if (cursorPos.y > 0)
						{
							cursorPos.x += maxHeight;
							maxHeight = 0;
						}
						switch (newLine)
						{
							case ScrollLayout.NewLine.Center:
								cursorPos.y = Height / 2;
								break;
							case ScrollLayout.NewLine.CenterFitSize:
								cursorPos.y = Height / 2;
								rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, Height);
								break;
							case ScrollLayout.NewLine.LeftOrUp:
								cursorPos.y = rectTransform.sizeDelta.y / 2;
								break;
							case ScrollLayout.NewLine.RightOrDown:
								cursorPos.y = Height - rectTransform.sizeDelta.y / 2;
								break;
						}
						//设置位置
						setPostionFunc(rectTransform, cursorPos + rectTransform.sizeDelta.x / 2 * Vector2.right);
						//换新行
						cursorPos.y = 0;
						cursorPos.x += rectTransform.sizeDelta.x + gap;
					}
				}
				//设置content高度
				contentTrans.sizeDelta = new Vector2(cursorPos.x + maxHeight - (childCount > 0 ? gap : 0), contentTrans.sizeDelta.y);
			}
#endif
		}

#if UNITY_EDITOR
		/// <summary>
		/// 设置好控件
		/// </summary>
		private void SetComponent()
		{
			//contentTrans
			if (contentTrans == null)
			{
				//说明是第一次
				var existScrollRect = GetComponent<ScrollRect>();
				List<Transform> listItems = new List<Transform>();
				if (existScrollRect != null)
				{
					while (existScrollRect.content.childCount > 0)
					{
						var first = existScrollRect.content.GetChild(0);
						listItems.Add(first);
						first.SetParent(this.transform);
					}

					var findLayoutGroup = existScrollRect.content.GetComponent<HorizontalOrVerticalLayoutGroup>();
					if (findLayoutGroup != null)
					{
						this.gap = findLayoutGroup.spacing;
					}

					//删除viewport
					DestroyImmediate(existScrollRect.viewport.gameObject);
					DestroyImmediate(existScrollRect);
					
				}

				contentTrans = new GameObject("Content Transform", typeof(RectTransform)).transform as RectTransform;
				contentTrans.SetParent(this.transform);
				contentTrans.transform.localPosition = Vector3.zero;
				contentTrans.localScale = Vector3.one;

				if (listItems.Count > 0)
				{
					foreach (var aItem in listItems)
					{
						aItem.SetParent(contentTrans);
					}
					listItems.Clear();
				}

			}

			//Image
			bool isNew;
			var image = Tools.AddComponentIfNotExist<Image>(this.gameObject,out isNew);
			if (isNew)
			{
				image.sprite = null;
				image.color = new Color(1, 1, 1, 0.2f);
			}


			//Mask
			var mask = Tools.AddComponentIfNotExist<Mask>(this.gameObject,out isNew);
			if (isNew)
			{
				mask.showMaskGraphic = true;
			}

			//Scroll
			var scrollRect = Tools.AddComponentIfNotExist<ScrollRect>(this.gameObject,out isNew);
			if (isNew) {
				scrollRect.decelerationRate = 0.04f;
			}
			EnableScrollByScrollDirection();
			scrollRect.viewport = this.transform as RectTransform;
			scrollRect.content = contentTrans;

			//交换horizontalBar和verticalBar
			if (scrollDirection == ScrollDirection.Vertical)
			{
				if (scrollRect.verticalScrollbar == null && scrollRect.horizontalScrollbar != null)
				{
					scrollRect.verticalScrollbar = scrollRect.horizontalScrollbar;
					scrollRect.horizontalScrollbar = null;
				}
			}
			else if (scrollDirection == ScrollDirection.Horizontal)
			{
				if (scrollRect.horizontalScrollbar == null && scrollRect.verticalScrollbar != null)
				{
					scrollRect.horizontalScrollbar = scrollRect.verticalScrollbar;
					scrollRect.verticalScrollbar = null;
				}
			}

		}

#endif

		private void SetAllData(bool applyLocate)
		{
			bool locateScrollDataEnable = applyLocate;
			if (locateScrollDataEnable && (locateScrollData != null) && (this.listData.Contains(locateScrollData)) && locateScrollData.isPositionInited)
			{
				if (scrollDirection == ScrollDirection.Vertical)
				{
					this.oldLocatePosition = locateScrollData.anchoredPosition.y;
				}
				else if (scrollDirection == ScrollDirection.Horizontal)
				{
					this.oldLocatePosition = locateScrollData.anchoredPosition.x;
				}
			}
			else
			{
				locateScrollDataEnable = false;
			}
			InitCursor();
			var dataCount = this.listData.Count;
			for (int i = 0; i < dataCount; i++)
			{
				var curData = this.listData[i];
				setSingleDataAction(curData);
			}
			EndSetData();
			if (locateScrollDataEnable)
			{
				if (scrollDirection == ScrollDirection.Vertical)
				{
					var offset = this.oldLocatePosition - locateScrollData.anchoredPosition.y;
					this.contentTrans.anchoredPosition += Vector2.up * offset;
				}
				else if (scrollDirection == ScrollDirection.Horizontal)
				{
					var offset = this.oldLocatePosition - locateScrollData.anchoredPosition.x;
					this.contentTrans.anchoredPosition += Vector2.right * offset;
				}
			}
			else
			{
				if (!applyLocate)
				{
					Jump(this.resetNormalizedPosition, false);
				}
			}
			locateScrollData = null;
		}

		/// <summary>
		/// 当内容确定下来之后需要调用这个
		/// </summary>
		private void EndSetData()
		{
			UpdateContentSize();
			UpdateEnableScroll();
		}

		private bool JumpToWhenVertical(float target)
		{
			scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, target, Time.deltaTime * this.jumpToSpeed);
			if (Mathf.Abs(scrollRect.verticalNormalizedPosition - target) < 0.001f)
			{
				scrollRect.verticalNormalizedPosition = target;
				return true;
			}
			else
			{
				return false;
			}
		}

		private bool JumpToWhenHorizontal(float target)
		{
			scrollRect.horizontalNormalizedPosition = Mathf.Lerp(scrollRect.horizontalNormalizedPosition, target, Time.deltaTime * this.jumpToSpeed);
			if (Mathf.Abs(scrollRect.horizontalNormalizedPosition - target) < 0.001f)
			{
				scrollRect.horizontalNormalizedPosition = target;
				return true;
			}
			else
			{
				return false;
			}
		}

		private void SetSingleContentDataWhenVertical(ScrollData data)
		{
			if (data.newLine == ScrollLayout.NewLine.None)
			{
				//发生过偏移，并且这次物体的右边界超过宽度
				if (cursorPos.x > 0 && cursorPos.x + data.width > Width)
				{
					//那么执行换行操作
					cursorPos.x = 0;
					cursorPos.y += maxHeight;
					maxHeight = 0;
				}
				//设置位置
				data.SetAnchoredPosition(cursorPos + data.Size / 2);
				//更新光标
				cursorPos += Vector2.right * (data.width + gap);
				//更新最大高度
				if (maxHeight < data.height + gap)
				{
					maxHeight = data.height + gap;
				}
			}
			else
			{
				//发生过偏移
				if (cursorPos.x > 0)
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
					case ScrollLayout.NewLine.CenterFitSize:
						cursorPos.x = Width / 2;
						data.width = Width;
						break;
					case ScrollLayout.NewLine.LeftOrUp:
						cursorPos.x = data.width/2;
						break;
					case ScrollLayout.NewLine.RightOrDown:
						cursorPos.x = Width - data.width / 2;
						break;
				}
				
				//设置位置
				data.SetAnchoredPosition(cursorPos + data.height / 2 * Vector2.up);
				//换新行
				cursorPos.x = 0;
				cursorPos.y += data.height + gap;
			}
		}

		private void SetSingleContentDataWhenHorizontal(ScrollData data)
		{
			//指定使用新行
			if (data.newLine == ScrollLayout.NewLine.None)
			{
				//发生过偏移，并且这次物体的右边界超过宽度
				if (cursorPos.y > 0 && cursorPos.y + data.height > Height)
				{
					//那么执行换行操作
					cursorPos.y = 0;
					cursorPos.x += maxHeight;
					maxHeight = 0;
				}
				//设置位置
				data.SetAnchoredPosition(cursorPos + data.Size / 2);
				//更新光标
				cursorPos += Vector2.up * (data.height + gap);
				//更新最大高度
				if (maxHeight < data.width + gap)
				{
					maxHeight = data.width + gap;
				}
			}
			else
			{
				//发生过偏移
				if (cursorPos.y > 0)
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
					case ScrollLayout.NewLine.CenterFitSize:
						cursorPos.y = Height / 2;
						data.height = Height;
						break;
					case ScrollLayout.NewLine.LeftOrUp:
						cursorPos.y = data.height / 2;
						break;
					case ScrollLayout.NewLine.RightOrDown:
						cursorPos.x = Width - data.width / 2;
						break;
				}

				//居中
				cursorPos.y = Height / 2;
				//设置位置
				data.SetAnchoredPosition(cursorPos + data.width / 2 * Vector2.right);
				//换新行
				cursorPos.y = 0;
				cursorPos.x += data.width + gap;
			}
		}

		private void UpdateEnableScroll()
		{
			//超出边界的时候才允许滚动
			if (enableScrollOnlyWhenOutOfBounds)
			{
				if (scrollDirection == ScrollDirection.Vertical)
				{
					if ((this.contentTrans as RectTransform).sizeDelta.y > Height)
					{
						EnableScrollByScrollDirection();
					}
					else
					{
						DisableScroll();
					}
				}
				else
				{
					if ((this.contentTrans as RectTransform).sizeDelta.x > Width)
					{
						EnableScrollByScrollDirection();
					}
					else
					{
						DisableScroll();
					}
				}
			}
			else
			{
				EnableScrollByScrollDirection();
			}
		}

		private void DisableScroll()
		{
			scrollRect.horizontal = false;
			scrollRect.vertical = false;
			scrollRect.enabled = false;
		}

		private void EnableScrollByScrollDirection()
		{
			scrollRect.enabled = true;
			if (scrollDirection == ScrollDirection.Vertical)
			{
				scrollRect.horizontal = false;
				scrollRect.vertical = true;
			}
			else if (scrollDirection == ScrollDirection.Horizontal)
			{
				scrollRect.horizontal = true;
				scrollRect.vertical = false;
			}
		}

		/// <summary>
		/// 注册对象池
		/// </summary>
		/// <returns>返回是否注册成功</returns>
		private bool RegistObjectPool()
		{
			if (contentTrans.childCount == 0)
			{
				Debug.LogWarning("请把需要创建的对象置于contentTrans节点下！");
				return false;
			}
			while (contentTrans.childCount > 0)
			{
				//把每个需要缓存的对象置于外部
				var origin = contentTrans.GetChild(0).gameObject;
				origin.transform.SetParent(this.transform);
				origin.SetActive(false);
				//生成对应数量的缓存
				List<GameObject> list = new List<GameObject>();
				for (int i = 0; i < registPoolCount; i++)
				{
					var clone = GameObject.Instantiate(origin.gameObject, this.transform);
					clone.name = clone.name.Substring(0, clone.name.Length - 7);
					list.Add(clone);
				}
				if (objectPoolDic.ContainsKey(origin.name))
				{
					Debug.LogWarning("请确保缓存对象没有重名情况！");
					return false;
				}
				else
				{
					objectPoolDic.Add(origin.name, new ObjectPool(origin, list, contentTrans));
				}
			}
			return true;
		}

		/// <summary>
		/// 当什么点击出现后，都应该停止跳转
		/// </summary>
		/// <param name="eventData"></param>
		public void OnInitializePotentialDrag(PointerEventData eventData)
		{
			StopJumpTo();
		}

		private float oldLocatePosition = 0;
		private ScrollData locateScrollData = null;

		private void ApplyMaxCount()
		{
			if (this.maxCount > 0 && listData.Count > this.maxCount)
			{
				if (locateScrollData == null)
				{
					locateScrollData = listData[listData.Count - 2];
				}
				var scrollData = listData[0];
				if (locateScrollData == scrollData)
				{
					locateScrollData = listData[1];
				}
				if (scrollData.dataSource != null)
				{
					dic_DataSource_ScrollData.Remove(scrollData.dataSource);
				}
				scrollData.Hide();
				listData.RemoveAt(0);
				if (dataChanged < DataChange.ResetRemoved)
				{
					dataChanged = DataChange.Removed;
				}
			}
		}

		#endregion

		#region 用于检测变化的值

#if UNITY_EDITOR
		private bool CheckSetComponent()
		{
			bool result = false;
			if (m_OldScrollDirection != this.scrollDirection)
			{
				result = true;
				m_TurnSameAction += () =>
				{
					m_OldScrollDirection = this.scrollDirection;
				};
			}
			return result;
		}
		
		private bool CheckSetContentChildren()
		{
			bool result = false;
			if (m_OldSizeDelta != (this.transform as RectTransform).sizeDelta)
			{
				result = true;
				m_TurnSameAction += () =>
				{
					m_OldSizeDelta = (this.transform as RectTransform).sizeDelta;
				};
			}
			if (m_OldGap != this.gap)
			{
				result = true;
				m_TurnSameAction += () =>
				{
					m_OldGap = gap;
				};
			}

			if (contentTrans != null && m_OldChildCount != this.contentTrans.childCount)
			{
				result = true;
				m_TurnSameAction += () =>
				{
					m_OldChildCount = this.contentTrans.childCount;
				};
			}
			if (m_OldScrollDirection != scrollDirection)
			{
				result = true;
				m_TurnSameAction += () =>
				{
					m_OldScrollDirection = scrollDirection;
				};
			}
			if (m_OldBorder != border)
			{
				result = true;
				m_TurnSameAction += () =>
				{
					m_OldBorder = border;
				};
			}
			return result;
		}

		private void OnDrawGizmos()
		{
			if (drawGizmos)
			{
				Gizmos.color = Color.green;
				//基本点触区域
				Tools.DrawRectBounds(this.transform.position, Width * this.transform.lossyScale.x, Height * this.transform.lossyScale.y, Color.green);
				//滚动区域
				if (contentTrans != null)
				{
					Tools.DrawRectRange(Tools.GetRectBounds(contentTrans.pivot, contentTrans.lossyScale,
						contentTrans.rect.width, contentTrans.rect.height, contentTrans.position, scrollBounds),
						this.transform.position.z, Color.green);
				}
			}
		}

#endif

		private void StopJumpTo()
		{
			if (jumpToCoroutine != null)
			{
				StopCoroutine(jumpToCoroutine);
				jumpToCoroutine = null;
			}
		}

		private void StopMovement()
		{
			scrollRect.StopMovement();
			StopJumpTo();
		}

		private IEnumerator StartJumpTo(float normalizedPosition)
		{
			while (true)
			{
				yield return null;
				if (jumpToAction(normalizedPosition))
				{
					break;
				}
			}
		}

		#endregion

		#region 外部方法

		public Action<string, Transform, object> setItemContent { get; private set; }

		private Dictionary<object, ScrollData> dic_DataSource_ScrollData = new Dictionary<object, ScrollData>();

		//todo 这块的优化没想好
		/*
		private Dictionary<GameObject, ScrollData> dic_GameObject_ScrollData = new Dictionary<GameObject, ScrollData>();

		public void AttachScrollData(GameObject go, ScrollData scrollData)
		{
			if (dic_GameObject_ScrollData.ContainsKey(go))
			{
				dic_GameObject_ScrollData[go] = scrollData;
			}
			else
			{
				dic_GameObject_ScrollData.Add(go, scrollData);
			}
		}
		*/
		public void SetItemContentDelegate(Action<string, Transform, System.Object> setItemContent)
		{
			this.setItemContent = setItemContent;
		}

		public void Set(object dataSource)
		{
			if (dic_DataSource_ScrollData.ContainsKey(dataSource))
			{
				var scrollData = dic_DataSource_ScrollData[dataSource];
				scrollData.Update(true, false);
			}
			else
			{
				Debug.LogWarning("无法找到该dataSource:" + dataSource.ToString());
			}
		}

		public void Refresh()
		{
			if (listData.Count == 0)
			{
				return;
			}
			for (int i = visibleStartIndex; i <= visibleEndIndex; i++)
			{
				if (i < 0 || i >= listData.Count)
				{
					break;
				}
				listData[i].Update(true,false);
			}
		}

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
				dataChanged = DataChange.ResetRemoved;
			}
		}

		public bool RemoveLast()
		{
			return Remove(listData.Count - 1);
		}

		public bool RemoveFirst()
		{
			return Remove(0);
		}

		public bool Remove(int index)
		{
			if (index >= 0 && index < listData.Count)
			{
				var scrollData = listData[index];
				if (scrollData.dataSource != null)
				{
					dic_DataSource_ScrollData.Remove(scrollData.dataSource);
				}
				scrollData.Hide();
				listData.RemoveAt(index);
				if (dataChanged < DataChange.ResetRemoved)
				{
					dataChanged = DataChange.Removed;
				}
				return true;
			}
			else
			{
				Debug.LogWarning("无法找到index:" + index);
				return false;
			}
		}

		public bool Remove(object dataSource)
		{
			if (dic_DataSource_ScrollData.ContainsKey(dataSource))
			{
				var scrollData = dic_DataSource_ScrollData[dataSource];
				dic_DataSource_ScrollData.Remove(dataSource);
				scrollData.Hide();
				listData.Remove(scrollData);
				if (this.dataChanged < DataChange.ResetRemoved)
				{
					dataChanged = DataChange.Removed;
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
			var objectPool = objectPoolDic[prefabName];
			ScrollData newScrollData = new ScrollData(this, prefabName, newDataSource, onResize);
			newScrollData.OnResize();
			listData.Insert(insertIndex, newScrollData);
			ApplyMaxCount();
			if (this.dataChanged < DataChange.ResetRemoved)
			{
				this.dataChanged = DataChange.Removed;
			}
			if (newDataSource != null)
			{
				dic_DataSource_ScrollData.Add(newDataSource, newScrollData);
			}
		}

		/// <summary>
		/// 跳转
		/// 垂直滚动时 0表示最上方，1表示最下方
		/// 水平滚动时 0表示最左方，1表示最右方
		/// animated 是否保留动画
		/// </summary>
		/// <param name="normalizedPosition"></param>
		public void Jump(float normalizedPosition, bool animated = true)
		{
			if (!this.gameObject.activeSelf) { return; }
			//自身应该停止移动
			StopMovement();
			//然后由程序控制移动
			if (scrollDirection == ScrollDirection.Vertical)
			{
				//为了确保最上方是0，最下方是1
				normalizedPosition = 1 - normalizedPosition;
			}
			if (animated)
			{
				jumpToCoroutine = StartCoroutine(StartJumpTo(normalizedPosition));
			}
			else
			{
				if (scrollDirection == ScrollDirection.Vertical)
				{
					scrollRect.verticalNormalizedPosition = normalizedPosition;
				}
				else if (scrollDirection == ScrollDirection.Horizontal)
				{
					scrollRect.horizontalNormalizedPosition = normalizedPosition;
				}
			}
		}

		/// <summary>
		/// 根据字符串获得理想高度
		/// </summary>
		public float GetPreferHeightByString(string prefabName, string str)
		{
			if (!objectPoolDic.ContainsKey(prefabName))
			{
				Debug.LogWarning("要调用的预制体没有注册 prefabName:" + prefabName);
				return 0;
			}
			var objectPool = objectPoolDic[prefabName];
			return objectPool.CalculateHeightByFitString(str);
		}

		/// <summary>
		/// 遍历预制体，用于结合lua代码
		/// </summary>
		public void ForeachOriginPrefabs(Action<string, Transform> foreachFunc)
		{
			Init();
			foreach (var prefabName in objectPoolDic.Keys)
			{
				foreachFunc(prefabName, objectPoolDic[prefabName].origin.transform);
			}
		}

		/// <summary>
		/// 全部的元素数量
		/// </summary>
		public int GetCount()
		{
			return listData.Count;
		}


		/// <summary>
		/// 某种或多种元素的总数量
		/// </summary>
		public int GetCount(params string[] prefabNames)
		{
			int count = 0;
			foreach (var curData in listData)
			{
				if (prefabNames.Contains(curData.objectPool.prefabName)) {
					count++;
				}
			}
			return count;
		}
		
		/// <summary>
		/// 设置最大数量，超过这个数量会自动删除第一个
		/// </summary>
		public void SetMaxCount(int maxCount)
		{
			this.maxCount = maxCount;
		}

		/// <summary>
		/// 除了某种或者多种元素的总数量
		/// </summary>
		public int GetCountExcept(params string[] prefabNames)
		{
			int count = 0;
			foreach (var curData in listData)
			{
				if (prefabNames.NotContains(curData.objectPool.prefabName))
				{
					count++;
				}
			}
			return count;
		}

		#endregion

		#region Add方法

		public void Add<T>(string prefabName, T t) where T : ScrollData
		{
			Init();
			if (!objectPoolDic.ContainsKey(prefabName))
			{
				Debug.LogWarning("要增加的预制体没有注册 prefabName:" + prefabName);
				return;
			}
			t.OnResize();
			//如果之前什么都没有，那么不需要一个个添加进去
			if (listData.Count <= 0) { dataChanged = DataChange.ResetRemoved; }
			listData.Add(t);
			ApplyMaxCount();
			if (dataChanged < DataChange.Removed)
			{
				dataChanged = DataChange.Added;
				this.setSingleDataAction(t);
			}
			if (t.dataSource != null)
			{
				dic_DataSource_ScrollData.Add(t.dataSource, t);
			}
		}

		public void Add(string prefabName, object dataSource, Func<object, Vector2> onResize = null)
		{
			Init();
			if (!objectPoolDic.ContainsKey(prefabName))
			{
				Debug.LogWarning("要增加的预制体没有注册 prefabName:" + prefabName);
				return;
			}
			var objectPool = objectPoolDic[prefabName];
			ScrollData scrollData = new ScrollData(this, prefabName, dataSource, onResize);
			scrollData.OnResize();
			//如果之前什么都没有，那么不需要一个个添加进去
			if (listData.Count <= 0) { dataChanged = DataChange.ResetRemoved; }
			listData.Add(scrollData);
			ApplyMaxCount();
			if (dataChanged < DataChange.Removed)
			{
				dataChanged = DataChange.Added;
				this.setSingleDataAction(scrollData);
			}
			if (dataSource != null)
			{
				dic_DataSource_ScrollData.Add(dataSource, scrollData);
			}
		}

		/*
		public void Insert(string prefabName, object dataSource, Func<object, Vector2> onResize = null)
		{
			Init();

		}
		*/

		#endregion

	}
}