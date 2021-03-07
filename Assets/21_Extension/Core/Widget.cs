using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{
    public abstract class Widget : IDisposable
    {

        protected ExBase exBase;
        protected Action completeAction;
        private List<IActionTrigger> registedTriggers;

        public Widget(ExBase uiBase, Action completeAction)
        {
            this.exBase = uiBase;
            this.completeAction = completeAction;
        }

        /// <summary>
        /// called when this widget is removed
        /// </summary>
        public virtual void Dispose()
        {
            if (registedTriggers != null)
            {
                foreach (var aTrigger in registedTriggers)
                {
                    this.exBase.UnRegistEvent(aTrigger);
                }
                ListPool<IActionTrigger>.Release(registedTriggers);
                registedTriggers = null;
            }
        }

        /// <summary>
        /// called when exBase is Actived
        /// </summary>
        public virtual void OnEnable() { }

        /// <summary>
        /// called when exBase is Disactived
        /// </summary>
        public virtual void OnDisable() { }

        /// <summary>
        /// return true means still need update
        /// </summary>
        public virtual bool OnUpdate() { return false; }

        /// <summary>
        /// for exbase to call
        /// </summary>
        public void Update()
        {
            this.exBase.needUpdate |= this.OnUpdate();
        }

        protected void StartUpdate()
        {
            this.exBase.StartUpdate();
        }

        #region Event

        protected void RegistEvent(string eventName, Action aAction)
        {
            var actionTrigger = new ActionTrigger(aAction);
            this.exBase.RegistEvent(eventName, actionTrigger);
            AddToRegistedTriggers(actionTrigger);
        }

        protected void RegistEvent<T>(string eventName, Action<T> aAction)
        {
            var actionTrigger = new ActionTrigger<T>(aAction);
            this.exBase.RegistEvent(eventName, actionTrigger);
            AddToRegistedTriggers(actionTrigger);
        }

        protected void RegistEvent<T1, T2>(string eventName, Action<T1, T2> aAction)
        {
            var actionTrigger = new ActionTrigger<T1, T2>(aAction);
            this.exBase.RegistEvent(eventName, actionTrigger);
            AddToRegistedTriggers(actionTrigger);
        }

        private void AddToRegistedTriggers(IActionTrigger trigger)
        {
            if (registedTriggers == null)
            {
                registedTriggers = ListPool<IActionTrigger>.Get();
            }
            registedTriggers.Add(trigger);
        }
        #endregion

    }

}