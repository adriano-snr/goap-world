using ReGoap.Unity.FSMExample.OtherScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomBankManager : MonoBehaviour {
    public static CustomBankManager Instance;
    public CustomBank[] Banks;
    private int currentIndex;

    protected virtual void Awake() {
        if (Instance != null)
            throw new UnityException("[BankManager] Can have only one instance per scene.");
        Instance = this;
    }

    public CustomBank GetBank() {
        var result = Banks[currentIndex];
        currentIndex = currentIndex++ % Banks.Length;
        return result;
    }

    public int GetBanksCount() {
        return Banks.Length;
    }
}
