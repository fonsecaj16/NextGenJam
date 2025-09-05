using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerActivator : MonoBehaviour
{
    public Action<Collider> OnTriggered;

    private void OnTriggerEnter(Collider other)
    {
        OnTriggered?.Invoke(other);
    }
}
