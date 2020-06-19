using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BanSupport
{
	public class ScrollSystemSetSizeWindow : EditorWindow
	{

		private ScrollSystem scrollSystem;

		private float tileWidth;
		private float tileHeight;
		private int rowCount = 0;
		private int colCount = 0;
		public static void ShowWindow(ScrollSystem scrollSystem)
		{
			//ScrollSystemSetSizeWindow window = ScriptableObject.CreateInstance<ScrollSystemSetSizeWindow>();
			//window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
			//window.ShowPopup();
			if (scrollSystem.contentTrans.childCount <= 0)
			{
				Debug.LogError("ScrollSystem至少需要放置一个模板预制体");
				return;
			}
			var window = GetWindow<ScrollSystemSetSizeWindow>("ScrollSystemSetSizeWindow");
			window.scrollSystem = scrollSystem;
			var originTransform = scrollSystem.contentTrans.GetChild(0) as RectTransform;
			window.tileWidth = originTransform.sizeDelta.x;
			window.tileHeight = originTransform.sizeDelta.y;
		}

		void OnGUI()
		{
			this.scrollSystem = EditorGUILayout.ObjectField("当前ScrollSystem:",this.scrollSystem, typeof(ScrollSystem), true) as ScrollSystem;
			//GUILayout.Label("当前ScrollSystem:" + (scrollSystem != null ? scrollSystem.name : "null"));
			if (scrollSystem == null) { return; }
			tileHeight = EditorGUILayout.FloatField("元素高度", tileHeight);
			tileWidth = EditorGUILayout.FloatField("元素宽度", tileWidth);
			GUILayout.BeginHorizontal();
			rowCount = EditorGUILayout.IntField("行数", rowCount);
			if (GUILayout.Button("设置高度"))
			{
				Undo.RecordObject(scrollSystem.transform as RectTransform, "设置高度");
				SetHeight();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			colCount = EditorGUILayout.IntField("列数", colCount);
			if (GUILayout.Button("设置宽度"))
			{
				Undo.RecordObject(scrollSystem.transform as RectTransform, "设置宽度");
				SetWidth();
			}
			GUILayout.EndHorizontal();

			if (GUILayout.Button("设置高度和宽度"))
			{
				Undo.RecordObject(scrollSystem.transform as RectTransform, "设置高度和宽度");
				SetWidth();
				SetHeight();
			}

		}

		private void SetWidth()
		{
			var rectTransform = scrollSystem.transform as RectTransform;
			float width = colCount * tileWidth + (colCount - 1) * scrollSystem.Spacing.x + scrollSystem.Border.x * 2;
			rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);
		}

		private void SetHeight()
		{
			var rectTransform = scrollSystem.transform as RectTransform;
			float height = rowCount * tileHeight + (rowCount - 1) * scrollSystem.Spacing.y + scrollSystem.Border.y * 2;
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);
		}

	}
}