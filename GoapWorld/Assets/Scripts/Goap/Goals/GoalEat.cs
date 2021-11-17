using ReGoap.Unity;

public class GoalEat : ReGoapGoal<string, object> {
    protected override void Awake() {
        base.Awake();
        goal.Set(Literals.hasEaten, true);
        goal.Set(Literals.reconcilePosition, true);
    }

    public override string ToString() {
        return string.Format("GoapGoal('{0}', '{1}')", Name, true);
    }
}
