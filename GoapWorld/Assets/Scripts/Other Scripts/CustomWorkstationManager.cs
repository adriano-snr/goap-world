using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomWorkstationManager : MonoBehaviour {
    public static CustomWorkstationManager Instance;
    public CustomWorkstation[] Workstations;
    private int currentIndex;

    protected virtual void Awake() {
        if (Instance != null)
            throw new UnityException("[WorkstationsManager] Can have only one instance per scene.");
        Instance = this;
    }

    public CustomWorkstation GetWorkstation() {
        var result = Workstations[currentIndex];
        currentIndex = currentIndex++ % Workstations.Length;
        return result;
    }
}
