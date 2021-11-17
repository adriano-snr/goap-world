using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class PreconditionsEffectsPair
{
    public List<KeyValuePair<string, object>> preconditions;
    public List<KeyValuePair<string, object>> effects;
    public PreconditionsEffectsPair() {
        preconditions = new List<KeyValuePair<string, object>>();
        effects = new List<KeyValuePair<string, object>>();
    }
    public void AddEffect(string tag, object val) {
        effects.Add(new KeyValuePair<string, object>(tag, val));
    }
    public void AddPrecondition(string tag, object val) {
        preconditions.Add(new KeyValuePair<string, object>(tag, val));
    }
    public override string ToString() {
        //return base.ToString();
        var sb = new StringBuilder();
        sb.Append("[[EFF: ");
        for (int i = 0; i < effects.Count; i++) {
            sb.Append($"({effects[i].Key} = {effects[i].Value})");
        }
        sb.Append("## PRE: ");
        for (int i = 0; i < preconditions.Count; i++) {
            sb.Append($"({preconditions[i].Key} = {preconditions[i].Value})");
        }
        sb.Append("]]");
        return sb.ToString();
    }
}
