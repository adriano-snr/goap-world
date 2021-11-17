using ReGoap.Unity;

public class GoalCollect : ReGoapGoal<string, object> {
    public bool AllowFromBank = true;
    public string ResourceName;
    public float ResourceQuantity = 3f;

    protected override void Awake() {
        base.Awake();
        goal.Set(Literals.CollectedResource(ResourceName), true);
        //if (ResourceName == "Fruit Bowl") {
        //    goal.Set("craftedFruit Bowl", true);
        //}
        //else if (ResourceName == "Axe") {
        //    goal.Set("craftedAxe", true);
        //}
        SetQuantity(ResourceQuantity);
        goal.Set(Literals.reconcilePosition, true);
        //if (!AllowFromBank) {
        //    goal.Set("gatheredFromBank" + ResourceName, false);
        //}
    }

    public override string ToString() {
        return string.Format("GoapGoal('{0}', '{1}')", Name, ResourceName);
    }
    public void SetQuantity(float quantity) {
        if (quantity > 1f) {
            ResourceQuantity = quantity;
            goal.Set(Literals.CollectedQuantityResource(ResourceName), quantity);
        }
        else {
            ResourceQuantity = 1f;
            goal.Remove(Literals.CollectedQuantityResource(ResourceName));
        }
    }
}
