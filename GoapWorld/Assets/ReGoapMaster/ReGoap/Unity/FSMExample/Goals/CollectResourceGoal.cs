﻿using ReGoap.Core;

namespace ReGoap.Unity.FSMExample.Goals
{
    public class CollectResourceGoal : ReGoapGoal<string, object>
    {
        public string ResourceName;

        protected override void Awake()
        {
            base.Awake();
            goal.Set(Literals.CollectedResource(ResourceName), true);
            goal.Set(Literals.reconcilePosition, true);
        }

        public override string ToString()
        {
            return string.Format("GoapGoal('{0}', '{1}')", Name, ResourceName);
        }
    }
}

