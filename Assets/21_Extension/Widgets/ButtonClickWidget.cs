using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BanSupport
{
	/// <summary>
	/// 用于按钮点击事件
	/// </summary>
	public class ButtonClickWidget : Widget
	{

		private float recoverTime;
		private float cdTime;

		public ButtonClickWidget(ButtonEx buttonEx, Action completeAction, float cdTime) : base(buttonEx, completeAction)
		{
			this.recoverTime = float.MinValue;
			this.cdTime = cdTime;
			RegistEvent<PointerEventData>("OnClick", this.OnClick);
		}

        public override string ToString()
		{
			return string.Format("ClickWidget cdTime:{0} recoverTime:{1}", cdTime.ToString("0.00"), Mathf.Max(0, recoverTime - Time.realtimeSinceStartup).ToString("0.00"));
		}

		private void OnClick(PointerEventData eventData)
		{
            if (IsReady())
            {
				if (this.completeAction != null) { this.completeAction(); }
			}
        }

		private bool IsReady()
		{
			if (Time.realtimeSinceStartup > this.recoverTime)
			{
				this.recoverTime = Time.realtimeSinceStartup + cdTime;
				return true;
			}
			else
			{
				return false;
			}
		}

	}
}