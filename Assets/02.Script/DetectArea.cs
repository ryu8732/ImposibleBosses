using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectArea : MonoBehaviour
{
    public Action<Collider> CollisionStayEvent;
    public Action<Collider> CollisionExitEvent;

    private void OnTriggerStay(Collider other) { CollisionStayEvent(other); }
    private void OnTriggerExit(Collider other) { CollisionExitEvent(other); }
}
