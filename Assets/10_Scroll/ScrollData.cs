using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{

	public class ScrollData
	{

		public static Vector2 DEFAULT_ANCHOR = new Vector2(0, 1);

		public ScrollData() { }

		public ScrollData(ScrollSystem scrollSystem, string prefabName, object dataSource, Func<object, Vector2> onResize)
		{
			Init(scrollSystem, prefabName, dataSource, onResize);
		}

		protected void Init(ScrollSystem scrollSystem, string prefabName, object dataSource, Func<object, Vector2> onResize)
		{
			this.scrollSystem = scrollSystem;
			this.objectPool = scrollSystem.ObjectPoolDic[prefabName];
			this.dataSource = dataSource;
			this.newLine = objectPool.newLine;
			this.onResize = onResize;
			this.isPositionInited = false;
		}

		//基本的属性值
		public float width;
		public float height;
		public ScrollLayout.NewLine newLine;
		public ScrollSystem.ObjectPool objectPool;
		public System.Object dataSource;
		public ScrollSystem scrollSystem;
		public Vector2 anchoredPosition;

		private Func<object, Vector2> onResize;
		public bool isVisible { get; private set; }
		public bool isPositionInited { get; private set; }

		private RectBounds rectBounds = new RectBounds();
		private uint lastUpdateFrame = 0;
		private RectTransform targetTrans = null;

		public Vector2 Size
		{
			get
			{
				return new Vector2(width, height);
			}
		}

		public Vector3 GetWorldPosition()
		{
			return Tools.GetWorldPosByAnchoredPos(scrollSystem.contentTrans, anchoredPosition, DEFAULT_ANCHOR);
		}

		/// <summary>
		/// 设置宽度和高度
		/// </summary>
		public bool OnResize()
		{
			bool changed = false;
			if (onResize != null)
			{
				var newSize = onResize(dataSource);
				if (newSize.x > 0)
				{
					if (this.width != newSize.x)
					{
						this.width = newSize.x;
						changed = true;
					}
				}
				else
				{
					this.width = objectPool.prefabWidth;
				}
				if (newSize.y > 0)
				{
					if (this.height != newSize.y)
					{
						this.height = newSize.y;
						changed = true;
					}
				}
				else
				{
					this.height = objectPool.prefabHeight;
				}
			}
			else
			{
				var rectTrans = objectPool.origin.transform as RectTransform;
				this.width = rectTrans.sizeDelta.x;
				this.height = rectTrans.sizeDelta.y;
			}
			return changed;
		}

		public void Hide()
		{
			this.isVisible = false;
			if (this.targetTrans != null)
			{
				objectPool.Recycle(this.targetTrans.gameObject);
				//scrollSystem.AttachScrollData(this.targetGo, null);
				this.targetTrans = null;
			}
		}

		/// <summary>
		/// 更新内容
		/// </summary>
		/// <param name="refresh">表示强制刷新</param>
		public void Update(bool refreshContent,bool refreshPosition)
		{
			if (isVisible)
			{
				if (this.targetTrans == null)
				{
					this.targetTrans = objectPool.Get().transform as RectTransform;
					refreshContent = true;
					refreshPosition = true;
					//scrollSystem.AttachScrollData(this.targetGo, this);
				}
				if (refreshPosition)
				{
					this.targetTrans.sizeDelta  = new Vector2(this.width, this.height);
					this.targetTrans.anchoredPosition = anchoredPosition;
				}
				if (refreshContent)
				{
					this.scrollSystem.setItemContent(objectPool.prefabName, this.targetTrans, dataSource);
				}

#if UNITY_EDITOR
				ShowGizmosBounds();
#endif
			}
		}

		public void ShowGizmosBounds()
		{
			if (scrollSystem.DrawGizmos)
			{
				Tools.DrawRectBounds(GetWorldPosition(), scrollSystem.contentTrans.lossyScale.x * width, scrollSystem.contentTrans.lossyScale.y * height, Color.red);
			}
		}

		/// <summary>
		/// 计算世界坐标位置，并且计算是否可见
		/// 这里只是模拟计算位置，不对预制体进行任何操作
		/// </summary>
		public void CheckVisible(uint frame)
		{
			if (frame > lastUpdateFrame)
			{				
				lastUpdateFrame = frame;
				isVisible = rectBounds.Overlaps(scrollSystem.scrollBounds);

				/*
				//根据contentTrans更新世界坐标
				this.worldPosition = Tools.GetUIPosByAnchoredPos(
					scrollSystem.contentTrans, 
					this.anchoredPosition + scrollSystem.forceCenterOffset, 
					DEFAULT_ANCHOR
				);
				*/

			}
		}

		/// <summary>
		/// 设置位置
		/// </summary>
		public void SetAnchoredPosition(Vector2 position)
		{
			this.isPositionInited = true;
			position.y = -position.y;
			anchoredPosition = position;

			rectBounds.left = anchoredPosition.x - 0.5f * width;
			rectBounds.right = anchoredPosition.x + 0.5f * width;
			rectBounds.up = anchoredPosition.y + 0.5f * height;
			rectBounds.down = anchoredPosition.y - 0.5f * height;

			//Debug.Log("anchoredPosition:"+ anchoredPosition + " rectBounds:"+rectBounds);
		}

	}
}