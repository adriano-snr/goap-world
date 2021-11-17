using ReGoap.Core;
using ReGoap.Unity;
using UnityEngine;

public class CustomAdvancedMemory : ReGoapMemory<string, object> {
    private IReGoapSensor<string, object>[] sensors;

    public float SensorsUpdateDelay = 0.3f;
    private float sensorsUpdateCooldown;

    #region UnityFunctions
    protected override void Awake() {
        base.Awake();
        sensors = GetComponentsInChildren<IReGoapSensor<string, object>>();
        foreach (var sensor in sensors) {
            sensor.Init(this);
        }
    }

    protected virtual void Update() {
        if (Time.time > sensorsUpdateCooldown) {
            sensorsUpdateCooldown = Time.time + SensorsUpdateDelay;

            foreach (var sensor in sensors) {
                sensor.UpdateSensor();
            }
        }
    }
    #endregion
}
