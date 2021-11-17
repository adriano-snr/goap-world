using System.Collections.Generic;
using ReGoap.Unity;
using UnityEngine;
using System.Linq;

public class BankContentsSensor : ReGoapSensor<string, object> {
    private CustomBank myBank;

    public float detectionDistance = 50f;
    void Start() {
        RefreshSensor();
    }
    public void RefreshSensor() {
        if (myBank == null) {
            var banks = new Dictionary<CustomBank, Vector3>(CustomBankManager.Instance.Banks.Length);
            var nearBanks = CustomBankManager.Instance.Banks.Where(x => Vector3.Distance(transform.parent.transform.position, x.transform.position) <= detectionDistance).ToList();
            if (nearBanks.Count > 0) {
                myBank = nearBanks[0];
            }
        }
        if (myBank != null) {
            var ws = memory.GetWorldState();
            var resources = myBank.GetResources();
            foreach (var resource in resources) {
                ws.Set("ownAny" + resource.Key, (float)resource.Value > 0f);
                ws.Set("own" + resource.Key, (float)resource.Value);
            }
        }

    }
    public override void UpdateSensor() {
        base.UpdateSensor();
        RefreshSensor();
    }
}
