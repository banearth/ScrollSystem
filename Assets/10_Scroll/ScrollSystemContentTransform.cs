using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{
	[ExecuteInEditMode]
	public class ScrollSystemContentTransform : MonoBehaviour
	{
		private ScrollSystem _scrollSystem = null;
		private ScrollSystem scrollSystem
		{
			get
			{
				if (this._scrollSystem == null)
				{
					this._scrollSystem = this.transform.parent?.GetComponent<ScrollSystem>();
				}
				return this._scrollSystem;
			}
		}

		private RectTransform _value = null;

		public RectTransform Value
		{
			get
			{
				if (_value == null)
				{
					_value = this.transform as RectTransform;
				}
				return _value;
			}
		}

#if UNITY_EDITOR
		private void OnTransformChildrenChanged()
		{
			Debug.Log("OnTransformChildrenChanged");
			if (Application.isPlaying)
			{
				return;
			}
			scrollSystem?.OnContentChildrenChanged();
		}
#endif

	}
}