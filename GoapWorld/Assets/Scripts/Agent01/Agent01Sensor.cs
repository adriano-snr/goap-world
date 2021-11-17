using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReGoap.Unity;

public class Agent01Sensor : ReGoapSensor<string, object>
{
    void FixedUpdate() {
        var worldState = memory.GetWorldState();
        worldState.Set("mySensorValue", 1); // like always myValue can be anything... it's a GoapState :)
    }
}
