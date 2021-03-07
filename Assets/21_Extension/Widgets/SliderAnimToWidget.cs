using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{
	/// <summary>
	/// 用于给Slider做动画
	/// </summary>
	public class SliderAnimToWidget : TimeWidget
	{

		private SliderEx sliderEx;
		private float fromNormalizedValue;
		private float toNormalizedValue;

		public SliderAnimToWidget(SliderEx sliderEx, Action completeAction, float time, bool timeScaleEnable, float toNormalizedValue) :
			base(sliderEx, completeAction, time, timeScaleEnable)
		{
			this.sliderEx = sliderEx;
			this.fromNormalizedValue = this.sliderEx.slider.normalizedValue;
			this.toNormalizedValue = toNormalizedValue;
		}

		public override bool OnUpdate()
		{
			base.UpdateTime();
			this.sliderEx.slider.normalizedValue = Mathf.Lerp(this.fromNormalizedValue,this.toNormalizedValue, base.GetPercent());
			return base.OnUpdate();
		}

	}
}