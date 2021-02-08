using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BanSupport
{
	[CustomEditor(typeof(ScrollSystem))]
	public class ScrollSystemEditor : Editor
	{

		private ScrollSystem script { get { return target as ScrollSystem; } }
		public override void OnInspectorGUI()
		{

			if (Application.isPlaying)
			{
				GUILayout.Label("-----只在运行时候显示-----");
				var dic = script.ObjectPoolDic;
				foreach (var key in dic.Keys)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label("预制体:" + key);
					GUILayout.Label("库存数量:" + dic[key].pool.countInactive.ToString());
					GUILayout.EndHorizontal();
				}
			}
			else
			{

				GUILayout.BeginHorizontal();

				if (GUILayout.Button("子物体手动刷新"))
				{
					script.SetContentChildren();
				}

				if (GUILayout.Button("打开几乘几设置窗口"))
				{
					ScrollSystemSetSizeWindow.ShowWindow(script);
				}

				GUILayout.EndHorizontal();

				//默认排列
				//int newCorner = GUILayout.SelectionGrid(script.startCorner, new string[] {
						//"Left Up", "Right Up" , "Left Down", "Right Down" }, 2);
				//if (script.startCorner != newCorner)
				//{
				//	script.startCorner = newCorner;
				//	script.SetContentChildren();
				//	EditorUtility.SetDirty(script);
				//}
				
			}

			DrawDefaultInspector();
		}

	}

}