using ReGoap.Unity;
using System.Collections.Generic;
using UnityEngine;

public class GoalBuildStructure : ReGoapGoal<string, object> {
    public Vector3 buildPosition;
    public GameObject Blueprint;
    private IStructure BlueprintStructure;
    //private List<KeyValuePair<string, float>> materialsNeeded;

    protected override void Awake() {
        base.Awake();
        BlueprintStructure = Blueprint.GetComponent<IStructure>();//Carrega a interface de estrutura a partir do plano no prefab.
        //materialsNeeded = BlueprintStructure.GetNeededResources();//Obtém a lista de materiais necessários para essa construção.
        goal.Set(Literals.BuildFinished(BlueprintStructure.GetName()), true);
        //for (int i = 0; i < materialsNeeded.Count; i++) {
        //    goal.Set(Literals.BuildHasResource(materialsNeeded[i].Key), materialsNeeded[i].Value);
        //}
    }

    public override string ToString() {
        return string.Format("GoapGoal('{0}')", Name);
    }
    public void SetBuildPos(Vector3 pos) {
        goal.Set(Literals.buildPosition, pos);
    }

}
