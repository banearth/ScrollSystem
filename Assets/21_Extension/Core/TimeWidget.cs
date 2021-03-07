using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{
    public abstract class TimeWidget : Widget
    {

        protected bool timeScaleEnable;
        protected float handledTime { get { if (timeScaleEnable) { return Time.time; } else { return Time.realtimeSinceStartup; } } }
        protected float targetTime;
        protected float curTime;
        protected float periodTime;

        public TimeWidget(ExBase exBase, Action completeAction, float time, bool timeScaleEnable) : base(exBase, completeAction)
        {
            StartUpdate();
            this.timeScaleEnable = timeScaleEnable;
            this.periodTime = time;
            this.curTime = handledTime;
            this.targetTime = handledTime + this.periodTime;
        }

        public override bool OnUpdate()
        {
            if (this.curTime <= this.targetTime)
            {
                return true;
            }
            else
            {
                if (this.completeAction != null) { this.completeAction(); }
                return false;
            }
        }

        public void UpdateTime()
        {
            this.curTime = handledTime;
        }

		public float GetPercent()
		{
			if (periodTime > 0)
			{
                return 1f - Mathf.Clamp01((this.targetTime - curTime) / this.periodTime);

            }
			else
			{
                return 1f;
			}
			
		}

		public int GetLeftSeconds()
        {
            return Mathf.Max(0, Mathf.CeilToInt(this.targetTime - curTime));
        }

    }
}