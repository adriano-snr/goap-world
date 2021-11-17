using ReGoap.Unity;
using ReGoap.Unity.FSMExample.OtherScripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Char01 : CustomAgent01Advanced<string, object>, IEater {
    public float TreeQuantityGoal = 10f;
    public float WaterQuantityGoal = 10f;
    public float OreQuantityGoal = 10f;
    public float AxeQuantityGoal = 1f;
    public float Energy = 10f;
    public float EnergyDuration = 3f;
    public float EnergyPerMeal = 20f;
    private float EnergyConsumptionTime;
    private GoalEat goalEat;
    protected override void Awake() {
        base.Awake();
        EnergyConsumptionTime = Time.time + EnergyDuration;
        goalEat = GetComponent<GoalEat>();
    }
    protected override bool CalculateNewGoal(bool forceStart = false, PreviousPlanResult planResult = PreviousPlanResult.Null) {
        if (IsPlanning) return false;
        if (!forceStart && (Time.time - lastCalculationTime <= CalculationDelay)) return false;
        lastCalculationTime = Time.time;

        interruptOnNextTransition = false;
        UpdatePossibleGoals();
        //var watch = System.Diagnostics.Stopwatch.StartNew();

        //##########################################
        switch (planResult) {
            case PreviousPlanResult.Null:
                break;
            case PreviousPlanResult.Failed:
                break;
            case PreviousPlanResult.Interrupted:
                break;
            case PreviousPlanResult.Suceeded:
                //currentGoal.SetPriority(possibleGoals.Min(x => x.GetPriority()) - 1);
                if (currentGoal.GetName() == Literals.GoalCollectTree || currentGoal.GetName() == Literals.GoalCollectWater || currentGoal.GetName() == Literals.GoalCollectOre || currentGoal.GetName() == Literals.GoalCollectAxe) {
                    var cGoal = currentGoal as GoalCollect;
                    var worldState = memory.GetWorldState();
                    worldState.TryGetValue("banks", out object obj);
                    var banks = obj as Dictionary<Bank, Vector3>;
                    var resourceName = ((GoalCollect)currentGoal).ResourceName;
                    var bank = banks.Keys.ElementAt(0);
                    var resourceCount = bank.GetResource(resourceName);
                    worldState.Set("own" + resourceName, resourceCount);
                    var treeGoal = goals.First(x => x.GetName() == Literals.GoalCollectTree);
                    var waterGoal = goals.First(x => x.GetName() == Literals.GoalCollectWater);
                    var oreGoal = goals.First(x => x.GetName() == Literals.GoalCollectOre);
                    var axeGoal = goals.First(x => x.GetName() == Literals.GoalCollectAxe);
                    switch (resourceName) {
                        case Literals.resourceNameTree:
                            if (resourceCount >= TreeQuantityGoal) {
                                cGoal.WarnPossibleGoal = false;
                            }
                            else {
                                cGoal.SetQuantity(Mathf.Min(3f, TreeQuantityGoal - resourceCount));
                            }
                            break;
                        case Literals.resourceNameWater:
                            if (resourceCount >= WaterQuantityGoal) {
                                cGoal.WarnPossibleGoal = false;
                            }
                            else {
                                cGoal.SetQuantity(Mathf.Min(3f, WaterQuantityGoal - resourceCount));
                            }
                            break;
                        case Literals.resourceNameOre:
                            if (resourceCount >= OreQuantityGoal) {
                                cGoal.WarnPossibleGoal = false;
                            }
                            else {
                                cGoal.SetQuantity(Mathf.Min(3f, OreQuantityGoal - resourceCount));
                            }
                            break;
                        case Literals.resourceNameAxe:
                            if (resourceCount >= AxeQuantityGoal) {
                                cGoal.WarnPossibleGoal = false;
                            }
                            break;
                        default:
                            break;
                    }
                }
                break;
            default:
                break;
        }
        if (Energy <= -10f) {
            //still > distress call
        }
        else if (Energy <= -5f) {
            //emergency feed
        }
        else if (Energy <= 0f) {
            //feed
            goalEat.WarnPossibleGoal = true;
            goalEat.SetPriority(10);
        }
        else if (Energy >= 10f) {
            goalEat.WarnPossibleGoal = false;
            goalEat.SetPriority(0);
        }
        //##########################################


        startedPlanning = true;
        currentReGoapPlanWorker = ReGoapPlannerManager<string, object>.Instance.Plan(this, BlackListGoalOnFailure ? currentGoal : null,
            currentGoal != null ? currentGoal.GetPlan() : null, OnDonePlanning);
        return true;
    }
    protected override void Update() {
        base.Update();
    }
    protected void FixedUpdate() {
        if (Time.time > EnergyConsumptionTime && Energy >= -10f) {
            Energy -= 1f;
            EnergyConsumptionTime = Time.time + EnergyDuration;
        }
        
    }
    public void Eat() {
        Energy += EnergyPerMeal;
    }
}
