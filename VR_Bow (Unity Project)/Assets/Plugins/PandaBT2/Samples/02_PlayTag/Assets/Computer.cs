using UnityEngine;
using System.Collections;

using PandaBT;

namespace PandaBT.Examples.PlayTag
{
	public class Computer : MonoBehaviour
	{

		public float speed = 1.0f; // per second.
		public Color it;
		public Color notIt;

		public Dialogue tagDialogue;



		Player player;

		#region variables
		[PandaVariable] Vector3 destination = Vector3.zero;
		[PandaVariable] Vector3 playerPosition => player != null ? player.gameObject.transform.position : Vector3.zero;
		[PandaVariable] Vector3 randomPosition 
		{
			get
			{
				var pos = Random.insideUnitSphere * 5.0f;
				pos.y = 0.0f;
				return pos;
			}
		}
		#endregion

		#region tasks

		[PandaTask]
		bool IsIt = true; // Whether the agent is "It".

		bool _IsColliding_Player = false;

		[PandaTask]
		bool IsColliding_Player
		{
			get
			{
				return _IsColliding_Player;
			}
		}

		/*
		 * Whether the player is near.
		 */
		[PandaTask]
		void IsPlayerNear()
		{
			float distanceToPlayer = Vector3.Distance(player.transform.position, this.transform.position);
			PandaTask.Complete(  distanceToPlayer < 4.0f );
		}

		/*
		* Pop a text over the agent.
		*/
		[PandaTask]
		bool Say(string text)
		{
			tagDialogue.SetText(text);
			tagDialogue.speaker = this.gameObject;
			tagDialogue.ShowText();
			return true;
		}

		/*
		 * Move to the destination at the current speed and succeeds when the destination has been reached.
		 */
		[PandaTask]
		void MoveTo(Vector3 position)
		{
			var delta = position - transform.position;

			if (transform.position != position)
			{
				var velocity = delta.normalized * speed;
				transform.position = transform.position + velocity * Time.deltaTime;

				// Check for overshooting the destination.
				// Succeed when the destination is reached.
				var newDelta = position - transform.position;
				if (Vector3.Dot(newDelta, delta) < 0.0f)
				{
					transform.position = position;
				}
			}

			if (transform.position == position)
				PandaTask.Succeed();
		}

		/*
		 * Succeeds when the current destination direction is safe. 
		 */
		[PandaTask]
		bool IsDirectionSafe
		{ 
			get
			{
				Vector3 playerDirection = (player.transform.position - this.transform.position).normalized;
				Vector3 destinatioDirection = (destination - this.transform.position).normalized;
				bool isSafe = Vector3.Angle(destinatioDirection, playerDirection) > 45.0f;
				return isSafe;
			}
		}

		/*
		 * Set the current speed and succeeds. 
		 */
		[PandaTask]
		bool SetSpeed( float speed )
		{
			this.speed = speed;
			return true;
		}


		 /*
		 * Tag and apply the color accordingly.
		 */
		[PandaTask]
		bool Tag()
		{
			DoTag();
			return true;
		}

		#endregion

		void DoTag()
		{
			IsIt = !IsIt;
		}

		private void Awake()
		{
			player = FindObjectOfType<Player>();
		}

		// Use this for initialization
		void Start()
		{
			DoTag();
		}



		void OnTriggerEnter(Collider other)
		{
			if (other.gameObject == player.gameObject )
				_IsColliding_Player = true;      
		}

		void OnTriggerExit(Collider other)
		{
			if (other.gameObject == player.gameObject)
				_IsColliding_Player = false;       
		}

	}
}
