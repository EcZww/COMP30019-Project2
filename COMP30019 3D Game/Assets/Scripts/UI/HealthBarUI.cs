using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthBarUI : MonoBehaviour
{
    public GameObject healthUIPrefab;
    public Transform barPoint;
    public bool alwaysVisible = false;
    public float visibleTime;
    [HideInInspector]
    public float timeLess;
    [HideInInspector]
    public Image healthSliderImage;
    public Text HealthValue;
    public Transform UIbar;
    new Transform camera;
    CharacterStats currentStats;

    void Awake()
    {
        currentStats = GetComponent<CharacterStats>();
        currentStats.UpdateHealthBarOnAttack += UpdateHealthBar;
    }

    void Start()
    {
        camera = Camera.main.transform;
        foreach(Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if(canvas.CompareTag("EnemyHealthBar"))
            {
                UIbar = Instantiate(healthUIPrefab, canvas.transform).transform;
                healthSliderImage = UIbar.GetChild(0).GetComponent<Image>();
                HealthValue = UIbar.GetChild(1).GetComponent<Text>();
                UIbar.gameObject.SetActive(alwaysVisible);
            }
        }
    }

    void LateUpdate() {
        if(UIbar != null)
        {
            UIbar.position = barPoint.position;
            UIbar.forward = camera.forward;
            if (timeLess<=0 && !alwaysVisible) UIbar.gameObject.SetActive(false);
            else timeLess -= Time.deltaTime;
        }
    }

    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        UIbar.gameObject.SetActive(true);
        timeLess = visibleTime;
        float healthRatio = (float)currentHealth/maxHealth;
        healthSliderImage.fillAmount = healthRatio;
        HealthValue.text = currentHealth + "/" + maxHealth;
    }
}
