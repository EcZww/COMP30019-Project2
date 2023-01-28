using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Web;
using System.IO;
public class Cooldown : MonoBehaviour
{
    [SerializeField]
    private Image imageCooldown;
    [SerializeField]
    private Text textCooldown;
    [HideInInspector]
    public float cooldownTime;
    [HideInInspector]
    public float cooldownTimeLess;
    [HideInInspector]
    public bool hasSkill = false;

    void Start()
    {
        textCooldown.gameObject.SetActive(false);
        imageCooldown.fillAmount = 0.0f;
    }

    void Update()
    {
        ApplyCooldown();
    }

    void ApplyCooldown() {
        if (!hasSkill) {
            textCooldown.gameObject.SetActive(true);
            textCooldown.text = "∞";
            textCooldown.fontSize = 70;
            imageCooldown.fillAmount = 1.0f;
            return;
        }
        if(cooldownTimeLess < 0) {
            textCooldown.gameObject.SetActive(false);
            imageCooldown.fillAmount = 0.0f;
        } else {
            textCooldown.fontSize = 40;
            textCooldown.gameObject.SetActive(true);
            textCooldown.text = Mathf.RoundToInt(cooldownTimeLess).ToString();
            imageCooldown.fillAmount = cooldownTimeLess / cooldownTime;
        }
    }

}
