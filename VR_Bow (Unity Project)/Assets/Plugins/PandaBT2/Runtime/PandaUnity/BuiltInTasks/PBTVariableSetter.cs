using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PandaBT.Builtin
{
    public class PBTVariableSetter : MonoBehaviour
    {
        [PandaTask] bool Set(IVariable var, bool value) { var.value = value; return true; }
        [PandaTask] bool Set(IVariable var, int value) { var.value = value; return true; }
        [PandaTask] bool Set(IVariable var, float value) { var.value = value; return true; }
        [PandaTask] bool Set(IVariable var, string value) { var.value = value; return true; }
        
        [PandaTask] bool Set(IVariable var, Vector2 value) { var.value = value; return true; }
        [PandaTask] bool Set(IVariable var, Vector3 value) { var.value = value; return true; }
        [PandaTask] bool Set(IVariable var, Vector4 value) { var.value = value; return true; }
        [PandaTask] bool Set(IVariable var, Matrix4x4 value) { var.value = value; return true; }
        [PandaTask] bool Set(IVariable var, Quaternion value) { var.value = value; return true; }
        [PandaTask] bool Set(IVariable var, Color value) { var.value = value; return true; }
        
        [PandaTask] bool Set(IVariable var, GameObject value) { var.value = value; return true; }
        [PandaTask] bool Set(IVariable var, Transform value) { var.value = value; return true; }
        [PandaTask] bool Set(IVariable var, Material value) { var.value = value; return true; }

    }

}
