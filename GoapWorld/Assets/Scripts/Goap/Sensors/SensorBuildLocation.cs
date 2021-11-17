using System.Collections.Generic;
using ReGoap.Unity;
using ReGoap.Unity.FSMExample.OtherScripts;
using UnityEngine;
using System.Linq;

public class SensorBuildLocation : ReGoapSensor<string, object> {
    //private Dictionary<CustomBank, Vector3> banks;

    public float MinPowDistanceToBeNear = 1f;
    public float detectionDistance = 20f;
    //void Start() {
    //    //banks = new Dictionary<CustomBank, Vector3>(CustomBankManager.Instance.Banks.Length);
    //    //banks = new Dictionary<CustomBank, Vector3>(CustomBankManager.Instance.Banks.Length);
    //    //var nearBanks = CustomBankManager.Instance.Banks.Where(x => Vector3.Distance(transform.parent.transform.position, x.transform.position) <= detectionDistance);
    //    //foreach (var bank in nearBanks) {
    //    //    banks[bank] = bank.transform.position;
    //    //}

    //    //var worldState = memory.GetWorldState();
    //    //worldState.Set("seeBank", CustomBankManager.Instance != null && CustomBankManager.Instance.Banks.Length > 0);
    //    //worldState.Set("banks", banks);
    //}
}
