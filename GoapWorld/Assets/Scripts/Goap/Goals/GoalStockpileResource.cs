using ReGoap.Unity;

public class GoalStockpileResource : ReGoapGoal<string, object> {
    public bool autoRandomize = true;
    public string ResourceName;
    public float ResourceQuantityGoal = 10f;
    public float GatherPerTrip = 5f;

    protected override void Awake() {
        base.Awake();
        //goal.Set("own" + ResourceName, ResourceQuantityGoal);
        //goal.Set("ownAny" + ResourceName, ResourceQuantityGoal > 0);

        if (autoRandomize) AutoRandomize();

        //if (ResourceName == "Fruit Bowl") {
        //    goal.Set("craftedFruit Bowl", true);
        //}
        //else if (ResourceName == "Axe") {
        //    goal.Set("craftedAxe", true);
        //}

        goal.Set(Literals.CollectedQuantityResource(ResourceName), GatherPerTrip);
        goal.Set(Literals.CollectedResource(ResourceName), GatherPerTrip > 0);
    }

    public override string ToString() {
        return string.Format("GoapGoal('{0}', '{1}')", Name, ResourceName);
    }
    void AutoRandomize() {
        ResourceQuantityGoal = UnityEngine.Random.Range(10, 50);
        GatherPerTrip = UnityEngine.Random.Range(1, 5);

    }
}
