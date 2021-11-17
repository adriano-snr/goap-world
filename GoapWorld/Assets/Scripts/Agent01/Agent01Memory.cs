using ReGoap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent01Memory : ReGoapMemory<string, object> {
    public void Init() {
        Awake();
    }
    public void SetValue(string key, object value) {
        state.Set(key, value);
    }
}
