using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum ItemType {
    Food,
    Coin
}

public enum CoinType {
    Copper,
    Silver,
    Gold
}

public abstract class ItemObject : ScriptableObject {
    public ItemType type;
    public CoinType coinType;
    
    public InventoryObject objectsInventory;
    [TextArea(15, 20)] public string description;
}