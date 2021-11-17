using ReGoap.Unity;
using ReGoap.Unity.FSMExample.OtherScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class SensorFreshWater : ReGoapSensor<string, object> {
//    private Dictionary<int, Vector3> waterSources;


//    void Start() {
//        var sources = FindObjectsOfType<WaterHandler>();
//        waterSources = new Dictionary<int, Vector3>(sources.Length);
//        var worldState = memory.GetWorldState();
//        if (sources.Length > 0) {
//            worldState.Set("seeWater", true);
//            for (int i = 0; i < sources.Length; i++) {
//                waterSources[i] = sources[i].transform.position;
//            }
//            worldState.Set("waterSources", waterSources);
//        }
//        else {
//            worldState.Set("seeWater", false);
//        }
//    }
//}
public class SensorFreshWater : ReGoapSensor<string, object> {
    protected Dictionary<IResource, Vector3> resourcesPosition;

    protected virtual void Awake() {
        resourcesPosition = new Dictionary<IResource, Vector3>();
    }

    protected virtual void UpdateResources(IResourceManager manager) {
        resourcesPosition.Clear();
        var resources = manager.GetResources();
        for (int index = 0; index < resources.Count; index++) {
            var resource = resources[index];
            resourcesPosition[resource] = resource.GetTransform().position;
        }
    }
}
