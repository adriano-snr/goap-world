using ReGoap.Unity.FSMExample.OtherScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class CustomResource01 : MonoBehaviour, IResource
//{
//    public bool infinite;
//    public float capacity;
//    public string ResourceName;
//    public float GetCapacity() {
//        if (infinite) return 1000;
//        return capacity;
//    }

//    public string GetName() {
//        return ResourceName;
//    }

//    public int GetReserveCount() {
//        return 0;
//    }

//    public Transform GetTransform() {
//        return transform;
//    }

//    public void RemoveResource(float value) {
//        return;
//    }

//    public void Reserve(int id) {
//        return;
//    }

//    public void Unreserve(int id) {
//        return;
//    }

//    // Start is called before the first frame update
//    void Start()
//    {

//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }
//}
public class CustomResource01 : Resource {
    public float MinScalePercentage = 0.1f;
    private Vector3 startingScale;

    protected override void Awake() {
        base.Awake();
        startingScale = transform.localScale;
    }

    public override void RemoveResource(float value) {
        //base.RemoveResource(value);
        //transform.localScale = startingScale * (MinScalePercentage + (1f - MinScalePercentage) * (Capacity / startingCapacity)); // scale down based on capacity
    }
    public CustomResource01() {
        Capacity = 1000;
    }
}
