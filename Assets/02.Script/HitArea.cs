using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitArea : MonoBehaviour
{

    public Action<Collider> CollisionStayEvent;
    public Action<Collider> CollisionExitEvent;

    private void OnTriggerEnter(Collider other) { CollisionStayEvent(other); }
    //private void OnTriggerExit(Collider other) { CollisionExitEvent(other); }
}
