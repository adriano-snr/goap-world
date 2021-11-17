using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStructure
{
    string GetName();
    Vector3 GetFootprint();
    List<KeyValuePair<string, float>> GetNeededResources();
    int GetStage();
    int GetLastStage();
    void AddMaterial(string material, int amount);
    List<KeyValuePair<string, float>> GetMissingMaterialList();
    int getTotalMissingMaterial();
    int GetTotalMaterial();
}
