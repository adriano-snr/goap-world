using ReGoap.Unity;
using ReGoap.Unity.FSMExample.OtherScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomBagSensor : ReGoapSensor<string, object> {
    private ResourcesBag resourcesBag;

    void Awake() {
        resourcesBag = GetComponentInParent<ResourcesBag>();
    }

    public override void UpdateSensor() {
        var state = memory.GetWorldState();
        foreach (var pair in resourcesBag.GetResources()) {
            state.Set(Literals.HasResource(pair.Key), pair.Value > 0);
            state.Set(Literals.HasQuantityResource(pair.Key), pair.Value);
        }
    }
}
