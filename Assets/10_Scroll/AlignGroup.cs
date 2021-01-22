using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BanSupport
{
	public class AlignGroup
	{
		public ScrollSystem scrollSystem;
		public Vector2 cursorPos;
		public float maxHeight;
		public float maxWidth;
		public float oldMaxWidth;
		public RectBounds rectBounds;
		public List<ScrollData> listData;
		public Func<Vector2> getCenterOffset;

		public Dictionary<object, ScrollData> dic_DataSource_ScrollData = new Dictionary<object, ScrollData>();
		private List<ScrollData> listVisibleScrollData = new List<ScrollData>(8);
		private List<ScrollData> listNextVisibleScrollData = new List<ScrollData>(8);

		public float Width
		{
			get
			{
				return rectBounds.Width;
			}
		}

		public float Height
		{
			get
			{
				return rectBounds.Height;
			}
		}

		public AlignGroup(ScrollSystem scrollSystem, int index)
		{
			this.scrollSystem = scrollSystem;
			this.rectBounds = this.scrollSystem.GetGroupRectBounds(index);
			this.listData = new List<ScrollData>();
			if (scrollSystem.ScrollDirection_GET == ScrollSystem.ScrollDirection.Vertical)
			{
				getCenterOffset = () => { return new Vector2((Width - scrollSystem.border.x - maxWidth) / 2, 0); };
			}
			else if (scrollSystem.ScrollDirection_GET == ScrollSystem.ScrollDirection.Horizontal)
			{
				getCenterOffset = () => { return new Vector2(0, (Height - scrollSystem.border.y - maxHeight) / 2); };
			}
			InitCursor();
		}

		public void InitCursor()
		{
			this.cursorPos = new Vector2(scrollSystem.Border.x, scrollSystem.Border.y);
			this.maxHeight = 0;
			this.maxWidth = 0;
			this.oldMaxWidth = 0;
		}
	}

}