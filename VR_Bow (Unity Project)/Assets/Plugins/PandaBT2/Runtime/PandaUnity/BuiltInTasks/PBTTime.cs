using PandaBT.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PandaBT.Builtin
{
    public class PBTTime : MonoBehaviour
    {


		/// <summary>
		/// Run for \p duration seconds then succeed.
		/// </summary>
		/// <param name="message"></param>
		[PandaTask]
		public void Wait(float duration)
		{
			WaitSeconds(duration);
		}

		/// <summary>
		/// Run for \p duration seconds then succeed.
		/// </summary>
		/// <param name="message"></param>
		[PandaTask]
		public void WaitSeconds(float duration)
		{
			if (PandaTask.isStarting)
			{
				PandaTask.data = Time.time;
			}
			float startTime = (float)PandaTask.data;

			float elapsedTime = Time.time - startTime;
			if (UserTask.isInspected)
			{
				float tta = Mathf.Clamp(duration - elapsedTime, 0.0f, float.PositiveInfinity);
				PandaTask.debugInfo = string.Format("t-{0:0.000}", tta);
			}

			if (elapsedTime >= duration)
			{
				PandaTask.debugInfo = "t-0.000";
				PandaTask.Succeed();
			}
		}


		public class WaitRandomFloatInfo
		{
			public float elapsedTime;
			public float duration;
		}
		/// <summary>
		/// Pick a number in the specified range, wait that number of seconds then succeed.
		/// </summary>
		/// <param name="message"></param>
		/// Thanks to a_horned_goat
		[PandaTask]
		public void WaitRandom(float min, float max)
		{
			var info = PandaTask.data != null ? (WaitRandomFloatInfo)PandaTask.data : (WaitRandomFloatInfo)(PandaTask.data = new WaitRandomFloatInfo());

			if (PandaTask.isStarting)
			{
				info.duration = Random.Range(min, max);
				info.elapsedTime = -Time.deltaTime;
			}

			var duration = info.duration;

			info.elapsedTime += Time.deltaTime;

			if (UserTask.isInspected)
			{
				float tta = Mathf.Clamp(duration - info.elapsedTime, 0.0f, float.PositiveInfinity);
				PandaTask.debugInfo = string.Format("t-{0:0.000}", tta);
			}

			if (info.elapsedTime >= duration)
			{
				PandaTask.debugInfo = "t-0.000";
				PandaTask.Succeed();
			}
		}



		/// <summary>
		/// Run for \p ticks ticks then succeed.
		/// </summary>
		/// <param name="ticks"></param>
		[PandaTask]
		public void Wait(int ticks = 1)
		{
			WaitTicks(ticks);
		}
		public class TaskInfoWaitInt
		{
			public int elapsedTicks = 0;
		}
		/// <summary>
		/// Run for \p ticks ticks then succeed.
		/// </summary>
		/// <param name="ticks"></param>
		[PandaTask]
		public void WaitTicks(int ticks=1)
		{
			var info = PandaTask.data != null ? (TaskInfoWaitInt)PandaTask.data : (TaskInfoWaitInt)(PandaTask.data = new TaskInfoWaitInt());
			if (PandaTask.isStarting)
			{
				info.elapsedTicks = 0;
			}
			else
			{
				// increment tickcount
				info.elapsedTicks++;
			}

			if (UserTask.isInspected)
				PandaTask.debugInfo = string.Format("n-{0}", ticks - info.elapsedTicks);

			if (info.elapsedTicks >= ticks)
			{
				PandaTask.debugInfo = "n-0";
				PandaTask.Succeed();
			}

		}

		public class TaskInfoWaitNextFrame
		{
			public int startFrame = 0;
		}
		/// <summary>
		/// Run for \p ticks ticks then succeed.
		/// </summary>
		/// <param name="frames"></param>
		[PandaTask]
		public void WaitFrames(int frames = 1)
		{
			TaskInfoWaitNextFrame info = null;
			if (PandaTask.isStarting)
			{
				PandaTask.data = info = new TaskInfoWaitNextFrame();
				info.startFrame = Time.frameCount;
			}
			else
			{
				info = PandaTask.data as TaskInfoWaitNextFrame;
			}

			var frameCount = Time.frameCount - info.startFrame;

			if (UserTask.isInspected)
				PandaTask.debugInfo = string.Format("f-{0}", frames - frameCount);

			if (frameCount >= frames)
			{
				PandaTask.debugInfo = "f-0";
				PandaTask.Succeed();
			}

		}


		/// <summary>
		/// Run for \p duration unscaled seconds then succeed.
		/// </summary>
		/// <param name="duration"></param>
		[PandaTask]
		public void RealtimeWait(float duration)
		{
			WaitRealtimeSeconds(duration);
		}

		/// <summary>
		/// Run for \p duration unscaled seconds then succeed.
		/// </summary>
		/// <param name="duration"></param>
		[PandaTask]
		public void WaitRealtimeSeconds(float duration)
		{
			if (PandaTask.isStarting)
			{
				PandaTask.data = Time.unscaledTime;
			}

			float startTime = (float)PandaTask.data;

			float elapsedTime = Time.unscaledTime - startTime;

			if (UserTask.isInspected)
			{
				float tta = Mathf.Clamp(duration - elapsedTime, 0.0f, float.PositiveInfinity);
				PandaTask.debugInfo = string.Format("t-{0:0.000}", tta);
			}

			if (elapsedTime >= duration)
			{
				PandaTask.debugInfo = "t-0.000";
				PandaTask.Succeed();
			}
		}
	}
}
