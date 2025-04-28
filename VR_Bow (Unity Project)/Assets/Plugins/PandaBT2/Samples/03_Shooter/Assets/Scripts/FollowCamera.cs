using UnityEngine;
using System.Collections;
using PandaBT;

namespace PandaBT.Examples
{
    public class FollowCamera : MonoBehaviour
    {

        public GameObject target;

        Vector3 offset = Vector3.zero;

        // Use this for initialization
        void Start()
        {
            if (target != null)
            {
                offset = this.transform.position - target.transform.position;
                offset.x = offset.z = 0.0f;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if( target != null)
                this.transform.position = target.transform.position + offset;
        }
    }
}
