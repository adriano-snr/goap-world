using System;
using System.Collections.Generic;
using System.Linq;
using ReGoap.Core;
using ReGoap.Unity;
using ReGoap.Unity.FSMExample.OtherScripts;
using UnityEngine;
using L = Literals;

public class ActionBuildStructureV2 : ReGoapAction<string, object> {
    private const string tagBuildStage = "BuildStage";
    private const string tagBuildStages = "BuildStages";

    public GameObject Blueprint;
    private IStructure BlueprintStructure;

    private GameObject Building;
    private IStructure BuildingStructure;

    private List<KeyValuePair<string, float>> materialsNeeded;

    private ResourcesBag resourcesBag;
    private List<ReGoapState<string, object>> settingsList;

    protected override void Awake() {
        base.Awake();
        resourcesBag = GetComponentInParent<ResourcesBag>();//Carrega o inventário do npc.
        BlueprintStructure = Blueprint.GetComponent<IStructure>();//Carrega a interface de estrutura a partir do plano no prefab.
        materialsNeeded = BlueprintStructure.GetNeededResources();//Obtém a lista de materiais necessários para essa construção.
        settingsList = new List<ReGoapState<string, object>>();//Inicializa a lista de configurações da ação.
    }
    public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData) => base.CheckProceduralCondition(stackData) && stackData.settings.HasKey(L.buildPosition);//A condição primária desta ação é ter selecionado um local para a construção.
    public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData) {
        var ws = agent.GetMemory().GetWorldState();
        if (ws.HasKey(L.clearBuildActionV2Info)) {
            ws.Remove(L.clearBuildActionV2Info);
            Building = null;
            BuildingStructure = null;
            settingsList.Clear();
        }

        var mats = new List<KeyValuePair<string, float>>();
        if (Building != null) {
            mats.AddRange(BuildingStructure.GetMissingMaterialList());
            settingsList.Clear();
        }
        else {
            mats.AddRange(materialsNeeded);
        }
        //settingsList.Clear();
        if (!stackData.goalState.HasKey(L.BuildFinished(BlueprintStructure.GetName())) && !stackData.goalState.HasKey(tagBuildStage)) {
            settingsList.Clear();
        }
        if (settingsList.Count == 0 && (stackData.goalState.HasKey(L.BuildFinished(BlueprintStructure.GetName())) || stackData.goalState.HasKey(tagBuildStage))) {
            settings.Set(L.buildPosition, stackData.goalState.Get(L.buildPosition));//Salva a posição da construção nas configurações da ação. Este valor deve ser fornecido pelo objetivo raiz.
            var stages = new List<PreconditionsEffectsPair>();
            var pos = stackData.goalState.Get(L.buildPosition);
            
            var edStage = new PreconditionsEffectsPair();
            edStage.AddEffect(L.BuildFinished(BlueprintStructure.GetName()), true);
            edStage.AddEffect(tagBuildStage, 0);
            edStage.AddPrecondition(tagBuildStage, 1);
            edStage.AddPrecondition(L.isAtPosition, pos);
            stages.Add(edStage);


            var stageCount = 1;
            var lastGreaterThanZero = 0;
            for (int i = 0; i < mats.Count; i++) {
                var resoureName = mats[i].Key;
                var amount = (float)mats[i].Value;
                for (float j = amount; j > 0; j--) {
                    var cStage = new PreconditionsEffectsPair();

                    cStage.AddEffect(tagBuildStage, stageCount);
                    if (i > 0 && j == amount) cStage.AddEffect(L.BuildHasResource(mats[i - 1].Key), 0f);
                    cStage.AddEffect(L.BuildHasResource(resoureName), j);
                    cStage.AddEffect(L.HasResource(resoureName), false);
                    stageCount++;
                    cStage.AddPrecondition(tagBuildStage, stageCount);
                    cStage.AddPrecondition(L.BuildHasResource(resoureName), j - 1);
                    cStage.AddPrecondition(L.HasResource(resoureName), true);
                    cStage.AddPrecondition(L.isAtPosition, pos);

                    stages.Add(cStage);
                    lastGreaterThanZero = i;
                }
            }

            var stStage = new PreconditionsEffectsPair();
            stStage.AddEffect(tagBuildStage, stageCount);
            stStage.AddEffect("BuildStarted", true);
            stStage.AddEffect(L.buildPosition, pos);
            stStage.AddEffect(L.BuildHasResource(mats[lastGreaterThanZero].Key), 0f);
            stStage.AddPrecondition(L.isAtPosition, pos);
            stages.Add(stStage);

            settings.Set(tagBuildStages, stages);
            settingsList.Add(settings.Clone());
        }
        return settingsList;
    }
    public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData) {
        preconditions.Clear();
        int stageIndex = 0;
        if (stackData.goalState.HasKey(tagBuildStage)) stageIndex = (int)stackData.goalState.Get(tagBuildStage);
        var stages = (List<PreconditionsEffectsPair>)stackData.settings.Get(tagBuildStages);
        var stage = stages[stageIndex];
        for (int i = 0; i < stage.preconditions.Count; i++) preconditions.Set(stage.preconditions[i].Key, stage.preconditions[i].Value);
        return preconditions;
    }
    public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData) {
        effects.Clear();
        int stageIndex = 0;
        if (stackData.goalState.HasKey(tagBuildStage)) stageIndex = (int)stackData.goalState.Get(tagBuildStage);
        var stages = (List<PreconditionsEffectsPair>)stackData.settings.Get(tagBuildStages);
        var stage = stages[stageIndex];
        for (int i = 0; i < stage.effects.Count; i++) effects.Set(stage.effects[i].Key, stage.effects[i].Value);
        return effects;
    }

    public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, ReGoapState<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail) {
        base.Run(previous, next, settings, goalState, done, fail);
        var ws = agent.GetMemory().GetWorldState();

        //Load current building project info
        var objectTag = (string)ws.Get(L.currentBuildObjectTag);
        var structureTag = (string)ws.Get(L.currentBuildStructureTag);

        if (Building == null) {
            if (ws.HasKey(objectTag)) {
                Building = (GameObject)ws.Get(objectTag);
                BuildingStructure = (IStructure)ws.Get(structureTag);
            }
            else {
                Building = Instantiate(Blueprint);
                Building.transform.position = (Vector3)settings.Get(L.buildPosition);
                BuildingStructure = Building.GetComponent<IStructure>();
                ws.Set(objectTag, Building);
                ws.Set(structureTag, BuildingStructure);
            }
            done(this);
        }
        else {
            var inventory = resourcesBag.GetResources();
            var missing = BuildingStructure.GetMissingMaterialList();
            var success = false;
            if (!missing.Any(x => x.Value > 0f)) {
                success = true;
                done(this);
            }
            else {
                for (int i = 0; i < missing.Count; i++) {
                    if (missing[i].Value == 0f) continue;
                    var key = missing[i].Key;
                    if (inventory.ContainsKey(key)) {
                        inventory.TryGetValue(key, out float amount);
                        if (amount >= 1) {
                            BuildingStructure.AddMaterial(key, 1);
                            resourcesBag.RemoveResource(key, 1f);
                            success = true;
                            done(this);
                            break;
                        }
                    }
                }
            }
            if (!success) {
                fail(this);
            }
        }
    }
}
