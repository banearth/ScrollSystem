using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BanSupport
{
	[CustomEditor(typeof(ScrollSystem))]
	public class CustomLoopScrollRectEditor : Editor
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
					GUILayout.Label("预制体:"+key);
					GUILayout.Label("库存数量:"+dic[key].list.Count.ToString());
					GUILayout.EndHorizontal();
				}
			}
			else
			{
				if (GUILayout.Button("子物体手动刷新"))
				{
					script.SetContentChildren();
				}
			}
			DrawDefaultInspector();
		}

	}

}