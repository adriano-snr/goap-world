using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

public class StatCounter : MonoBehaviour
{
    public int Period = 30;
    public bool updateLog = true;
    private Dictionary<string, Dictionary<string, object>> stats;
    private Dictionary<string, Dictionary<string, object>> villageStats;
    private Dictionary<string, Dictionary<string, object>> plans;
    public static StatCounter Instance;
    void Awake() {
        if (Instance != null) throw new UnityException("[StatCounter] Can have only one instance per scene.");
        Instance = this;
        stats = new Dictionary<string, Dictionary<string, object>>();
        villageStats = new Dictionary<string, Dictionary<string, object>>();
        plans = new Dictionary<string, Dictionary<string, object>>();
        //WriteStatsToFile(true);
        //WriteVillageStatsToFile(true);
        //WritePlansToFile(true);
        StartCoroutine(UpdateLog(true));
    }
    private IEnumerator UpdateLog(bool startNew) {
        while (true) {
            if (!updateLog) break;
            WriteStatsToFile(startNew);
            WriteVillageStatsToFile(startNew);
            WritePlansToFile(startNew);
            startNew = false;
            yield return new WaitForSeconds(Period);
        }
        yield return null;
    }
    public static void AddStat(string charName, string statName, object value) {
        if (!Instance.stats.ContainsKey(charName)) {
            Instance.stats[charName] = new Dictionary<string, object>();
        }
        if (!Instance.stats[charName].ContainsKey(statName)) {
            Instance.stats[charName][statName] = value;
        }
        else {
            double dvaleu;
            if (double.TryParse(value.ToString(), out dvaleu)) {
                var current = double.Parse(Instance.stats[charName][statName].ToString());
                Instance.stats[charName][statName] = current + dvaleu;
            }
            else {
                Instance.stats[charName][statName] = value;
            }
        }
        if (charName.EndsWith("Village 01")) AddVillageStat("Village 01", statName, value);
        else if (charName.EndsWith("Village 02")) AddVillageStat("Village 02", statName, value);
        else if (charName.EndsWith("Village 03")) AddVillageStat("Village 03", statName, value);
    }
    static void AddVillageStat(string villageName, string statName, object value) {
        if (!Instance.villageStats.ContainsKey(villageName)) {
            Instance.villageStats[villageName] = new Dictionary<string, object>();
        }
        if (!Instance.villageStats[villageName].ContainsKey(statName)) {
            Instance.villageStats[villageName][statName] = value;
        }
        else {
            double dvaleu;
            if (double.TryParse(value.ToString(), out dvaleu)) {
                var current = double.Parse(Instance.villageStats[villageName][statName].ToString());
                Instance.villageStats[villageName][statName] = current + dvaleu;
            }
            else {
                Instance.villageStats[villageName][statName] = value;
            }
        }
    }
    public static void AddPlan(string charName, string goalName, string time, List<string> plan) {
        if (!Instance.plans.ContainsKey(charName)) {
            Instance.plans[charName] = new Dictionary<string, object>();
        }
        var sb = new StringBuilder();
        sb.AppendLine($"Goal name: { goalName }.");
        sb.AppendLine(" > Plan start:");
        int count = 1;
        foreach (var action in plan) {
            sb.AppendLine($"            { count++ }. {action}");
        }
        sb.Append(" > Plan conclusion");
        Instance.plans[charName].Add($"Plan at { time } seconds", sb.ToString());
    }
    public static string StatsToString(Dictionary<string, Dictionary<string, object>> data) {
        var sb = new StringBuilder();
        sb.AppendLine($"------------------ Begin Game Stats at time: { Time.time } ------------------");
        foreach (var npcName in data.Keys) {
            var charData = data[npcName];
            sb.AppendLine($"Char name: {npcName}; Stats: {{");
            foreach (var stat in charData.Keys) {
                sb.AppendLine($"    { stat }: { charData[stat] }");
            }
            sb.AppendLine("}");
        }
        sb.AppendLine($"------------------ End Game Stats at time: { Time.time } ------------------");
        return sb.ToString();
    }
    public static string PlansToString() {
        var sb = new StringBuilder();
        sb.AppendLine($"------------------ Begin Game Plans at time: { Time.time } ------------------");
        foreach (var npcName in Instance.plans.Keys) {
            var charData = Instance.plans[npcName];
            sb.AppendLine($"Char name: {npcName}; Plans: {{");
            foreach (var plan in charData.Keys) {
                sb.AppendLine($"    { plan }: { charData[plan] }");
            }
            sb.AppendLine("}");
        }
        sb.AppendLine($"------------------ End Game Plans at time: { Time.time } ------------------");
        Instance.plans.Clear();
        return sb.ToString();
    }
    public static void WriteStatsToFile(bool beginNew) {
        Debug.Log($"Log update for { Time.time } seconds");
        string path = Application.persistentDataPath + "/Log.txt";
        if (beginNew) File.WriteAllText(path, "");
        else {
            StreamWriter writer = new StreamWriter(path, true);
            writer.Write(StatsToString(Instance.stats));
            writer.Close();
        }
    }
    public static void WriteVillageStatsToFile(bool beginNew) {
        //Debug.Log("Log updated.");
        string path = Application.persistentDataPath + "/LogVillageTotals.txt";
        if (beginNew) File.WriteAllText(path, "");
        else {
            StreamWriter writer = new StreamWriter(path, true);
            writer.Write(StatsToString(Instance.villageStats));
            writer.Close();
        }
    }
    public static void WritePlansToFile(bool beginNew) {
        //Debug.Log("Log updated.");
        string path = Application.persistentDataPath + "/PlanLog.txt";
        if (beginNew) File.WriteAllText(path, "");
        else {
            //Write some text to the test.txt file
            StreamWriter writer = new StreamWriter(path, true);
            //writer.WriteLine("Test");
            writer.Write(PlansToString());
            writer.Close();
            //StreamReader reader = new StreamReader(path);
            //Print the text from the file
            //Debug.Log(reader.ReadToEnd());
            //reader.Close();
        }
    }
}
