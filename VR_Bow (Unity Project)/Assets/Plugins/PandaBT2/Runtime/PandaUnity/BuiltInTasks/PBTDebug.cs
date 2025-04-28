using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PandaBT.Builtin
{
    public class PBTDebug : MonoBehaviour
    {
		/// <summary>
		/// Log \p message to the console and succeed immediately.
		/// </summary>
		/// <param name="message"></param>
		[PandaTask]
		public void DebugLog(string message)
		{
			Debug.Log(message);
			PandaTask.Succeed();
		}

		/// <summary>
		/// Log the error \p message to the console and succeed immediately.
		/// </summary>
		/// <param name="message"></param>
		[PandaTask]
		public void DebugLogError(string message)
		{
			Debug.LogError(message);
			PandaTask.Succeed();
		}

		/// <summary>
		/// Log the warning \p message to the console and succeed immediately.
		/// </summary>
		/// <param name="message"></param>
		[PandaTask]
		public void DebugLogWarning(string message)
		{
			Debug.LogWarning(message);
			PandaTask.Succeed();
		}

		/// <summary>
		/// Break (pause the editor) and succeed immediately.
		/// </summary>
		[PandaTask]
		public void DebugBreak()
		{
			if( PandaTask.isStarting )
				Debug.Break();
			else
				PandaTask.Succeed();
		}

	}
}
