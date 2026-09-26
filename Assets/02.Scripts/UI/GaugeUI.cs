using UnityEngine;
using UnityEngine.UI;
using System;

namespace UI
{
    public class GaugeUI: BaseUI
    {
        [SerializeField] private Slider slider;
        
        public void Configure(
            float minValue = 0f, float maxValue = 100f,
            bool interactable = false, Navigation navigation = default, bool wholeNumbers = false)
        {
            if (slider == null)
            {
                Debug.LogError("[GaugeUI] Slider reference is missing.", this);
                return;
            }

            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.interactable = interactable;
            slider.navigation = navigation;
            slider.wholeNumbers = wholeNumbers;
        }

        public override void Init()
        {
            
        }

        public void SetGauge(float value, float maxValue)
        {
            if (slider == null)
            {
                Debug.LogError("[GaugeUI] Slider reference is missing.", this);
                return;
            }

            slider.maxValue = maxValue;
            slider.value = Mathf.Clamp(value, slider.minValue, slider.maxValue);
        }
    }
}