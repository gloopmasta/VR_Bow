using UnityEngine;
using System.Collections;

using PandaBT;
namespace PandaBT.Examples.Move
{
    public class Move : MonoBehaviour
    {

        // These variables can be used as task parameters from the BT script by prefixing the variable name with '@'.
        [PandaVariable] Vector2 bottomLeft = new Vector2(-1.0f, -1.0f);
        [PandaVariable] Vector2 bottomRight = new Vector2(1.0f, -1.0f);
        [PandaVariable] Vector2 topRight = new Vector2(1.0f, 1.0f);
        [PandaVariable] Vector2 topLeft = new Vector2(-1.0f, 1.0f);
        float speed = 1.0f; // current speed


        /*
         * Move to the `destination` position at the current speed.
         */
        [PandaTask]
        void MoveTo(Vector2 destination)
		{
            MoveTo(destination.x, destination.y);
		}

        [PandaTask]
        void MoveTo(Transform destination)
        {
            var p = destination.position;
            MoveTo(p.x, p.y);
        }


        /*
         * Move to the (x,y) position at the current speed.
         */
        [PandaTask]
        void MoveTo(float x, float y)
        {
            Vector3 destination = new Vector3(x, y, 0.0f);
            Vector3 delta = (destination - transform.position);
            Vector3 velocity = speed*delta.normalized;

            transform.position = transform.position + velocity * Time.deltaTime;

            Vector3 newDelta = (destination - transform.position);
            float d = newDelta.magnitude;
            PandaTask.debugInfo = "";
            if (PandaTask.isInspected)
                PandaTask.debugInfo = string.Format("d={0:0.000}", d);

            if ( Vector3.Dot(delta, newDelta) <= 0.0f || d < 1e-3)
            {
                transform.position = destination;
                PandaTask.Succeed();
                d = 0.0f;
                PandaTask.debugInfo = "d=0.000";
            }

        }

        // This structure is used to store data for rotation tweening.
        struct RotateTween
        {
            public Quaternion startRotation;
            public Quaternion endRotation;
            public float startTime;
        }

        /*
         * Rotate about the given angle for the given duration. 
         */
        [PandaTask]
        void Rotate(float angle, float duration)
        {
            RotateTween rt;

            // The Task.isStarting property is true on the first tick of a task.
            // We used it to perform initialization.
            if ( PandaTask.isStarting ) 
            {
                // Compute tweening data
                rt.startTime = Time.time;
                rt.startRotation = this.transform.localRotation;
                rt.endRotation   = Quaternion.AngleAxis( angle, Vector3.forward)*transform.localRotation;

                // PandaTask.data is a data emplacement attached to a Task.
                // Use it for storing any data requiered for the progression of a task.
                // We use it here to hold the tweening data.
                PandaTask.data = rt;
            }

            rt = PandaTask.GetData<RotateTween>(); // Retrieve the tweening data.

            // Interpolate the current rotation
            float elapsedTime = Time.time - rt.startTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            this.transform.localRotation = Quaternion.Lerp(rt.startRotation, rt.endRotation, t);


            // Display the tweening progression within the code viewer in the Inspector.
            if( PandaTask.isInspected )
                PandaTask.debugInfo = string.Format("t-{0:0.00}", duration - elapsedTime);

            // Succeed the task when the tweening is completed.
            if (t == 1.0f)
            {
                PandaTask.debugInfo = "t=1.00";
                PandaTask.Succeed();
            }
        }


    }
}
