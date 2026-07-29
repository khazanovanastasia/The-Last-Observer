using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monitor : InteractableObject
{
    public override void Interact()
    {
        base.Interact();
        ViewManager.Instance.SwitchToOverview();
    }
}
