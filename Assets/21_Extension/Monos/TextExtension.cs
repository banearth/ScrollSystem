using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

namespace BanSupport
{
    public static class TextExtension
    {

        private static bool CreateTextExIfNotExist(Text text, out TextEx textEx)
        {
            if (text == null)
            {
                Debug.LogError("call 'CreateTextEx' function with null Text param !");
                textEx = null;
                return false;
            }
            else
            {
                textEx = text.gameObject.GetComponent<UITextForUGUI>();
                if (textEx == null)
                {
                    var addedComponent = text.gameObject.AddComponent<UITextForUGUI>();
                    addedComponent.label = text;
                    textEx = addedComponent;
                }
                return true;
            }
        }

        private static bool CreateTextExIfNotExist(TextMeshProUGUI textMeshPro, out TextEx textEx)
        {
            if (textMeshPro == null)
            {
                Debug.LogError("call 'CreateTextEx' function with null TextMeshPro param !");
                textEx = null;
                return false;
            }
            else
            {
                textEx = textMeshPro.gameObject.GetComponent<UITextForTextMeshPro>();
                if (textEx == null)
                {
                    var addedComponent = textMeshPro.gameObject.AddComponent<UITextForTextMeshPro>();
                    addedComponent.label = textMeshPro;
                    textEx = addedComponent;
                }
                return true;
            }
        }

        public static TextEx AddCountDown(this Text text, float time, string format, Action onComplete = null, bool timeScaleEnable = false)
        {
            if (CreateTextExIfNotExist(text, out TextEx textEx))
            {
                textEx.AddCountDown(time, new string[] { format }, onComplete, timeScaleEnable);
            }
            return textEx;
        }

        public static TextEx AddCountDown(this Text text, float time, string[] formats, Action onComplete = null, bool timeScaleEnable = false)
        {
            if (CreateTextExIfNotExist(text, out TextEx textEx))
            {
                textEx.AddCountDown(time, formats, onComplete, timeScaleEnable);
            }
            return textEx;
        }

        public static TextEx RemoveCountDown(this Text text)
        {
            if (CreateTextExIfNotExist(text, out TextEx textEx))
            {
                textEx.RemoveCountDown();
            }
            return textEx;
        }

        public static TextEx AddCountDown(this TextMeshProUGUI text, float time, string format, Action onComplete = null, bool timeScaleEnable = false)
        {
            if (CreateTextExIfNotExist(text, out TextEx textEx))
            {
                textEx.AddCountDown(time, new string[] { format }, onComplete, timeScaleEnable);
            }
            return textEx;
        }

        public static TextEx AddCountDown(this TextMeshProUGUI text, float time, string[] formats, Action onComplete = null, bool timeScaleEnable = false)
        {
            if (CreateTextExIfNotExist(text, out TextEx textEx))
            {
                textEx.AddCountDown(time, formats, onComplete, timeScaleEnable);
            }
            return textEx;
        }

        public static TextEx RemoveCountDown(this TextMeshProUGUI text)
        {
            if (CreateTextExIfNotExist(text, out TextEx textEx))
            {
                textEx.RemoveCountDown();
            }
            return textEx;
        }

    }

    public class TextEx : ExBase
    {
        public virtual void SetTextValue(string str) { }

        public virtual string GetTextValue() { return string.Empty; }

        public void AddCountDown(float time, string[] countDownFormat, Action completeAction, bool timeScaleEnable)
        {
            //todo 这里可以复用一下之前的，作为优化
            AddWidget(new TextCountDownWidget(this, time, countDownFormat, completeAction, timeScaleEnable), true);
        }

        public void RemoveCountDown()
        {
            RemoveWidget<TextCountDownWidget>();
        }

    }

    public class UITextForUGUI : TextEx
    {
        public Text label;
        public override void SetTextValue(string str)
        {
            this.label.text = str;
        }
        public override string GetTextValue()
        {
            return this.label.text;
        }
    }

    public class UITextForTextMeshPro : TextEx
    {
        public TextMeshProUGUI label;

        public override void SetTextValue(string str)
        {
            this.label.text = str;
        }
        public override string GetTextValue()
        {
            return label.text;
        }
    }

}