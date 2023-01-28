using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class BrightnessSlider : MonoBehaviour
{
    public Slider brightnessSlider;
    void OnEnable() { 
        SaveDataManager.Instance.LoadBrightness();
        brightnessSlider.value = GameManager.Instance.brightness;
    }

    void OnDisable() { 
        SaveDataManager.Instance.SaveBrightness();
    }

    public void SetBrightnessLevel(float sliderValue) {
        GameManager.Instance.brightness = sliderValue;
        Screen.brightness = sliderValue;
    }
}
