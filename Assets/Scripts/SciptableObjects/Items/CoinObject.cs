using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Coin Object", menuName = "Game/Pickable Items/Coin")]
public class CoinObject : ItemObject {
    public float goldValue;

    public void Awake() {
        type = ItemType.Coin;
    }
}