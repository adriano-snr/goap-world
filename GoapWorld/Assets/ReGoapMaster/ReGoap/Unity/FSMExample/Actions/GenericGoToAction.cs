﻿using System;
using System.Collections.Generic;

using ReGoap.Core;
using ReGoap.Unity.FSMExample.FSM;

using UnityEngine;

namespace ReGoap.Unity.FSMExample.Actions
{ // you could also create a generic ExternalGoToAction : GenericGoToAction
//  which let you add effects / preconditions from some source (Unity, external file, etc.)
//  and then add multiple ExternalGoToAction to your agent's gameobject's behaviours
// you can use this without any helper class by having the actions that need to move to a position
//  or transform to have a precondition isAtPosition
    [RequireComponent(typeof(SmsGoTo))]
    public class GenericGoToAction : ReGoapAction<string, object>
    {
        // sometimes a Transform is better (moving target), sometimes you do not have one (last target position)
        //  but if you're using multi-thread approach you can't use a transform or any unity's API
        protected SmsGoTo smsGoto;

        protected override void Awake()
        {
            base.Awake();

            smsGoto = GetComponent<SmsGoTo>();
        }

        public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, ReGoapState<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
        {
            base.Run(previous, next, settings, goalState, done, fail);

            //if (settings.TryGetValue("objectivePosition", out var v)) smsGoto.GoTo((Vector3) v, OnDoneMovement, OnFailureMovement);
            if (settings.TryGetValue("objectivePosition", out var v)) {
                //##############
                //if (settings.TryGetValue("isAtSpeed", out var speed)) {
                //    smsGoto.Speed = (float)speed;
                //}
                //##############
                smsGoto.GoTo((Vector3)v, OnDoneMovement, OnFailureMovement);
            }
            else
                failCallback(this);
        }

        public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData)
        {
            return base.CheckProceduralCondition(stackData) && stackData.settings.HasKey("objectivePosition");
        }

        public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData)
        {
            if (stackData.settings.TryGetValue("objectivePosition", out var objectivePosition))
            {
                effects.Set("isAtPosition", objectivePosition);
                if (stackData.settings.HasKey(Literals.reconcilePosition))
                    effects.Set(Literals.reconcilePosition, true);
                //###################
                //if (stackData.settings.TryGetValue("isAtSpeed", out var speed)) {
                //    effects.Set("isAtSpeed", speed);
                //}
                //####################
            }
            else
            {
                effects.Clear();
            }
            return base.GetEffects(stackData);
        }

        public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData)
        {
            if (stackData.goalState.TryGetValue("isAtPosition", out var isAtPosition))
            {
                settings.Set("objectivePosition", isAtPosition);
                return base.GetSettings(stackData);
            }
            else if (stackData.goalState.HasKey(Literals.reconcilePosition) && stackData.goalState.Count == 1)
            {
                settings.Set("objectivePosition", stackData.agent.GetMemory().GetWorldState().Get("startPosition"));
                settings.Set(Literals.reconcilePosition, true);
                return base.GetSettings(stackData);
            }
            //########################
            //if (stackData.goalState.TryGetValue("isAtSpeed", out var speed)) {
            //    settings.Set("isAtSpeed", speed);
            //}
            //########################
            return new List<ReGoapState<string, object>>();
        }

        // if you want to calculate costs use a non-dynamic/generic goto action
        public override float GetCost(GoapActionStackData<string, object> stackData)
        {
            var distance = 0.0f;
            if (stackData.settings.TryGetValue("objectivePosition", out object objectivePosition)
                && stackData.currentState.TryGetValue("isAtPosition", out object isAtPosition))
            {
                if (objectivePosition is Vector3 p && isAtPosition is Vector3 r)
                    distance = (p - r).magnitude;
            }
            return base.GetCost(stackData) + Cost + distance;
        }

        protected virtual void OnFailureMovement()
        {
            failCallback(this);
        }

        protected virtual void OnDoneMovement()
        {
            doneCallback(this);
        }
    }
}
