using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Food Object", menuName = "Game/Pickable Items/Food")]
public class FoodObject : ItemObject {
    public float creditsValue;
    public float healthRestoreValue;

    public void Awake() {
        type = ItemType.Food;
    }
}