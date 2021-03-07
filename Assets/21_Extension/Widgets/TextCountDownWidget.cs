using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{
    /// <summary>
    /// 用于文字上显示倒计时
    /// </summary>
    public class TextCountDownWidget : TimeWidget
    {

        /// <summary>
        /// 使用的格式
        /// </summary>
        public class Format : IDisposable
        {
            public static List<char> KEY = new List<char> { 'd', 'h', 'm', 's' };
            public static int[] KEY_UNIT_REVERSE = new int[] { 60, 60, 24 };//多少个秒组成分，多少个分组成时，多少个时组成一天

            public int index;
            public string formatStr;
            private List<string> replacement;

            public Format(string formatStr)
            {
                this.formatStr = formatStr;
                //检查这个格式对应的Index
                this.index = int.MaxValue;

                bool indexMatched = false;
                for (int i = 0; i < KEY.Count; i++)
                {
                    if (formatStr.Contains(KEY[i].ToString()))
                    {
                        this.index = i;
                        indexMatched = true;
                        break;
                    }
                }

                if (!indexMatched)
                {
                    Debug.LogError("wrong format :" + formatStr);
                }
                //Get replacement
                this.replacement = ListPool<string>.Get();
                var curKey = "";
                for (int i = 0; i < formatStr.Length; i++)
                {
                    char c = formatStr[i];
                    //连接
                    if (curKey == "")
                    {
                        if (KEY.Contains(c))
                        {
                            curKey += c;
                        }
                    }
                    else if (curKey[0] == c)
                    {
                        curKey += c;
                    }
                    //截断
                    if (curKey != "" && ((i == formatStr.Length - 1) || (curKey[0] != c)))
                    {
                        if (!replacement.Contains(curKey))
                        {
                            replacement.Add(curKey);
                        }
                        curKey = "";
                    }
                }
            }

            public string GetTime(List<int> numbers)
            {
                string result = formatStr;
                foreach (var str in replacement)
                {
                    int curNumber = numbers[KEY.IndexOf(str[0])];
                    result = result.Replace(str, curNumber.ToString("D" + str.Length));
                }
                return result;
            }

            public void Dispose()
            {
                ListPool<string>.Release(this.replacement);
                this.replacement = null;
            }

        }

        private int lastLeftSeconds;
        private List<int> numbers;
        private int minFormatIndex;//用于处理比如格式只有秒的话，那么这个秒可以用于表示全部时间
        private List<Format> formats;
        private TextEx textEx;

        public TextCountDownWidget(TextEx textEx, float time, string[] formatStrs, Action completeAction, bool timeScaleEnable) :
            base(textEx, completeAction, time, timeScaleEnable)
        {
            this.lastLeftSeconds = int.MaxValue;
            this.numbers = ListPool<int>.Get();
            for (int i = 0; i < Format.KEY.Count; i++)
            {
                this.numbers.Add(0);
            }
            this.minFormatIndex = int.MaxValue;
            this.formats = ListPool<Format>.Get();
            for (int i = 0; i < formatStrs.Length; i++)
            {
                var aFormat = new Format(formatStrs[i]);
                if (this.minFormatIndex > aFormat.index)
                {
                    this.minFormatIndex = aFormat.index;
                }
                this.formats.Add(aFormat);
            }
            this.textEx = textEx;
        }

        public override void Dispose()
        {
            base.Dispose();

            //release number
            ListPool<int>.Release(this.numbers);
            this.numbers = null;

            //release formats
            foreach (var aFormat in this.formats)
            {
                aFormat.Dispose();
            }
            ListPool<Format>.Release(this.formats);
            this.formats = null;
        }

        public override bool OnUpdate()
        {
            base.UpdateTime();
            int leftSeconds = this.GetLeftSeconds();
            if (this.lastLeftSeconds != leftSeconds)
            {
                this.lastLeftSeconds = leftSeconds;
                ParseTime(this.lastLeftSeconds);
            }
            return base.OnUpdate();
        }

        private void ParseTime(int totalSeconds)
        {
            for (int i = 0; i < numbers.Count; i++)
            {
                var numberIndex = numbers.Count - 1 - i;
                if (i < Format.KEY_UNIT_REVERSE.Length && this.minFormatIndex < numberIndex)
                {
                    numbers[numberIndex] = totalSeconds % Format.KEY_UNIT_REVERSE[i];
                    totalSeconds /= Format.KEY_UNIT_REVERSE[i];
                }
                else
                {
                    numbers[numberIndex] = totalSeconds;
                    totalSeconds = 0;
                }
            }

            //查看哪个格式被匹配了
            int formatIndex = -1;
            for (int i = 0; i < numbers.Count; i++)
            {
                if (numbers[i] > 0)
                {
                    for (int j = 0; j < formats.Count; j++)
                    {
                        var curFormat = formats[j];
                        if (curFormat.index == i)
                        {
                            formatIndex = j;
                            break;
                        }
                    }
                }
                if (formatIndex >= 0) { break; }
            }
            if (formatIndex < 0) { formatIndex += formats.Count; }
            var finalStr = this.formats[formatIndex].GetTime(this.numbers);
            this.textEx?.SetTextValue(finalStr);
        }

    }
}