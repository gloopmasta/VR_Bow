using UnityEngine;
using System.Collections;
using PandaBT;
using System.Threading.Tasks;

namespace PandaBT.Examples.ChangeColor
{
    public class MyCube : MonoBehaviour
    {
        /*
         * Set the color to the specified rgb value and succeed.
         */
        [PandaTask] // <-- Attribute used to tag a class member as a task implementation.
        void SetColor(float r, float g, float b)
        {
            this.GetComponent<Renderer>().material.color = new Color(r, g, b);
            PandaTask.Succeed(); // Succeed this task
        }
    }
}
