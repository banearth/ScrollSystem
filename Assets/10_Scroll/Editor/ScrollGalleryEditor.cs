using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BanSupport
{
	[CustomEditor(typeof(ScrollGallery))]
	public class ScrollGalleryEditor : Editor
	{
		private ScrollGallery script { get { return target as ScrollGallery; } }
		public override void OnInspectorGUI()
		{

			GUILayout.BeginHorizontal();

			if (GUILayout.Button("根据splitCount自动分隔区域"))
			{
				script.CreateSplits();
			}

			if (GUILayout.Button("打开自动排列窗口"))
			{
				ScrollGalleryAutoArrangeWindow.ShowWindow(script);
			}

			GUILayout.EndHorizontal();

			
			DrawDefaultInspector();
		}

	}

}