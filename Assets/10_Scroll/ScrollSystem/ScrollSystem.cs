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

	public partial class ScrollSystem : MonoBehaviour
	{

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

		private ContentTransform _contentTrans = null;

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
		/// 为单个ScrollData进行布局
		/// </summary>
		private Action<ScrollData> onAlignScrollDataAction = null;

		/// <summary>
		/// 格式化预制体
		/// </summary>
		private Action<RectTransform> onFormatPrefab = null;

		/// <summary>
		/// 获得居中偏移量
		/// </summary>
		private Func<Vector2> onGetCenterOffset = null;

		/// <summary>
		/// 根据其实锚点转换
		/// </summary>
		private Func<Vector2, Vector2> onGetAnchoredPosition = null;

		/// <summary>
		/// 获得距离中心点的距离
		/// </summary>
		private Func<Vector2, float> onGetDistanceToCenter = null;

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
				result.distance = scrollSystem.onGetDistanceToCenter(curData.anchoredPosition);
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
						this.scrollSystem.CallItemRefresh(objectPool.prefabName, this.targetTrans.gameObject, dataSource);
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

		/// <summary>
		/// 
		/// </summary>
		[ExecuteAlways]
		public class ContentTransform : MonoBehaviour
		{
			private ScrollSystem _scrollSystem = null;
			private ScrollSystem scrollSystem
			{
				get
				{
					if (this._scrollSystem == null)
					{
						this._scrollSystem = this.transform.parent?.GetComponent<ScrollSystem>();
					}
					return this._scrollSystem;
				}
			}

			private RectTransform _value = null;

			public RectTransform Value
			{
				get
				{
					if (_value == null)
					{
						_value = this.transform as RectTransform;
					}
					return _value;
				}
			}

#if UNITY_EDITOR
			private void OnTransformChildrenChanged()
			{
				//只在编辑器环境下调用
				if (Application.isPlaying) { return; }
				//当子物体数量发生变化时候时，会通知ScrollSystem进行重新排列
				scrollSystem?.OnPrefebCountChanged();
			}
#endif

		}



		#endregion

	}
}