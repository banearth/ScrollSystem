using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BanSupport
{
	public class ScrollLayoutForText : ScrollLayout
	{

		[IntEnum(1, 2, "Text", "TextMeshPro")]
		public int textType = 1;
		[IntCondition("textType",1)]
		public Text text = null;
		[IntCondition("textType", 2)]
		public TextMeshProUGUI textMeshPro;
		private float heightOffset;

		protected override void Init()
		{
			var height = (this.transform as RectTransform).sizeDelta.y;
			if (textType == 1)
			{
				this.heightOffset = height - text.preferredHeight;
			}
			else if (textType == 2)
			{
				this.heightOffset = height - textMeshPro.preferredHeight;
			}
		}

		public override float GetHeightByStr(string str)
		{
			if (textType == 1)
			{
				text.text = str;
				return text.preferredHeight + heightOffset;
			}
			else if (textType == 2)
			{
				textMeshPro.text = str;
				return textMeshPro.preferredHeight + heightOffset;
			}
			else
			{
				return base.GetHeightByStr(str);
			}
		}

	}

}