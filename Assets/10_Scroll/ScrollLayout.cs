using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BanSupport.ScrollSystem
{
	/// <summary>
	/// 这个东西定义了基本的排列方式
	/// </summary>
	[ExecuteInEditMode]
	public class ScrollLayout : MonoBehaviour
	{
		public NewLine newLine = NewLine.None;

		public enum NewLine
		{
			None,//不启用新行
			Center,//居中
			LeftOrUp,//贴着左边或者下边
			RightOrDown,//贴着右边或者上边
		};

		private void Awake() { Init(); }

		protected virtual void Init() { }

		public virtual float GetHeightByStr(string str) { return 0; }

#if UNITY_EDITOR

		private NewLine oldNewLine = NewLine.None;
		private float oldWidth;
		private float oldHeight;

		private void Start()
		{
			if (Application.isPlaying) { return; }
			var rectTransform = this.transform as RectTransform;
			oldWidth = rectTransform.sizeDelta.x;
			oldHeight = rectTransform.sizeDelta.y;
		}

		private void Update()
		{
			if (Application.isPlaying) { return; }
			var changed = false;
			if (oldNewLine != newLine)
			{
				oldNewLine = newLine;
				changed = true;
			}
			var rectTransform = this.transform as RectTransform;
			if (oldWidth != rectTransform.sizeDelta.x)
			{
				oldWidth = rectTransform.sizeDelta.x;
				changed = true;
			}
			if (oldHeight != rectTransform.sizeDelta.y)
			{
				oldHeight = rectTransform.sizeDelta.y;
				changed = true;
			}
			if (changed)
			{
				ResetScrollSystem();
			}
		}

		private void OnDrawGizmos()
		{
			if (Application.isPlaying) { return; }
			if (Selection.transforms.Contains(this.transform) || Selection.transforms.Contains(temp => temp.IsChildOf(this.transform)))
			{
				var rectTransform = this.transform as RectTransform;
				var size = new Vector3(this.transform.lossyScale.x * rectTransform.sizeDelta.x, this.transform.lossyScale.y * rectTransform.sizeDelta.y, 0);
				Gizmos.color = new Color(0, 1, 0, 0.2f);
				Gizmos.DrawCube(this.transform.position, size);
			}
		}

		private void ResetScrollSystem()
		{
			var scrollSystem = this.gameObject.GetComponentInParent<ScrollSystem>();
			if (scrollSystem != null)
			{
				scrollSystem.SetContentChildren();
			}
		}

#endif

	}
}