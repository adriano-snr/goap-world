using ReGoap.Unity;

public class GoalIddle : ReGoapGoal<string, object> {
    protected override void Awake() {
        base.Awake();
        //goal.Set(Literals.reconcilePosition, true);
        goal.Set("randomIddling", true);
    }

    public override string ToString() {
        return string.Format("GoapGoal('{0}', '{1}')", Name, "Default");
    }
}
