using ReGoap.Core;
using ReGoap.Unity;
using ReGoap.Unity.FSMExample.OtherScripts;
using ReGoap.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionRetrieveFromBank : ReGoapAction<string, object> {
    protected ResourcesBag bag;
    protected CustomBank myBank;

    protected override void Awake() {
        base.Awake();
        bag = GetComponent<ResourcesBag>();
        if (bag == null) bag = GetComponentInParent<ResourcesBag>();
    }

    protected virtual string GetNeededResourceFromGoal(ReGoapState<string, object> goalState) {
        foreach (var pair in goalState.GetValues()) if (pair.Key.StartsWith(Literals.hasResource)) return pair.Key.Substring(Literals.hasResource.Length);
        return null;
    }

    public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData) {
        preconditions.Clear();
        if (stackData.settings.HasKey("resourceName") &&
            stackData.settings.TryGetValue("resourceName", out var resourceName) &&
            stackData.settings.TryGetValue("isAtPosition", out var resourcePosition)) {

            preconditions.Set("isAtPosition", resourcePosition);
            preconditions.Set("ownAny" + (string)resourceName, true);
        }
        return preconditions;
    }
    public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData) {
        effects.Clear();
        if (stackData.settings.TryGetValue("resourceName", out var obj)) {
            var resourceName = (string)stackData.settings.Get("resourceName");
            if (stackData.currentState.HasKey("own" + resourceName)/* && !stackData.goalState.HasKey("crafted" + resourceName)*/) {
                effects.Set(Literals.HasResource(resourceName), true);
                var owned = (float)stackData.currentState.Get("own" + resourceName);
                effects.Set("own" + resourceName, owned - 1f);
                effects.Set("ownAny" + resourceName, (owned - 1f) > 0);
                //effects.Set("gatheredFromBank" + resourceName, true);
            }
        }
        return effects;
    }

    public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData) {
        var newNeededResourceName = GetNeededResourceFromGoal(stackData.goalState);
        settings.Clear();
        if (newNeededResourceName != null) {
            var results = new List<ReGoapState<string, object>>();
            var banks = (Dictionary<CustomBank, Vector3>)agent.GetMemory().GetWorldState().Get("banks");
            var keys = banks.Keys.ToList();
            var myBank = keys[0];
            settings.Set("myBank", myBank);
            settings.Set("isAtPosition", banks[myBank]);
            settings.Set("resourceName", newNeededResourceName);
            results.Add(settings.Clone());
            return results;
        }
        return new List<ReGoapState<string, object>>();
    }

    public override float GetCost(GoapActionStackData<string, object> stackData) {
        var extraCost = -1000.0f;
        return base.GetCost(stackData) + extraCost;
    }
    public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData) {
        return 
            base.CheckProceduralCondition(stackData) && 
            bag != null &&
            //stackData.settings.HasKey("resource") && 
            agent.GetMemory().GetWorldState().HasKey("seeBank") && 
            (bool)agent.GetMemory().GetWorldState().Get("seeBank");
    }

    public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, ReGoapState<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail) {
        base.Run(previous, next, settings, goalState, done, fail);

        if (!settings.HasKey("myBank") || !settings.HasKey("resourceName")) fail(this);

        var resourceName = settings.Get("resourceName") as string;
        var myBank = (CustomBank)settings.Get("myBank");
        var ws = agent.GetMemory().GetWorldState();
        var resourceCount = myBank.GetResource(resourceName);

        if (myBank.RemoveResource(bag, resourceName, 1f)) {

            resourceCount = resourceCount - 1f;

            ws.Set("own" + resourceName, resourceCount);
            ws.Set("ownAny" + resourceName, resourceCount > 0);
            StatCounter.AddStat(transform.parent.name, $"Taken from bank { resourceName }", 1);
            done(this);
        }
        else {
            ws.Set("own" + resourceName, resourceCount);
            ws.Set("ownAny" + resourceName, resourceCount > 0);

            fail(this);
        }
    }
}