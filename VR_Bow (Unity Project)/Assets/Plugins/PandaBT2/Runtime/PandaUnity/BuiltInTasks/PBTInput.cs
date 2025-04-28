using PandaBT.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PandaBT.Builtin
{
    public class PBTInput : MonoBehaviour
    {
		// Is*

		/// <summary>
		/// Succeed if the key \p keycode is down on the current tick, otherwise fail.
		/// </summary>
		/// <param name="keycode"></param>
		[PandaTask]
		public void IsKeyDown(string keycode)
		{
			KeyCode k = GetKeyCode(keycode);
			PandaTask.Complete(Input.GetKeyDown(k));
		}

		/// <summary>
		/// Succeed if the key \p keycode is up on the current tick, otherwise fail.
		/// </summary>
		/// <param name="keycode"></param>
		[PandaTask]
		public void IsKeyUp(string keycode)
		{
			KeyCode k = GetKeyCode(keycode);
			PandaTask.Complete(Input.GetKeyUp(k));
		}

		/// <summary>
		/// Succeed if the key \p keycode is pressed on the current tick, otherwise fail.
		/// </summary>
		/// <param name="keycode"></param>
		[PandaTask]
		public void IsKeyPressed(string keycode)
		{
			KeyCode k = GetKeyCode(keycode);
			PandaTask.Complete(Input.GetKey(k));
		}

		/// <summary>
		/// Succeed if the mouse button \p button is pressed on the current tick, otherwise fail.
		/// </summary>
		/// <param name="button"></param>
		[PandaTask]
		public void IsMouseButtonPressed(int button)
		{
			PandaTask.Complete(Input.GetMouseButton(button));
		}


		/// <summary>
		/// Succeed if the mouse button \p button is up on the current tick, otherwise fail.
		/// </summary>
		/// <param name="button"></param>
		[PandaTask]
		public void IsMouseButtonUp(int button)
		{
			PandaTask.Complete(Input.GetMouseButtonUp(button));
		}

		/// <summary>
		/// Succeed if the mouse button \p button is down on the current tick, otherwise fail.
		/// </summary>
		/// <param name="button"></param>
		[PandaTask]
		public void IsMouseButtonDown(int button)
		{
			PandaTask.Complete(Input.GetMouseButtonDown(button));
		}

		/// <summary>
		/// Succeed if the button \p buttonName is up on the current tick, otherwise fail.
		/// </summary>
		/// <param name="button"></param>
		[PandaTask]
		public void IsButtonUp(string buttonName)
		{
			PandaTask.Complete(Input.GetButtonUp(buttonName));
		}

		/// <summary>
		/// Succeed if the button \p buttonName is down on the current tick, otherwise fail.
		/// </summary>
		/// <param name="button"></param>
		[PandaTask]
		public void IsButtonDown(string buttonName)
		{
			PandaTask.Complete(Input.GetButtonDown(buttonName));
		}

		/// <summary>
		/// Succeed if the button \p buttonName is pressed on the current tick, otherwise fail.
		/// </summary>
		/// <param name="button"></param>
		[PandaTask]
		public void IsButtonPressed(string buttonName)
		{
			PandaTask.Complete(Input.GetButton(buttonName));
		}


		// Wait*
		/// <summary>
		/// Run indefinitely and succeed when the key \p keycode is down.
		/// </summary>
		/// <param name="keycode"></param>
		[PandaTask]
		public void WaitKeyDown(string keycode)
		{
			KeyCode k = GetKeyCode(keycode);

			if (Input.GetKeyDown(k))
				PandaTask.Succeed();
		}

		/// <summary>
		/// Run indefinitely and succeed when any key is down.
		/// </summary>
		/// <param name="keycode"></param>
		[PandaTask]
		public void WaitAnyKeyDown()
		{
			if (!PandaTask.isStarting)
			{
				if (Input.anyKeyDown)
					PandaTask.Succeed();
			}
		}

		/// <summary>
		/// Run indefinitely and succeed when the key \p keycode is up.
		/// </summary>
		/// <param name="keycode"></param>
		[PandaTask]
		public void WaitKeyUp(string keycode)
		{
			KeyCode k = GetKeyCode(keycode);
			if (Input.GetKeyUp(k))
				PandaTask.Succeed();

		}

		/// <summary>
		/// Run indefinitely and succeed the mouse button \p button is up.
		/// </summary>
		/// <param name="button"></param>
		[PandaTask]
		public void WaitMouseButtonUp(int button)
		{
			if (Input.GetMouseButtonUp(button))
				PandaTask.Succeed();
		}

		/// <summary>
		/// Run indefinitely and succeed the mouse button \p button is down.
		/// </summary>
		/// <param name="button"></param>
		[PandaTask]
		public void WaitMouseButtonDown(int button)
		{
			if (Input.GetMouseButtonDown(button))
				PandaTask.Succeed();
		}

		/// <summary>
		///  Run indefinitely and succeed the button \p buttonName is up.
		/// </summary>
		/// <param name="buttonName"></param>
		[PandaTask]
		public void WaitButtonUp(string buttonName)
		{
			if (Input.GetButtonUp(buttonName))
				PandaTask.Succeed();
		}

		/// <summary>
		///  Run indefinitely and succeed the button \p buttonName is down.
		/// </summary>
		/// <param name="buttonName"></param>
		[PandaTask]
		public void WaitButtonDown(string buttonName)
		{
			if (Input.GetButtonUp(buttonName))
				PandaTask.Succeed();
		}

		// Hold*
		class HoldKeyInfo
		{
			public bool hasKeyBeenPressed;
			public float elapsedTime;
		}
		/// <summary>
		/// Run indefinitely and complete when the key \p keycode is up. Succeed if the key had been held for \p duration seconds, otherwise fail.
		/// </summary>
		/// <param name="keycode"></param>
		/// <param name="duration"></param>
		[PandaTask]
		public void HoldKey(string keycode, float duration)
		{
			var info = PandaTask.data != null ? (HoldKeyInfo)PandaTask.data : (HoldKeyInfo)(PandaTask.data = new HoldKeyInfo());

			if (PandaTask.isStarting)
			{
				info.hasKeyBeenPressed = false;
			}

			KeyCode k = GetKeyCode(keycode);

			if (Input.GetKeyDown(k))
			{
				info.hasKeyBeenPressed = true;
				info.elapsedTime = -Time.deltaTime;
			}

			if (info.hasKeyBeenPressed)
			{

				info.elapsedTime += Time.deltaTime;
				if (Input.GetKeyUp(k))
				{
					PandaTask.Complete(info.elapsedTime >= duration);
					PandaTask.debugInfo = "Done";

				}
				else
				{
					if (info.elapsedTime < duration)
					{
						if (UserTask.isInspected)
							PandaTask.debugInfo = string.Format("{0:000}%", Mathf.Clamp01(info.elapsedTime / duration) * 100.0f);
					}
					else
					{
						PandaTask.debugInfo = "Waiting for key release.";
					}
				}

			}

		}

		// IsNext*
		/// <summary>
		/// Run indefinitely and complete when any key is down. Succeed if the key is \p keycode, otherwise fail.
		/// </summary>
		/// <param name="keycode"></param>
		[PandaTask]
		public void IsNextKeyDown(string keycode)
		{
			if (!PandaTask.isStarting && Input.anyKeyDown)
			{
				bool isMouseButton = false;
				for (int i = 0; i < 3; i++)
				{
					if (Input.GetMouseButton(i))
					{
						isMouseButton = true;
						break;
					}
				}

				if (!isMouseButton)
				{
					KeyCode k = GetKeyCode(keycode);
					PandaTask.Complete(Input.GetKeyDown(k));
				}
			}

		}

		public KeyCode GetKeyCode(string keycode)
		{
			return (KeyCode)System.Enum.Parse(typeof(KeyCode), keycode);
		}
	}
}
