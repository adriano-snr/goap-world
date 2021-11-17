using ReGoap.Core;
using ReGoap.Unity;
using ReGoap.Unity.FSMExample.OtherScripts;
using ReGoap.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionGoTo : ReGoapAction<string, object> {
    private const string tagDesirePosition = "desiredPosition";

    public float Speed = 0.1f;
    //private List<Vector3> destiny;
    private Vector3 destVec;
    private Vector3 startVec;
    private int currentObjectiveIndex;
    private bool move;
    private float gatherCooldown;

    protected override void Awake() {
        base.Awake();
        move = false;
        Speed = UnityEngine.Random.Range(0.6f, 1f);
        //currentObjectiveIndex = -1;
        //destiny = new List<Vector3>();
    }
    public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData) {
        preconditions.Clear();
        return preconditions;
    }
    public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData) {
        effects.Clear();
        effects.Set(Literals.isAtPosition, stackData.settings.Get(tagDesirePosition));
        if (stackData.goalState.HasKey("iddlingSpeed")) {
            effects.Set("iddlingSpeed", stackData.goalState.Get("iddlingSpeed"));
        }
        effects.Set(Literals.reconcilePosition, true);
        return effects;
    }

    public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData) {
        settings.Clear();
        if (stackData.goalState.HasKey(Literals.isAtPosition)) {
            settings.Set(tagDesirePosition, stackData.goalState.Get(Literals.isAtPosition)); 
            if (stackData.goalState.HasKey("iddlingSpeed")) {
                settings.Set("iddlingSpeed", stackData.goalState.Get("iddlingSpeed"));
            }
            //settings.Set(Literals.reconcilePosition, true);
            //destiny.Insert(0, (Vector3)stackData.goalState.Get(Literals.isAtPosition));
        }
        //else if (stackData.goalState.has)
        var result = new List<ReGoapState<string, object>>();
        if (settings.Count > 0) {
            result.Add(settings.Clone());
        }
        return result;
    }

    public override float GetCost(GoapActionStackData<string, object> stackData) {
        return 1f;
    }
    public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData) {
        return base.CheckProceduralCondition(stackData);
    }

    public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, ReGoapState<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail) {
        base.Run(previous, next, settings, goalState, done, fail);
        move = true;
        destVec = (Vector3)settings.Get(tagDesirePosition);
        startVec = transform.parent.position;
        if (settings.HasKey("iddlingSpeed")) {
            Speed = (float)settings.Get("iddlingSpeed");
        }
        //currentObjectiveIndex++;
        //var thisSettings = settings;
    }
    protected void Update() {
        if (!move) return;
        var cDestiny = destVec;
        var newPosition = (cDestiny - startVec).normalized * Speed;
        transform.parent.position += newPosition;
        if ((cDestiny - transform.parent.position).magnitude <= Speed) {
            move = false;
            transform.parent.position = cDestiny;
            doneCallback(this);
        }
    }
}
