using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/*
todo
1自定义布局功能
2外部预制体导入
3Flush功能
4显示顺序问题
*/

namespace BanSupport
{

	public class ScrollSystem : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
	{

		#region -----------------------只读-----------------------

		/// <summary>
		/// Content节点名字
		/// </summary>
		private static readonly string ContentTransformName = "ContentTransform";

		/// <summary>
		/// 用途：给拳皇项目做重复图案背景
		/// 备注：不区分大小写，字母相同都会忽略掉
		/// </summary>
		private static readonly string[] ignorePrefabNames = new string[] { "background" };

		public bool DrawGizmos
		{
			get
			{
				return drawGizmos;
			}
		}

		public Vector2 Border
		{
			get
			{
				return border;
			}
		}

		public Vector2 Spacing
		{
			get
			{
				return spacing;
			}
		}

		/// <summary>
		/// 像素单位的宽度
		/// </summary>
		public float Width
		{
			get
			{
				return SelfRectTransform.rect.width;
			}
		}

		/// <summary>
		/// 像素单位的宽度
		/// </summary>
		public float Height
		{
			get
			{
				return SelfRectTransform.rect.height;
			}
		}

		/// <summary>
		/// 内容节点
		/// </summary>
		public ScrollSystemContentTransform ContentTrans
		{
			get
			{
				if (_contentTrans == null)
				{
					_contentTrans = this.transform.Find(ContentTransformName)?.GetComponent<ScrollSystemContentTransform>();
				}
				return _contentTrans;
			}
		}

		/// <summary>
		/// 自身RectTransform
		/// </summary>
		public RectTransform SelfRectTransform
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

		/// <summary>
		/// 本体
		/// </summary>
		private ScrollRect ScrollRect
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

		/// <summary>
		/// 预制体的锚点
		/// </summary>
		public Vector2 PrefabAnchor
		{
			get
			{
				return prefabAnchor;
			}
		}

		#endregion

		#region -----------------------枚举-----------------------

		public enum ScrollDirection
		{
			Vertical,
			Horizontal
		}

		public enum DataAddOrRemove
		{
			None,//表示无变化，不需要操作
			Added,//表示在末尾添加，不需要位置重构，只需要刷新显示
			Removed,//表示需要位置重构
		}

		public enum DataChange
		{
			None,//表示不需要进行操作
			OnlyPosition,//仅仅位置更新
			BothPositionAndContent,//位置和内容都更新
		}

		#endregion

		#region -----------------------内部-----------------------

		private ScrollSystemContentTransform _contentTrans = null;

		private RectTransform _selfRectTramsform = null;

		private ScrollRect _scrollRect = null;

		/// <summary>
		/// 编辑器模式下不会操作这个变量
		/// </summary>
		private bool inited = false;

		/// <summary>
		/// 这个scrollRect的范围，用于检测是否超出了范围
		/// 当位置发生改变的时候，这个值也需要更新
		/// </summary>
		private RectBounds bounds;

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
		/// 用于关联玩家的数据和ScrollData
		/// </summary>
		private Dictionary<object, ScrollData> dic_DataSource_ScrollData = new Dictionary<object, ScrollData>();

		/// <summary>
		/// 当前显示的ScrollData
		/// </summary>
		private List<ScrollData> listVisibleScrollData = new List<ScrollData>(8);

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

		/// <summary>
		/// 为了防止重复计算
		/// </summary>
		private float oldMaxWidth = 0;

		/// <summary>
		/// 视图最大尺寸，用于计算跳转使用
		/// </summary>
		private float contentSize = 0;

		/// <summary>
		/// 用于决定是否在show的时候进行数据操作
		/// </summary>
		private DataAddOrRemove dataAddOrRemove = DataAddOrRemove.None;

		/// <summary>
		/// 用于更新内容或者位置
		/// </summary>
		private DataChange dataChange = DataChange.None;

		/// <summary>
		/// 跳转状态
		/// </summary>
		private JumpState jumpState = null;

		/// <summary>
		/// 内容的中心点
		/// </summary>
		private Vector2 contentCenterPosition;

		/// <summary>
		/// 预制体的锚点
		/// </summary>
		private Vector2 prefabAnchor;

		/// <summary>
		/// 增加Data的
		/// </summary>
		private int addDataStartIndex = 0;

		#endregion

		#region -----------------------事件-----------------------

		/// <summary>
		/// 打开回调（物体从无到有的时候）
		/// 参数依次为 （预制体名字，实例化物体，数据）
		/// </summary>
		private Action<string, GameObject, object> onItemOpen = null;

		/// <summary>
		/// 关闭回调（物体从有到无的时候）
		/// 参数依次为 （预制体名字，实例化物体，数据）
		/// </summary>
		private Action<string, GameObject, object> onItemClose = null;

		/// <summary>
		/// 刷新回调（内容变化的时候）
		/// 参数依次为 （预制体名字，实例化物体，数据）
		/// </summary>
		private Action<string, GameObject, object> onItemRefresh = null;

		/// <summary>
		/// 获得距离中心点的距离
		/// </summary>
		private Func<Vector2, float> getDistanceToCenter = null;

		/// <summary>
		/// 为单个ScrollData进行布局
		/// </summary>
		private Action<ScrollData> alignScrollDataAction = null;

		/// <summary>
		/// 获得居中偏移量
		/// </summary>
		private Func<Vector2> getCenterOffset;

		/// <summary>
		/// 根据其实锚点转换
		/// </summary>
		private Func<Vector2, Vector2> getAnchoredPosition;

		#endregion

		#region -----------------------可编辑-----------------------

		/// <summary>
		/// Gizmos相关
		/// </summary>
		[SerializeField]
		private bool drawGizmos = true;

		/// <summary>
		/// 用途:裁切方式
		/// 备注:0不进行任何操作
		///			1使用Mask裁切
		///			2使用RectMask裁切
		///			3清理所有
		/// </summary>
		[IntEnum(0, 1, 2, 3, "None", "Mask", "RectMask", "Clear")]
		[SerializeField]
		[Header("<布局>")]
		private int clipType = 0;

		//用途：排列起始角
		//备注：0左上，1右上，2左下，3右下
		[IntEnum(0, 1, 2, 3, "Left Up", "Right Up", "Left Down", "Right Down")]
		[SerializeField]
		private int startCorner = 0;

		//用途：排列时边缘留空
		//备注：内容跟外边框的距离
		[Tooltip("边界")]
		[SerializeField]
		private Vector2 border = Vector2.zero;

		/// <summary>
		/// 元素之间的间隔
		/// </summary>
		[SerializeField]
		private Vector2 spacing = Vector2.one * 10;

		//用途：自动排列居中
		//备注：如果是垂直排列，对应水平居中；如果是水平，对应的垂直居中
		[Tooltip("用途：自动排列居中\n备注：如果是垂直排列，对应水平居中；如果是水平，对应的垂直居中")]
		[SerializeField]
		private bool isCenter = false;

		/// <summary>
		/// 当发生重置的时候，设置的位置
		/// </summary>
		[Range(0, 1)]
		[Tooltip("当发生重置的时候，设置的位置")]
		[SerializeField]
		[Header("<跳转>")]
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
		/// 滚动方向
		/// </summary>
		[SerializeField]
		private ScrollDirection scrollDirection = ScrollDirection.Vertical;

		#endregion

		#region -----------------------内部类-----------------------

		/// <summary>
		/// 使用二分法来快速确定显示的ScrollData
		/// </summary>
		public class SearchGroup
		{
			/// <summary>
			/// 左索引
			/// </summary>
			public int left;

			/// <summary>
			/// 右索引
			/// </summary>
			public int right;

			/// <summary>
			/// 中间索引
			/// </summary>
			public int middle;

			/// <summary>
			/// 是否找到
			/// </summary>
			public bool found;

			/// <summary>
			/// 距离scrollsystem的距离
			/// </summary>
			public float distance;

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

		}

		/// <summary>
		/// 用于处理跳转
		/// </summary>
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
				//if (!scrollSystem.moveEnable) { return; }
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
				scrollSystem.ScrollRect.StopMovement();
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
				scrollSystem.ScrollRect.StopMovement();
				state = animated ? State.Animated : State.Directly;
				this.targetScrollData = targetScrollData;
				this.targetNormalizedPos = 0;
			}

		}

		/// <summary>
		/// 用于封装预制体
		/// </summary>
		public class PrefabGroup
		{
			public string prefabName { get; private set; }
			public ScrollLayout.NewLine newLine { private set; get; }
			public GameObject origin;
			public GameObject bindOrigin;
			public GameObjectPool pool;

			public float prefabWidth { private set; get; }

			public float prefabHeight { private set; get; }

			private ScrollSystem scrollSystem;

			public PrefabGroup(GameObject origin, ScrollSystem scrollSystem, int registPoolCount)
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
				this.pool = new GameObjectPool(origin, scrollSystem.ContentTrans.Value, registPoolCount);
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

		/// <summary>
		/// 滚动物核心数据
		/// </summary>
		public class ScrollData
		{

			public ScrollData(ScrollSystem scrollSystem, string prefabName, object dataSource, Func<object, Vector2> getSize)
			{
				Init(scrollSystem, prefabName, dataSource, getSize);
			}

			protected void Init(ScrollSystem scrollSystem, string prefabName, object dataSource, Func<object, Vector2> getSize)
			{
				this.scrollSystem = scrollSystem;
				this.objectPool = scrollSystem.ObjectPoolDic[prefabName];
				this.dataSource = dataSource;
				this.newLine = objectPool.newLine;
				this.getSize = getSize;
				this.isPositionInited = false;
			}

			//基本的属性值
			public float width;
			public float height;
			public ScrollLayout.NewLine newLine;
			public ScrollSystem.PrefabGroup objectPool;
			public System.Object dataSource;
			public ScrollSystem scrollSystem;
			public Vector2 anchoredPosition;
			public Vector2 originPosition;
			private int lastFrameCount;
			private bool sizeCalculated = false;

			private Func<object, Vector2> getSize;
			public bool isVisible { get; private set; }
			public bool isPositionInited { get; private set; }

			private RectBounds bounds;
			private RectTransform targetTrans = null;

			public float Left { get { return bounds.left; } }
			public float Right { get { return bounds.right; } }
			public float Up { get { return bounds.up; } }
			public float Down { get { return bounds.down; } }

			public Vector2 Size { get { return new Vector2(width, height); } }

			public Vector3 GetWorldPosition()
			{
				return Tools.GetUIPosByAnchoredPos(scrollSystem.ContentTrans.Value, anchoredPosition, scrollSystem.PrefabAnchor);
			}

			/// <summary>
			/// 设置宽度和高度，返回是否发生过改变
			/// </summary>
			public bool CalculateSize(bool forceCalculate = false)
			{
				float oldWidth = this.width;
				float oldHeight = this.height;
				if (forceCalculate || (sizeCalculated == false))
				{
					sizeCalculated = true;
					if (getSize != null)
					{
						var newSize = getSize(dataSource);
						if (newSize.x >= 0)
						{
							this.width = newSize.x;
						}
						else
						{
							this.width = objectPool.prefabWidth;
						}
						if (newSize.y >= 0)
						{
							this.height = newSize.y;
						}
						else
						{
							this.height = objectPool.prefabHeight;
						}
					}
					else
					{
						this.width = objectPool.prefabWidth;
						this.height = objectPool.prefabHeight;
					}
					return (oldWidth != this.width) || (oldHeight != this.height);
				}
				else
				{
					return false;
				}
			}

			/// <summary>
			/// 隐藏内容
			/// </summary>
			public void Hide()
			{
				this.isVisible = false;
				if (this.targetTrans != null)
				{
					//离开视野
					this.scrollSystem.CallItemClose(objectPool.prefabName, this.targetTrans.gameObject, this.dataSource);
					objectPool.Release(this.targetTrans.gameObject);
					this.targetTrans = null;
				}
			}

			/// <summary>
			/// 更新内容和位置
			/// </summary>
			public void Show(ScrollSystem.DataChange dataChange)
			{
				if (isVisible)
				{
					if (this.targetTrans == null)
					{
						//如果实体第一次出现，那么需要更新位置和内容
						dataChange = ScrollSystem.DataChange.BothPositionAndContent;
						//进入视野
						this.targetTrans = objectPool.Get().transform as RectTransform;
						//Callback
						this.scrollSystem.CallItemOpen(objectPool.prefabName, this.targetTrans.gameObject, this.dataSource);
#if UNITY_EDITOR
					DrawRect();
#endif
					}
					//更新位置
					if (dataChange >= ScrollSystem.DataChange.OnlyPosition)
					{
						this.targetTrans.sizeDelta = new Vector2(this.width, this.height);
						this.targetTrans.anchoredPosition = anchoredPosition;
					}
					//更新内容
					if (dataChange >= ScrollSystem.DataChange.BothPositionAndContent)
					{
						//Callback

						if (this.scrollSystem.onItemRefresh != null)
						{
							this.scrollSystem.onItemRefresh(objectPool.prefabName, this.targetTrans.gameObject, dataSource);
						}
					}
				}
			}

			/// <summary>
			/// 只检查是否可见
			/// </summary>
			public bool IsVisible()
			{
				if (Time.frameCount == lastFrameCount) { return this.isVisible; }
				this.lastFrameCount = Time.frameCount;
				this.isVisible = bounds.Overlaps(scrollSystem.bounds);
				return this.isVisible;
			}

			/// <summary>
			/// 设置位置
			/// </summary>
			public void SetAnchoredPosition(Vector2 originPosition)
			{
				this.isPositionInited = true;
				this.originPosition = originPosition;
				this.anchoredPosition = scrollSystem.TransAnchoredPosition(originPosition);
				UpdateRectBounds();
			}

			/// <summary>
			/// 设置居中偏移量
			/// </summary>
			public void SetCenterOffset(Vector2 offset)
			{
				if (newLine == ScrollLayout.NewLine.None)
				{
					this.anchoredPosition = scrollSystem.TransAnchoredPosition(this.originPosition + offset);
					UpdateRectBounds();
				}
			}

			/// <summary>
			/// 更新边界区域
			/// </summary>
			private void UpdateRectBounds()
			{
				this.bounds.left = anchoredPosition.x - 0.5f * width;
				this.bounds.right = anchoredPosition.x + 0.5f * width;
				this.bounds.up = anchoredPosition.y + 0.5f * height;
				this.bounds.down = anchoredPosition.y - 0.5f * height;
			}

			/// <summary>
			/// 在Gizmos绘制
			/// </summary>
			private void DrawRect()
			{
				if (scrollSystem.DrawGizmos)
				{
					var localScale = scrollSystem.ContentTrans.Value.lossyScale;
					Tools.DrawRect(GetWorldPosition(), localScale.x * width, localScale.y * height, Color.red);
				}
			}

		}

		#endregion

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
							this.alignScrollDataAction(this.listData[i]);
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
				InitGetCenterOffset();
				SetGetAnchoredPosition();
				InitFormatPrefabRectTransform();
				InitCursor();
				InitContentTrans();
				//注册预制体对象池
				RegistObjectPool();
				//注册滚动监听
				ScrollRect.onValueChanged.AddListener(OnValueChanged);
				if (scrollDirection == ScrollDirection.Vertical)
				{
					alignScrollDataAction = AlignScrollDataWhenVertical;
					getDistanceToCenter = GetDistanceToCenterWhenVeritical;
					jumpState = new JumpState(this, SetNormalizedPosWhenVertical, GetNormalizedPosWhenVertical);
				}
				else if (scrollDirection == ScrollDirection.Horizontal)
				{
					alignScrollDataAction = SetSingleContentDataWhenHorizontal;
					getDistanceToCenter = GetDistanceToCenterWhenHorizontal;
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
			switch (startCorner) {
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
				switch (startCorner) {
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
			var returnPosition = getAnchoredPosition(position);
			return returnPosition;
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
		/// applyLocate 表示保持界面固定不动
		/// </summary>
		private void SetAllData()
		{
			InitCursor();
			var dataCount = this.listData.Count;
			for (int i = 0; i < dataCount; i++)
			{
				var curData = this.listData[i];
				alignScrollDataAction(curData);
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

		#region -----------------------内部事件-----------------------

		/// <summary>
		/// 设置锚点方法
		/// </summary>
		private void SetGetAnchoredPosition()
		{
			switch (startCorner)
			{
				case 0:
					//Left Up
					getAnchoredPosition = origin =>
					{
						origin.y = -origin.y;
						return origin;
					};
					break;
				case 1:
					//Right Up
					getAnchoredPosition = origin =>
					{
						origin.x = -origin.x;
						origin.y = -origin.y;
						return origin;
					};
					break;
				case 2:
					//Left Down
					getAnchoredPosition = origin =>
					{
						return origin;
					};
					break;
				case 3:
					//Right Down
					getAnchoredPosition = origin =>
					{
						origin.x = -origin.x;
						return origin;
					};
					break;
			}
		}

		private void SetGetCenterOffset()
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
			if (listData.Count <= 0) {
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
		public void Jump(int index,bool animated = false)
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
				if (prefabNames.Contains(curData.objectPool.prefabName)) {
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

		#region -----------------------仅用于编辑器模式下-----------------------

#if UNITY_EDITOR

		private int _startCorner = int.MinValue;
		private int _clipType = 0;

		private Vector2 _size = Vector2.zero;
		private ScrollDirection _scrollDirection = ScrollDirection.Vertical;
		private Vector2 _border = Vector2.zero;
		private Vector2 _spacing = Vector2.one * 10;
		private int _childCount = 0;
		private bool _centered = false;
		private Action _beSameAction;

		public class CheckMode
		{
			public static int Component = 1 << 0;
			public static int ContentChildren = 1 << 1;
		}

		private void OnValidate()
		{
			StartCoroutine(OnValidateEnd());
			CheckValueChange(CheckMode.ContentChildren);
		}

		//因为存在Component的Add和Remove，所以不能直接在OnValidate里面调用
		private IEnumerator OnValidateEnd()
		{
			yield return new WaitForEndOfFrame();
			CheckValueChange(CheckMode.Component);
		}
		
		//haha
		private void OnRectTransformDimensionsChange()
		{
			CheckValueChange(CheckMode.ContentChildren);
		}

		public void OnContentChildrenChanged()
		{
			CheckValueChange(CheckMode.ContentChildren);
		}

		private void CheckValueChange(int checkMode)
		{
			if (Application.isPlaying)
			{
				return;
			}
			if ((checkMode & CheckMode.Component) > 0)
			{
				if (CheckSetComponent())
				{
					SetComponent();
				}
			}
			if ((checkMode & CheckMode.ContentChildren) > 0)
			{
				if (CheckSetContentChildren())
				{
					SetContentChildren();
				}
			}
			if (_beSameAction != null)
			{
				_beSameAction();
				_beSameAction = null;
				RefreshWhenEditor();
			}
		}

		private bool CheckSetComponent()
		{
			bool result = false;
			if (_scrollDirection != this.scrollDirection)
			{
				result = true;
				_beSameAction += () =>
				{
					_scrollDirection = this.scrollDirection;
				};
			}
			if (_clipType != this.clipType)
			{
				result = true;
				_beSameAction += () =>
				{
					_clipType = this.clipType;
				};
			}
			return result;
		}

		private bool CheckSetContentChildren()
		{
			bool result = false;
			if (_startCorner != this.startCorner)
			{
				result = true;
				_beSameAction += () =>
				{
					_startCorner = this.startCorner;
				};
			}
			if (_size != (this.transform as RectTransform).sizeDelta)
			{
				result = true;
				_beSameAction += () =>
				{
					_size = (this.transform as RectTransform).sizeDelta;
				};
			}

			if (_spacing != this.spacing)
			{
				result = true;
				_beSameAction += () =>
				{
					_spacing = spacing;
				};
			}
			if (_scrollDirection != scrollDirection)
			{
				result = true;
				_beSameAction += () =>
				{
					_scrollDirection = scrollDirection;
				};
			}
			if (_border != border)
			{
				result = true;
				_beSameAction += () =>
				{
					_border = border;
				};
			}
			if (_centered != isCenter)
			{
				result = true;
				_beSameAction += () =>
				{
					_centered = isCenter;
				};
			}
			if (ContentTrans != null)
			{
				if (_childCount != this.ContentTrans.Value.childCount)
				{
					result = true;
					_beSameAction += () =>
					{
						_childCount = this.ContentTrans.Value.childCount;
					};
				}
			}
			return result;
		}

		private void SetComponent()
		{
			
		}

		public void SetContentChildren()
		{
			if (this.ContentTrans == null)
			{
				return;
			}
			//初始化
			InitGetCenterOffset();
			SetGetAnchoredPosition();
			InitFormatPrefabRectTransform();
			InitCursor();
			InitContentTrans();

			Dictionary<RectTransform, Vector2> dic_RectTransform_AnchoredPosition = new Dictionary<RectTransform, Vector2>();
			var childCount = this.ContentTrans.Value.childCount;
			if (scrollDirection == ScrollDirection.Vertical)
			{
				for (int i = 0; i < childCount; i++)
				{
					var rectTransform = this.ContentTrans.Value.GetChild(i) as RectTransform;
					if (IsPrefabNameIgnored(rectTransform.name)) { continue; }
					formatPrefabRectTransform(rectTransform);
					ScrollLayout.NewLine newLine = ScrollLayout.NewLine.None;
					var layout = this.ContentTrans.Value.GetChild(i).GetComponent<ScrollLayout>();
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
				ContentTrans.Value.sizeDelta = new Vector2(ContentTrans.Value.sizeDelta.x, cursorPos.y + maxHeight - (childCount > 0 ? spacing.y : 0) + border.y);
				float centerOffset = 0;
				if (isCenter)
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
					var rectTransform = this.ContentTrans.Value.GetChild(i) as RectTransform;
					if (IsPrefabNameIgnored(rectTransform.name)) { continue; }
					formatPrefabRectTransform(rectTransform);
					ScrollLayout.NewLine newLine = ScrollLayout.NewLine.None;
					var layout = this.ContentTrans.Value.GetChild(i).GetComponent<ScrollLayout>();
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
				ContentTrans.Value.sizeDelta = new Vector2(cursorPos.x + maxHeight - (childCount > 0 ? spacing.x : 0) + border.x, ContentTrans.Value.sizeDelta.y);
				float centerOffset = 0;
				if (isCenter)
				{
					centerOffset = (Height - border.y - maxWidth) / 2;
				}
				foreach (var rectTransform in dic_RectTransform_AnchoredPosition.Keys)
				{
					rectTransform.anchoredPosition = TransAnchoredPosition(dic_RectTransform_AnchoredPosition[rectTransform] + Vector2.up * centerOffset);
				}
			}
			dic_RectTransform_AnchoredPosition.Clear();
		}

		//private void RefreshWhenEditor()
		//{
		//	Debug.Log("RefreshWhenEditor");
		//	LayoutRebuilder.MarkLayoutForRebuild(this.transform as RectTransform);
		//	Canvas.ForceUpdateCanvases();
		//	UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
		//}

		private void RefreshWhenEditor()
		{

			//haha
			switch (clipType)
			{
				case 0:
					//不进行任何裁切相关的处理
					break;
				case 1:
					//-------------------使用Mask裁切-------------------
					//-------------------删除不必要的-------------------
					Tools.RemoveComponent<NonDrawingGraphic>(this.gameObject);
					Tools.RemoveComponent<RectMask2D>(this.gameObject);
					//-------------------增加需要的-------------------
					if (Tools.AddComponent<Image>(this.gameObject, out Image newImage))
					{
						newImage.sprite = null;
						newImage.color = new Color(1, 1, 1, 0.2f);
					}
					if (Tools.AddComponent<Mask>(this.gameObject, out Mask newMask))
					{
						newMask.showMaskGraphic = true;
					}
					break;
				case 2:
					//-------------------使用RectMask裁切-------------------
					//-------------------删除不必要的-------------------
					Tools.RemoveComponent<Image>(this.gameObject);
					Tools.RemoveComponent<Mask>(this.gameObject);
					//-------------------增加需要的-------------------
					Tools.AddComponent<NonDrawingGraphic>(this.gameObject, out NonDrawingGraphic _);
					Tools.AddComponent<RectMask2D>(this.gameObject, out RectMask2D _);
					break;
				case 3:
					//-------------------清理所有-------------------
					Tools.RemoveComponent<NonDrawingGraphic>(this.gameObject);
					Tools.RemoveComponent<RectMask2D>(this.gameObject);
					Tools.RemoveComponent<Image>(this.gameObject);
					Tools.RemoveComponent<Mask>(this.gameObject);
					break;
			}

			//ScrollRect
			if (Tools.AddComponent<ScrollRect>(this.gameObject, out ScrollRect newScrollRect))
			{
				newScrollRect.decelerationRate = 0.04f;
			}
			EnableScrollDirection();
			ScrollRect.viewport = this.transform as RectTransform;
			ScrollRect.content = ContentTrans.Value;

			//交换horizontalBar和verticalBar
			if (scrollDirection == ScrollDirection.Vertical)
			{
				if (ScrollRect.verticalScrollbar == null && ScrollRect.horizontalScrollbar != null)
				{
					ScrollRect.verticalScrollbar = ScrollRect.horizontalScrollbar;
					ScrollRect.horizontalScrollbar = null;
				}
			}
			else if (scrollDirection == ScrollDirection.Horizontal)
			{
				if (ScrollRect.horizontalScrollbar == null && ScrollRect.verticalScrollbar != null)
				{
					ScrollRect.horizontalScrollbar = ScrollRect.verticalScrollbar;
					ScrollRect.verticalScrollbar = null;
				}
			}

			//-----------------ContentTrans-----------------
			if (_contentTrans == null)
			{
				var tempTrans = new GameObject(ContentTransformName, typeof(RectTransform)).transform as RectTransform;
				_contentTrans = tempTrans.gameObject.AddComponent<ScrollSystemContentTransform>();
				_contentTrans.Value.SetParent(this.transform);
				_contentTrans.Value.gameObject.layer = this.gameObject.layer;
				_contentTrans.Value.localPosition = Vector3.zero;
				_contentTrans.Value.localScale = Vector3.one;
			}

		}

		private void OnDrawGizmos()
		{
			if (drawGizmos)
			{
				Gizmos.color = Color.green;
				//基本点触区域
				Tools.DrawRect(this.transform.position, Width * this.transform.lossyScale.x, Height * this.transform.lossyScale.y, Color.green);
				//滚动区域
				if (this.ContentTrans != null)
				{
					var contentRect = Tools.GetRectBounds(ContentTrans.Value,out float worldPosZ);
					Tools.DrawRect(contentRect, worldPosZ, Color.green);
					if ((border.x > 0 || border.y > 0) && (ContentTrans.Value.rect.width > 2 * border.x) && (ContentTrans.Value.rect.height > 2 * border.y))
					{
						var localScale = ContentTrans.Value.lossyScale;
						var borderOffsetX = localScale.x * border.x;
						var borderOffsetY = localScale.y * border.y;
						contentRect.left += borderOffsetX;
						contentRect.right -= borderOffsetX;
						contentRect.up -= borderOffsetY;
						contentRect.down += borderOffsetY;
						Tools.DrawRect(contentRect, worldPosZ, Color.cyan);
					}
				}
			}
		}

#endif

		#endregion

	}
}