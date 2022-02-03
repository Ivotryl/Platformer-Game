using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventory : MonoBehaviour {
    public InventoryObject foodInventory;
    public InventoryObject coinInventory;

    public List<InventorySlot> foodList = new List<InventorySlot>();
    public List<InventorySlot> coinList = new List<InventorySlot>();
    
    private void Start() {
        ClearLists(coinInventory, coinList);
    }

    private void OnEnable() {
        //Subscribe to OnPickup() event;
        PickableBehaviour.OnPickup += UpdateList;
        //Subscribirse buscando FindOfType<PickableBehaviour>();
    }

    private void OnDisable() {
        //Unsubscribe to OnPickup() event;
        PickableBehaviour.OnPickup -= UpdateList;
    }

    private void OnApplicationQuit() {
        ClearLists(coinInventory, coinList);
    }

    public void UpdateList(ItemObject pickedItem) {
        //item.objectsInventory.AddItem(item, 1);
        //pickedItem.objectsInventory.AddItem(pickedItem, 1);
        //coinList = coinInventory.Container;
    }

    private void ClearLists(InventoryObject inventory, List<InventorySlot> list) {
        for (int i = 0; i < inventory.Container.Count; i++) {
            inventory.Container[i].ClearAmount();
        }
        list = inventory.Container;
    }
}