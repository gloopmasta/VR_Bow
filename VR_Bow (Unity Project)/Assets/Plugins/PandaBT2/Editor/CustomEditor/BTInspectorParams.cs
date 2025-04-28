using UnityEngine;


namespace PandaBT.Runtime
{
    public struct BTInspectorParams
    {
        public BehaviourTree.UpdateOrder tickOn;
        public float customTickInterval;
        public bool reapeatRoot;
        public int scriptCount;
        public bool repeatRetryOnSameTick;
        public PandaBTScript[] scripts;

        public static bool equals(BTInspectorParams a, BTInspectorParams b)
        {

            bool sameScripts = BTInspectorParams.sameScripts(a, b);

            return a.tickOn == b.tickOn
                && a.customTickInterval == b.customTickInterval
                && a.reapeatRoot == b.reapeatRoot
                && a.scriptCount == b.scriptCount
                && a.repeatRetryOnSameTick == b.repeatRetryOnSameTick
                && sameScripts;
        }

        public static bool sameScripts(BTInspectorParams a, BTInspectorParams b)
        {

            if (a.scripts == null && b.scripts == null)
                return true;

            if (a.scripts == null && b.scripts != null)
                return false;

            if (a.scripts != null && b.scripts == null)
                return false;


            bool sameScripts = false;

            if (a.scripts.Length == b.scripts.Length)
            {
                sameScripts = true;
                for (int i = 0; i < a.scripts.Length; i++)
                {
                    if (a.scripts[i] != b.scripts[i])
                    {
                        sameScripts = false;
                        break;
                    }

                }
            }
            return sameScripts;
        }
    }

}

