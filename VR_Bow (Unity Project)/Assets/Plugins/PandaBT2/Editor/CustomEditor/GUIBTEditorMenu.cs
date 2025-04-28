using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


using PandaBT.Compilation;
namespace PandaBT.Runtime
{
    public class GUIBTEditorMenu
    {
        [MenuItem("Window/Panda BT 2 Editor/Break Point/Break On Start", false, 72)]
        public static void Break_PointSet_Start()
        {
            EditableBTScript.BreakPoint_Set(Status.Running);
        }

        [MenuItem("Window/Panda BT 2 Editor/Break Point/Break On Succeed", false, 72)]
        public static void Break_PointSet_Succeed()
        {
            EditableBTScript.BreakPoint_Set(Status.Succeeded);
        }

        [MenuItem("Window/Panda BT 2 Editor/Break Point/Break On Fail", false, 72)]
        public static void Break_PointSet_Fail()
        {
            EditableBTScript.BreakPoint_Set(Status.Failed);
        }

        [MenuItem("Window/Panda BT 2 Editor/Break Point/Break On Abort", false, 72)]
        public static void Break_PointSet_Abort()
        {
            EditableBTScript.BreakPoint_Set(Status.Aborted);
        }

        [MenuItem("Window/Panda BT 2 Editor/Break Point/Clear", false, 72)]
        public static void Break_PointSet_Clear()
        {
            EditableBTScript.BreakPoint_Set(Status.Ready);
        }

        [MenuItem("Window/Panda BT 2 Editor/Break Point/Clear All", false, 72)]
        public static void Break_PointSet_ClearAll()
        {
            EditableBTScript.BreakPoint_ClearAll();
        }

    }
}
