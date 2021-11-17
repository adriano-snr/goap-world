using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HouseController : MonoBehaviour, IStructure {
    public string BlueprintName;
    // Start is called before the first frame update
    public GameObject[] Wall01;
    public GameObject[] Wall02;
    public GameObject[] Wall03;
    public GameObject[] Wall04;
    public GameObject[] Roof;
    private Dictionary<string, float> addedMaterial;
    private int totalAddedMaterial;
    private int totalMaterial;

    void Awake() {
        addedMaterial = new Dictionary<string, float>();
        totalMaterial = GetTotalMaterial();
        totalAddedMaterial = 0;
        UpdateBuildingToStage(GetStage());
    }
    public Vector3 GetFootprint() => new Vector3(8, 0, 8);
    public int GetTotalMaterial() {
        var list = GetNeededResources();
        var res = (int)list.Sum(x => x.Value);
        return res;
    }
    public List<KeyValuePair<string, float>> GetNeededResources() {
        var result = new List<KeyValuePair<string, float>>();
        result.Add(new KeyValuePair<string, float>("Tree", 4));
        result.Add(new KeyValuePair<string, float>("Ore", 1));
        return result;
    }
    public int GetStage() {
        if (totalAddedMaterial == 0) return 1;
        if (totalAddedMaterial == 1) return 2;
        if (totalAddedMaterial == 2) return 3;
        if (totalAddedMaterial == 3) return 4;
        if (totalAddedMaterial == 4) return 5;
        if (totalAddedMaterial == 5) return 6;
        return 7;
    }
    public int GetLastStage() => 7;
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
        SetWallsStage(stage);
        Roof[0].SetActive(stage > 4);
        Roof[1].SetActive(stage > 5);
    }
    private void SetWallsStage(int stage) {
        SetWallStage(Wall01, stage);
        SetWallStage(Wall02, stage);
        SetWallStage(Wall03, stage);
        SetWallStage(Wall04, stage);
    }
    private void SetWallStage(GameObject[] wall, int stage) {
        wall[0].SetActive(stage > 1);
        wall[1].SetActive(stage > 2);
        wall[2].SetActive(stage > 3);
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

    public string GetName() {
        return BlueprintName;
    }
}
