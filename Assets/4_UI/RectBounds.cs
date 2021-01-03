using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

		#region All Attributes

		public float Width
		{
			get
			{
				return right - left;
			}
			set
			{
				var x = (left + right) / 2;
				left = x - value / 2;
				right = x + value / 2;
			}
		}

		public float Height
		{
			get
			{
				return Mathf.Max(0, up - down);
			}
			set
			{
				var y = (up + down) / 2;
				up = y + value / 2;
				down = y - value / 2;
			}
		}

		public Vector2 Size
		{
			get
			{
				return new Vector2(Width, Height);
			}
		}

		public Vector2 LeftUpPos
		{
			get
			{
				return new Vector2(left, up);
			}
		}

		public Vector2 LeftDownPos
		{
			get
			{
				return new Vector2(left, down);
			}
		}

		public Vector2 RightUpPos
		{
			get
			{
				return new Vector2(right, up);
			}
		}

		public Vector2 RightDownPos
		{
			get
			{
				return new Vector2(right, down);
			}
		}

		public Vector2 MiddleUpPos
		{
			get
			{
				return new Vector2((left + right) / 2, up);
			}
		}

		public Vector2 MiddleDownPos
		{
			get
			{
				return new Vector2((left + right) / 2, down);
			}
		}

		public Vector2 LeftMiddlePos
		{
			get
			{
				return new Vector2(left, (up + down) / 2);
			}
		}

		public Vector2 RightMiddlePos
		{
			get
			{
				return new Vector2(right, (up + down) / 2);
			}
		}

		public Vector2 Position
		{
			get
			{
				return new Vector2((left + right) / 2, (up + down) / 2);
			}
			set
			{
				var x = (left + right) / 2;
				var y = (up + down) / 2;
				var newX = value.x;
				var newY = value.y;
				var offsetX = newX - x;
				var offsetY = newY - y;
				left += offsetX;
				right += offsetX;
				up += offsetY;
				down += offsetY;
			}
		}

		#endregion

		public RectBounds(RectTransform rectTransform):
			this(rectTransform.pivot,
				rectTransform.lossyScale,
				rectTransform.rect.width,
				rectTransform.rect.height,
				rectTransform.position){}

		public RectBounds(float left, float right, float up, float down)
		{
			this.left = left;
			this.right = right;
			this.up = up;
			this.down = down;
		}

		public RectBounds(Vector2 pivot, Vector3 lossyScale, float pixelWidth, float pixelHeight, Vector3 worldPosition)
		{
			float width =  pixelWidth* lossyScale.x;
			float height = pixelHeight * lossyScale.y;
			this.left = worldPosition.x + width * (-pivot.x);
			this.right = worldPosition.x + width * (1 - pivot.x);
			this.up = worldPosition.y + height * (1 - pivot.y);
			this.down = worldPosition.y + height * (-pivot.y);
		}

		public RectBounds(Vector3 worldPos, float width, float height)
		{
			this.left = worldPos.x - width / 2;
			this.right = worldPos.x + width / 2;
			this.up = worldPos.y + height / 2;
			this.down = worldPos.y - height / 2;
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

		public void Encapsulate(RectBounds other)
		{
			Encapsulate(other.LeftUpPos);
			Encapsulate(other.RightDownPos);
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

		public void ForeachAllPos(Action<Vector3> func)
		{
			func(LeftUpPos);
			func(RightUpPos);
			func(RightDownPos);
			func(LeftDownPos);
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

		/// <summary>
		/// 使用Debug画方块
		/// </summary>
		public void Draw(Color color)
		{
			Debug.DrawLine(this.LeftUpPos, this.RightUpPos, color);
			Debug.DrawLine(this.LeftDownPos, this.RightDownPos, color);
			Debug.DrawLine(this.LeftUpPos, this.LeftDownPos, color);
			Debug.DrawLine(this.RightUpPos, this.RightDownPos, color);
		}

		public void Draw(Color color, float z)
		{
			var pos1 = new Vector3(this.LeftUpPos.x, this.LeftUpPos.y, z);
			var pos2 = new Vector3(this.RightUpPos.x, this.RightUpPos.y, z);
			var pos3 = new Vector3(this.RightDownPos.x, this.RightDownPos.y, z);
			var pos4 = new Vector3(this.LeftDownPos.x, this.LeftDownPos.y, z);
			Debug.DrawLine(pos1, pos2, color);
			Debug.DrawLine(pos2, pos3, color);
			Debug.DrawLine(pos3, pos4, color);
			Debug.DrawLine(pos4, pos1, color);
		}

	}

}








//public static RectBounds GetRectBounds(Vector2 pivot, Vector3 lossyScale, float width, float height, Vector3 worldPosition)
//{
//	width = width * lossyScale.x;
//	height = height * lossyScale.y;
//	var rectRange = new RectBounds();
//	rectRange.left = worldPosition.x + width * (-pivot.x);
//	rectRange.right = worldPosition.x + width * (1 - pivot.x);
//	rectRange.up = worldPosition.y + height * (1 - pivot.y);
//	rectRange.down = worldPosition.y + height * (-pivot.y);
//	return rectRange;
//}



//public static RectBounds GetRectBounds(RectTransform rectTransform)
//{
//	var pivot = rectTransform.pivot;
//	var width = rectTransform.rect.width * rectTransform.lossyScale.x;
//	var height = rectTransform.rect.height * rectTransform.lossyScale.y;
//	var rectRange = new RectBounds();
//	rectRange.left = rectTransform.transform.position.x + width * (-pivot.x);
//	rectRange.right = rectTransform.transform.position.x + width * (1 - pivot.x);
//	rectRange.up = rectTransform.transform.position.y + height * (1 - pivot.y);
//	rectRange.down = rectTransform.transform.position.y + height * (-pivot.y);
//	return rectRange;
//}


///// <summary>
///// 使用Debug画方块
///// </summary>
////public static void DrawRectRange(RectBounds rectRange,float z,Color color)
////{
////	var allPos = rectRange.GetAllPos(z);
////	Debug.DrawLine(allPos[0], allPos[1], color);
////	Debug.DrawLine(allPos[1], allPos[3], color);
////	Debug.DrawLine(allPos[2], allPos[3], color);
////	Debug.DrawLine(allPos[0], allPos[2], color);
////}

///// <summary>
///// 获得RectTransform的边界，用一个来接收
///// </summary>
//public static RectBounds GetRectBounds(RectTransform rectTransform)
//{
//	var pivot = rectTransform.pivot;
//	var width = rectTransform.rect.width * rectTransform.lossyScale.x;
//	var height = rectTransform.rect.height * rectTransform.lossyScale.y;
//	var rectRange = new RectBounds();
//	rectRange.left = rectTransform.transform.position.x + width * (-pivot.x);
//	rectRange.right = rectTransform.transform.position.x + width * (1 - pivot.x);
//	rectRange.up = rectTransform.transform.position.y + height * (1 - pivot.y);
//	rectRange.down = rectTransform.transform.position.y + height * (-pivot.y);
//	return rectRange;
//}


//public static RectBounds GetRectBounds(Vector2 pivot, Vector3 lossyScale, float width, float height, Vector3 worldPosition)
//{
//	width = width * lossyScale.x;
//	height = height * lossyScale.y;
//	var rectRange = new RectBounds();
//	rectRange.left = worldPosition.x + width * (-pivot.x);
//	rectRange.right = worldPosition.x + width * (1 - pivot.x);
//	rectRange.up = worldPosition.y + height * (1 - pivot.y);
//	rectRange.down = worldPosition.y + height * (-pivot.y);
//	return rectRange;
//}



///// <summary>
///// Width和Height都是世界距离，不是像素距离
///// </summary>
//public static void DrawRectBounds(Vector3 pos, float width, float height, Color color)
//{
//	var leftUpPos = pos + Vector3.left * width / 2 + Vector3.up * height / 2;
//	var rightUpPos = pos + Vector3.right * width / 2 + Vector3.up * height / 2;
//	var leftDownPos = pos + Vector3.left * width / 2 + Vector3.down * height / 2;
//	var rightDownPos = pos + Vector3.right * width / 2 + Vector3.down * height / 2;
//	Debug.DrawLine(leftUpPos, rightUpPos, color);
//	Debug.DrawLine(leftDownPos, rightDownPos, color);
//	Debug.DrawLine(leftUpPos, leftDownPos, color);
//	Debug.DrawLine(rightUpPos, rightDownPos, color);
//}
