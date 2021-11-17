using ReGoap.Core;
using ReGoap.Unity;
using ReGoap.Unity.FSMExample.FSM;
using ReGoap.Unity.FSMExample.OtherScripts;
using ReGoap.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ActionIddle : ReGoapAction<string, object> {
    private Vector3 desiredPos;
    public float MaxStraying = 5f;
    public float iddlingSpeed;
    private SmsGoTo smsGoTo;
    protected override void Awake() {
        base.Awake();
        //smsGoTo = GetComponent<SmsGoTo>();
        //if (smsGoTo == null) smsGoTo = GetComponentInParent<SmsGoTo>();
        //previousSpeed = smsGoTo.Speed;
    }
    public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData) {
        preconditions.Clear();
        settings.TryGetValue("isAtPosition", out var pos);
        //settings.TryGetValue("isAtSpeed", out var spd);
        preconditions.Set("isAtPosition", (Vector3)pos);
        //preconditions.Set("iddlingSpeed", (float)iddlingSpeed);
        return preconditions;
    }
    public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData) {
        effects.Clear();
        effects.Set("randomIddling", true);
        return effects;
    }
    public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData) {
        settings.Clear();
        var results = new List<ReGoapState<string, object>>();
        var pos = (Vector3)stackData.currentState.Get("homePosition");
        desiredPos = new Vector3(pos.x + UnityEngine.Random.Range(-MaxStraying, MaxStraying), pos.y, pos.z + UnityEngine.Random.Range(-MaxStraying, MaxStraying));
        settings.Set("isAtPosition", desiredPos);
        //settings.Set("isAtSpeed", 5f);
        results.Add(settings.Clone());
        return results;
    }
    public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, ReGoapState<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail) {
        base.Run(previous, next, settings, goalState, done, fail);
        //smsGoTo.Speed = previousSpeed;
        doneCallback(this);
    }
}
