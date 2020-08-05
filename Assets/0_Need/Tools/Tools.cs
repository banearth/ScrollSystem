using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BanSupport
{

	public static partial class Tools
	{

		#region 时间相关

		public static System.Diagnostics.Stopwatch StartWatch()
		{
			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
			stopwatch.Start();
			return stopwatch;
		}

		public static double StopWatch(System.Diagnostics.Stopwatch watch)
		{
			watch.Stop();
			return watch.Elapsed.TotalMilliseconds;
		}


		/// <summary>
		/// 把总秒转化为时分秒
		/// </summary>
		public static void TransSecondsToHoursMinutesSeconds(uint totalSeconds, out uint hours, out uint minutes, out uint seconds)
		{
			seconds = totalSeconds % 60;
			totalSeconds /= 60;
			minutes = totalSeconds % 60;
			totalSeconds /= 60;
			hours = totalSeconds;
		}

		/// <summary>
		/// 以秒作为单位
		/// </summary>
		public static long GetUnixStamp()
		{
			return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
		}

		/// <summary>
		/// 到每天的几点多少秒
		/// </summary>
		public static uint GetSecondsFromWantHour(int wantHour)
		{
			var now = System.DateTime.Now;
			var year = now.Year;
			var month = now.Month;
			var day = now.Day;
			var wantTime = new System.DateTime(year, month, day, wantHour, 0, 0);
			var offset = (wantTime - now).Ticks;
			Debug.Log("offset :" + offset);
			if (offset >= 0)
			{
				return (uint)(offset / 10000000);
			}
			else
			{
				return (uint)(offset / 10000000 + (24 * 60 * 60));
			}
		}

		#endregion

		#region UI坐标相关

		//WorldPos：世界坐标
		//UIPos：在UI层的世界坐标
		//ScreenPos：屏幕坐标
		//AnchoredPos：锚点坐标
		
		public static Vector3 GetUIPosByScreenPos(Vector2 screenPos, Canvas canvas)
		{
			Vector3 UIPos;
			RectTransformUtility.ScreenPointToWorldPointInRectangle(
			canvas.transform as RectTransform,
			screenPos,
			canvas.worldCamera,
			out UIPos);
			return UIPos;
		}
		
		public static Vector3 GetUIPosByWorldPos(Vector3 worldPos, Canvas canvas, Camera worldCamera)
		{
			Vector3 screenPos = worldCamera.WorldToScreenPoint(worldPos);
			return GetUIPosByScreenPos(screenPos, canvas);
		}

		public static Vector3 GetUIPosByAnchoredPos(RectTransform rectTransform, Vector2 anchoredPosition, Vector2 anchors)
		{
			return new Vector3(
				((-rectTransform.pivot.x + anchors.x) * rectTransform.rect.width + anchoredPosition.x) * rectTransform.lossyScale.x + rectTransform.position.x,
				((1 - rectTransform.pivot.y - (1 - anchors.y)) * rectTransform.rect.height + anchoredPosition.y) * rectTransform.lossyScale.y + rectTransform.position.y,
				rectTransform.position.z);
		}
		
		public static Vector2 GetAnchoredPosByUIPos(RectTransform rectTransform, Vector3 uiPos, Vector2 anchors)
		{
			return new Vector2(
				(uiPos.x - rectTransform.position.x) / rectTransform.lossyScale.x + (rectTransform.pivot.x - anchors.x) * rectTransform.rect.width,
				(uiPos.y - rectTransform.position.y) / rectTransform.lossyScale.y + (rectTransform.pivot.y - anchors.y) * rectTransform.rect.height);
		}

		#endregion

		#region 2D计算相关

		/// <summary>
		/// 是否使用弧度制，默认不使用
		/// angle是否在from到to的这段
		/// </summary>
		public static bool IsBetweenAngle(float angle, float from, float to, bool useArcOrNot = false)
		{
			FormatAngle(from, ref to, useArcOrNot);
			FormatAngle(from, ref angle, useArcOrNot);
			return angle >= from && angle <= to;
		}

		/// <summary>
		/// 根据From来标准化To，To永远只会比From大，但是不会超过一圈
		/// </summary>
		public static void FormatAngle(float from, ref float to, bool useArcOrNot = false)
		{
			float addAmount;
			if (useArcOrNot)
			{
				addAmount = 2 * Mathf.PI;
			}
			else
			{
				addAmount = 360;
			}
			var minus = (to - from) / addAmount;
			if (minus < 0)
			{
				to += Mathf.CeilToInt(-minus) * addAmount;
			}
			else if (minus > 1)
			{
				to -= (int)minus * addAmount;
			}
		}

		public static float GetAngle2D(Vector3 center, Vector3 target)
		{
			return GetAngle2D(center.x, center.y, target.x, target.y);
		}

		public static float GetAngle2D(float centerX, float centerY, float targetX, float targetY)
		{
			var returnAngle = Mathf.Atan2(targetX - centerX, targetY - centerY) * 180 / Mathf.PI;
			if (returnAngle < 0) { returnAngle += 360; }
			return returnAngle;
		}

		/// <summary>
		/// 获得2D线段的长度
		/// </summary>
		public static float GetLineMagnitude2D(float x1, float y1, float x2, float y2)
		{
			return Mathf.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
		}

		/// <summary>
		/// 获得2D环境下围绕点(x1,y1)旋转，使得跟随该点旋转的点(x2,y2)向量(a,b)，可以指向目标点(x3,y3)所需的旋转角度
		/// </summary>
		public static float GetRotateShootAngle2D(float x1, float y1, float x2, float y2, float a, float b, float x3, float y3)
		{
			float rotateAngle = 0;
			float crossX, crossY;
			GetPointVerticalCrossToLine2D(x1, y1, x2, y2, a, b, out crossX, out crossY);
			float dist1 = GetLineMagnitude2D(crossX, crossY, x1, y1);
			float dist2 = GetLineMagnitude2D(x1, y1, x3, y3);
			var angle1 = GetTriangleAngleAB_AC(GetLineMagnitude2D(crossX, crossY, x3, y3), dist1, dist2);
			if (dist1 < dist2)
			{
				var angle2 = Mathf.Acos(dist1 / dist2) * 180 / Mathf.PI;
				int leftOrRight = GetPointPlaceLineLeftOrRight2D(x1, y1, crossX, crossY, a, b);
				rotateAngle = leftOrRight * (angle1 - angle2);
			}
			else
			{
				int leftOrRight = GetPointPlaceLineLeftOrRight2D(x3, y3, x1, y1, crossX - x1, crossY - y1);
				rotateAngle = leftOrRight * angle1;
			}
			return rotateAngle;
		}

		/// <summary>
		/// 获得2D环境下点(x,y)距离直线的(x1,y1) 向量(a,b)的距离
		/// </summary>
		public static float GetPointDistanceToLine2D(float x, float y, float x1, float y1, float a, float b)
		{
			return (b * x - a * y + a * y1 - b * x1) / Mathf.Sqrt(a * a + b * b);
		}

		/// <summary>
		/// 获得2D环境下点(x,y)距离直线的(x1,y1) 向量(a,b)左侧还是右侧
		/// </summary>
		public static int GetPointPlaceLineLeftOrRight2D(float x, float y, float x1, float y1, float a, float b)
		{
			return (int)Mathf.Sign(b * x - a * y + a * y1 - b * x1);
		}

		/// <summary>
		/// 获得2D环境下的，点(x,y)到直线(x1,y1)向量(a,b)的垂线交点坐标
		/// </summary>
		public static void GetPointVerticalCrossToLine2D(float x, float y, float x1, float y1, float a, float b, out float crossX, out float crossY)
		{
			//float c = a * y2 - b * x2;
			//crossX = (a * a * x1 + a * b * y1 - b * c) / (a * a + b * b);
			//crossY = (b * crossX + c) / a;
			//第二种方法
			var distance = GetPointDistanceToLine2D(x, y, x1, y1, a, b);
			var c = Mathf.Sqrt(a * a + b * b);
			crossX = x + -b / c * distance;
			crossY = y + a / c * distance;
		}

		/// <summary>
		/// 获得直线与圆的相交点
		/// </summary>
		public static Vector2 GetLineCrossCirclePos2D(float x1, float y1, float a_by_c, float b_by_c, float radius, float circleX, float circleY)
		{
			float crossX, crossY;
			GetPointVerticalCrossToLine2D(circleX, circleY, x1, y1, a_by_c, b_by_c, out crossX, out crossY);
			var distance = Mathf.Sqrt(radius * radius - (crossX - circleX) * (crossX - circleX) - (crossY - circleY) * (crossY - circleY));
			return new Vector2(crossX + a_by_c * distance, crossY + b_by_c * distance);
		}

		#endregion

		#region Transform Vector Quaternion相关

		/// <summary>
		/// 过该点并且与平面XZ相垂直的相交点
		/// </summary>
		public static bool GetPosFromLineCrossPlane(Vector3 pos, Transform planeTrans, out Vector3 crossPos)
		{
			return GetPosFromLineCrossPlane(pos, planeTrans.up, planeTrans.position, planeTrans.up, out crossPos);
		}

		/// <summary>
		/// 点与平面的垂直点
		/// </summary>
		public static bool GetPosFromLineCrossPlane(Vector3 linePos, Vector3 lineDir, Vector3 planePos, Vector3 planeNormal, out Vector3 crossPos)
		{
			float a, b, c;
			float a2, b2, c2;
			float a3, b3, c3;
			float a4, b4, c4;
			a = linePos.x;
			b = linePos.y;
			c = linePos.z;
			a2 = lineDir.x;
			b2 = lineDir.y;
			c2 = lineDir.z;
			a3 = planePos.x;
			b3 = planePos.y;
			c3 = planePos.z;
			a4 = planeNormal.x;
			b4 = planeNormal.y;
			c4 = planeNormal.z;
			float param = a2 * a4 + b2 * b4 + c2 * c4;
			if (param > 0 || param > 0)
			{
				float n = (-a * a4 + a3 * a4 - b * b4 + b3 * b4 - c * c4 + c3 * c4) / param;
				crossPos = linePos + lineDir * n;
				return true;
			}
			else
			{
				crossPos = Vector3.zero;
				return false;
			}
		}

		/// <summary>
		/// 获得为了让子物体curTrans保持在某角度时所需的父物体targetTrans角度
		/// </summary>
		public static Quaternion GetInvserseRotation(Transform curTrans, Transform targetTrans)
		{
			var returnRotation = Quaternion.identity;
			while (curTrans != targetTrans)
			{
				returnRotation = returnRotation * Quaternion.Inverse(curTrans.localRotation);
				curTrans = curTrans.parent;
			}
			return returnRotation;
		}

		/// <summary>
		/// 在XZ平面下返回一个从Dir2旋转到Dir1所需的旋转角度
		/// </summary>
		/// <param name="dir1">目标角度</param>
		/// <param name="dir2">当前角度</param>
		/// <returns></returns>
		public static bool GetRotateAngleFromDir2ToDir1XZ(Vector3 dir1, Vector3 dir2, out float angle)
		{
			dir1.y = 0;
			dir2.y = 0;
			if (Vector3.SqrMagnitude(dir1 - dir2) > 0)
			{
				angle = Vector3.Angle(dir1, dir2) * (Vector3.Cross(dir1, dir2).y > 0 ? -1 : 1);
				return true;
			}
			else
			{
				angle = 0;
				return false;
			}
		}

		/// <summary>
		/// 得到三角形AB,AC的夹角
		/// </summary>
		public static float GetTriangleAngleAB_AC(Vector3 A, Vector3 B, Vector3 C)
		{
			return GetTriangleAngleAB_AC(Vector3.Distance(B, C), Vector3.Distance(A, C), Vector3.Distance(A, B));
		}

		/// <summary>
		/// 得到三角形AB,AC的夹角
		/// </summary>
		public static float GetTriangleAngleAB_AC(float a, float b, float c)
		{
			var cosValue = (b * b + c * c - a * a) / (2 * b * c);
			if (cosValue >= 1)
			{
				return 0;
			}
			else
			{
				return Mathf.Acos(cosValue) * 180 / Mathf.PI;
			}
		}

		/// <summary>
		/// 根据摇杆的输入值以及当前摄像机的位置方向，得到理想的世界方向
		/// </summary>
		public static Vector3 GetWorldDir(Vector2 v2)
		{
			return GetWorldDir(v2.x, v2.y);
		}

		/// <summary>
		/// 根据摇杆的输入值以及主摄像机的位置方向，得到理想的世界方向
		/// </summary>
		/// <param name="x">摇杆水平值</param>
		/// <param name="y">摇杆垂直值</param>
		/// <returns></returns>
		public static Vector3 GetWorldDir(float x, float y)
		{
			var forwardDir = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1));
			var rightDir = Vector3.Cross(forwardDir, Vector3.down);
			var wantDir = forwardDir * y + rightDir * x;
			return wantDir.normalized;
		}

		#endregion

		#region Draw相关

		/// <summary>
		/// Gizmos画出一个BoxCollider
		/// </summary>
		public static void GizmosDrawBoxCollider(BoxCollider boxCollider)
		{
			var transform = boxCollider.transform;
			var oldMatrix = Gizmos.matrix;
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, (transform.lossyScale));
			Gizmos.DrawCube(Vector3.zero + boxCollider.center, boxCollider.size);
			Gizmos.matrix = oldMatrix;
		}

		/// <summary>
		/// Gizmos画出一个BoxCollider
		/// </summary>
		public static void GizmosDrawBoxCollider(BoxCollider boxCollider, Color color)
		{
			var transform = boxCollider.transform;
			var oldColor = Gizmos.color;
			var oldMatrix = Gizmos.matrix;
			Gizmos.color = color;
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, (transform.lossyScale));
			Gizmos.DrawCube(Vector3.zero + boxCollider.center, boxCollider.size);
			Gizmos.matrix = oldMatrix;
			Gizmos.color = oldColor;
		}

		/// <summary>
		/// Gizmos画出一个交叉点
		/// </summary>
		public static void GizmosDrawCrossPoint(Vector3 pos, float size, Color color)
		{
			var oldColor = Gizmos.color;
			Gizmos.color = color;
			Gizmos.DrawLine(pos + Vector3.left * size, pos + Vector3.right * size);
			Gizmos.DrawLine(pos + Vector3.up * size, pos + Vector3.down * size);
			Gizmos.DrawLine(pos + Vector3.forward * size, pos + Vector3.back * size);
			Gizmos.color = oldColor;
		}

		/// <summary>
		/// 用于绘制角色骨骼信息
		/// </summary>
		public static void GizmosDrawBone(Transform rootTrans, List<Transform> ignoreTrans = null)
		{
			var trans = rootTrans.GetComponentsInChildren<Transform>();
			for (int i = 0; i < trans.Length; i++)
			{
				if (ignoreTrans != null && (ignoreTrans.Contains(trans[i]) || ignoreTrans.Contains(trans[i].parent)))
				{
					continue;
				}
				//Debug.Log(trans[i].name + " :" + trans[i].parent);
				GizmosDrawRectangle(trans[i].position, trans[i].parent.position);
			}
		}

		/// <summary>
		/// 画出一个长方体，以pos1 pos2作为首尾点，默认为红色
		/// </summary>
		public static void GizmosDrawRectangle(Vector3 pos1, Vector3 pos2, float radius = 0.02f)
		{
			var oldMatrix = Gizmos.matrix;
			var oldColor = Gizmos.color;
			Gizmos.color = new Color(1, 0, 0, 0.5f);
			var distance = Vector3.Distance(pos1, pos2);
			if (distance > 0.001f)
			{
				Gizmos.matrix = Matrix4x4.TRS((pos1 + pos2) / 2, Quaternion.LookRotation(pos2 - pos1), Vector3.one);
				Gizmos.DrawCube(Vector3.zero, new Vector3(radius, radius, distance));
			}
			Gizmos.matrix = oldMatrix;
			Gizmos.color = oldColor;
		}

		/// <summary>
		/// Debug.DrawLine画出一个交叉点
		/// </summary>
		public static void DrawCrossPoint(Vector3 pos, float size, Color color, float during = 0)
		{
			if (during < 0)
			{
				Debug.DrawLine(pos + Vector3.left * size, pos + Vector3.right * size, color);
				Debug.DrawLine(pos + Vector3.up * size, pos + Vector3.down * size, color);
				Debug.DrawLine(pos + Vector3.forward * size, pos + Vector3.back * size, color);
			}
			else
			{
				Debug.DrawLine(pos + Vector3.left * size, pos + Vector3.right * size, color, during);
				Debug.DrawLine(pos + Vector3.up * size, pos + Vector3.down * size, color, during);
				Debug.DrawLine(pos + Vector3.forward * size, pos + Vector3.back * size, color, during);
			}
		}

		#endregion

		#region String操作

		/// <summary>
		/// 返回带颜色的String
		/// </summary>
		public static string GetRichStr(string str, Color color)
		{
			var colorStr = ColorUtility.ToHtmlStringRGB(color);
			return string.Format("<color=#{0}>{1}</color>", colorStr, str);
		}

		/// <summary>
		/// 返回一个str的repeat
		/// </summary>
		public static string GetRepeatStr(string str, int repeatCount)
		{
			string resultStr = "";
			for (int i = 0; i < repeatCount; i++)
			{
				resultStr += str;
			}
			return resultStr;
		}

		/// <summary>
		/// 用于在Debug中使用
		/// color 必须为 #000000 格式
		/// </summary>
		public static string GetRichText(string str, string color)
		{
			return string.Format("<color={0}>{1}</color>", color, str);
		}

		/// <summary>
		/// 得到一个uint数组的字符串
		/// </summary>
		public static string GetArrayText(uint[] array)
		{
			if (array == null) { return "null"; }
			string returnStr = "[";
			for (int i = 0; i < array.Length; i++)
			{
				returnStr += array[i] + ",";
			}
			if (array.Length > 0)
			{
				returnStr = returnStr.Substring(0, returnStr.Length - 1);
			}
			return returnStr + "]";
		}

		#endregion

		#region WorldText

		private static Transform _WorldTextTrans;

		private static Transform WorldTextTrans {
			get {
				if (_WorldTextTrans == null) {
					_WorldTextTrans = new GameObject("WorldText").transform;
				}
				return _WorldTextTrans;
			}
		}

		public static TextMesh CreateWorldText(string str, Vector3 position, int fontSize = 40)
		{
			var textMesh = new GameObject(WorldTextTrans.childCount.ToString()).AddComponent<TextMesh>();
			textMesh.anchor = TextAnchor.MiddleCenter;
			textMesh.transform.SetParent(WorldTextTrans);
			textMesh.transform.position = position;
			textMesh.fontSize = fontSize;
			textMesh.text = str;
			return textMesh;
		}

		#endregion

		#region Others

		public static T AddComponentIfNotExist<T>(GameObject go) where T : Component
		{
			var t = go.GetComponent<T>();
			if (t == null)
			{
				t = go.AddComponent<T>();
			}
			return t;
		}

		public static T AddComponentIfNotExist<T>(GameObject go, out bool isNew) where T : Component
		{
			var t = go.GetComponent<T>();
			if (t == null)
			{
				t = go.AddComponent<T>();
				isNew = true;
			}
			else
			{
				isNew = false;
			}
			return t;
		}

		/// <summary>
		/// Vector3直接打印出来精度不高，故使用本方法
		/// </summary>
		public static void PrintVector3(Vector3 v3)
		{
			Debug.Log(v3.x.ToString("0.00000") + " y:" + v3.y.ToString("0.00000") + " z:" + v3.z.ToString("0.00000"));
		}

		#endregion

		#region Scene

		public static void ReloadScene()
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}

		public static void LoadScene(string sceneName)
		{
			SceneManager.LoadScene(sceneName);
		}

		#endregion


		public static GameObject CreateObj(GameObject prefab, Transform trans)
		{
			return GameObject.Instantiate<GameObject>(prefab, trans.position, Quaternion.identity, trans);
		}

		public static GameObject CreateObj(GameObject prefab, Transform trans, Vector3 position)
		{
			return GameObject.Instantiate<GameObject>(prefab, position, Quaternion.identity, trans);
		}

		public static Vector3 GetRandomCirclePos(Vector3 originPos, float maxRadius)
		{
			var random = UnityEngine.Random.insideUnitCircle;
			var finalPos = new Vector3(random.x * maxRadius, 0, random.y * maxRadius) + originPos;
			return finalPos;
		}

		public static Vector3 GetRandomCirclePos(Vector3 originPos, float minRadius, float maxRadius)
		{
			float radius = UnityEngine.Random.Range(minRadius, maxRadius);
			float angle = UnityEngine.Random.Range(0, Mathf.PI * 2);
			var finalPos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius + originPos;
			return finalPos;
		}

		public static void GetDeviceWidthAndHeight(out int width, out int height)
		{
#if UNITY_EDITOR
			System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
			System.Reflection.MethodInfo GetMainGameView = T.GetMethod("GetMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			object Res = GetMainGameView.Invoke(null, null);
			var gameView = (UnityEditor.EditorWindow)Res;
			var prop = gameView.GetType().GetProperty("currentGameViewSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var gvsize = prop.GetValue(gameView, new object[0] { });
			var gvSizeType = gvsize.GetType();
			height = (int)gvSizeType.GetProperty("height", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(gvsize, new object[0] { });
			width = (int)gvSizeType.GetProperty("width", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(gvsize, new object[0] { });
#else
			width = Screen.width;
			height = Screen.height;
#endif
		}

	}

}