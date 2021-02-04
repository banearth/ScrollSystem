using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanSupport;

public class ScrollLayoutForText : ScrollLayout
{

	public Text fitLabel = null;
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
