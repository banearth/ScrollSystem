using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BanSupport
{

	[ExecuteInEditMode]
	public class ScrollSystem : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
	{

		#region 众多参数

		private static string[] ignorePrefabNames = new string[] { "background"};

		[Tooltip("尽量保持剧中")]
		[SerializeField]
		private bool centered = false;

		public bool Centered { get { return centered; } }

		[HideInInspector]
		public int startCorner = 0;

		[Tooltip("边界")]
		[SerializeField]
		private Vector2 border = Vector2.zero;

		public Vector2 Border { get { return border; } }

		/// <summary>
		/// 元素之间的间隔
		/// </summary>
		[SerializeField]
		private Vector2 spacing = Vector2.one * 10;

		public Vector2 Spacing { get { return spacing; } }

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

		public ScrollDirection Direction { get { return scrollDirection; } }

		public RectTransform selfRectTransform
		{
			get
			{
				if (_selfRectTramsform == null)
				{
					_selfRectTramsform = this.transform as RectTransform;
				}
				return _selfRectTramsform;
			}
		}

		private RectTransform _selfRectTramsform = null;

		/// <summary>
		/// 像素单位的宽度
		/// </summary>
		public float Width
		{
			get
			{
				return selfRectTransform.rect.width;
			}
		}

		/// <summary>
		/// 像素单位的宽度
		/// </summary>
		public float Height
		{
			get
			{
				return selfRectTransform.rect.height;
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
		private Dictionary<string, PrefabGroup> objectPoolDic = new Dictionary<string, PrefabGroup>();

		/// <summary>
		/// 光标的位置
		/// </summary>
		private Vector2 cursorPos;

		/// <summary>
		/// 最后一行的最大高度，用于在换行的时候使用
		/// </summary>
		private float maxHeight;

		/// <summary>
		/// 所有行里面的最大宽度，用于在居中的时候计算偏移量
		/// </summary>
		private float maxWidth = 0;

		private float oldMaxWidth = 0;

		/// <summary>
		/// 视图最大尺寸，用于计算跳转使用
		/// </summary>
		private float contentSize = 0;

		private Action<ScrollData> setSingleDataAction;

		private bool inited = false;

		public enum DataChange
		{
			None,//表示无变化，不需要操作
			Added,//表示在末尾添加，不需要位置重构，只需要刷新显示
			Removed,//表示需要位置重构
		}

		/// <summary>
		/// 用于决定是否在show的时候进行数据操作
		/// </summary>
		private DataChange dataChanged = DataChange.None;

		public enum WillShowState
		{
			None,//表示不需要进行操作
			OnlyPosition,//仅仅位置更新
			BothPositionAndContent,//位置和内容都更新
		}

		public WillShowState willShowState = WillShowState.None;

		private bool moveEnable = true;

		/// <summary>
		/// 跳转状态
		/// </summary>
		private JumpState jumpState = null;

		/// <summary>
		/// 获得距离中心点的距离
		/// </summary>
		private Func<Vector2, float> getDistanceToCenter;

		/// <summary>
		/// 用于更直观展示剩余缓存数量
		/// </summary>
		public Dictionary<string, PrefabGroup> ObjectPoolDic
		{
			get
			{
				return objectPoolDic;
			}
		}

		private Vector2 centerAnchoredPosition;
		private Vector2 prefabAnchor;

		public Vector2 PrefabAnchor { get { return prefabAnchor; } }

#if UNITY_EDITOR

		//----------------------------------用于记录编辑器环境下变化的值----------------------------------
		private Vector2 m_OldSizeDelta = Vector2.zero;
		private ScrollDirection m_OldScrollDirection = ScrollDirection.Vertical;
		private Vector2 m_OldBorder = Vector2.zero;
		private Vector2 m_OldSpacing = Vector2.one * 10;
		private int m_OldChildCount = 0;
		private bool m_Centered = false;
		private Action m_TurnSameAction;
#endif

		#endregion

		#region 内部类

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

			public static SearchGroup Get(int left, int right, ScrollSystem scrollSystem)
			{
				var result = ObjectPool<SearchGroup>.Get();
				result.left = left;
				result.right = right;
				result.middle = (left + right) / 2;
				var curData = scrollSystem.listData[result.middle];
				result.distance = scrollSystem.getDistanceToCenter(curData.anchoredPosition);
				result.found = curData.IsVisible();
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

		public class JumpState
		{
			private enum State { None, Directly, Animated }
			private State state = State.None;
			private float targetNormalizedPos;
			private ScrollData targetScrollData;
			private Action<float> setNormalizedPos;
			private Func<float> getNormalizedPos;
			private ScrollSystem scrollSystem;

			public JumpState(ScrollSystem scrollSystem, Action<float> setNormalizedPos, Func<float> getNormalizedPos)
			{
				this.scrollSystem = scrollSystem;
				this.setNormalizedPos = setNormalizedPos;
				this.getNormalizedPos = getNormalizedPos;
			}

			public bool Update()
			{
				if (state != State.None)
				{
					if (scrollSystem.scrollDirection == ScrollDirection.Vertical)
					{
						float offset = scrollSystem.contentSize - scrollSystem.Height;
						if (offset <= 0)
						{
							this.targetNormalizedPos = 0;
						}
						else
						{
							if (this.targetScrollData != null)
							{
								switch (scrollSystem.startCorner)
								{
									case 0: //Left Up
									case 1: //Right Up
										this.targetNormalizedPos = 1 - (this.targetScrollData.originPosition.y - this.targetScrollData.height / 2) / offset;
										break;
									case 2: //Left Down
									case 3: //Right Down
										this.targetNormalizedPos = (this.targetScrollData.originPosition.y - this.targetScrollData.height / 2) / offset;
										break;
								}
							}
						}
					}
					else
					{
						float offset = scrollSystem.contentSize - scrollSystem.Width;
						if (offset <= 0)
						{
							this.targetNormalizedPos = 0;
						}
						else
						{
							if (this.targetScrollData != null)
							{
								switch (scrollSystem.startCorner)
								{
									case 0: //Left Up
									case 2: //Left Down
										this.targetNormalizedPos = (this.targetScrollData.originPosition.x - this.targetScrollData.width / 2) / offset;
										break;
									case 1: //Right Up
									case 3: //Right Down
										this.targetNormalizedPos = 1 - (this.targetScrollData.originPosition.x - this.targetScrollData.width / 2) / offset;
										break;
								}
							}
						}
					}
					this.targetScrollData = null;
					this.targetNormalizedPos = Mathf.Clamp01(this.targetNormalizedPos);

					//根据state来判断如何跳转
					switch (state)
					{
						case State.Directly:
							this.setNormalizedPos(targetNormalizedPos);
							state = State.None;
							break;
						case State.Animated:
							float lerpNormalizedPos = Mathf.Lerp(this.getNormalizedPos(), targetNormalizedPos, Time.deltaTime * scrollSystem.jumpToSpeed);
							var pixelDistance = Mathf.Abs(lerpNormalizedPos - targetNormalizedPos) * scrollSystem.contentSize;
							if (pixelDistance < 1)
							{
								this.setNormalizedPos(this.targetNormalizedPos);
								state = State.None;
							}
							else
							{
								this.setNormalizedPos(lerpNormalizedPos);
							}
							break;
					}
					return true;
				}
				else
				{
					return false;
				}

			}

			public void Stop()
			{
				state = State.None;
			}

			public void Do(float targetNormalizedPos, bool animated)
			{
				if (!scrollSystem.moveEnable) { return; }
				scrollSystem.scrollRect.StopMovement();
				state = animated ? State.Animated : State.Directly;
				this.targetScrollData = null;
				targetNormalizedPos = Mathf.Clamp01(targetNormalizedPos);
				if (scrollSystem.scrollDirection == ScrollDirection.Vertical)
				{
					switch (scrollSystem.startCorner)
					{
						case 0: //Left Up
						case 1: //Right Up
							this.targetNormalizedPos = 1 - targetNormalizedPos;
							break;
						case 2: //Left Down
						case 3: //Right Down
							this.targetNormalizedPos = targetNormalizedPos;
							break;
					}
				}
				else
				{
					switch (scrollSystem.startCorner)
					{
						case 0: //Left Up
						case 2: //Left Down
							this.targetNormalizedPos = targetNormalizedPos;
							break;
						case 1: //Right Up
						case 3: //Right Down
							this.targetNormalizedPos = 1 - targetNormalizedPos;
							break;
					}
				}
			}

			public void Do(ScrollData targetScrollData, bool animated)
			{
				if (!scrollSystem.moveEnable) { return; }
				scrollSystem.scrollRect.StopMovement();
				state = animated ? State.Animated : State.Directly;
				this.targetScrollData = targetScrollData;
				this.targetNormalizedPos = 0;
			}

		}

		public class PrefabGroup
		{
			public string prefabName { get; private set; }
			public GameObject origin;
			public GameObject bindOrigin;
			public GameObjectPool pool;
			public ScrollLayout.NewLine newLine { private set; get; }

			public float prefabWidth { private set; get; }

			public float prefabHeight { private set; get; }

			private ScrollSystem scrollSystem;

			public PrefabGroup(GameObject origin, ScrollSystem scrollSystem,int registPoolCount)
			{
				this.prefabName = origin.name;
				this.origin = origin;
				this.scrollSystem = scrollSystem;
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
				this.pool = new GameObjectPool(origin, scrollSystem.contentTrans, registPoolCount);
			}

			public GameObject Get()
			{
				return this.pool.Get();
			}

			public void Release(GameObject obj)
			{
				this.pool.Release(obj);
			}

			public void BindScript(Action<string, GameObject> bindFunc)
			{
				if (bindOrigin == null)
				{
					bindOrigin = Instantiate(origin, scrollSystem.transform);
					bindOrigin.name = bindOrigin.name.Substring(0, bindOrigin.name.Length - 7) + "_BindScript";
					bindOrigin.SetActive(true);
					bindOrigin.SetActive(false);
				}
				bindFunc(prefabName, bindOrigin);
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

				//初始化
				InitGetCenterOffset();
				InitTransAnchoredPosition();
				InitFormatPrefabRectTransform();
				InitCursor();
				InitContentTrans();

				//注册预制体对象池
				RegistObjectPool();
				//注册滚动监听
				scrollRect.onValueChanged.AddListener(OnValueChanged);

				if (scrollDirection == ScrollDirection.Vertical)
				{
					setSingleDataAction = SetSingleContentDataWhenVertical;
					getDistanceToCenter = GetDistanceToCenterWhenVeritical;
					jumpState = new JumpState(this,SetScrollRectNormalizedPosWhenVertical, GetScrollRectNormalizedPosWhenVertical);
				}
				else if (scrollDirection == ScrollDirection.Horizontal)
				{
					setSingleDataAction = SetSingleContentDataWhenHorizontal;
					getDistanceToCenter = GetDistanceToCenterWhenHorizontal;
					jumpState = new JumpState(this, SetScrollRectNormalizedPosWhenHorizontal, GetScrollRectNormalizedPosWhenHorizontal);
				}
			}
		}

		/// <summary>
		/// 当Content发生了滑动 
		/// </summary>
		private void OnValueChanged(Vector2 newPos)
		{
			if (willShowState == WillShowState.OnlyPosition)
			{
				willShowState = WillShowState.OnlyPosition;
			}
		}

		private void OnDestroy()
		{
			this.listData.Clear();
		}

		/// <summary>
		/// 用于检测一些关键值得改变
		/// </summary>
		private void Update()
		{
			if (Application.isPlaying)
			{

				if (dataChanged != DataChange.None)
				{
					switch (dataChanged)
					{
						case DataChange.Added:
							for (int i = this.addModeStartIndex; i < this.listData.Count; i++)
							{
								var scrollData = this.listData[i];
								this.setSingleDataAction(scrollData);
							}
							EndSetData();
							break;
						case DataChange.Removed:
							SetAllData();
							break;
					}
					dataChanged = DataChange.None;
					if (willShowState < WillShowState.OnlyPosition)
					{
						willShowState = WillShowState.OnlyPosition;
					}
				}

				//跳转相关
				if (jumpState.Update())
				{
					if (willShowState < WillShowState.OnlyPosition)
					{
						willShowState = WillShowState.OnlyPosition;
					}
				}
				if (willShowState != WillShowState.None)
				{
					Show();
				}
			}
#if UNITY_EDITOR
			else
			{
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
			}
#endif
		}

		private float GetDistanceToCenterWhenVeritical(Vector2 anchoredPosition)
		{
			return Mathf.Abs(anchoredPosition.y - centerAnchoredPosition.y);
		}

		private float GetDistanceToCenterWhenHorizontal(Vector2 anchoredPosition)
		{
			return Mathf.Abs(anchoredPosition.x - centerAnchoredPosition.x);
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
			switch (startCorner) {
				case 0:
					scrollBounds.left = -rectTransfrom.anchoredPosition.x;
					scrollBounds.right = Width - rectTransfrom.anchoredPosition.x;
					scrollBounds.up = -rectTransfrom.anchoredPosition.y;
					scrollBounds.down = -Height - rectTransfrom.anchoredPosition.y;
					centerAnchoredPosition.x = Width / 2 - rectTransfrom.anchoredPosition.x;
					centerAnchoredPosition.y = -Height / 2 - rectTransfrom.anchoredPosition.y;
					break;
				case 1:
					scrollBounds.left = -Width - rectTransfrom.anchoredPosition.x;
					scrollBounds.right = -rectTransfrom.anchoredPosition.x;
					scrollBounds.up = -rectTransfrom.anchoredPosition.y;
					scrollBounds.down = -Height - rectTransfrom.anchoredPosition.y;
					centerAnchoredPosition.x = Width / 2 - rectTransfrom.anchoredPosition.x;
					centerAnchoredPosition.y = -Height / 2 - rectTransfrom.anchoredPosition.y;
					break;
				case 2:
					scrollBounds.left = -rectTransfrom.anchoredPosition.x;
					scrollBounds.right = Width - rectTransfrom.anchoredPosition.x;
					scrollBounds.up = Height - rectTransfrom.anchoredPosition.y;
					scrollBounds.down = -rectTransfrom.anchoredPosition.y;
					centerAnchoredPosition.x = Width / 2 - rectTransfrom.anchoredPosition.x;
					centerAnchoredPosition.y = Height / 2 - rectTransfrom.anchoredPosition.y;
					break;
				case 3:
					scrollBounds.left = -Width - rectTransfrom.anchoredPosition.x;
					scrollBounds.right = -rectTransfrom.anchoredPosition.x;
					scrollBounds.up = Height - rectTransfrom.anchoredPosition.y;
					scrollBounds.down = -rectTransfrom.anchoredPosition.y;
					centerAnchoredPosition.x = Width / 2 - rectTransfrom.anchoredPosition.x;
					centerAnchoredPosition.y = Height / 2 - rectTransfrom.anchoredPosition.y;
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

		/// <summary>
		/// 根据ListData内容展示所需展示的，这里经过了优化以确保在运行时保持较高效率
		/// </summary>
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

			while (searchList.Count > 0 && (--maxSearchTimes > 0)) {
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

			//根据位置上下寻找
			if (found)
			{
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

		public void ShowGizmosBounds()
		{
			listData.ForEach(temp => {
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
			//防止有些人机器上找不到
			if (contentTrans == null)
			{
				contentTrans = this.transform.GetChild(0) as RectTransform;
			}
			if (scrollDirection == ScrollDirection.Vertical)
			{
				switch (startCorner) {
					case 0://Left Up
					case 1://Right Up
						contentTrans.pivot = new Vector2(0.5f, 1);
						contentTrans.anchorMin = new Vector2(0, 1);
						contentTrans.anchorMax = new Vector2(1, 1);
						contentTrans.offsetMin = new Vector2(0, 0);
						contentTrans.offsetMax = new Vector2(0, 0);
						break;
					case 2://Left Down
					case 3://Right Down
						contentTrans.pivot = new Vector2(0.5f, 0);
						contentTrans.anchorMin = new Vector2(0, 0);
						contentTrans.anchorMax = new Vector2(1, 0);
						contentTrans.offsetMin = new Vector2(0, 0);
						contentTrans.offsetMax = new Vector2(0, 0);
						break;
				}
			}
			else if (scrollDirection == ScrollDirection.Horizontal)
			{
				switch (startCorner)
				{
					case 0://Left Up
					case 2://Left Down
						contentTrans.pivot = new Vector2(0, 0.5f);
						contentTrans.anchorMin = new Vector2(0, 0);
						contentTrans.anchorMax = new Vector2(0, 1);
						contentTrans.offsetMin = new Vector2(0, 0);
						contentTrans.offsetMax = new Vector2(0, 0);
						break;
					case 1://Right Up
					case 3://Right Down
						contentTrans.pivot = new Vector2(1, 0.5f);
						contentTrans.anchorMin = new Vector2(1, 0);
						contentTrans.anchorMax = new Vector2(1, 1);
						contentTrans.offsetMin = new Vector2(0, 0);
						contentTrans.offsetMax = new Vector2(0, 0);
						break;
				}
			}
		}

		private void UpdateCentered()
		{
			if (centered)
			{
				if (oldMaxWidth == maxWidth)
				{
					return;
				}
				oldMaxWidth = maxWidth;
				var centerOffset = getCenterOffset();
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
				contentTrans.sizeDelta = new Vector2(contentTrans.sizeDelta.x, this.contentSize);
			}
			else if (scrollDirection == ScrollDirection.Horizontal)
			{
				this.contentSize = cursorPos.x + maxHeight - (listData.Count > 0 ? spacing.x : 0) + border.x;
				contentTrans.sizeDelta = new Vector2(this.contentSize, contentTrans.sizeDelta.y);
			}
		}

		private Func<Vector2, Vector2> transAnchoredPosition;

		private void InitTransAnchoredPosition()
		{
			switch (startCorner)
			{
				case 0:
					//Left Up
					transAnchoredPosition = origin =>
					{
						origin.y = -origin.y;
						return origin;
					};
					break;
				case 1:
					//Right Up
					transAnchoredPosition = origin =>
					{
						origin.x = -origin.x;
						origin.y = -origin.y;
						return origin;
					};
					break;
				case 2:
					//Left Down
					transAnchoredPosition = origin =>
					{
						return origin;
					};
					break;
				case 3:
					//Right Down
					transAnchoredPosition = origin =>
					{
						origin.x = -origin.x;
						return origin;
					};
					break;
			}
		}

		/// <summary>
		/// 转化AnchoredPosition
		/// </summary>
		public Vector2 TransAnchoredPosition(Vector2 position)
		{
			var returnPosition = transAnchoredPosition(position);
			return returnPosition;
		}

		private Func<Vector2> getCenterOffset;

		private void InitGetCenterOffset()
		{
			if (scrollDirection == ScrollDirection.Vertical)
			{
				getCenterOffset = () => { return new Vector2((Width - border.x - maxWidth) / 2, 0); };
			}
			else if (scrollDirection == ScrollDirection.Horizontal)
			{
				getCenterOffset = () => { return new Vector2(0, (Height - border.y - maxWidth) / 2); };
			}
		}

		public Vector2 GetCenterOffset()
		{
			return getCenterOffset();
		}

		private Action<RectTransform> formatPrefabRectTransform;

		/// <summary>
		/// 格式化预制体
		/// </summary>
		private void InitFormatPrefabRectTransform()
		{
			switch (startCorner)
			{
				case 0:
					//Left Up
					formatPrefabRectTransform = rectTransform =>
					{
						rectTransform.pivot = new Vector2(0.5f, 0.5f);
						rectTransform.anchorMin = new Vector2(0, 1);
						rectTransform.anchorMax = new Vector2(0, 1);
					};
					break;
				case 1:
					//Right Up
					formatPrefabRectTransform = rectTransform =>
					{
						rectTransform.pivot = new Vector2(0.5f, 0.5f);
						rectTransform.anchorMin = new Vector2(1, 1);
						rectTransform.anchorMax = new Vector2(1, 1);
					};
					break;
				case 2:
					//Left Down
					formatPrefabRectTransform = rectTransform =>
					{
						rectTransform.pivot = new Vector2(0.5f, 0.5f);
						rectTransform.anchorMin = new Vector2(0, 0);
						rectTransform.anchorMax = new Vector2(0, 0);
					};
					break;
				case 3:
					//Right Down
					formatPrefabRectTransform = rectTransform =>
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
		/// 根据子物体来排列内容，这个方法只在编辑器环境下使用
		/// </summary>
		public void SetContentChildren()
		{
#if UNITY_EDITOR
			if (this.contentTrans == null)
			{
				return;
			}

			//初始化
			InitGetCenterOffset();
			InitTransAnchoredPosition();
			InitFormatPrefabRectTransform();
			InitCursor();
			InitContentTrans();

			Dictionary<RectTransform, Vector2> dic_RectTransform_AnchoredPosition = new Dictionary<RectTransform, Vector2>();
			var childCount = this.contentTrans.childCount;
			if (scrollDirection == ScrollDirection.Vertical)
			{
				for (int i = 0; i < childCount; i++)
				{
					var rectTransform = this.contentTrans.GetChild(i) as RectTransform;
					if (IsPrefabNameIgnored(rectTransform.name)) { continue; }
					formatPrefabRectTransform(rectTransform);
					ScrollLayout.NewLine newLine = ScrollLayout.NewLine.None;
					var layout = this.contentTrans.GetChild(i).GetComponent<ScrollLayout>();
					if (layout != null)
					{
						newLine = layout.newLine;
					}
					if (newLine == ScrollLayout.NewLine.None)
					{
						//发生过偏移，并且这次物体的右边界超过宽度
						if (cursorPos.x > border.x && cursorPos.x + rectTransform.sizeDelta.x > Width - border.x)
						{
							//那么执行换行操作
							cursorPos.x = border.x;
							cursorPos.y += maxHeight;
							maxHeight = 0;
						}
						//设置位置
						dic_RectTransform_AnchoredPosition.Add(rectTransform, cursorPos + rectTransform.sizeDelta / 2);
						//更新光标
						cursorPos.x += rectTransform.sizeDelta.x;
						//更新最大宽度
						if (maxWidth < cursorPos.x) { maxWidth = cursorPos.x; }
						//增加间隔
						if (rectTransform.sizeDelta.x > 0)
						{
							cursorPos.x += spacing.x;
						}
						float curMaxHeight = rectTransform.sizeDelta.y > 0 ? (rectTransform.sizeDelta.y + spacing.y) : 0;
						//更新最大高度
						if (maxHeight < curMaxHeight)
						{
							maxHeight = curMaxHeight;
						}
					}
					else
					{
						//发生过偏移，换行
						if (cursorPos.x > border.x)
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
							case ScrollLayout.NewLine.LeftOrUp:
								{
									cursorPos.x = rectTransform.sizeDelta.x / 2 + border.x;
								}
								break;
							case ScrollLayout.NewLine.RightOrDown:
								{
									cursorPos.x = Width - rectTransform.sizeDelta.x / 2 - border.x;
								}
								break;
						}
						//设置位置（需要注意这里直接赋给位置，不需要进行居中处理）
						rectTransform.anchoredPosition = TransAnchoredPosition(cursorPos + rectTransform.sizeDelta.y / 2 * Vector2.up);
						//换新行
						cursorPos.x = border.x;
						cursorPos.y += rectTransform.sizeDelta.y + spacing.y;
					}
				}
				//设置content高度
				contentTrans.sizeDelta = new Vector2(
					contentTrans.sizeDelta.x,
					cursorPos.y + maxHeight - (childCount > 0 ? spacing.y : 0) + border.y
				);
				float centerOffset = 0;
				if (centered)
				{
					centerOffset = (Width - border.x - maxWidth) / 2;
				}
				foreach (var rectTransform in dic_RectTransform_AnchoredPosition.Keys)
				{
					rectTransform.anchoredPosition = TransAnchoredPosition(dic_RectTransform_AnchoredPosition[rectTransform] + Vector2.right * centerOffset);
				}
			}
			else if (scrollDirection == ScrollDirection.Horizontal)
			{
				for (int i = 0; i < childCount; i++)
				{
					var rectTransform = this.contentTrans.GetChild(i) as RectTransform;
					if (IsPrefabNameIgnored(rectTransform.name)) { continue; }
					formatPrefabRectTransform(rectTransform);
					ScrollLayout.NewLine newLine = ScrollLayout.NewLine.None;
					var layout = this.contentTrans.GetChild(i).GetComponent<ScrollLayout>();
					if (layout != null)
					{
						newLine = layout.newLine;
					}
					if (newLine == ScrollLayout.NewLine.None)
					{
						//发生过偏移，并且这次物体的右边界超过宽度
						if (cursorPos.y > border.y && cursorPos.y + rectTransform.sizeDelta.y > Height - border.y)
						{
							//那么执行换行操作
							cursorPos.y = border.y;
							cursorPos.x += maxHeight;
							maxHeight = 0;
						}
						//设置位置
						dic_RectTransform_AnchoredPosition.Add(rectTransform, cursorPos + rectTransform.sizeDelta / 2);
						//更新光标
						cursorPos.y += rectTransform.sizeDelta.y;
						//更新最大宽度
						if (maxWidth < cursorPos.y) { maxWidth = cursorPos.y; }
						//增加间隔
						if (rectTransform.sizeDelta.y > 0)
						{
							cursorPos.y += spacing.y;
						}
						//更新最大高度
						float curMaxHeight = rectTransform.sizeDelta.x > 0 ? (rectTransform.sizeDelta.x + spacing.x) : 0;
						if (maxHeight < curMaxHeight)
						{
							maxHeight = curMaxHeight;
						}
					}
					else
					{
						//发生过偏移，换行
						if (cursorPos.y > border.y)
						{
							cursorPos.x += maxHeight;
							maxHeight = 0;
						}
						switch (newLine)
						{
							case ScrollLayout.NewLine.Center:
								{
									cursorPos.y = Height / 2;
								}
								break;
							case ScrollLayout.NewLine.LeftOrUp:
								{
									cursorPos.y = rectTransform.sizeDelta.y / 2 + border.y;
								}
								break;
							case ScrollLayout.NewLine.RightOrDown:
								{
									cursorPos.y = Height - rectTransform.sizeDelta.y / 2 - border.y;
								}
								break;
						}
						//设置位置（需要注意这里直接赋给位置，不需要进行居中处理）
						rectTransform.anchoredPosition = TransAnchoredPosition(cursorPos + rectTransform.sizeDelta.x / 2 * Vector2.right);
						//换新行
						cursorPos.y = border.y;
						cursorPos.x += rectTransform.sizeDelta.x + spacing.x;
					}
				}
				//设置content高度
				contentTrans.sizeDelta = new Vector2(
					cursorPos.x + maxHeight - (childCount > 0 ? spacing.x : 0) + border.x,
					contentTrans.sizeDelta.y
				);
				float centerOffset = 0;
				if (centered)
				{
					centerOffset = (Height - border.y - maxWidth) / 2;
				}
				foreach (var rectTransform in dic_RectTransform_AnchoredPosition.Keys)
				{
					rectTransform.anchoredPosition = TransAnchoredPosition(dic_RectTransform_AnchoredPosition[rectTransform] + Vector2.up * centerOffset);
				}
			}
			dic_RectTransform_AnchoredPosition.Clear();
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
				contentTrans = new GameObject("Content Transform", typeof(RectTransform)).transform as RectTransform;
				contentTrans.SetParent(this.transform);
				contentTrans.transform.localPosition = Vector3.zero;
				contentTrans.localScale = Vector3.one;
			}

			//Image
			bool isNew;
			var image = Tools.AddComponentIfNotExist<Image>(this.gameObject, out isNew);
			if (isNew)
			{
				image.sprite = null;
				image.color = new Color(1, 1, 1, 0.2f);
			}


			//Mask
			var mask = Tools.AddComponentIfNotExist<Mask>(this.gameObject, out isNew);
			if (isNew)
			{
				mask.showMaskGraphic = true;
			}

			//Scroll
			var scrollRect = Tools.AddComponentIfNotExist<ScrollRect>(this.gameObject, out isNew);
			if (isNew) {
				scrollRect.decelerationRate = 0.04f;
			}
			EnableScrollDirection();
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
				setSingleDataAction(curData);
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
			UpdateEnableScroll();
			//Add重置
			this.addModeStartIndex = 0;
		}

		private float GetScrollRectNormalizedPosWhenVertical()
		{
			return scrollRect.verticalNormalizedPosition;
		}

		private float GetScrollRectNormalizedPosWhenHorizontal()
		{
			return scrollRect.horizontalNormalizedPosition;
		}

		private void SetScrollRectNormalizedPosWhenVertical(float normalizedPos)
		{
			scrollRect.verticalNormalizedPosition = normalizedPos;
		}

		private void SetScrollRectNormalizedPosWhenHorizontal(float normalizedPos)
		{
			scrollRect.horizontalNormalizedPosition = normalizedPos;
		}

		private void SetSingleContentDataWhenVertical(ScrollData data)
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

		private void UpdateEnableScroll()
		{
			//超出边界的时候才允许滚动
			if (enableScrollOnlyWhenOutOfBounds)
			{
				if (scrollDirection == ScrollDirection.Vertical)
				{
					if ((this.contentTrans as RectTransform).sizeDelta.y > Height)
					{
						EnableScrollDirection();
					}
					else
					{
						DisableScrollDirection();
					}
				}
				else
				{
					if ((this.contentTrans as RectTransform).sizeDelta.x > Width)
					{
						EnableScrollDirection();
					}
					else
					{
						DisableScrollDirection();
					}
				}
			}
			else
			{
				EnableScrollDirection();
			}
		}

		private void DisableScrollDirection()
		{
			scrollRect.horizontal = false;
			scrollRect.vertical = false;
			scrollRect.enabled = false;
		}

		private void EnableScrollDirection()
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
			List<RectTransform> allChildren = new List<RectTransform>();
			for (int i = 0; i < contentTrans.childCount; i++)
			{
				allChildren.Add(contentTrans.GetChild(i) as RectTransform);
			}
			foreach (var originRectTransform in allChildren)
			{
				if (IsPrefabNameIgnored(originRectTransform.name)) { continue; }
				//确保提前格式化
				formatPrefabRectTransform(originRectTransform);
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
			if (m_OldSpacing != this.spacing)
			{
				result = true;
				m_TurnSameAction += () =>
				{
					m_OldSpacing = spacing;
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
			if (m_Centered != centered)
			{
				result = true;
				m_TurnSameAction += () =>
				{
					m_Centered = centered;
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
					Tools.DrawRectRange(Tools.GetRectBounds(contentTrans, scrollBounds), this.transform.position.z, Color.green);
					if ((border.x > 0 || border.y > 0) && (contentTrans.rect.width > 2 * border.x) && (contentTrans.rect.height > 2 * border.y))
					{
						scrollBounds.left += contentTrans.lossyScale.x * border.x;
						scrollBounds.right -= contentTrans.lossyScale.x * border.x;
						scrollBounds.up -= contentTrans.lossyScale.y * border.y;
						scrollBounds.down += contentTrans.lossyScale.y * border.y;
						Tools.DrawRectRange(scrollBounds, this.transform.position.z, Color.green);
					}
				}
			}
		}

#endif

		#endregion

		#region 外部方法

		private Dictionary<object, ScrollData> dic_DataSource_ScrollData = new Dictionary<object, ScrollData>();

		private List<ScrollData> listVisibleScrollData = new List<ScrollData>(8);

		private List<ScrollData> listNextVisibleScrollData = new List<ScrollData>(8);

		public Action<string, GameObject, object> onItemOpen { get; private set; }

		public Action<string, GameObject, object> onItemClose { get; private set; }

		public Action<string, GameObject, object> onItemRefresh { get; private set; }

		public void SetOnItemRefresh(Action<string, GameObject, object> onItemRefresh)
		{
			this.onItemRefresh = onItemRefresh;
		}

		public void SetOnItemClose(Action<string, GameObject, object> onItemClose)
		{
			this.onItemClose = onItemClose;
		}

		public void SetOnItemOpen(Action<string, GameObject, object> onItemOpen)
		{
			this.onItemOpen = onItemOpen;
		}

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
					dataChanged = DataChange.Removed;
				}
				else
				{
					scrollData.Update(WillShowState.BothPositionAndContent);
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
				referScrollData.Update(WillShowState.BothPositionAndContent);
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
			dataChanged = DataChange.Removed;
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
				dataChanged = DataChange.Removed;
				Jump(this.resetNormalizedPosition);
			}
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
				if (dataChanged < DataChange.Removed)
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
				if (this.dataChanged < DataChange.Removed)
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
		/// 倒序排列
		/// </summary>
		public void Reverse()
		{
			if (listData.Count <= 0) {
				return;
			}
			listData.Reverse();
			if (this.dataChanged < DataChange.Removed)
			{
				this.dataChanged = DataChange.Removed;
			}
		}

		/// <summary>
		/// 通过一个索引来跳转倒某个位置
		/// </summary>
		public void JumpDataByIndex(int index,bool animated = false)
		{
			if (index >= 0 && index < listData.Count)
			{
				JumpData(listData[index].dataSource, animated);
			}
			else
			{
				Debug.LogWarning("无法找到该Index:" + index);
			}
		}

		/// <summary>
		/// 跳转到某个数据
		/// </summary>
		public void JumpData(object dataSource, bool animated = false)
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
			jumpState.Do(normalizedPosition,animated);
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
		/// 除了某种或者多种元素的总数量
		/// </summary>
		public int GetCountExcept(params string[] prefabNames)
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

		private int addModeStartIndex = 0;

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
				dataChanged = DataChange.Removed;
				this.Jump(this.resetNormalizedPosition);
			}
			listData.Add(scrollData);
			if (dataChanged < DataChange.Added)
			{
				addModeStartIndex = listData.Count - 1;
				dataChanged = DataChange.Added;
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
			if (this.dataChanged < DataChange.Removed)
			{
				this.dataChanged = DataChange.Removed;
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

		#endregion

		#region 一些辅助方法

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

		/// <summary>
		/// 传过来一个原始的rectTransform，就会自动创建所有需要的组件
		/// </summary>
		public static ScrollSystem Create(Transform target)
		{
#if UNITY_EDITOR
			var scrollsystem = target.gameObject.AddComponent<ScrollSystem>();
			scrollsystem.SetComponent();
			var image = scrollsystem.GetComponent<Image>();
			image.type = Image.Type.Sliced;
			image.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource(typeof(Sprite), "UI/Skin/UIMask.psd") as Sprite;
			image.color = Color.white;
			var mask = scrollsystem.GetComponent<Mask>();
			mask.showMaskGraphic = false;
			return scrollsystem;
#else
			return null;
#endif
		}

		/// <summary>
		/// 添加元素
		/// </summary>
		public void AddInEditor(Transform target)
		{
#if UNITY_EDITOR
			target.SetParent(this.contentTrans);
#endif
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

		public void SetOnBeginDrag(Action<PointerEventData> onBeginDrag)
		{
			this.onBeginDrag = onBeginDrag;
		}

		public void SetOnEndDrag(Action<PointerEventData> onEndDrag)
		{
			this.onEndDrag = onEndDrag;
		}

		public void SetOnDrag(Action<PointerEventData> onDrag)
		{
			this.onDrag = onDrag;
		}

		public void SetScrollDirection(bool isHorOrVer)
		{
			this.scrollRect.horizontal = isHorOrVer;
			this.scrollRect.vertical = !isHorOrVer;
		}

		public void DisableMovement()
		{
			this.jumpState.Stop();
			this.scrollRect.StopMovement();
			this.scrollRect.enabled = false;
			this.moveEnable = false;
		}

		public void EnableMovement()
		{
			this.scrollRect.enabled = true;
			this.moveEnable = true;
		}

		#endregion


	}
}