using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PandaBT.Builtin
{
    public class PBTComponentEnableDisable : MonoBehaviour
    {
		[PandaTask]
		public void Enable(Renderer component)
		{
			component.enabled = true;
			PandaTask.Succeed();
		}

		[PandaTask]
		public void Disable(Renderer component)
		{
			component.enabled = false;
			PandaTask.Succeed();
		}

		[PandaTask]
		public void Enable(Collider component)
		{
			component.enabled = true;
			PandaTask.Succeed();
		}

		[PandaTask]
		public void Disable(Collider component)
		{
			component.enabled = false;
			PandaTask.Succeed();
		}

		[PandaTask]
		public void Enable(Behaviour component)
		{
			component.enabled = true;
			PandaTask.Succeed();
		}

		[PandaTask]
		public void Disable(Behaviour component)
		{
			component.enabled = false;
			PandaTask.Succeed();
		}
	}
}
