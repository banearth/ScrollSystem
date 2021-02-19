using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{

	/// <summary>
	/// 用于表示一个矩形区域的四个边界的世界坐标
	/// </summary>
	public struct RectBounds
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

		public RectBounds(float left, float right, float up, float down)
		{
			this.left = left;
			this.right = right;
			this.up = up;
			this.down = down;
		}

		public RectBounds(RectTransform rectTransform) :this(
			rectTransform.pivot,
			rectTransform.lossyScale,
			rectTransform.rect.width,
			rectTransform.rect.height,
			rectTransform.position){ }

		public RectBounds(Vector2 pivot, Vector3 lossyScale, float pixelWidth, float pixelHeight, Vector3 worldPosition)
		{
			float width = pixelWidth * lossyScale.x;
			float height = pixelHeight * lossyScale.y;
			this.left = worldPosition.x + width * (-pivot.x);
			this.right = worldPosition.x + width * (1 - pivot.x);
			this.up = worldPosition.y + height * (1 - pivot.y);
			this.down = worldPosition.y + height * (-pivot.y);
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
			var rectBounds = new RectBounds(rectTransform);
			Encapsulate(rectBounds.LeftUpPos);
			Encapsulate(rectBounds.RightDownPos);
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

		public override string ToString()
		{
			return "left:" + left + " right:" + right + " up:" + up + " down:" + down;
		}

	}

	partial class Tools
	{

		public static void DrawRect(Vector3 worldPos, float worldWidth, float worldHeight, Color color)
		{
			DrawRect(GetRectBounds(worldPos, worldWidth, worldHeight), worldPos.z, color);
		}

		public static void DrawRect(RectBounds rectBounds, float worldPosZ, Color color)
		{
			Vector3 leftUpPos = rectBounds.LeftUpPos;
			Vector3 rightUpPos = rectBounds.RightUpPos;
			Vector3 rightDownPos = rectBounds.RightDownPos;
			Vector3 leftDownPos = rectBounds.LeftDownPos;
			leftUpPos.z = worldPosZ;
			rightUpPos.z = worldPosZ;
			rightDownPos.z = worldPosZ;
			leftDownPos.z = worldPosZ;
			Debug.DrawLine(leftUpPos, rightUpPos, color);
			Debug.DrawLine(rightUpPos, rightDownPos, color);
			Debug.DrawLine(rightDownPos, leftDownPos, color);
			Debug.DrawLine(leftDownPos, leftUpPos, color);
		}

		public static void DrawRect(RectTransform rectTransform, Color color)
		{
			var rectBounds = GetRectBounds(rectTransform, out float worldPosZ);
			DrawRect(rectBounds, worldPosZ, color);
		}

		public static RectBounds GetRectBounds(RectTransform rectTransform,out float worldPosZ)
		{
			var pivot = rectTransform.pivot;
			var width = rectTransform.rect.width * rectTransform.lossyScale.x;
			var height = rectTransform.rect.height * rectTransform.lossyScale.y;
			var rectBounds = new RectBounds();
			rectBounds.left = rectTransform.transform.position.x + width * (-pivot.x);
			rectBounds.right = rectTransform.transform.position.x + width * (1 - pivot.x);
			rectBounds.up = rectTransform.transform.position.y + height * (1 - pivot.y);
			rectBounds.down = rectTransform.transform.position.y + height * (-pivot.y);
			worldPosZ = rectTransform.position.z;
			return rectBounds;
		}

		public static RectBounds GetRectBounds(Vector2 worldPos, float worldWidth, float worldHeight)
		{
			var rectBounds = new RectBounds();
			rectBounds.left = worldPos.x - worldWidth / 2;
			rectBounds.right = worldPos.x + worldWidth / 2;
			rectBounds.up = worldPos.y + worldHeight / 2;
			rectBounds.up = worldPos.y - worldHeight / 2;
			return rectBounds;
		}

	}

}

//public static RectBounds GetRectBounds(Vector2 pivot, Vector3 lossyScale, float width, float height, Vector3 worldPosition)
//{
//	width = width * lossyScale.x;
//	height = height * lossyScale.y;
//	var rectBounds = new RectBounds();
//	rectBounds.left = worldPosition.x + width * (-pivot.x);
//	rectBounds.right = worldPosition.x + width * (1 - pivot.x);
//	rectBounds.up = worldPosition.y + height * (1 - pivot.y);
//	rectBounds.down = worldPosition.y + height * (-pivot.y);
//	return rectBounds;
//}