using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace BanSupport
{
    public static class ButtonExtension
    {

        private static bool CreateButtonExIfNotExist(GameObject go, out ButtonEx buttonEx)
        {
            if (go == null)
            {
                Debug.LogWarning("call 'CreateButtonEx' function with null GameObject param !");
                buttonEx = null;
                return false;
            }
            else
            {
                buttonEx = go.GetComponent<ButtonEx>();
                if (buttonEx == null)
                {
                    buttonEx = go.AddComponent<ButtonEx>();
                }
                return true;
            }
        }

        private static bool CreateButtonExIfNotExist(Button button, out ButtonEx buttonEx)
        {
            if (button == null)
            {
                Debug.LogWarning("call 'CreateButtonEx' function with null Button param !");
                buttonEx = null;
                return false;
            }
            return CreateButtonExIfNotExist(button.gameObject, out buttonEx);
        }

        #region GameObject.AddClick

        public static ButtonEx AddClick(this GameObject gameObject, Action action, float cdTime = 0)
        {
            if (CreateButtonExIfNotExist(gameObject, out ButtonEx buttonEx))
            {
                buttonEx.AddClick(action, cdTime);
            }
            return buttonEx;
        }

        public static ButtonEx RemoveClick(this GameObject gameObject)
        {
            if (CreateButtonExIfNotExist(gameObject, out ButtonEx buttonEx))
            {
                buttonEx.RemoveClick();
            }
            return buttonEx;
        }

        #endregion

        #region GameObject.AddHold

        public static ButtonEx AddHold(this GameObject gameObject, Action action, float cdTime)
        {
            if (CreateButtonExIfNotExist(gameObject, out ButtonEx buttonEx))
            {
                buttonEx.AddHold(action, cdTime);
            }
            return buttonEx;
        }

        public static ButtonEx RemoveHold(this GameObject gameObject)
        {
            if (CreateButtonExIfNotExist(gameObject, out ButtonEx buttonEx))
            {
                buttonEx.RemoveHold();
            }
            return buttonEx;
        }

        #endregion

        #region GameObject.AddRepeat

        public static ButtonEx AddRepeat(this GameObject gameObject, Action action, float cdTime)
        {
            if (CreateButtonExIfNotExist(gameObject, out ButtonEx buttonEx))
            {
                buttonEx.AddRepeat(action, cdTime);
            }
            return buttonEx;
        }

        public static ButtonEx RemoveRepeat(this GameObject gameObject)
        {
            if (CreateButtonExIfNotExist(gameObject, out ButtonEx buttonEx))
            {
                buttonEx.RemoveRepeat();
            }
            return buttonEx;
        }

        #endregion

        #region Button.AddClick

        public static ButtonEx AddClick(this Button button, Action action, float cdTime = 0)
        {
            if (CreateButtonExIfNotExist(button, out ButtonEx buttonEx))
            {
                buttonEx.AddClick(action, cdTime);
            }
            return buttonEx;
        }

        public static ButtonEx RemoveClick(this Button button)
        {
            if (CreateButtonExIfNotExist(button, out ButtonEx buttonEx))
            {
                buttonEx.RemoveClick();
            }
            return buttonEx;
        }

        #endregion

        #region Button.AddHold

        public static ButtonEx AddHold(this Button button, Action action, float cdTime)
        {
            if (CreateButtonExIfNotExist(button, out ButtonEx buttonEx))
            {
                buttonEx.AddHold(action, cdTime);
            }
            return buttonEx;
        }

        public static ButtonEx RemoveHold(this Button button)
        {
            if (CreateButtonExIfNotExist(button, out ButtonEx buttonEx))
            {
                buttonEx.RemoveHold();
            }
            return buttonEx;
        }

        #endregion

        #region Button.AddRepeat

        public static ButtonEx AddRepeat(this Button button, Action action, float cdTime)
        {
            if (CreateButtonExIfNotExist(button, out ButtonEx buttonEx))
            {
                buttonEx.AddRepeat(action, cdTime);
            }
            return buttonEx;
        }

        public static ButtonEx RemoveRepeat(this Button button)
        {
            if (CreateButtonExIfNotExist(button, out ButtonEx buttonEx))
            {
                buttonEx.RemoveRepeat();
            }
            return buttonEx;
        }


        #endregion

        #region Others

        public static ButtonEx SetInteractable(this Button button, bool b)
        {
            if (CreateButtonExIfNotExist(button, out ButtonEx buttonEx))
            {
				buttonEx.SetInteractable(b);
			}
            return buttonEx;
        }

		public static ButtonEx SetInteractable(this GameObject go, bool b)
		{
			if (CreateButtonExIfNotExist(go, out ButtonEx buttonEx))
			{
				buttonEx.SetInteractable(b);
			}
			return buttonEx;
		}

		public static bool IsInteractable(this GameObject go)
		{
			if (CreateButtonExIfNotExist(go, out ButtonEx buttonEx))
			{
				return buttonEx.IsInteractable();
			}
			return false;
		}

		public static bool IsInteractable(this Button button)
		{
			if (CreateButtonExIfNotExist(button, out ButtonEx buttonEx))
			{
				return buttonEx.IsInteractable();
			}
			return false;
		}

		public static ButtonEx LoadStyleSetting(this GameObject go, List<ButtonStyle> styles)
		{
			if (CreateButtonExIfNotExist(go, out ButtonEx buttonEx))
			{
				buttonEx.LoadStyleSetting(styles);
			}
			return buttonEx;
		}

        public static ButtonEx LoadStyleSetting(this Button button, List<ButtonStyle> styles)
        {
            if (CreateButtonExIfNotExist(button, out ButtonEx buttonEx))
            {
                buttonEx.LoadStyleSetting(styles);
            }
            return buttonEx;
        }

        public static ButtonEx SetStyle(this GameObject go, string styleName)
		{
            if (CreateButtonExIfNotExist(go, out ButtonEx buttonEx))
            {
                buttonEx.SetStyle(styleName);
            }
            return buttonEx;
        }

        public static ButtonEx SetStyle(this Button button, string styleName)
        {
            if (CreateButtonExIfNotExist(button, out ButtonEx buttonEx))
            {
                buttonEx.SetStyle(styleName);
            }
            return buttonEx;
        }

        #endregion

    }

    public class ButtonEx : ExBase, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {

        /// <summary>
        /// 本脚本事件逻辑层的隔断
        /// </summary>
        private bool interactable = true;

        #region AddClick

        public ButtonEx AddClick(Action action, float cdTime)
        {
            AddWidget(new ButtonClickWidget(this, action, cdTime));
            return this;
        }

        public ButtonEx RemoveClick()
        {
            RemoveWidget<ButtonClickWidget>();
            return this;
        }

        #endregion

        #region AddHold

        public ButtonEx AddHold(Action mainAction, float cdTime)
        {
            AddWidget(new ButtonHoldWidget(this, mainAction, cdTime));
            return this;
        }

        public ButtonEx RemoveHold()
        {
            RemoveWidget<ButtonHoldWidget>();
            return this;
        }

        #endregion

        #region AddRepeat

        public ButtonEx AddRepeat(Action mainAction, float cdTime)
        {
            AddWidget(new ButtonRepeatWidget(this, mainAction, cdTime));
            return this;
        }

        public ButtonEx RemoveRepeat()
        {
            RemoveWidget<ButtonRepeatWidget>();
            return this;
        }

        #endregion

        #region Others

        /// <summary>
        /// 目前Style是Style，Interactable是Interactable，彼此分离，互不影响
        /// </summary>
        public ButtonEx LoadStyleSetting(List<ButtonStyle>  styles)
		{
			AddWidget(new ButtonStyleWidget(this, styles), true);
			return this;
		}

        public ButtonEx SetStyle(string styleName)
		{
            SendEvent("OnStyle", styleName);
            return this;
		}

        /// <summary>
        /// 目前Style是Style，Interactable是Interactable，彼此分离，互不影响
        /// </summary>
		public ButtonEx SetInteractable(bool b)
        {
            this.interactable = b;
            if (this.TryGetComponent(out Button aButton))
            {
                aButton.interactable = b;
            }
			return this;
        }

		public bool IsInteractable()
		{
			return this.interactable;
		}

		#endregion

		#region 系统函数

        public void OnPointerClick(PointerEventData eventData)
        {
            if (this.interactable)
            {
                SendEvent("OnClick", eventData);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (this.interactable)
            {
                SendEvent("OnPointDown", eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (this.interactable)
            {
                SendEvent("OnPointUp", eventData);
            }
        }

        #endregion

    }

}