using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{
    public class ExBase : MonoBehaviour
	{

        #region Update

        [NonSerialized]
        public bool needUpdate;

        /// <summary>
        /// for manager call
        /// </summary>
        public bool DoUpdate()
        {
            this.needUpdate = false;
            ForeachAllWidgets(aWidget => aWidget.Update());
            return this.needUpdate;
        }

        /// <summary>
        /// for widgets call
        /// </summary>
        public void StartUpdate()
        {
            UIExtensionManager.Add(this);
        }

        /// <summary>
        /// for widgets call
        /// </summary>
        public void StopUpdate()
        {
            UIExtensionManager.Remove(this);
        }

        #endregion

        #region Widget

        private Dictionary<Type, List<Widget>> widgetDic = new Dictionary<Type, List<Widget>>();

        /// <summary>
        /// 增加一个小功能
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="widget"></param>
        /// <param name="unique">保持这种类型小功能只有这一个实体</param>
        protected void AddWidget<T>(T widget, bool unique = false) where T : Widget
        {
            if (widget == null) { return; }
            var type = typeof(T);
            if (!widgetDic.ContainsKey(type))
            {
                widgetDic.Add(type, ListPool<Widget>.Get());
            }
            var widgets = widgetDic[type];
            if (unique)
            {
                if (widgets.Count > 0)
                {
                    widgets.ForEach(aWidget => aWidget.Dispose());
                    widgets.Clear();
                }
                widgets.Add(widget);
            }
            else
            {
                if (!widgets.Contains(widget))
                {
                    widgets.Add(widget);
                }
            }
        }

        public bool RemoveWidget<T>() where T : Widget
        {
            var type = typeof(T);
            if (!widgetDic.ContainsKey(type))
            {
                return false;
            }
            var widgets = widgetDic[type];
            widgets.ForEach(aWidget => aWidget.Dispose());
            ListPool<Widget>.Release(widgets);
            widgetDic.Remove(type);
            return true;
        }

        public bool RemoveWidget(Widget widget)
        {
            if (widget == null) { return false; }
            var type = widget.GetType();
            if (!widgetDic.ContainsKey(type))
            {
                return false;
            }
            var widgets = widgetDic[type];
            if (widgets.Contains(widget))
            {
                widgets.Remove(widget);
                widget.Dispose();
                if (widgets.Count <= 0)
                {
                    ListPool<Widget>.Release(widgets);
                    widgetDic.Remove(type);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ForeachWidgets<T>(Action<T> action) where T : Widget
        {
            var type = typeof(T);
            if (!widgetDic.ContainsKey(type))
            {
                return;
            }
            var widgets = widgetDic[type];
            foreach (var aWidget in widgets)
            {
                action((T)aWidget);
            }
        }

        private void ForeachAllWidgets(Action<Widget> action)
        {
            foreach (var type in widgetDic.Keys)
            {
                foreach (var widget in widgetDic[type])
                {
                    action(widget);
                }
            }
        }

        public string GetWidgetState()
        {
            var list = ListPool<string>.Get();
            foreach (var type in widgetDic.Keys)
            {
                foreach (var widget in widgetDic[type])
                {
                    list.Add(widget.ToString());
                }
            }
            var resultStr = string.Join("\n", list);
            ListPool<string>.Release(list);
            return resultStr;
        }

        #endregion

        #region Event

        private Dictionary<string, List<IActionTrigger>> eventDic = new Dictionary<string, List<IActionTrigger>>();

        public void RegistEvent(string eventName, IActionTrigger actionTrigger)
        {
            if (!eventDic.ContainsKey(eventName))
            {
                eventDic.Add(eventName, ListPool<IActionTrigger>.Get());
            }
            var triggerList = eventDic[eventName];
            if (!triggerList.Contains(actionTrigger))
            {
                triggerList.Add(actionTrigger);
            }
        }

        public void UnRegistEvent(IActionTrigger actionTrigger)
        {
            bool deleteKey = false;
            string deleteKeyName = "";
            foreach (var eventName in eventDic.Keys)
            {
                var triggerList = eventDic[eventName];
                if (triggerList.Contains(actionTrigger))
                {
                    triggerList.Remove(actionTrigger);
                    if (triggerList.Count <= 0)
                    {
                        ListPool<IActionTrigger>.Release(triggerList);
                        deleteKey = true;
                        deleteKeyName = eventName;
                    }
                    break;
                }
            }
            if (deleteKey)
            {
                eventDic.Remove(deleteKeyName);
            }
        }

        public void SendEvent(string eventName)
        {
            if (!eventDic.ContainsKey(eventName))
            {
                return;
            }
            var triggerList = eventDic[eventName];
            foreach (var aTrigger in triggerList)
            {
                aTrigger.OnAction();
            }
        }

        public void SendEvent<T>(string eventName, T t)
        {
            if (!eventDic.ContainsKey(eventName))
            {
                return;
            }
            var triggerList = eventDic[eventName];
            foreach (var aTrigger in triggerList)
            {
                aTrigger.OnAction(t);
            }
        }


        public void SendEvent<T1, T2>(string eventName, T1 t1, T2 t2)
        {
            if (!eventDic.ContainsKey(eventName))
            {
                return;
            }
            var triggerList = eventDic[eventName];
            foreach (var aTrigger in triggerList)
            {
                aTrigger.OnAction(t1, t2);
            }
        }

		#endregion

		#region Unity

		private void Awake()
		{
            UIExtensionManager.Init();
        }

		private void OnEnable()
        {
            ForeachAllWidgets(aWidget => aWidget.OnEnable());
            StartUpdate();
        }

        private void OnDisable()
        {
            ForeachAllWidgets(aWidget => aWidget.OnDisable());
            StopUpdate();
        }

        private void OnDestroy()
        {
            foreach (var type in widgetDic.Keys)
            {
                var widgets = widgetDic[type];
                foreach (var aWidget in widgets)
                {
                    aWidget.Dispose();
                }
                ListPool<Widget>.Release(widgets);
            }
            this.widgetDic.Clear();
            this.widgetDic = null;
        }

        #endregion

    }
}