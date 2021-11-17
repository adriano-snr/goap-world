using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAgent01Advanced<T, W> : CustomAgent01<T, W> {
    #region UnityFunctions
    protected virtual void Update() {
        possibleGoalsDirty = true;

        if (currentActionState == null) {
            if (!IsPlanning)
                CalculateNewGoal();
            return;
        }
    }
    #endregion
}
