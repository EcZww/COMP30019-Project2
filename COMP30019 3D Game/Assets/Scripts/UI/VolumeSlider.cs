using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider volumeSlider;
    void OnEnable() { 
        SaveDataManager.Instance.LoadVolume();
        volumeSlider.value = GameManager.Instance.volume;
    }

    void OnDisable() { 
        SaveDataManager.Instance.SaveVolume();
    }

    public void SetVolumeLevel(float sliderValue) {
        GameManager.Instance.volume = sliderValue;
        mixer.SetFloat("MusicVol", Mathf.Log10(sliderValue) * 20);
    }
}
