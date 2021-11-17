using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReGoap.Unity;

public class Agent01Goal : ReGoapGoal<string, object>
{
    protected override void Awake() {
        base.Awake();
        goal.Set("myRequirement", 1);
    }
}
