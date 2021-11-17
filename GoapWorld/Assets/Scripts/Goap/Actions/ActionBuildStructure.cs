using System;
using System.Collections.Generic;
using ReGoap.Core;
using ReGoap.Unity;
using ReGoap.Unity.FSMExample.OtherScripts;
using L = Literals;
using UnityEngine;
public class ActionBuildStructure : ReGoapAction<string, object> {
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
        BlueprintStructure = Blueprint.GetComponent<HouseController>();//Carrega a interface de estrutura a partir do plano no prefab.
        materialsNeeded = BlueprintStructure.GetNeededResources();//Obtém a lista de materiais necessários para essa construção.
        settingsList = new List<ReGoapState<string, object>>();//Inicializa a lista de configurações da ação.
    }
    public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData) => base.CheckProceduralCondition(stackData) && stackData.settings.HasKey(L.buildPosition);//A condição primária desta ação é ter selecionado um local para a construção.
    public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData) {
        settingsList.Clear();
        settings.Set(L.buildPosition, stackData.goalState.Get(L.buildPosition));//Salva a posição da construção nas configurações da ação. Este valor deve ser fornecido pelo objetivo raiz.
        settingsList.Add(settings.Clone());
        return settingsList;
    }
    public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData) {
        preconditions.Clear();
        var gs = stackData.goalState;
        var st = stackData.settings;
        var cs = stackData.currentState;
        preconditions.Set(L.isAtPosition, settings.Get(L.buildPosition));
        if (gs.HasKey(L.BuildFinished(BlueprintStructure.GetName()))) {//Se a construção não foi iniciada, define como precondição o npc estar no local para clamar o território, com isto o personagem criará as fundações da construção no local, impedindo que outros npcs tentem construir no mesmo lugar.

            if (cs.HasKey(L.myHouseGameObject)) {
                //continuar construção interrompida.
                var amount = (int)cs.Get(L.myHouseBuildProgress);
                for (int i = 0; i < materialsNeeded.Count; i++) {
                    if (cs.HasKey(L.MyHouseHasResource(materialsNeeded[i].Key))) {

                    }
                }
                if (amount == 1) {
                    preconditions.Set(L.buildStarted, true);
                    SetPreconditionHasSomeMissingMat(gs, st);
                }
                else {
                    preconditions.Set(L.buildProgress, amount - 1);
                    SetPreconditionHasSomeMissingMat(gs, st);
                }
            }
            else {
                var total = BlueprintStructure.GetTotalMaterial();//Recupera a quantidade total de materiais necessários para a construção.
                preconditions.Set(L.buildProgress, total - 1);
                SetPreconditionHasSomeMissingMat(gs, st);
            }
        }
        else if (gs.HasKey(L.buildProgress)) {
            var amount = (int)gs.Get(L.buildProgress);
            if (amount == 1) {
                preconditions.Set(L.buildStarted, true);
                SetPreconditionHasSomeMissingMat(gs, st);
            }
            else {
                preconditions.Set(L.buildProgress, amount - 1);
                SetPreconditionHasSomeMissingMat(gs, st);
            }
        }
        return preconditions;
    }
    public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData) {
        effects.Clear();
        if (preconditions.HasKey(L.buildProgress)) {
            SetSpecificResourceProgressEffect();

            var progressAmount = (int)preconditions.Get(L.buildProgress);
            if (progressAmount + 1 == BlueprintStructure.GetTotalMaterial()) effects.Set(L.BuildFinished(BlueprintStructure.GetName()), true);
            else effects.Set(L.buildProgress, progressAmount + 1);

        }
        else if (preconditions.HasKey(L.buildStarted)) {
            effects.Set(L.buildProgress, 1);
            SetSpecificResourceProgressEffect();
        }
        else {
            effects.Set(L.buildStarted, true);
            for (int i = 0; i < materialsNeeded.Count; i++) effects.Set(L.BuildHasResource(materialsNeeded[i].Key), 0f);
            effects.Set(L.buildPosition, settings.Get(L.buildPosition));
        }
        return effects;
    }
    private void SetPreconditionHasSomeMissingMat(ReGoapState<string, object> gs, ReGoapState<string, object> st) {
        for (int i = 0; i < materialsNeeded.Count; i++) {
            var resourceName = materialsNeeded[i].Key;
            if (gs.HasKey(L.BuildHasResource(resourceName))) {
                var amount = (float)gs.Get(L.BuildHasResource(resourceName));
                if (amount > 0) {
                    preconditions.Set(L.BuildHasResource(resourceName), amount - 1);
                    preconditions.Set(L.HasResource(resourceName), true);
                    break;
                }
                else {
                    //preconditions.Set(L.HasResource(resourceName), false);
                }
            }
        }
    }
    private void SetSpecificResourceProgressEffect() {
        for (int i = 0; i < materialsNeeded.Count; i++) {
            var resourceName = materialsNeeded[i].Key;
            var hasKey = preconditions.HasKey(L.HasResource(resourceName));
            if (hasKey) {
                var keyValue = (bool)preconditions.Get(L.HasResource(resourceName));
                if (keyValue) {
                    object resourceAmountObject;
                    preconditions.TryGetValue(L.BuildHasResource(resourceName), out resourceAmountObject);
                    float resourceAmount = (float)resourceAmountObject;
                    effects.Set(L.BuildHasResource(resourceName), resourceAmount + 1);
                    effects.Set(L.HasResource(resourceName), false);
                }
                //else {
                //    effects.Set(L.HasResource(resourceName), false);
                //}
            }
            else {
                effects.Set(L.HasResource(resourceName), false);
            }
        }
    }

    public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, ReGoapState<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail) {
        base.Run(previous, next, settings, goalState, done, fail);
        var ws = agent.GetMemory().GetWorldState();
        if (Building == null) {
            if (ws.HasKey(L.myHouseGameObject)) {
                Building = (GameObject)ws.Get(L.myHouseGameObject);
                BuildingStructure = (IStructure)ws.Get(L.myHouseStructure);
            }
            else {
                Building = Instantiate(Blueprint);
                Building.transform.position = (Vector3)settings.Get(L.buildPosition);
                BuildingStructure = Building.GetComponent<HouseController>();
                ws.Set(L.myHouseGameObject, Building);
                ws.Set(L.myHouseStructure, BuildingStructure);
            }
        }
        else {
            var inventory = resourcesBag.GetResources();
            var missing = BuildingStructure.GetMissingMaterialList();
            for (int i = 0; i < missing.Count; i++) {
                var key = missing[i].Key;
                if (inventory.ContainsKey(key)) {

                    BuildingStructure.AddMaterial(key, 1);
                    resourcesBag.RemoveResource(key, 1f);
                    break;
                }
            }
        }
        done(this);
    }
    public override void Exit(IReGoapAction<string, object> next) {
        base.Exit(next);
        var worldState = agent.GetMemory().GetWorldState();
        //worldState.Set("I am blue", true);
        //foreach (var pair in effects) {
        //    worldState.Set(pair.Key, pair.Value);
        //}
    }
}
