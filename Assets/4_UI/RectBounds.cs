using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{

	/// <summary>
	/// 用于表示一个矩形区域的四个边界的世界坐标
	/// </summary>
	public class RectBounds
	{
		//左边界的位置
		public float left;
		//右边界的位置（更大的那个）
		public float right;
		//上边界的位置（更大的那个）
		public float up;
		//下边界的位置
		public float down;

		public float width { get { return Mathf.Max(0, right - left); } }

		public float height { get { return Mathf.Max(0, up - down); } }

		public Vector2 size { get { return new Vector2(width, height); } }

		public Vector2 center { get { return new Vector2((left + right) / 2, (up + down) / 2); } }

		public RectBounds() { }

		public RectBounds(RectTransform rectTransfrom)
		{
			ParseRectTransform(rectTransfrom,out left,out right,out up,out down);
		}

		public bool Overlaps(RectBounds other)
		{
			if (this.right >= other.left &&
				this.left <= other.right &&
				this.down <= other.up &&
				this.up >= other.down)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// 扩展使得自身可以包含某个点
		/// </summary>
		public void Encapsulate(Vector2 pos)
		{
			if (pos.x < left)
			{
				left = pos.x;
			}
			else if (pos.x > right)
			{
				right = pos.x;
			}
			if (pos.y < down)
			{
				down = pos.y;
			}
			else if (pos.y > up)
			{
				up = pos.y;
			}
		}

		public void Encapsulate(RectTransform rectTransform)
		{
			ParseRectTransform(rectTransform,out float left, out float right, out float up, out float down);
			var pos1 = new Vector2(left, up);
			var pos2 = new Vector2(right, down);
			Encapsulate(pos1);
			Encapsulate(pos2);
		}

		private void ParseRectTransform(RectTransform rectTransform, out float left, out float right, out float up, out float down)
		{
			Vector3[] v = new Vector3[4];
			rectTransform.GetWorldCorners(v);
			left = v[0].x;
			right = v[2].x;
			up = v[2].y;
			down = v[0].y;
		}

		public bool Contains(Vector2 pos)
		{
			if (pos.x >= left && pos.x <= right && pos.y <= up && pos.y >= down)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public Vector2 LeftUpPos
		{
			get
			{
				return new Vector2(left, up);
			}
		}

		public Vector2 RightUpPos
		{
			get
			{
				return new Vector2(right, up);
			}
		}

		public Vector2 LeftDownPos
		{
			get
			{
				return new Vector2(left, down);
			}
		}

		public Vector2 RightDownPos
		{
			get
			{
				return new Vector2(right, down);
			}
		}

		public Vector3[] GetAllPos(float z)
		{
			return new Vector3[] {
				new Vector3(left,up,z),
				new Vector3(right,up,z),
				new Vector3(left,down,z),
				new Vector3(right,down,z),
			};
		}

		public override string ToString()
		{
			return "left:" + left + " right:" + right + " up:" + up + " down:" + down;
		}

	}

	partial class Tools
	{

		public static void DrawRectBounds(Vector2 pos, float z, float width, float height, Color color)
		{
			DrawRectBounds(new Vector3(pos.x, pos.y, z), width, height, color);
		}

		/// <summary>
		/// Width和Height都是世界距离，不是像素距离
		/// </summary>
		public static void DrawRectBounds(Vector3 pos, float width, float height, Color color)
		{
			var leftUpPos = pos + Vector3.left * width / 2 + Vector3.up * height / 2;
			var rightUpPos = pos + Vector3.right * width / 2 + Vector3.up * height / 2;
			var leftDownPos = pos + Vector3.left * width / 2 + Vector3.down * height / 2;
			var rightDownPos = pos + Vector3.right * width / 2 + Vector3.down * height / 2;
			Debug.DrawLine(leftUpPos, rightUpPos, color);
			Debug.DrawLine(leftDownPos, rightDownPos, color);
			Debug.DrawLine(leftUpPos, leftDownPos, color);
			Debug.DrawLine(rightUpPos, rightDownPos, color);
		}

		/// <summary>
		/// 使用Debug画方块
		/// </summary>
		public static void DrawRectRange(RectBounds rectBounds, Color color)
		{
			Debug.DrawLine(rectBounds.LeftUpPos, rectBounds.RightUpPos, color);
			Debug.DrawLine(rectBounds.LeftDownPos, rectBounds.RightDownPos, color);
			Debug.DrawLine(rectBounds.LeftUpPos, rectBounds.LeftDownPos, color);
			Debug.DrawLine(rectBounds.RightUpPos, rectBounds.RightDownPos, color);
		}

		/// <summary>
		/// 使用Debug画方块
		/// </summary>
		public static void DrawRectRange(RectBounds rectRange,float z,Color color)
		{
			var allPos = rectRange.GetAllPos(z);
			Debug.DrawLine(allPos[0], allPos[1], color);
			Debug.DrawLine(allPos[1], allPos[3], color);
			Debug.DrawLine(allPos[2], allPos[3], color);
			Debug.DrawLine(allPos[0], allPos[2], color);
		}

		/// <summary>
		/// 获得RectTransform的边界，用一个来接收
		/// </summary>
		public static RectBounds GetRectBounds(RectTransform rectTransform,RectBounds rectRange = null)
		{
			var pivot = rectTransform.pivot;
			var width = rectTransform.rect.width * rectTransform.lossyScale.x;
			var height = rectTransform.rect.height * rectTransform.lossyScale.y;
            if (rectRange == null) { rectRange = new RectBounds(); }
            rectRange.left = rectTransform.transform.position.x + width * (-pivot.x);
            rectRange.right = rectTransform.transform.position.x + width * (1 - pivot.x);
            rectRange.up = rectTransform.transform.position.y + height * (1 - pivot.y);
            rectRange.down = rectTransform.transform.position.y + height * (-pivot.y);
			return rectRange;
		}

		public static RectBounds GetRectBounds(Vector2 pivot, Vector3 lossyScale, float width, float height,
			Vector3 worldPosition, RectBounds rectRange = null)
		{
			width = width * lossyScale.x;
			height = height * lossyScale.y;
			if (rectRange == null) { rectRange = new RectBounds(); }
			rectRange.left = worldPosition.x + width * (-pivot.x);
			rectRange.right = worldPosition.x + width * (1 - pivot.x);
			rectRange.up = worldPosition.y + height * (1 - pivot.y);
			rectRange.down = worldPosition.y + height * (-pivot.y);
			return rectRange;
		}

	}

}