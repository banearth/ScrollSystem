using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BanSupport
{
	public partial class ScrollSystem
	{

		/// <summary>
		/// Content节点名字
		/// </summary>
		private static readonly string ContentTransformName = "ContentTransform";

		/// <summary>
		/// 用途：给拳皇项目做重复图案背景
		/// 备注：不区分大小写，字母相同都会忽略掉
		/// </summary>
		private static readonly string[] ignorePrefabNames = new string[] { "background" };

		/// <summary>
		/// 用途：是否绘制ScrollSystem外边框
		/// </summary>
		public bool DrawGizmos
		{
			get
			{
				return drawGizmos;
			}
		}

		/// <summary>
		/// 通途：定义ScrollSystem边缘的大小
		/// 备注：外部读取用于快速设置几成几布局
		/// </summary>
		public Vector2 Border
		{
			get
			{
				return border;
			}
		}

		/// <summary>
		/// 用途：定义ScrollSystem元素之间的间隔
		/// 备注：外部读取用于快速设置几成几布局
		/// </summary>
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
		public ContentTransform ContentTrans
		{
			get
			{
				if (_contentTrans == null)
				{
					_contentTrans = this.transform.Find(ContentTransformName)?.GetComponent<ContentTransform>();
					if (_contentTrans == null)
					{
						_contentTrans = SelfRectTransform.AddChild(ContentTransformName, true).gameObject.AddComponent<ContentTransform>();
					}
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

	}
}