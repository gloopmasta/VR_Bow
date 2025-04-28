using UnityEngine;
using System.Collections;

namespace PandaBT.Examples.Shooter
{
    public class Patroller : MonoBehaviour
    {
        public WaypointPath waypointPath;

        Unit self;
        int waypointIndex;

        // Use this for initialization
        void Start()
        {
            waypointIndex = 0;
            self = GetComponent<Unit>();
        }

        // Update is called once per frame
        void Update()
        {

        }


        [PandaTask]
        bool NextWaypoint()
        {
            if (waypointPath != null)
            {
                waypointIndex++;
                if(PandaTask.isInspected )
                    PandaTask.debugInfo = string.Format("i={0}", waypointArrayIndex);
            }
            return true;
        }

        [PandaTask]
        bool SetDestination_Waypoint()
        {
            bool isSet = false;
            if (waypointPath != null)
            {
                var i = waypointArrayIndex;
                var p = waypointPath.waypoints[i].position;
                isSet = self.SetDestination(p);
            }
            return isSet;
        }

        [PandaTask]
        public bool SetDestination_Waypoint(int i)
        {
            bool isSet = false;
            if (waypointPath != null)
            {
                var p = waypointPath.waypoints[i].position;
                isSet = self.SetDestination(p);
            }
            return isSet;
        }

        [PandaTask]
        public void MoveTo(int i)
        {
            SetDestination_Waypoint(i);
            self.MoveTo_Destination();
            self.WaitArrival();
        }

        [PandaTask]
        public void LookAt(int i)
        {
            self.SetTarget( waypointPath.waypoints[i].transform.position);
            self.AimAt_Target();
        }


        int waypointArrayIndex
        {
            get
            {
                int i = 0;
                if( waypointPath.loop)
                {
                    i = waypointIndex % waypointPath.waypoints.Length;
                }
                else
                {
                    int n = waypointPath.waypoints.Length;
                    i = waypointIndex % (n*2);

                    if( i > n-1 )
                        i = (n-1) - (i % n); 
                }

                return i;
            }
        }
    }
}
