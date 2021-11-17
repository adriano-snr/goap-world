using System;
using System.Collections.Generic;

using ReGoap.Core;
using ReGoap.Unity;
using ReGoap.Unity.FSMExample.OtherScripts;
using ReGoap.Utilities;

using UnityEngine;

public class ActionCustomCraftRecipe : ReGoapAction<string, object> {
    public ScriptableObject RawRecipe;
    private IRecipe recipe;
    private ResourcesBag resourcesBag;
    private List<ReGoapState<string, object>> settingsList;

    protected override void Awake() {
        base.Awake();
        recipe = RawRecipe as IRecipe;
        if (recipe == null)
            throw new UnityException("[CraftRecipeAction] The rawRecipe ScriptableObject must implement IRecipe.");
        resourcesBag = GetComponentInParent<ResourcesBag>();

        // could implement a more flexible system that handles dynamic resources's count
        foreach (var pair in recipe.GetNeededResources()) {
            preconditions.Set(Literals.HasResource(pair.Key), true);
        }
        //effects.Set(Literals.HasResource(recipe.GetCraftedResource()), true);
        //effects.Set(Literals.HasQuantityResource(recipe.GetCraftedResource()), 1f);

        settingsList = new List<ReGoapState<string, object>>();
    }

    public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData) {
        if (settingsList.Count == 0)
            CalculateSettingsList(stackData);
        return settingsList;
    }
    public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData) {
        effects.Clear();

        effects.Set(Literals.HasResource(recipe.GetCraftedResource()), true);
        if (stackData.goalState.HasKey(Literals.HasQuantityResource(recipe.GetCraftedResource()))) {
            var amount = (float)stackData.goalState.Get(Literals.HasQuantityResource(recipe.GetCraftedResource()));
            effects.Set(Literals.HasQuantityResource(recipe.GetCraftedResource()), amount);
        }
        else {
            effects.Set(Literals.HasQuantityResource(recipe.GetCraftedResource()), 1f);
        }
        //if (stackData.goalState.HasKey("gatheredFromBank" + recipe.GetCraftedResource())) {
        //    effects.Set("gatheredFromBank" + recipe.GetCraftedResource(), false);
        //}
        effects.Set("crafted" + recipe.GetCraftedResource(), true);
        return effects;
    }
    public override float GetCost(GoapActionStackData<string, object> stackData) {
        float weight = 0f;
        if (!stackData.goalState.HasKey("crafted" + recipe.GetCraftedResource())) {
            weight = 1000;
        }
        return base.GetCost(stackData) + weight;
    }

    private void CalculateSettingsList(GoapActionStackData<string, object> stackData) {
        settingsList.Clear();
        // push all available workstations
        foreach (var workstationsPair in (Dictionary<CustomWorkstation, Vector3>)stackData.currentState.Get("workstations")) {
            settings.Set("workstation", workstationsPair.Key);
            settings.Set("workstationPosition", workstationsPair.Value);
            //if (stackData.goalState.HasKey("gatherFromBank" + recipe.GetCraftedResource())) {
            //    var allowFromBank = (bool)stackData.goalState.Get("gatherFromBank" + recipe.GetCraftedResource());
            //    if (!allowFromBank)
            //        settings.Set("gatherFromBank" + recipe.GetCraftedResource(), false);
            //}
            settingsList.Add(settings.Clone());
        }
    }

    public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData) {
        return base.CheckProceduralCondition(stackData) && stackData.settings.HasKey("workstation");
    }

    public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData) {
        if (stackData.settings.TryGetValue("workstationPosition", out var workstationPosition))
            preconditions.Set("isAtPosition", workstationPosition);
        return preconditions;
    }

    public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, ReGoapState<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail) {
        base.Run(previous, next, settings, goalState, done, fail);
        var workstation = settings.Get("workstation") as CustomWorkstation;
        if (workstation != null && workstation.CraftResource(resourcesBag, recipe)) {
            ReGoapLogger.Log("[CraftRecipeAction] crafted recipe " + recipe.GetCraftedResource());
            done(this);
            StatCounter.AddStat(transform.parent.name, $"Crafted { recipe.GetCraftedResource() }", 1);
        }
        else {
            fail(this);
        }
    }
}
