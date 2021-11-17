using ReGoap.Core;
using ReGoap.Unity;
using ReGoap.Unity.FSMExample.FSM;
using ReGoap.Unity.FSMExample.OtherScripts;
using ReGoap.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionEat : ReGoapAction<string, object> {
    protected ResourcesBag bag;
    protected override void Awake() {
        base.Awake();
        bag = GetComponent<ResourcesBag>();
        if (bag == null) bag = GetComponentInParent<ResourcesBag>();
    }
    public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData) {
        preconditions.Clear();
        preconditions.Set(Literals.HasResource(Literals.resourceNameOre), true);
        preconditions.Set(Literals.HasResource(Literals.resourceNameTree), true);
        preconditions.Set(Literals.HasResource(Literals.resourceNameWater), true);

        return preconditions;
    }
    public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData) {
        effects.Clear();

        //settings.TryGetValue(Literals.HasQuantityResource(Literals.resourceNameOre), out var quantity);
        //effects.Set(Literals.HasQuantityResource(Literals.resourceNameOre), (float)quantity - 1f);
        effects.Set(Literals.hasEaten, true);

        return effects;
    }
    public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData) {
        settings.Clear();
        var results = new List<ReGoapState<string, object>>();

        //stackData.currentState.TryGetValue(liter)

        results.Add(settings.Clone());
        return results;
    }
    public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, ReGoapState<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail) {
        base.Run(previous, next, settings, goalState, done, fail);
        var char1 = agent as IEater;
        char1.Eat();
        //if (bag.GetResource(Literals.resourceNameOre) > 0)
            bag.RemoveResource(Literals.resourceNameOre, 1f);
        //if (bag.GetResource(Literals.resourceNameTree) > 0)
            bag.RemoveResource(Literals.resourceNameTree, 1f);
        //if (bag.GetResource(Literals.resourceNameWater) > 0)
            bag.RemoveResource(Literals.resourceNameWater, 1f);
        doneCallback(this);
    }
}
