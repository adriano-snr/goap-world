using ReGoap.Unity;
using ReGoap.Unity.FSMExample.OtherScripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Char02 : AgentChildComponents, IEater {
    public bool autoRandomize = true;

    /// <summary>
    /// Resistência aos elementos. Quanto maior, mais tempo leva para querer uma casa.
    /// </summary>
    public int Endurance = -1;
    private string CharName { get { return gameObject.name; } }


    public float TreeQuantityGoal = 10f;
    public float WaterQuantityGoal = 10f;
    public float OreQuantityGoal = 10f;
    public float AxeQuantityGoal = 1f;

    /// <summary>
    /// O quão alimentado o personagem está
    /// </summary>
    public float FoodLevel = 10f;
    /// <summary>
    /// Segundos que cada unidade de alimento dura.
    /// </summary>
    public float FoodUnitDuration = 3f;
    /// <summary>
    /// Unidades de alimento recebida por refeição.
    /// </summary>
    public float FoodUnitPerMeal = 20f;
    public bool WarforgedBody = true;
    //private float EnergyConsumptionTime;
    private GoalEat goalEat;
    private GoalBuildStructure goalBuildStructure;
    private GoalStockpileResource goalStockpileResource;
    public GameObject structureToBuild;

    protected override void Awake() {
        base.Awake();
        if (autoRandomize) AutoRandomize();
        //EnergyConsumptionTime = Time.time + FoodUnitDuration;
        goalEat = GetComponentInChildren<GoalEat>();
        goalBuildStructure = GetComponentInChildren<GoalBuildStructure>();
        goalStockpileResource = GetComponentInChildren<GoalStockpileResource>();
        if (Endurance == -1) Endurance = Random.Range(0, 100);
    }
    protected override void Start() {
        base.Start();
        if (goalBuildStructure != null) {
            StartCoroutine(EnduranceUpdate());
        }
        if (goalEat != null) {
            StartCoroutine(HungerUpdate());
        }
        if (goalStockpileResource != null) {
            //StartCoroutine(HungerUpdate());
        }
    }

    void AutoRandomize() {
        Endurance = Random.Range(10, 120);
        FoodLevel = Random.Range(5, 20);
        FoodUnitDuration = Random.Range(5, 10);
        FoodUnitPerMeal = Random.Range(10, 30);
        TreeQuantityGoal = Random.Range(3, 20);
        WaterQuantityGoal = Random.Range(3, 20);
        OreQuantityGoal = Random.Range(3, 20);
        AxeQuantityGoal = 1;
    }

    protected override bool CalculateNewGoal(bool forceStart = false, PreviousPlanResult planResult = PreviousPlanResult.Null) {
        if (IsPlanning) return false;
        if (!forceStart && (Time.time - lastCalculationTime <= CalculationDelay)) return false;
        lastCalculationTime = Time.time;

        interruptOnNextTransition = false;
        UpdatePossibleGoals();
        //var watch = System.Diagnostics.Stopwatch.StartNew();
        //##########################################
        if (currentGoal != null && planResult == PreviousPlanResult.Null) {//Adicionação devido a problema de temporização entre loop de update e pushaction. Loop update não tem informação de parâmetros de estado de plano que push action passa.
            planResult = ((ReGoapGoal<string, object>)currentGoal).planState;
        }

        var ws = memory.GetWorldState();
        if (!ws.HasKey("homePosition")) ws.Set("homePosition", transform.position);
        if (goalStockpileResource != null) {
            var amountOwned = 0f;
            if (ws.HasKey("own" + goalStockpileResource.ResourceName)) {
                amountOwned = (float)ws.Get("own" + goalStockpileResource.ResourceName);
            }
            if (amountOwned < goalStockpileResource.ResourceQuantityGoal) {
                goalStockpileResource.WarnPossibleGoal = true;
            }
        }
        switch (planResult) {
            case PreviousPlanResult.Null:
                if (goalBuildStructure != null && !ws.HasKey(Literals.myHouseLocation)) {
                    ws.Set(Literals.currentBuildObjectTag, Literals.myHouseGameObject);
                    ws.Set(Literals.currentBuildStructureTag, Literals.myHouseStructure);
                    var locations = FindBuildLocation(structureToBuild.GetComponent<HouseController>());
                    if (locations.Count == 0) {
                        goalBuildStructure.WarnPossibleGoal = false;
                        break;
                    }
                    var houseLocation = locations[Random.Range(0, locations.Count - 1)];
                    goalBuildStructure.SetBuildPos(houseLocation);
                    ws.Set(Literals.myHouseLocation, houseLocation);
                }
                break;
            case PreviousPlanResult.Failed:
                break;
            case PreviousPlanResult.Interrupted:
                break;
            case PreviousPlanResult.Suceeded://currentGoal.SetPriority(possibleGoals.Min(x => x.GetPriority()) - 1);
                if (Literals.IsCollectGoal(currentGoal.GetName())) {
                    var cGoal = currentGoal as GoalCollect;
                    var resourceName = ((GoalCollect)currentGoal).ResourceName;
                    var resourceCount = (float)ws.Get("own" + resourceName);
                    switch (resourceName) {
                        case Literals.resourceNameTree: AdjustCollectGoal(resourceCount, cGoal, TreeQuantityGoal);break;
                        case Literals.resourceNameWater: AdjustCollectGoal(resourceCount, cGoal, WaterQuantityGoal); break;
                        case Literals.resourceNameOre: AdjustCollectGoal(resourceCount, cGoal, OreQuantityGoal); break;
                        case Literals.resourceNameAxe: AdjustCollectGoal(resourceCount, cGoal, AxeQuantityGoal, false); break;
                        default: break;
                    }
                }
                else if (currentGoal.GetName() == Literals.GoalBuildHouse) {
                    
                    //set next house info
                    int houseCount;
                    if (ws.HasKey(Literals.builtHousesCount)) {
                        houseCount = (int)ws.Get(Literals.builtHousesCount);
                        houseCount = 2;
                        StatCounter.AddStat(gameObject.name, "Houses Built", "2 houses");
                        StatCounter.AddStat(gameObject.name, $"House { houseCount } Built At", Mathf.RoundToInt(Time.time));
                        ws.Set(Literals.builtHousesCount, houseCount);
                    }
                    else {
                        houseCount = 1;
                        StatCounter.AddStat(gameObject.name, "Houses Built", "1 house");
                        StatCounter.AddStat(gameObject.name, $"House 1 Built At", Mathf.RoundToInt(Time.time));
                        ws.Set(Literals.builtHousesCount, houseCount);
                    }
                    if (houseCount == 2) {
                        goalBuildStructure.WarnPossibleGoal = false;
                        break;
                    }

                    ws.Set(Literals.clearBuildActionV2Info, true);
                    ws.Set(Literals.currentBuildObjectTag, Literals.currentBuildObject);
                    ws.Set(Literals.currentBuildStructureTag, Literals.currentBuildStructure);


                    var locations = FindBuildLocation(structureToBuild.GetComponent<HouseController>());
                    if (locations.Count == 0) {
                        goalBuildStructure.WarnPossibleGoal = false;
                        break;
                    }
                    var currentBuilLocation = locations[Random.Range(0, locations.Count - 1)];
                    goalBuildStructure.SetBuildPos(currentBuilLocation);
                    ws.Set(Literals.currentBuilLocation, currentBuilLocation);
                    currentGoal.SetPlan(null);
                    //clear action

                }
                else if (currentGoal.GetName() == Literals.GoalStockpileResource) {
                    goalStockpileResource.WarnPossibleGoal = false;
                }
                else if (currentGoal.GetName() == Literals.GoalBuildBank) {
                }
                break;
            default:
                break;
        }
        if (goalEat != null) {
            //if (FoodLevel <= -100f) {
            //    //still > distress call
            //}
            //else if (FoodLevel <= -50f) {
            //    //emergency feed
            //}
            if (FoodLevel <= 0f && !goalEat.WarnPossibleGoal) {
                //feed
                //Debug.Log("eat goal activated");
                goalEat.WarnPossibleGoal = true;
                goalEat.SetPriority(10);
            }
            else if (FoodLevel >= 10f && goalEat.WarnPossibleGoal) {
                goalEat.WarnPossibleGoal = false;
                goalEat.SetPriority(0);
            }
        }
        //##########################################


        startedPlanning = true;
        currentReGoapPlanWorker = ReGoapPlannerManager<string, object>.Instance.Plan(this, BlackListGoalOnFailure ? currentGoal : null, currentGoal?.GetPlan(), OnDonePlanning);
        return true;
    }
    private bool AdjustCollectGoal(float resourceCount, GoalCollect cGoal, float ResourceQuantityGoal, bool SetNewQuantity = true) {
        if (resourceCount >= ResourceQuantityGoal) {
            cGoal.WarnPossibleGoal = false;
            return true;
        }
        else if (SetNewQuantity) {
            var newAmount = Mathf.Max(0f, ResourceQuantityGoal - resourceCount);

            /*
             * ##############################################################################################################################
             * Workaround temporário para evitar evitar que colete do próprio banco para colocar de volta e completar o objetivo de coletar.
             * SOLUÇÃO DEFINITIVA IMPLEMENTAR: Utilizar tags diferentes para retirada do banco, coleta de mapa e deposito em banco.
             * ##############################################################################################################################
             */
            if (newAmount == 1f) newAmount = 2f;
            cGoal.SetQuantity(newAmount);
            if (newAmount == 0f) {
                cGoal.WarnPossibleGoal = false;
                return true;
            }
            return false;
        }
        return false;
    }
    public void Eat() {
        //Debug.Log("Had a meal");
        FoodLevel += FoodUnitPerMeal;
        StatCounter.AddStat(name, "Meals eaten", 1);
    }
    public List<Vector3> FindBuildLocation(IStructure structure) {
        var maxIterations = 10;
        var maxDistanceFromAgent = 40f;
        //var proximityPriority = "";
        Vector3 pos;
        var positions = new List<Vector3>();
        var footprint = structure.GetFootprint();
        for (int i = 0; i < maxIterations; i++) {
            //############=====>  Add check for map edge
            pos = new Vector3(GetRandom(transform.position.x, maxDistanceFromAgent), 0, GetRandom(transform.position.z, maxDistanceFromAgent));
            var colliders = Physics.OverlapSphere(pos, Mathf.Max(footprint.x, footprint.z));
            if (colliders.Length == 0) {
                positions.Add(pos);
            }
        }
        return positions;
    }
    private float GetRandom(float center, float mag) {
        return Random.Range(center - mag, center + mag);
    }
    private IEnumerator EnduranceUpdate() {
        while (true) {
            float date = Time.time;
            var ws = memory.GetWorldState();
            if (!ws.HasKey(Literals.myHouseGameObject) && !goalBuildStructure.WarnPossibleGoal) {
                var wantHouse = Random.Range(0f, 1f);
                if (wantHouse <= 1f - (float)Endurance / 100) {
                    goalBuildStructure.WarnPossibleGoal = true;
                    //Debug.Log($"{CharName} decided to build houses at { Time.realtimeSinceStartup }");
                    break;
                }
                else {
                    Endurance -= 1;
                }
            }
            else {
                yield return null;
            }
            yield return new WaitForSeconds(5);
            float newTime = Time.time;
            //Debug.Log($"delta time: {newTime - date}");
        }
        yield return null;
    }
    private IEnumerator HungerUpdate() {
        while (true) {
            if (WarforgedBody) {
                yield return null;
            }
            else {
                FoodLevel -= 1f;
                yield return new WaitForSeconds(FoodUnitDuration);
            }
        }
    }
}
