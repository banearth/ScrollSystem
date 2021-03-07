using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BanSupport
{
    public class ButtonHoldWidget : Widget
    {

        private float recoverTime;
        private float cdTime;
        private bool needUpdate;

        public ButtonHoldWidget(ButtonEx uiButton, Action completeAction, float cdTime) : base(uiButton, completeAction)
        {
            this.recoverTime = float.MaxValue;
            this.cdTime = cdTime;
            this.needUpdate = false;
            RegistEvent("OnPointDown", OnPointDown);
            RegistEvent("OnPointUp", OnPointUp);
        }

        public override string ToString()
        {
            float showTime;
            if (recoverTime >= float.MaxValue)
            {
                showTime = 0;
            }
            else
            {
                showTime = Mathf.Max(0, recoverTime - Time.realtimeSinceStartup);
            }
            return string.Format("HoldWidget cdTime:{0} recoverTime:{1}", cdTime.ToString("0.00"), showTime.ToString("0.00"));
        }

        public override bool OnUpdate()
        {
            if (this.needUpdate)
            {
                if (IsReady())
                {
					if (this.completeAction != null) { this.completeAction(); }
					return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public override void OnDisable()
        {
            this.needUpdate = false;
        }

        public void OnPointDown()
        {
            this.needUpdate = true;
            this.recoverTime = Time.realtimeSinceStartup + cdTime;
            StartUpdate();
        }

        public void OnPointUp()
        {
            this.needUpdate = false;
        }

        private bool IsReady()
        {
            if (Time.realtimeSinceStartup > this.recoverTime)
            {
                this.recoverTime = float.MaxValue;
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}