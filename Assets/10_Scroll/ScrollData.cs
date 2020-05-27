﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{

	public class ScrollData
	{

		public ScrollData() { }

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
		public ScrollSystem.ObjectPool objectPool;
		public System.Object dataSource;
		public ScrollSystem scrollSystem;
		public Vector2 anchoredPosition;
		public Vector2 originPosition;

		private Func<object, Vector2> getSize;
		public bool isVisible { get; private set; }
		public bool isPositionInited { get; private set; }

		private RectBounds rectBounds = new RectBounds();
		private uint lastUpdateFrame = 0;
		private RectTransform targetTrans = null;

		public float Left { get { return rectBounds.left; } }
		public float Right { get { return rectBounds.right; } }
		public float Up { get { return rectBounds.up; } }
		public float Down { get { return rectBounds.down; } }

		public Vector2 Size
		{
			get
			{
				return new Vector2(width, height);
			}
		}

		public Vector3 GetWorldPosition()
		{
			return Tools.GetWorldPosByAnchoredPos(scrollSystem.contentTrans, anchoredPosition, scrollSystem.PrefabAnchor);
		}

		/// <summary>
		/// 设置宽度和高度
		/// </summary>
		public bool CalculateSize()
		{
			bool changed = false;
			if (getSize != null)
			{
				var newSize = getSize(dataSource);
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
				//离开视野
				if (this.scrollSystem.onItemClose != null)
				{
					this.scrollSystem.onItemClose(objectPool.prefabName, this.targetTrans.gameObject, this.dataSource);
				}
				objectPool.Recycle(this.targetTrans.gameObject);
				this.targetTrans = null;
			}
		}

		/// <summary>
		/// 更新内容
		/// </summary>
		public void Update(bool refreshContent, bool refreshPosition)
		{
			if (isVisible)
			{
				if (this.targetTrans == null)
				{
					//进入视野
					this.targetTrans = objectPool.Get().transform as RectTransform;
					refreshContent = true;
					refreshPosition = true;
					if (this.scrollSystem.onItemOpen!= null)
					{
						this.scrollSystem.onItemOpen(objectPool.prefabName, this.targetTrans.gameObject, this.dataSource);
					}
				}
				if (refreshPosition)
				{
					this.targetTrans.sizeDelta = new Vector2(this.width, this.height);
					this.targetTrans.anchoredPosition = anchoredPosition;
#if UNITY_EDITOR
					ShowGizmosBounds();
#endif
				}
				if (refreshContent)
				{
					//根据data刷新
					if (this.scrollSystem.onItemRefresh != null)
					{
						this.scrollSystem.onItemRefresh(objectPool.prefabName, this.targetTrans.gameObject, dataSource);
					}
				}
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
		/// 只检查是否可见
		/// </summary>
		public bool IsVisible(uint frame)
		{
			if (frame > lastUpdateFrame)
			{
				lastUpdateFrame = frame;
				isVisible = rectBounds.Overlaps(scrollSystem.scrollBounds);
			}
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

		private void UpdateRectBounds()
		{
			this.rectBounds.left = anchoredPosition.x - 0.5f * width;
			this.rectBounds.right = anchoredPosition.x + 0.5f * width;
			this.rectBounds.up = anchoredPosition.y + 0.5f * height;
			this.rectBounds.down = anchoredPosition.y - 0.5f * height;
		}

	}
}