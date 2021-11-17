using System.Collections.Generic;
using ReGoap.Unity;
using UnityEngine;
using System.Linq;

public class CustomWorkstationSensor : ReGoapSensor<string, object> {
    private Dictionary<CustomWorkstation, Vector3> workstations;

    public float detectionDistance = 20f;
    void Start() {
        RefreshSensor();
    }
    public void RefreshSensor() {
        workstations = new Dictionary<CustomWorkstation, Vector3>(CustomWorkstationManager.Instance.Workstations.Length);
        var nearWorkstations = CustomWorkstationManager.Instance.Workstations.Where(x => Vector3.Distance(transform.parent.transform.position, x.transform.position) <= detectionDistance);
        foreach (var workstation in nearWorkstations) {
            workstations[workstation] = workstation.transform.position; // workstations are static
        }

        var worldState = memory.GetWorldState();
        worldState.Set("workstations", workstations);
    }
}
