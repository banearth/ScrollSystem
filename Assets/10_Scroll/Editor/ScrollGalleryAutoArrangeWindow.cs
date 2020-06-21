using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BanSupport
{
	public class ScrollGalleryAutoArrangeWindow : EditorWindow
	{
		private ScrollGallery scrollGallery;

		private float spacing = 10;
		private Vector2 scaleDelta = new Vector2(-0.1f, -0.1f);
		private Vector2 widthAndHeightDelta = new Vector2(-10, -10);
		private RectTransform maskTransform = null;
		public static void ShowWindow(ScrollGallery scrollGallery)
		{
			var window = GetWindow<ScrollGalleryAutoArrangeWindow>("ScrollGalleryAutoArrangeWindow");
			window.scrollGallery = scrollGallery;
			var r = window.position;
			r.width = 500;
			r.x = Screen.currentResolution.width / 2 - r.width / 2;
			r.y = Screen.currentResolution.height / 2 - r.height / 2;
			window.position = r;
		}

		private void OnGUI()
		{
			this.scrollGallery = EditorGUILayout.ObjectField("当前ScrollSystem:", this.scrollGallery, typeof(ScrollGallery), true) as ScrollGallery;
			if (this.scrollGallery == null) { return; }
			GUILayout.Label("当前滚动方向:" + ((this.scrollGallery.Direction == ScrollGallery.ScrollDirection.Vertical) ? "垂直" : "水平"));
			GUILayout.BeginHorizontal();
			this.spacing = Mathf.Max(0, EditorGUILayout.FloatField("间隔:", this.spacing));
			if (GUILayout.Button("自动排列"))
			{
				AutoArrangeBySpacing();
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("对齐:");
			if (this.scrollGallery.Direction == ScrollGallery.ScrollDirection.Vertical)
			{
				if (GUILayout.Button("左对齐"))
				{
					AutoArrangeByFitLeft();
				}
				if (GUILayout.Button("中对齐"))
				{
					AutoArrangeByFitCenter();
				}
				if (GUILayout.Button("右对齐"))
				{
					AutoArrangeByFitRight();
				}
			}
			else
			{
				if (GUILayout.Button("上对齐"))
				{
					AutoArrangeByFitUp();
				}
				if (GUILayout.Button("中对齐"))
				{
					AutoArrangeByFitCenter();
				}
				if (GUILayout.Button("下对齐"))
				{
					AutoArrangeByFitDown();
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Scale Delta:");
			this.scaleDelta = EditorGUILayout.Vector2Field("", this.scaleDelta);
			if (GUILayout.Button("等差调整"))
			{
				DoScale();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Width&Height Delta:");
			this.widthAndHeightDelta = EditorGUILayout.Vector2Field("", this.widthAndHeightDelta);
			if (GUILayout.Button("等差调整"))
			{
				DoWidthAndHeight();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			this.maskTransform = EditorGUILayout.ObjectField("Mask:", this.maskTransform, typeof(RectTransform), true) as RectTransform;
			if (this.maskTransform != null && GUILayout.Button("匹配内容"))
			{
				FitMask();
			}
			GUILayout.EndHorizontal();

		}

		private void AutoArrangeBySpacing()
		{
			var mainTransform = scrollGallery.Splits[scrollGallery.MainIndex + 1];
			if (scrollGallery.Direction == ScrollGallery.ScrollDirection.Vertical)
			{
				var lastTransfrom = mainTransform;
				for (int i = scrollGallery.MainIndex; i >= 0; i--)
				{
					var curTransform = scrollGallery.Splits[i];
					Undo.RecordObject(curTransform, "根据spacing自动排列位置");
					curTransform.anchoredPosition = lastTransfrom.anchoredPosition + Vector2.up * (spacing + lastTransfrom.sizeDelta.y / 2 * lastTransfrom.localScale.y + curTransform.sizeDelta.y / 2 * curTransform.localScale.y);
					lastTransfrom = curTransform;
				}
				lastTransfrom = mainTransform;
				for (int i = scrollGallery.MainIndex + 2; i < scrollGallery.Splits.Length; i++)
				{
					var curTransform = scrollGallery.Splits[i];
					Undo.RecordObject(curTransform, "根据spacing自动排列位置");
					curTransform.anchoredPosition = lastTransfrom.anchoredPosition + Vector2.down * (spacing + lastTransfrom.sizeDelta.y / 2 * lastTransfrom.localScale.y + curTransform.sizeDelta.y / 2 * curTransform.localScale.y);
					lastTransfrom = curTransform;
				}
			}
			else
			{
				var lastTransfrom = mainTransform;
				for (int i = scrollGallery.MainIndex; i >= 0; i--)
				{
					var curTransform = scrollGallery.Splits[i];
					Undo.RecordObject(curTransform, "根据spacing自动排列位置");
					curTransform.anchoredPosition = lastTransfrom.anchoredPosition + Vector2.left * (spacing + lastTransfrom.sizeDelta.x / 2 * lastTransfrom.localScale.x + curTransform.sizeDelta.x / 2 * curTransform.localScale.x);
					lastTransfrom = curTransform;
				}
				lastTransfrom = mainTransform;
				for (int i = scrollGallery.MainIndex + 2; i < scrollGallery.Splits.Length; i++)
				{
					var curTransform = scrollGallery.Splits[i];
					Undo.RecordObject(curTransform, "根据spacing自动排列位置");
					curTransform.anchoredPosition = lastTransfrom.anchoredPosition + Vector2.right * (spacing + lastTransfrom.sizeDelta.x / 2 * lastTransfrom.localScale.x + curTransform.sizeDelta.x / 2 * curTransform.localScale.x);
					lastTransfrom = curTransform;
				}
			}
		}

		private void AutoArrangeByFitLeft()
		{
			var mainTransform = scrollGallery.Splits[scrollGallery.MainIndex + 1];
			float leftEdge = mainTransform.anchoredPosition.x - mainTransform.sizeDelta.x / 2 * mainTransform.localScale.x;
			for (int i = 0; i < this.scrollGallery.Splits.Length; i++)
			{
				var curTransform = this.scrollGallery.Splits[i];
				if (curTransform == mainTransform) { continue; }
				Undo.RecordObject(curTransform, "AutoArrangeByFitLeft");
				var newAnchoredPosition = curTransform.anchoredPosition;
				newAnchoredPosition.x = leftEdge + curTransform.sizeDelta.x / 2 * curTransform.localScale.x;
				curTransform.anchoredPosition = newAnchoredPosition;
			}
		}

		private void AutoArrangeByFitRight()
		{
			var mainTransform = scrollGallery.Splits[scrollGallery.MainIndex + 1];
			float rightEdge = mainTransform.anchoredPosition.x + mainTransform.sizeDelta.x / 2 * mainTransform.localScale.x;
			for (int i = 0; i < this.scrollGallery.Splits.Length; i++)
			{
				var curTransform = this.scrollGallery.Splits[i];
				if (curTransform == mainTransform) { continue; }
				Undo.RecordObject(curTransform, "AutoArrangeByFitRight");
				var newAnchoredPosition = curTransform.anchoredPosition;
				newAnchoredPosition.x = rightEdge - curTransform.sizeDelta.x / 2 * curTransform.localScale.x;
				curTransform.anchoredPosition = newAnchoredPosition;
			}
		}

		private void AutoArrangeByFitUp()
		{
			var mainTransform = scrollGallery.Splits[scrollGallery.MainIndex + 1];
			float upEdge = mainTransform.anchoredPosition.y + mainTransform.sizeDelta.y / 2 * mainTransform.localScale.y;
			for (int i = 0; i < this.scrollGallery.Splits.Length; i++)
			{
				var curTransform = this.scrollGallery.Splits[i];
				if (curTransform == mainTransform) { continue; }
				Undo.RecordObject(curTransform, "AutoArrangeByFitUp");
				var newAnchoredPosition = curTransform.anchoredPosition;
				newAnchoredPosition.y = upEdge - curTransform.sizeDelta.y / 2 * curTransform.localScale.y;
				curTransform.anchoredPosition = newAnchoredPosition;
			}
		}

		private void AutoArrangeByFitDown()
		{
			var mainTransform = scrollGallery.Splits[scrollGallery.MainIndex + 1];
			float downEdge = mainTransform.anchoredPosition.y - mainTransform.sizeDelta.y / 2 * mainTransform.localScale.y;
			for (int i = 0; i < this.scrollGallery.Splits.Length; i++)
			{
				var curTransform = this.scrollGallery.Splits[i];
				if (curTransform == mainTransform) { continue; }
				Undo.RecordObject(curTransform, "AutoArrangeByFitDown");
				var newAnchoredPosition = curTransform.anchoredPosition;
				newAnchoredPosition.y = downEdge + curTransform.sizeDelta.y / 2 * curTransform.localScale.y;
				curTransform.anchoredPosition = newAnchoredPosition;
			}
		}

		private void AutoArrangeByFitCenter()
		{
			var mainTransform = scrollGallery.Splits[scrollGallery.MainIndex + 1];
			if (scrollGallery.Direction == ScrollGallery.ScrollDirection.Vertical)
			{
				float center = mainTransform.anchoredPosition.x;
				for (int i = 0; i < this.scrollGallery.Splits.Length; i++)
				{
					var curTransform = this.scrollGallery.Splits[i];
					if (curTransform == mainTransform) { continue; }
					Undo.RecordObject(curTransform, "AutoArrangeByFitCenter");
					var newAnchoredPosition = curTransform.anchoredPosition;
					newAnchoredPosition.x = center;
					curTransform.anchoredPosition = newAnchoredPosition;
				}
			}
			else
			{
				float center = mainTransform.anchoredPosition.y;
				for (int i = 0; i < this.scrollGallery.Splits.Length; i++)
				{
					var curTransform = this.scrollGallery.Splits[i];
					if (curTransform == mainTransform) { continue; }
					Undo.RecordObject(curTransform, "AutoArrangeByFitCenter");
					var newAnchoredPosition = curTransform.anchoredPosition;
					newAnchoredPosition.y = center;
					curTransform.anchoredPosition = newAnchoredPosition;
				}
			}
		}

		private void DoScale()
		{
			var mainTransform = scrollGallery.Splits[scrollGallery.MainIndex + 1];
			var curScale = mainTransform.localScale;
			for (int i = scrollGallery.MainIndex; i >= 0; i--)
			{
				var curTransform = scrollGallery.Splits[i];
				Undo.RecordObject(curTransform, "DoScale");
				curScale += new Vector3(scaleDelta.x, scaleDelta.y, 0);
				curTransform.localScale = curScale;
			}
			curScale = mainTransform.localScale;
			for (int i = scrollGallery.MainIndex + 2; i < scrollGallery.Splits.Length; i++)
			{
				var curTransform = scrollGallery.Splits[i];
				Undo.RecordObject(curTransform, "DoScale");
				curScale += new Vector3(scaleDelta.x, scaleDelta.y, 0);
				curTransform.localScale = curScale;
			}
		}

		private void DoWidthAndHeight()
		{
			var mainTransform = scrollGallery.Splits[scrollGallery.MainIndex + 1];
			var curWidthAndHeight = mainTransform.sizeDelta;
			for (int i = scrollGallery.MainIndex; i >= 0; i--)
			{
				var curTransform = scrollGallery.Splits[i];
				Undo.RecordObject(curTransform, "DoHeightAndWidth");
				curWidthAndHeight += widthAndHeightDelta;
				curTransform.sizeDelta = curWidthAndHeight;
			}
			curWidthAndHeight = mainTransform.sizeDelta;
			for (int i = scrollGallery.MainIndex + 2; i < scrollGallery.Splits.Length; i++)
			{
				var curTransform = scrollGallery.Splits[i];
				Undo.RecordObject(curTransform, "DoHeightAndWidth");
				curWidthAndHeight += widthAndHeightDelta;
				curTransform.sizeDelta = curWidthAndHeight;
			}
		}

		private void FitMask()
		{
			RectBounds rectBounds = new RectBounds(scrollGallery.Splits[1]);
			for (int i = 2; i < scrollGallery.Splits.Length - 1; i++)
			{
				var curSplit = scrollGallery.Splits[i];
				rectBounds.Encapsulate(curSplit);
			}
			Undo.RecordObject(this.maskTransform, "FitMask");
			List<RectTransform> listChildren = new List<RectTransform>();
			foreach (var aTrans in this.maskTransform)
			{
				listChildren.Add(aTrans as RectTransform);
			}
			listChildren.ForEach(temp => temp.SetParent(null));
			this.maskTransform.anchoredPosition = rectBounds.center;
			this.maskTransform.sizeDelta = rectBounds.size;
			listChildren.ForEach(temp => temp.SetParent(this.maskTransform));
			listChildren.Clear();
			Debug.Log("FitMask");
		}

	}
}



