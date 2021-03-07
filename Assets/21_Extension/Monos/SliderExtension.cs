using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BanSupport
{

    public static class SliderExtension
    {

        private static bool CreateSliderExIfNotExist(Slider slider, out SliderEx sliderEx)
        {
            if (slider == null)
            {
                Debug.LogError("call 'CreateSliderEx' function with null Slider param !");
                sliderEx = null;
                return false;
            }
            else
            {
                sliderEx = slider.gameObject.GetComponent<SliderEx>();
                if (sliderEx == null)
                {
                    var addedComponent = slider.gameObject.AddComponent<SliderEx>();
                    addedComponent.slider = slider;
                    sliderEx = addedComponent;
                }
                return true;
            }
        }

        public static SliderEx AddAnimTo(this Slider slider, float toNormalizedValue, float time, Action onComplete = null, bool timeScaleEnable = false)
        {
            if (CreateSliderExIfNotExist(slider, out SliderEx sliderEx))
            {
                sliderEx.AddAnimTo(toNormalizedValue, time, onComplete, timeScaleEnable);
            }
            return sliderEx;
        }

        public static SliderEx RemoveAnimTo(this Slider slider)
        {
            if (CreateSliderExIfNotExist(slider, out SliderEx sliderEx))
            {
                sliderEx.RemoveAnimTo();
            }
            return sliderEx;
        }

    }

    public class SliderEx : ExBase
    {
        public Slider slider;

        public void AddAnimTo(float toNormalizedValue, float time, Action onComplete, bool timeScaleEnable)
        {
            AddWidget(new SliderAnimToWidget(this, onComplete, time, timeScaleEnable, toNormalizedValue), true);
        }

        public void RemoveAnimTo()
        {
            RemoveWidget<SliderAnimToWidget>();
        }

    }



}