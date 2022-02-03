using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour {
    [SerializeField] private FloatReference currentHealth, maxHealth;
    [SerializeField] private float currentHP;
    [SerializeField] private float maxHP;
    [SerializeField] private float secondsTillNextPassiveRegen;
    [SerializeField] private float passiveRegenAmount;
    [SerializeField] private float passiveRegenMultiplier;
    [SerializeField] private bool hasPassiveRegen;
    [SerializeField] private bool isPassiveRegen;
    private bool hasDied = false;
    private bool isDead = false;
    private Coroutine passiveHealthRegenCor;

    [SerializeField] private float barSpeed;
    [SerializeField] private float lerpSpeed;
    [SerializeField] private bool isLerped = true;

    [SerializeField] private Image hpFill;
    [SerializeField] private Image hpDamageEffect;
    [SerializeField] private Image hpHealEffect;
    [SerializeField] private TMP_Text hpPercentageText;
    [SerializeField] private Animator headImageAnimator;
    private float currentFillAmount;

    private Coroutine fillRegenCor;
    private Coroutine damageEffectCor;
    private Coroutine healEffectCor;
    private WaitForSeconds regenTick = new WaitForSeconds(0.01f);

    private void Start() {
        Respawn();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.J)) {
            float r = Random.Range(5.0f, 10.0f);
            TakeDamage(r);
        }
        else if (Input.GetKeyDown(KeyCode.K)) {
            float r = Random.Range(5.0f, 10.0f);
            RestoreHealth(r);
        }

        currentHP = currentHealth.Value;
        maxHP = maxHealth.Value;
    }

    private void TakeDamage(float damageAmount) {
        currentHealth.Value -= damageAmount;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0.0f, maxHealth.Value);

        if (0.0f < currentHealth.Value) {
            // Create event OnDamageRecieved();
            // OnDamageRecieved.Invoke();
            headImageAnimator.SetTrigger("Damaged");
        }
        else if (currentHealth.Value <= 0.0f) {
            currentHealth.Value = 0.0f;
            isDead = true;
            if (!hasDied && isDead) {
                PlayerDeath();
                // OnPlayerDeath.Invoke();
                headImageAnimator.SetTrigger("Death");
            }
        }

        // Create event OnDamageRecieved() and: 1) Drain Health bar; 2) Play Damage sound effect; 3) Play Damage particles and/or animations.
        // Create event OnPlayerDeath() and: 1) Drain Health bar; 2) Play Death sound effect; 3) Play Death particles and/or animations; 4) Update UI;
        // Check if it was a Normal Hit (0.0f < currentHealth.Value) or a Death Hit (currentHealth.Value <= 0.0f) and play corresponding event
        ReduceHP(0.5f);
    }

    private void RestoreHealth(float healAmount) {
        currentHealth.Value += healAmount;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0.0f, maxHealth.Value);

        if (maxHealth.Value <= currentHealth.Value) {
            currentHealth.Value = maxHealth.Value;
        }

        // Create event OnHealing() and: 1) Regenerate Health bar; 2) Play Healing sound effect; 3) Play Healing particles and/or animations.
        HealHP(0.5f);
    }

    private void Respawn() {
        currentHealth.Value = maxHealth.Value;
        currentHP = currentHealth.Value;
        maxHP = maxHealth.Value;

        StopAllCoroutines();

        //OnRespawn();
        hpFill.fillAmount = currentHealth.Value / maxHealth.Value;
        hpDamageEffect.fillAmount = currentHealth.Value / maxHealth.Value;
        hpHealEffect.fillAmount = currentHealth.Value / maxHealth.Value;

        UpdateText();

        StopAllCoroutines();
    }

    private void PlayerDeath() {
        Debug.Log("You are Dead");
        hasDied = true;
        Respawn();
        hasDied = false;
        isDead = false;
    }

    private void ReduceHP(float secondsToWait = 0.0f) {
        StopCoroutinesInList();

        if (fillRegenCor != null) {
            StopCoroutine(fillRegenCor);
        }
        if (healEffectCor != null) {
            StopCoroutine(healEffectCor);
        }
        fillRegenCor = StartCoroutine(SmoothBarFilling(hpFill, fillRegenCor));
        healEffectCor = StartCoroutine(SmoothBarFilling(hpHealEffect, healEffectCor));

        if (damageEffectCor != null) {
            StopCoroutine(damageEffectCor);
        }
        damageEffectCor = StartCoroutine(SmoothBarFilling(hpDamageEffect, damageEffectCor, secondsToWait));

        if (0.0f < currentHealth.Value && currentHealth.Value < maxHealth.Value && hasPassiveRegen) {
            if (passiveHealthRegenCor != null) {
                StopCoroutine(passiveHealthRegenCor);
            }
            secondsTillNextPassiveRegen = secondsToWait + 2.5f;
            passiveHealthRegenCor = StartCoroutine(PassiveHealthRegeneration(secondsTillNextPassiveRegen));
        }
    }

    private void HealHP(float secondsToWait = 0.0f) {
        StopCoroutinesInList();

        if (fillRegenCor != null) {
            StopCoroutine(fillRegenCor);
        }
        fillRegenCor = StartCoroutine(SmoothBarFilling(hpFill, fillRegenCor, secondsToWait));

        if (healEffectCor != null) {
            StopCoroutine(healEffectCor);
        }
        if (damageEffectCor != null) {
            StopCoroutine(damageEffectCor);
        }
        healEffectCor = StartCoroutine(SmoothBarFilling(hpHealEffect, healEffectCor));
        damageEffectCor = StartCoroutine(SmoothBarFilling(hpDamageEffect, damageEffectCor));

        if (0.0f < currentHealth.Value && currentHealth.Value < maxHealth.Value && hasPassiveRegen) {
            if (passiveHealthRegenCor != null) {
                StopCoroutine(passiveHealthRegenCor);
            }
            secondsTillNextPassiveRegen = secondsToWait + 0.25f;
            passiveHealthRegenCor = StartCoroutine(PassiveHealthRegeneration(secondsTillNextPassiveRegen));
        }
    }

    private IEnumerator SmoothBarFilling(Image fillBar, Coroutine coroutine, float secondsToWait = 0.0f) {
        GetCurrentFill();
        yield return new WaitForSeconds(secondsToWait);
        
        bool lerpFillMode = isLerped ? true : false;

        while (fillBar.fillAmount != currentFillAmount) {
            if (lerpFillMode) {
                fillBar.fillAmount = Mathf.Lerp(fillBar.fillAmount, currentFillAmount, Time.deltaTime * lerpSpeed);
            }
            else {
                fillBar.fillAmount = Mathf.MoveTowards(fillBar.fillAmount, currentFillAmount, Time.deltaTime * barSpeed);
            }
            
            UpdateText();
            yield return regenTick;

            coroutine = null;
        }
        coroutine = null;
    }

    private IEnumerator PassiveHealthRegeneration(float secondsToWait = 0.0f) {
        isPassiveRegen = false;
        yield return new WaitForSeconds(secondsToWait);
        isPassiveRegen = true;

        while (currentHealth.Value != maxHealth.Value) {
            currentHealth.Value += (passiveRegenAmount * passiveRegenMultiplier * Time.deltaTime);
            currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0.0f, maxHealth.Value);

            UpdateText();

            UpdateBars();

            yield return regenTick;
        }

        while (currentHealth.Value == maxHealth.Value && hpFill.fillAmount != hpHealEffect.fillAmount) {
            UpdateBars();

            yield return regenTick;
            isPassiveRegen = false;
            passiveHealthRegenCor = null;
        }
        isPassiveRegen = false;
        passiveHealthRegenCor = null;
    }

    private void GetCurrentFill() {
        currentFillAmount = currentHealth.Value / maxHealth.Value;
        currentFillAmount = Mathf.Clamp(currentFillAmount, 0.0f, 1.0f);
    }

    private void UpdateBars() {
        GetCurrentFill();
        hpHealEffect.fillAmount = currentFillAmount;
        hpDamageEffect.fillAmount = hpHealEffect.fillAmount;

        bool lerpFillMode = isLerped ? true : false;
        
        if (lerpFillMode) {
            hpFill.fillAmount = Mathf.Lerp(hpFill.fillAmount, hpHealEffect.fillAmount, Time.deltaTime * lerpSpeed * 0.5f);
        }
        else {
            hpFill.fillAmount = Mathf.MoveTowards(hpFill.fillAmount, hpHealEffect.fillAmount, Time.deltaTime * barSpeed * 0.5f);
        }
    }

    private void UpdateText() {
        hpPercentageText.text = string.Format("{00:#.0}%", currentHealth.Value);
        if (currentHealth.Value == maxHealth.Value) hpPercentageText.text = "100%";
    }

    private void StopCoroutinesInList() {
        StopAllCoroutines();
    }
}