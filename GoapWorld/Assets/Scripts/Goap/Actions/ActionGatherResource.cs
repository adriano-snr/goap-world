using ReGoap.Core;
using ReGoap.Unity;
using ReGoap.Unity.FSMExample.OtherScripts;
using ReGoap.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ActionGatherResource : ReGoapAction<string, object> {
    public float MaxResourcesCount = 5.0f;
    public float ResourcesCostMultiplier = 10.0f;
    public float ReservedCostMultiplier = 50.0f;

    public bool ExpandOnAllResources = false;

    public float TimeToGather = 0.5f;
    public float ResourcePerAction = 1f;
    protected ResourcesBag bag;
    protected Vector3? resourcePosition;
    protected IResource resource;

    private float gatherCooldown;

    protected override void Awake() {
        base.Awake();
        TimeToGather = UnityEngine.Random.Range(0.3f, 0.6f);
        bag = GetComponent<ResourcesBag>();
        if (bag == null) bag = GetComponentInParent<ResourcesBag>();
    }

    protected virtual string GetNeededResourceFromGoal(ReGoapState<string, object> goalState) {
        foreach (var pair in goalState.GetValues()) {
            if (pair.Key.StartsWith(Literals.hasResource)) {
                return pair.Key.Substring(Literals.hasResource.Length);
            }
        }
        return null;
    }

    public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData) {
        preconditions.Clear();
        if (stackData.settings.HasKey("resource") && stackData.settings.TryGetValue("resourcePosition", out var resourcePosition)) {
            preconditions.Set("isAtPosition", resourcePosition);
            //########################################################################
            stackData.settings.TryGetValue("resource", out var obj);
            var resource = (IResource)obj;
            if (resource != null) {
                var resourceName = resource.GetName();
                if (stackData.goalState.HasKey(Literals.HasQuantityResource(resourceName))) {
                    stackData.goalState.TryGetValue(Literals.HasQuantityResource(resourceName), out var quantity);
                    if ((float)quantity > 1f) {
                        preconditions.Set(Literals.HasResource(resourceName), true);
                        preconditions.Set(Literals.HasQuantityResource(resourceName), (float)quantity - 1f);
                    }
                }
            }
            else {
                preconditions.Set(Literals.impossibleGoal, true);
            }
            //########################################################################
        }
        return preconditions;
    }
    public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData) {
        effects.Clear();
        if (stackData.settings.TryGetValue("resource", out var obj)) {
            var resource = (IResource)obj;
            if (resource != null) {
                var resourceName = resource.GetName();
                effects.Set(Literals.HasResource(resourceName), true);
                //########################################################################
                if (preconditions.HasKey(Literals.HasQuantityResource(resourceName))) {
                    preconditions.TryGetValue(Literals.HasQuantityResource(resourceName), out var quantity);
                    effects.Set(Literals.HasQuantityResource(resourceName), (float)quantity + 1f);
                }
                else {
                    effects.Set(Literals.HasQuantityResource(resourceName), 1f);
                }
                //########################################################################
            }
        }
        return effects;
    }

    public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData) {
        var newNeededResourceName = GetNeededResourceFromGoal(stackData.goalState);
        settings.Clear();
        if (newNeededResourceName != null && stackData.currentState.HasKey("resource" + newNeededResourceName)) {
            var results = new List<ReGoapState<string, object>>();
            ReGoap.Unity.FSMExample.Sensors.ResourcePair best = new ReGoap.Unity.FSMExample.Sensors.ResourcePair();
            var bestScore = float.MaxValue;
            foreach (var wantedResource in (List<ReGoap.Unity.FSMExample.Sensors.ResourcePair>)stackData.currentState.Get("resource" + newNeededResourceName)) {
                if (wantedResource.resource.GetCapacity() < ResourcePerAction) continue;
                // expanding on all resources is VERY expansive, expanding on the closest one is usually the best decision
                if (ExpandOnAllResources) {
                    settings.Set("resourcePosition", wantedResource.position);
                    settings.Set("resource", wantedResource.resource);
                    results.Add(settings.Clone());
                }
                else {
                    //var tryGetResult = stackData.goalState.TryGetValue("isAtPosition", out object isAtPosition);
                    var tryGetResult = stackData.currentState.TryGetValue("startPosition", out object isAtPosition);
                    var score = tryGetResult ? (wantedResource.position - (Vector3)isAtPosition).magnitude : 0.0f;
                    //var score = 0f;
                    if (tryGetResult) {
                        var delta = wantedResource.position - (Vector3)isAtPosition;
                        score = delta.magnitude;
                    }
                    score += ReservedCostMultiplier * wantedResource.resource.GetReserveCount();
                    score += ResourcesCostMultiplier * (MaxResourcesCount - wantedResource.resource.GetCapacity());
                    if (score < bestScore) {
                        bestScore = score;
                        best = wantedResource;
                    }
                }
            }
            if (!ExpandOnAllResources) {
                settings.Set("resourcePosition", best.position);
                settings.Set("resource", best.resource);
                results.Add(settings.Clone());
            }
            return results;
        }
        return new List<ReGoapState<string, object>>();
    }

    public override float GetCost(GoapActionStackData<string, object> stackData) {
        var extraCost = 0.0f;
        if (stackData.settings.HasKey("resource")) {
            var resource = (Resource)stackData.settings.Get("resource");
            extraCost += ReservedCostMultiplier * resource.GetReserveCount();
            extraCost += ResourcesCostMultiplier * (MaxResourcesCount - resource.GetCapacity());


            //Aumenta o custo de coleta de recurso em 10 vezes se já estiver carregando algum (Simulação rudimentar de peso).
            var resources = MultipleResourcesManager.Instance.Resources.Keys.ToList();
            for (int i = 0; i < resources.Count; i++) {
                var resourceManager = MultipleResourcesManager.Instance.Resources[resources[i]];
                var resourceName = resourceManager.GetResourceName();
                if (stackData.goalState.HasKey(Literals.HasResource(resourceName))) {
                    var val = (bool)stackData.goalState.Get(Literals.HasResource(resourceName));
                    if (val) extraCost = extraCost * 10;
                }
            }
        }
        return base.GetCost(stackData) + extraCost;
    }

    public override void Exit(IReGoapAction<string, object> next) {
        base.Exit(next);

        //var worldState = agent.GetMemory().GetWorldState();
        //foreach (var pair in effects.GetValues()) {
        //    worldState.Set(pair.Key, pair.Value);
        //}
    }
    public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData) {
        return base.CheckProceduralCondition(stackData) && bag != null && stackData.settings.HasKey("resource");
    }

    public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, ReGoapState<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail) {
        base.Run(previous, next, settings, goalState, done, fail);

        var thisSettings = settings;
        resourcePosition = (Vector3)thisSettings.Get("resourcePosition");
        resource = (IResource)thisSettings.Get("resource");

        if (resource == null || resource.GetCapacity() < ResourcePerAction)
            failCallback(this);
        else {
            gatherCooldown = Time.time + TimeToGather;
        }
    }

    public override void PlanEnter(IReGoapAction<string, object> previousAction, IReGoapAction<string, object> nextAction, ReGoapState<string, object> settings, ReGoapState<string, object> goalState) {
        if (settings.HasKey("resource")) {
            ((IResource)settings.Get("resource")).Reserve(GetHashCode());
        }
    }
    public override void PlanExit(IReGoapAction<string, object> previousAction, IReGoapAction<string, object> nextAction, ReGoapState<string, object> settings, ReGoapState<string, object> goalState) {
        if (settings.HasKey("resource")) {
            ((IResource)settings.Get("resource")).Unreserve(GetHashCode());
        }
    }
    protected void Update() {
        if (resource == null || resource.GetCapacity() < ResourcePerAction) {
            failCallback(this);
            return;
        }
        if (Time.time > gatherCooldown) {
            gatherCooldown = float.MaxValue;
            ReGoapLogger.Log("[GatherResourceAction] acquired " + ResourcePerAction + " " + resource.GetName());
            resource.RemoveResource(ResourcePerAction);
            bag.AddResource(resource.GetName(), ResourcePerAction);

            StatCounter.AddStat(transform.parent.name, $"Gather { resource.GetName() }", ResourcePerAction);

            doneCallback(this);
            if (settings.HasKey("resource")) {
                ((IResource)settings.Get("resource")).Unreserve(GetHashCode());
            }
        }
    }
}