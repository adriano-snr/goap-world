using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomBankToDebug : MonoBehaviour {
    public Text Text;
    public CustomBank bank;

    void FixedUpdate() {
        var result = "";
        foreach (var pair in bank.GetResources()) {
            result += string.Format("{0}: {1}\n", pair.Key, pair.Value);
        }
        Text.text = result;
    }
}
