using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine;

namespace BanSupport
{

	[Serializable]
	public struct ButtonStyle
	{
		public string styleName;
		public bool textColorEnable;
		public Color textColor;
		public enum ImageChangeType { None, Color, Material, Sprite }
		public ImageChangeType imageChangeType;
		public Color imageColor;
		public Material imageMaterial;
		public Sprite imageSprite;
	}

	/// <summary>
	/// 用于按钮样式改变，默认只更改找到的第一个Text
	/// </summary>
	public class ButtonStyleWidget : Widget
	{

		private Text label;
		private Image image;
		private List<ButtonStyle> styles;

		public ButtonStyleWidget(ExBase exBase, List<ButtonStyle> styles) : base(exBase, null)
		{
			this.label = exBase.GetComponentInChildren<Text>();
			this.image = exBase.GetComponent<Image>();
			this.styles = styles;
			RegistEvent<string>("OnStyle", OnStyle);
		}

		private void OnStyle(string name)
		{
			foreach (var curStyle in styles)
			{
				if (curStyle.styleName == name)
				{
					Apply(curStyle);
					return;
				}
			}
			Debug.LogWarning("can not find this style:" + name);
		}

		private void Apply(ButtonStyle style)
		{
			if (this.label != null)
			{
				if (style.textColorEnable)
				{
					this.label.color = style.textColor;
				}
			}
			if (this.image != null)
			{
				switch (style.imageChangeType)
				{
					case ButtonStyle.ImageChangeType.Color:
						this.image.color = style.imageColor;
						break;
					case ButtonStyle.ImageChangeType.Material:
						this.image.material = style.imageMaterial;
						break;
					case ButtonStyle.ImageChangeType.Sprite:
						this.image.sprite = style.imageSprite;
						if (this.image.type == Image.Type.Simple)
						{
							this.image.SetNativeSize();
						}
						break;
				}
			}
		}

	}
}

