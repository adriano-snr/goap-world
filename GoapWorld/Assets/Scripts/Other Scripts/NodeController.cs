using ReGoap.Unity.FSMExample.OtherScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NodeController : MonoBehaviour {
    public bool autoRandomize = true;
    private List<ResourceNode> nodes;
    public int respawnChance = 50;
    public int respawnChanceCooldownTimer = 1;
    public int DefaultCapacity = 1;
    public int count = 30;
    public float minDistance = 1f;
    public float maxDistance = 50f;
    public int TreeRolls = 5;
    public int RockRolls = 5;
    public int FruitTreeRolls = 5;
    public GameObject[] TreePrefabs;
    public GameObject[] RockPrefabs;
    public GameObject[] FruitTreePrefabs;

    // Start is called before the first frame update
    void Start() {
        if (autoRandomize) AutoRandomize();
        nodes = new List<ResourceNode>();
        for (int i = 0; i < count; i++) {
            var roll = Random.Range(1, TreeRolls + RockRolls + FruitTreeRolls);
            if (roll <= TreeRolls) {
                Spawn(TreePrefabs, "Tree", DefaultCapacity, 0.1f, new Vector3(2, 2, 2));

            }
            else if (TreeRolls < roll && roll <= RockRolls) {
                Spawn(RockPrefabs, "Ore", DefaultCapacity, 0.1f, new Vector3(15, 15, 15));
            }
            else {
                Spawn(FruitTreePrefabs, "Fruit", DefaultCapacity, 0.1f, new Vector3(1, 1, 1));
            }
        }
        StartCoroutine(Respawn());
    }
    void AutoRandomize() {
        TreeRolls = Random.Range(0, 100);
        RockRolls = Random.Range(0, 100 - TreeRolls);
        FruitTreeRolls = 100 - TreeRolls - RockRolls;
        count = Random.Range(0, 100);
    }
    void Spawn(GameObject[] pool, string ResourceName, int capacity, float minScalePercentage, Vector3 scale) {
        var obj = Instantiate(pool[Random.Range(0, pool.Length - 1)], transform);
        var signX = Random.Range(0, 9) >= 5 ? 1 : -1;
        var signZ = Random.Range(0, 9) >= 5 ? 1 : -1;
        obj.transform.localPosition = new Vector3(signX * Random.Range(minDistance, maxDistance), 2f, signZ * Random.Range(minDistance, maxDistance));
        obj.transform.localScale = scale;
        var resource = obj.AddComponent<PrimitiveResource>();
        resource.ResourceName = ResourceName;
        resource.MinScalePercentage = minScalePercentage;
        resource.SetStartingCapacity(capacity);
        MultipleResourcesManager.Instance.AddResource(resource);
        nodes.Add(new ResourceNode { obj = obj, resource = resource });
    }
    IEnumerator Respawn() {
        while (true) {
            if (respawnChance == 0 || nodes.Count == 0) break;
            if (Random.Range(0, 1f) <= ((float)respawnChance)/100) {
                var harvestedNodes = nodes.Where(x => x.resource.Capacity < x.resource.startingCapacity).ToList();
                if (harvestedNodes.Count > 0) {
                    var index = Random.Range(0, harvestedNodes.Count - 1);
                    //harvestedNodes[index].resource.AddResource(harvestedNodes[index].resource.startingCapacity);
                    harvestedNodes[index].resource.AddResource(1f);
                }
            }
            yield return new WaitForSeconds(respawnChanceCooldownTimer);
        }
        yield return null;
    }
    public class ResourceNode {
        public GameObject obj;
        public PrimitiveResource resource;
    }
}
