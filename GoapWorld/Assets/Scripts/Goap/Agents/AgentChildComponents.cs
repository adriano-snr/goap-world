using ReGoap.Core;
using ReGoap.Unity;
using ReGoap.Unity.FSMExample.OtherScripts;
using ReGoap.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentChildComponents : MonoBehaviour, IReGoapAgent<string, object>, IReGoapAgentHelper {
    public string Name;
    public float CalculationDelay = 0.5f;
    public bool BlackListGoalOnFailure;

    public bool CalculateNewGoalOnStart = true;

    protected float lastCalculationTime;

    protected List<IReGoapGoal<string, object>> goals;
    protected List<IReGoapAction<string, object>> actions;
    protected IReGoapMemory<string, object> memory;
    protected IReGoapGoal<string, object> currentGoal;

    protected ReGoapActionState<string, object> currentActionState;

    protected Dictionary<IReGoapGoal<string, object>, float> goalBlacklist;
    protected List<IReGoapGoal<string, object>> possibleGoals;
    protected bool possibleGoalsDirty;
    protected List<ReGoapActionState<string, object>> startingPlan;
    protected Dictionary<string, object> planValues;
    protected bool interruptOnNextTransition;

    protected bool startedPlanning;
    protected ReGoapPlanWork<string, object> currentReGoapPlanWorker;
    public bool IsPlanning {
        get { return startedPlanning && currentReGoapPlanWorker.NewGoal == null; }
    }
    public string GetName() {
        return gameObject.name;
    }

    #region UnityFunctions
    protected virtual void Awake() {
        lastCalculationTime = -100;
        goalBlacklist = new Dictionary<IReGoapGoal<string, object>, float>();

        RefreshGoalsSet();
        RefreshActionsSet();
        RefreshMemory();
    }

    protected virtual void Start() {
        if (CalculateNewGoalOnStart) {
            CalculateNewGoal(true);
        }
    }

    protected virtual void OnEnable() {

    }

    protected virtual void OnDisable() {
        if (currentActionState != null) {
            currentActionState.Action.Exit(null);
            currentActionState = null;
            currentGoal = null;
        }
    }
    protected virtual void Update() {
        possibleGoalsDirty = true;

        if (currentActionState == null) {
            if (!IsPlanning)
                CalculateNewGoal();
            return;
        }
    }
    #endregion
    protected virtual void UpdatePossibleGoals() {
        possibleGoalsDirty = false;
        if (goalBlacklist.Count > 0) {
            possibleGoals = new List<IReGoapGoal<string, object>>(goals.Count);
            foreach (var goal in goals)
                if (!goalBlacklist.ContainsKey(goal)) {
                    possibleGoals.Add(goal);
                }
                else if (goalBlacklist[goal] < Time.time) {
                    goalBlacklist.Remove(goal);
                    possibleGoals.Add(goal);
                }
        }
        else {
            possibleGoals = goals;
        }
    }

    protected virtual void TryWarnActionFailure(IReGoapAction<string, object> action) {
        if (action.IsInterruptable())
            WarnActionFailure(action);
        else
            action.AskForInterruption();
    }
    public enum PreviousPlanResult {
        Null,
        Failed,
        Interrupted,
        Suceeded
    }
    protected virtual bool CalculateNewGoal(bool forceStart = false, PreviousPlanResult planResult = PreviousPlanResult.Null) {
        if (IsPlanning)
            return false;
        if (!forceStart && (Time.time - lastCalculationTime <= CalculationDelay))
            return false;
        lastCalculationTime = Time.time;

        interruptOnNextTransition = false;
        UpdatePossibleGoals();
        startedPlanning = true;
        currentReGoapPlanWorker = ReGoapPlannerManager<string, object>.Instance.Plan(this, BlackListGoalOnFailure ? currentGoal : null,
            currentGoal != null ? currentGoal.GetPlan() : null, OnDonePlanning);

        return true;
    }

    protected virtual void OnDonePlanning(IReGoapGoal<string, object> newGoal) {
        startedPlanning = false;
        currentReGoapPlanWorker = default(ReGoapPlanWork<string, object>);
        if (newGoal == null) {
            if (currentGoal == null) {
                ReGoapLogger.LogWarning("GoapAgent " + this + " could not find a plan.");
            }
            return;
        }

        if (currentActionState != null)
            currentActionState.Action.Exit(null);
        currentActionState = null;
        currentGoal = newGoal;
        if (startingPlan != null) {
            for (int i = 0; i < startingPlan.Count; i++) {
                startingPlan[i].Action.PlanExit(i > 0 ? startingPlan[i - 1].Action : null, i + 1 < startingPlan.Count ? startingPlan[i + 1].Action : null, startingPlan[i].Settings, currentGoal.GetGoalState());
            }
        }
        startingPlan = currentGoal.GetPlan().ToList();
        ClearPlanValues();
        for (int i = 0; i < startingPlan.Count; i++) {
            startingPlan[i].Action.PlanEnter(i > 0 ? startingPlan[i - 1].Action : null, i + 1 < startingPlan.Count ? startingPlan[i + 1].Action : null, startingPlan[i].Settings, currentGoal.GetGoalState());
        }
        currentGoal.Run(WarnGoalEnd);
        PushAction();
    }
    public static string PlanToString(IEnumerable<IReGoapAction<string, object>> plan) {
        var result = "GoapPlan(";
        var reGoapActions = plan as IReGoapAction<string, object>[] ?? plan.ToArray();
        for (var index = 0; index < reGoapActions.Length; index++) {
            var action = reGoapActions[index];
            result += string.Format("'{0}'{1}", action, index + 1 < reGoapActions.Length ? ", " : "");
        }
        result += ")";
        return result;
    }

    public virtual void WarnActionEnd(IReGoapAction<string, object> thisAction) {
        if (currentActionState != null && thisAction != currentActionState.Action)
            return;
        PushAction();
    }

    protected virtual void PushAction() {
        if (interruptOnNextTransition) {
            CalculateNewGoal();
            return;
        }
        var plan = currentGoal.GetPlan();
        if (plan.Count == 0) {
            if (currentActionState != null) {
                currentActionState.Action.Exit(currentActionState.Action);
                currentActionState = null;
            }
            //CalculateNewGoal();
            ((ReGoapGoal<string, object>)currentGoal).planState = PreviousPlanResult.Suceeded;
            CalculateNewGoal(false, PreviousPlanResult.Suceeded);
        }
        else {
            var previous = currentActionState;
            currentActionState = plan.Dequeue();
            IReGoapAction<string, object> next = null;
            if (plan.Count > 0)
                next = plan.Peek().Action;
            if (previous != null)
                previous.Action.Exit(currentActionState.Action);
            currentActionState.Action.Run(previous != null ? previous.Action : null, next, currentActionState.Settings, currentGoal.GetGoalState(), WarnActionEnd, WarnActionFailure);
        }
    }

    public virtual void WarnActionFailure(IReGoapAction<string, object> thisAction) {
        if (currentActionState != null && thisAction != currentActionState.Action) {
            ReGoapLogger.LogWarning(string.Format("[GoapAgent] Action {0} warned for failure but is not current action.", thisAction));
            return;
        }
        if (BlackListGoalOnFailure)
            goalBlacklist[currentGoal] = Time.time + currentGoal.GetErrorDelay();
        CalculateNewGoal(true, PreviousPlanResult.Failed);
    }

    public virtual void WarnGoalEnd(IReGoapGoal<string, object> goal) {
        if (goal != currentGoal) {
            ReGoapLogger.LogWarning(string.Format("[GoapAgent] Goal {0} warned for end but is not current goal.", goal));
            return;
        }
        CalculateNewGoal();
    }

    public virtual void WarnPossibleGoal(IReGoapGoal<string, object> goal) {
        if ((currentGoal != null) && (goal.GetPriority() <= currentGoal.GetPriority()))
            return;
        if (currentActionState != null && !currentActionState.Action.IsInterruptable()) {
            interruptOnNextTransition = true;
            currentActionState.Action.AskForInterruption();
        }
        else
            CalculateNewGoal();
    }

    public virtual bool IsActive() {
        return enabled;
    }

    public virtual List<ReGoapActionState<string, object>> GetStartingPlan() {
        return startingPlan;
    }

    protected virtual void ClearPlanValues() {
        if (planValues == null)
            planValues = new Dictionary<string, object>();
        else {
            planValues.Clear();
        }
    }

    public virtual object GetPlanValue(string key) {
        return planValues[key];
    }

    public virtual bool HasPlanValue(string key) {
        return planValues.ContainsKey(key);
    }

    public virtual void SetPlanValue(string key, object value) {
        planValues[key] = value;
    }

    public virtual void RefreshMemory() {
        memory = GetComponentInChildren<IReGoapMemory<string, object>>();
    }

    public virtual void RefreshGoalsSet() {
        goals = new List<IReGoapGoal<string, object>>(GetComponentsInChildren<IReGoapGoal<string, object>>(false));
        possibleGoalsDirty = true;
    }

    public virtual void RefreshActionsSet() {
        actions = new List<IReGoapAction<string, object>>(GetComponentsInChildren<IReGoapAction<string, object>>(false));
    }

    public virtual List<IReGoapGoal<string, object>> GetGoalsSet() {
        if (possibleGoalsDirty)
            UpdatePossibleGoals();
        return possibleGoals;
    }

    public virtual List<IReGoapAction<string, object>> GetActionsSet() {
        return actions;
    }

    public virtual IReGoapMemory<string, object> GetMemory() {
        return memory;
    }

    public virtual IReGoapGoal<string, object> GetCurrentGoal() {
        return currentGoal;
    }

    public virtual ReGoapState<string, object> InstantiateNewState() {
        return ReGoapState<string, object>.Instantiate();
    }

    public override string ToString() {
        return string.Format("GoapAgent('{0}')", Name);
    }

    // this only works if the ReGoapAgent has been inherited. For "special cases" you have to override this
    public virtual Type[] GetGenericArguments() {
        return GetType().BaseType.GetGenericArguments();
    }
}
