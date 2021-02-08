using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{
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

#if UNITY_EDITOR
		private void OnTransformChildrenChanged()
		{
			if (Application.isPlaying)
			{
				return;
			}
			scrollSystem?.OnContentChildrenChanged();
		}
#endif

	}
}