using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class PickableBehaviour : MonoBehaviour {
    public ItemObject item;

    //public delegate void PickupEvent(ItemObject item = null);
    public delegate void PickupEvent(ItemObject item = null);
    public static event PickupEvent OnPickup;
    public PickupEvent2 OnPickup2 = new PickupEvent2();
    
    public void OnTriggerEnter2D() {
        OnPickup?.Invoke(item);
        OnPickup2?.Invoke(item);
        item.objectsInventory.AddItem(item, 1);
        DestroyThisObject();
    }

    public void DestroyThisObject() {
        this.gameObject.SetActive(false);
    }

    public class PickupEvent2 : UnityEvent<ItemObject> {

    }
}