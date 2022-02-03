using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GUIManager : MonoBehaviour {
    [SerializeField] private TMP_Text copperCoinsText;
    [SerializeField] private TMP_Text silverCoinsText;
    [SerializeField] private TMP_Text goldCoinsText;

    private int copperCoinsAmount;
    private int silverCoinsAmount;
    private int goldCoinsAmount;

    private void Start() {
        copperCoinsAmount = 0;
        silverCoinsAmount = 0;
        goldCoinsAmount = 0;

        UpdateText(copperCoinsText, copperCoinsAmount);
        UpdateText(silverCoinsText, silverCoinsAmount);
        UpdateText(goldCoinsText, goldCoinsAmount);
    }
    
    private void OnEnable() {
        PickableBehaviour.OnPickup += UpdateCoinCounters;
    }

    private void OnDisable() {
        PickableBehaviour.OnPickup -= UpdateCoinCounters;
    }

    private void UpdateCoinCounters(ItemObject item) {
        switch (item.coinType) {
            case (CoinType)0:
                copperCoinsAmount++;
                UpdateText(copperCoinsText, copperCoinsAmount);
            break;

            case (CoinType)1:
                silverCoinsAmount++;
                UpdateText(silverCoinsText, silverCoinsAmount);
            break;

            case (CoinType)2:
                goldCoinsAmount++;
                UpdateText(goldCoinsText, goldCoinsAmount);
            break;
        }
    }

    private void UpdateText(TMP_Text text, int amount) {
        text.text = amount.ToString();
    }
}
