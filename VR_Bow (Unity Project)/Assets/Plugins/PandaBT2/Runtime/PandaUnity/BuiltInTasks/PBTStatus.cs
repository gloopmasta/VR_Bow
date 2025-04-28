using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PandaBT.Builtin
{
    public class PBTStatus : MonoBehaviour
    {
		/// <summary>
		/// Succeed immediately.
		/// </summary>
		[PandaTask]
		public void Succeed() { PandaTask.Succeed(); }

		/// <summary>
		/// Fail immediately.
		/// </summary>
		[PandaTask]
		public void Fail() { PandaTask.Fail(); }

		/// <summary>
		/// Run indefinitely.
		/// </summary>
		[PandaTask]
		public void Running() { }

	}
}
