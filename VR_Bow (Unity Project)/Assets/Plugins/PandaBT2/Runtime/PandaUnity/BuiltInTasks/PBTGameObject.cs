using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PandaBT.Builtin
{
    public class PBTGameObject : MonoBehaviour
    {
		[PandaTask]
		void Activate(GameObject go)
		{
			SetActive(go, true);
		}

		[PandaTask]
		void Deactivate(GameObject go)
		{
			SetActive(go, false);
		}

		[PandaTask]
		void SetActive(GameObject go, bool isActive)
		{
			go.SetActive(isActive);
			PandaTask.Succeed();
		}

		[PandaTask]
		void IsActiveSelf(GameObject go)
		{
			PandaTask.Complete(go.activeSelf);
		}

		[PandaTask]
		void IsActiveInHierarchy(GameObject go)
		{
			PandaTask.Complete(go.activeInHierarchy);
		}

		[PandaTask]
		private void Destroy(GameObject go)
		{
			GameObject.Destroy(go);
			PandaTask.Succeed();
		}

	}
}
