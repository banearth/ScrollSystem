using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BanSupport;

public class ScrollLayoutForTextMeshPro : ScrollLayout
{

	public TextMeshProUGUI fitLabel;
	private float heightOffset;

	protected override void Init()
	{
		var height = (this.transform as RectTransform).sizeDelta.y;
		this.heightOffset = height - fitLabel.preferredHeight;
	}

	public override float GetHeightByStr(string str)
	{
		fitLabel.text = str;
		return fitLabel.preferredHeight + heightOffset;
	}

}
