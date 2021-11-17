using ReGoap.Unity.FSMExample.OtherScripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomBank : MonoBehaviour, IStructure {
    public string BlueprintName;
    public int startingStage;
    private ResourcesBag bankBag;
    private Dictionary<string, float> addedMaterial;
    private int totalAddedMaterial;
    private int totalMaterial;

    public GameObject modelFoundation;
    public GameObject modelLeft;
    public GameObject modelBack;
    public GameObject modelRight;
    public GameObject modelFront;
    public GameObject modelTop;
    public GameObject modelFinished;

    void Awake() {
        bankBag = gameObject.AddComponent<ResourcesBag>();
        addedMaterial = new Dictionary<string, float>();
        totalMaterial = GetTotalMaterial();
        totalAddedMaterial = 0;
        SetStartingStage(startingStage);
    }

    public float GetResource(string resourceName) {
        return bankBag.GetResource(resourceName);
    }

    public Dictionary<string, float> GetResources() {
        return bankBag.GetResources();
    }

    public bool AddResource(ResourcesBag resourcesBag, string resourceName, float value = 1f) {
        if (resourcesBag.GetResource(resourceName) >= value) {
            resourcesBag.RemoveResource(resourceName, value);
            bankBag.AddResource(resourceName, value);
            return true;
        }
        else {
            return false;
        }
    }
    public bool RemoveResource(ResourcesBag resourcesBag, string resourceName, float value = 1f) {
        if (bankBag.GetResource(resourceName) >= value) {
            bankBag.RemoveResource(resourceName, value);
            resourcesBag.AddResource(resourceName, value);
            return true;
        }
        else {
            return false;
        }
    }

    public string GetName() {
        return BlueprintName;
    }

    public Vector3 GetFootprint() {
        return new Vector3(3, 0, 3);
    }

    public List<KeyValuePair<string, float>> GetNeededResources() {
        var res = new List<KeyValuePair<string, float>>();
        res.Add(new KeyValuePair<string, float>("Tree", 7f));
        return res;
    }

    private void SetStartingStage(int stage) {
        switch (stage) {
            case 8:
                totalAddedMaterial = 7;
                addedMaterial.Add("Tree", 7f);
                UpdateBuildingToStage(8);
                break;
            default:break;
        }
    }
    public int GetStage() {
        if (totalAddedMaterial == 0) return 1;
        if (totalAddedMaterial == 1) return 2;
        if (totalAddedMaterial == 2) return 3;
        if (totalAddedMaterial == 3) return 4;
        if (totalAddedMaterial == 4) return 5;
        if (totalAddedMaterial == 5) return 6;
        if (totalAddedMaterial == 6) return 7;
        return 8;
    }
    public int GetLastStage() => 8;

    public void AddMaterial(string material, int amount) {
        if (addedMaterial.ContainsKey(material)) {
            addedMaterial[material] = addedMaterial[material] + amount;
        }
        else {
            addedMaterial.Add(material, amount);
        }
        totalAddedMaterial += amount;
        var finalStage = GetStage();
        UpdateBuildingToStage(finalStage);
    }
    private void UpdateBuildingToStage(int stage) {
        if (stage == 8) {
            modelFoundation.SetActive(false);
            modelLeft.SetActive(false);
            modelBack.SetActive(false);
            modelRight.SetActive(false);
            modelFront.SetActive(false);
            modelTop.SetActive(false);
            modelFoundation.SetActive(false);
            modelFinished.SetActive(true);
        }
        else {
            modelFoundation.SetActive(stage > 0);
            modelLeft.SetActive(stage > 1);
            modelBack.SetActive(stage > 2);
            modelRight.SetActive(stage > 3);
            modelFront.SetActive(stage > 4);
            modelTop.SetActive(stage > 5);
            modelFoundation.SetActive(stage > 6);
            modelFinished.SetActive(false);
        }
    }

    public List<KeyValuePair<string, float>> GetMissingMaterialList() {
        var res = new List<KeyValuePair<string, float>>();
        var need = GetNeededResources();
        for (int i = 0; i < need.Count; i++) {
            var key = need[i].Key;
            if (addedMaterial.ContainsKey(key)) {
                res.Add(new KeyValuePair<string, float>(key, need[i].Value - addedMaterial[key]));
            }
            else {
                res.Add(new KeyValuePair<string, float>(key, need[i].Value));
            }
        }
        return res;
    }

    public int getTotalMissingMaterial() {
        return totalMaterial - totalAddedMaterial;
    }

    public int GetTotalMaterial() {
        var list = GetNeededResources();
        var res = (int)list.Sum(x => x.Value);
        return res;
    }
}
