using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

[CreateAssetMenu(fileName = "New Event Manager", menuName = "Game/Events/Event Manager")]
public abstract class EventManager : ScriptableObject {
    //public delegate void PickupEvent(ItemObject item);
    //public static event PickupEvent OnPickup;
}