using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarUI : MonoBehaviour
{
    Text levelText;
    Text healthText;
    Image healthSliderImage;
    Image expSliderImage;

    void Awake() {
        //DontDestroyOnLoad(this.gameObject);
        levelText = transform.GetChild(5).GetComponent<Text>();
        healthSliderImage = transform.GetChild(0).GetComponent<Image>();
        healthText = transform.GetChild(3).GetComponent<Text>();
        expSliderImage = transform.GetChild(4).GetChild(0).GetComponent<Image>();
    }

    void Update() {
        if (GameManager.Instance.playerStats!=null) levelText.text = "Level   " + GameManager.Instance.playerStats.characterData.currentLevel.ToString("00");
        UpdateHealth();
        UpdateExp();
    }

    void UpdateHealth() {
        if (GameManager.Instance.playerStats != null) {
            float slider = (float)GameManager.Instance.playerStats.CurrentHealth / GameManager.Instance.playerStats.MaxHealth;
            healthSliderImage.fillAmount = slider;
            healthSliderImage.color = new Color(1-slider, slider, 0, 1);
            healthText.text = GameManager.Instance.playerStats.CurrentHealth + "/" + GameManager.Instance.playerStats.MaxHealth;
        }
    }

    void UpdateExp() {
        if (GameManager.Instance.playerStats != null) {
            float slider = (float)GameManager.Instance.playerStats.characterData.currentExp / GameManager.Instance.playerStats.characterData.baseExp;
            expSliderImage.fillAmount = slider;
        }
    }
}
