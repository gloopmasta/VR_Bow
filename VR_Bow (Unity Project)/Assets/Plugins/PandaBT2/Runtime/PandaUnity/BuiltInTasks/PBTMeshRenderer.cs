using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PandaBT
{
    public class PBTMeshRenderer : MonoBehaviour
    {

        [PandaTask]
        bool SetMesh(MeshFilter meshFilter, Mesh mesh)
		{
            meshFilter.sharedMesh = mesh;
            return true;
		}
    }
}
