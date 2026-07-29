using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorPlan : InteractableObject
{
    public override void Interact()
    {
        base.Interact();
        ViewManager.Instance.SwitchToFloorPlanView();
    }
}
