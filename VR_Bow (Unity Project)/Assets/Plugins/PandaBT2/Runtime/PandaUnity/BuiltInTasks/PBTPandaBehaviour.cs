using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PandaBT.Runtime;
using static PandaBT.Runtime.BehaviourTree;

namespace PandaBT.Builtin
{
    public class PBTPandaBehaviour : MonoBehaviour
    {
		#region PandaBehaviour
		[PandaTask]
		public void Run(BehaviourTree bt)
		{
			if (PandaTask.isStarting)
			{
				if (bt.tickOn != UpdateOrder.Manual)
				{
					throw new System.Exception("The task `Run` can only run PandaBehaviour ticked on 'manual'.");
				}
				bt.Reset();
			}

			bt.Tick();

			if (bt.status != Status.Running)
			{
				if (bt.status == Status.Succeeded)
					PandaTask.Succeed();
				else
					PandaTask.Fail();
			}

		}
		#endregion

	}
}
