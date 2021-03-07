using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BanSupport
{
	public class ButtonStyleSettting : MonoBehaviour
	{
		public List<ButtonStyle> styles;

		private void Awake()
		{
			this.gameObject.LoadStyleSetting(styles);
		}

	}
}