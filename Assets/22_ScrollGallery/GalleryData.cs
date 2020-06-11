using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{
	public class GalleryData
	{
		public float normalizedPos;
		public float recordNormalizedPos;
		public float returnNormalizedPos;
		public object dataSource;
		public Vector3 worldPosition;
		public Vector2 sizeDelta;
		public Vector3 scale;
		public bool isVisible;
		public ScrollGallery scrollGallery;
		public bool isSelected;

		private RectTransform targetTrans = null;

		public GalleryData(ScrollGallery scrollGallery, object dataSource)
		{
			this.scrollGallery = scrollGallery;
			this.dataSource = dataSource;
		}

		public bool UpdateSelect(int mainIndex)
		{
			var i = Mathf.RoundToInt(this.normalizedPos);
			var oldSelected = this.isSelected;
			this.isSelected = (i == mainIndex);
			return oldSelected == isSelected;
		}

		public void SetReturnPos()
		{
			this.returnNormalizedPos = Mathf.RoundToInt(this.normalizedPos);
		}

		public void RecordPos()
		{
			this.recordNormalizedPos = this.normalizedPos;
		}

		public void Move(float offset)
		{
			this.normalizedPos += offset;
		}

		public void MoveByRecord(float normalizedOffset)
		{
			this.normalizedPos = this.recordNormalizedPos + normalizedOffset;
		}

		public bool DoReturn(float progress)
		{
			this.normalizedPos = Mathf.Lerp(this.normalizedPos, this.returnNormalizedPos, progress);
			if (Mathf.Abs(this.normalizedPos - this.returnNormalizedPos) < 0.001)
			{
				this.normalizedPos = this.returnNormalizedPos;
				return true;
			}
			else
			{
				return false;
			}
		}

		public void Update(bool refreshContent, bool refreshPosition)
		{
			if (isVisible)
			{
				if (this.targetTrans == null)
				{
					this.targetTrans = scrollGallery.objectPool.Get().transform as RectTransform;
					refreshContent = true;
					refreshPosition = true;
					if (this.scrollGallery.onItemOpen != null)
					{
						this.scrollGallery.onItemOpen(this.targetTrans.gameObject, this.dataSource);
					}
				}
				if (refreshPosition)
				{
					this.targetTrans.localScale = this.scale;
					this.targetTrans.position = this.worldPosition;
					this.targetTrans.sizeDelta = this.sizeDelta;
				}
				if (refreshContent)
				{
					if (this.scrollGallery.onItemRefresh != null)
					{
						this.scrollGallery.onItemRefresh(this.targetTrans.gameObject, this.dataSource, this.isSelected);
					}
				}
			}
			else
			{
				if (this.targetTrans != null)
				{
					//离开视野
					if (this.scrollGallery.onItemClose != null)
					{
						this.scrollGallery.onItemClose(this.targetTrans.gameObject, this.dataSource);
					}
					scrollGallery.objectPool.Recycle(this.targetTrans.gameObject);
					this.targetTrans = null;
				}
			}
		}

	}
}