using UnityEngine;
using System.Collections;
using PandaBT;


namespace PandaBT.Examples.Shooter
{
	public class Unit: MonoBehaviour
	{


		public int team = 0; // Team the unit is in. A unit in a different team is an enemy.
		[PandaVariable] public float health = 10.0f; // HP
		public GameObject bulletPrefab;
		public GameObject jammedEffectPrefab;
		public float rotationSpeed = 1.0f;
		public float reloadRate = 0.5f; // How many bullet per second get restored.
		public int ammo = 5;

		public GameObject explosionPrefab;
		[HideInInspector]
		public Unit shotBy; // Last unit that shot this unit.

		[HideInInspector]
		public float lastShotTime; // Time when the unit has been shot for the last time.

		[HideInInspector]
		public Unit lastHit; // Last unit this unit shot to.

		[HideInInspector]
		public float lastHitTime; // Last time this unit hit another unit.

		[HideInInspector]
		public UnityEngine.AI.NavMeshAgent navMeshAgent;

		[HideInInspector]
		public Vector3 destination; // The movement destination.
		public Vector3 target;      // The position to aim to.

		[HideInInspector]
		public float startHealth; 



		[HideInInspector]
		public int startAmmo;

		[HideInInspector]
		public float lastReloadTime; // Last time a bullet has been restored.

		// Use this for initialization
		void Start()
		{
			lastShotTime = lastHitTime = float.NegativeInfinity;

			navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
			startHealth = health;
			destination = this.transform.position;

			startAmmo = ammo;
			lastReloadTime = float.NegativeInfinity;
		}

		void Update()
		{
			if( (Time.time - lastReloadTime) * reloadRate >= 1.0f )
			{
				ammo++;
				if (ammo > startAmmo) ammo = startAmmo;
				lastReloadTime = Time.time;
			}

		}


		Vector3 GetMousePosition()
		{
			var mousePosition = Input.mousePosition;
			mousePosition.z = -Camera.main.worldToCameraMatrix.MultiplyPoint(this.transform.position).z;
			var pos = Camera.main.ScreenToWorldPoint(mousePosition);
			return pos;
		}

		#region navigation tasks

		[PandaTask]
		public bool IsHealthLessThan( float health )
		{
			return this.health < health;
		}

		[PandaTask]
		public bool IsHealth_PercentLessThan(float percent)
		{
			return (this.health / startHealth)*100.0 < percent;
		}

		[PandaTask]
		public bool HasAmmo()
		{
			return ammo > 0;
		}

		[PandaTask]
		public bool Ammo_LessThan( int i )
		{
			if(PandaTask.isInspected)
				PandaTask.debugInfo = string.Format("a={0}", ammo);
			return ammo < i;
		}


		[PandaTask]
		public bool SetDestination(Vector3 p)
		{
			destination =  p;
			navMeshAgent.destination = destination;

			if( PandaTask.isInspected )
				PandaTask.debugInfo = string.Format("({0}, {1})", destination.x, destination.y);
			return true;
		}

		[PandaTask]
		public void WaitArrival()
		{
			if (PandaTask.isStarting )
				return;

			float d = navMeshAgent.remainingDistance;
			if (navMeshAgent.remainingDistance <= 1e-2)
			{
				d = 0.0f;
				PandaTask.Succeed();
			}

			PandaTask.debugInfo = string.Format("d-{0:0.00}", d );
		}

		[PandaTask]
		public void MoveTo(Vector3 dst)
		{
			SetDestination(dst);
			if (PandaTask.isStarting)
				navMeshAgent.isStopped = false;
			WaitArrival();
		}

		[PandaTask]
		public void MoveTo_Destination()
		{
			MoveTo(destination);
			WaitArrival();
		}

		[PandaTask]
		public bool SetDestination_Mouse()
		{
			var pos = GetMousePosition();
			SetDestination(pos);
			return true;
		}

		[PandaTask]
		public bool SetDestination_WASD()
		{
			bool isSet = false;
			Vector3 dir = Vector3.zero;

			if (Input.GetKey(KeyCode.W))
				dir += Vector3.forward;
			if (Input.GetKey(KeyCode.A))
				dir += Vector3.left;
			if (Input.GetKey(KeyCode.S))
				dir += Vector3.back;
			if (Input.GetKey(KeyCode.D))
				dir += Vector3.right;

			if (dir.magnitude > 0.0f)
			{
				var pos =this.transform.position + dir.normalized;
				SetDestination(pos);
				isSet = true;
			}


			return isSet;
		}


		[PandaTask]
		public bool Stop()
		{
			navMeshAgent.isStopped = true;
			return true;
		}

		[PandaTask]
		public bool LastShotTime_LessThan(float duration)
		{
			var t = (Time.time - lastShotTime);
			if(PandaTask.isInspected )
				PandaTask.debugInfo = string.Format("t={0:0.00}", t);
			return t < duration;
		}
		#endregion

		#region combat tasks

		[PandaTask]
		public bool Fire()
		{

			var bulletOb = ammo > 0? GameObject.Instantiate(bulletPrefab): GameObject.Instantiate(jammedEffectPrefab);

			bulletOb.transform.position = this.transform.position;
			bulletOb.transform.rotation = this.transform.rotation;
			if (ammo > 0)
			{
				

				var bullet = bulletOb.GetComponent<Bullet>();
				bullet.shooter = this.gameObject;

				ammo--;
				lastReloadTime = Time.time;
			}
			else
			{
				lastReloadTime = Time.time + (1.0f/reloadRate);
				bulletOb.transform.parent = this.transform;
			}


			return true;
		}

		[PandaTask]
		public bool SetTarget( Vector3 target )
		{
			this.target = target;
			this.target.y = this.transform.position.y;
			return true;
		}

		[PandaTask]
		public bool SetTarget_Mouse()
		{
			target = GetMousePosition();
			return true;
		}

		[PandaTask]
		bool SetTarget_Destination()
		{
			return SetTarget(destination);
		}

		[PandaTask]
		public void AimAt_Target()
		{
			var targetDelta = (target - this.transform.position);
			var targetDir = targetDelta.normalized;
			targetDir.y = 0.0f;

			var targetRot = Quaternion.FromToRotation(transform.forward, targetDir) * this.transform.rotation;
			var deltaRot = Quaternion.RotateTowards(this.transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
			this.transform.rotation = deltaRot;

			float angle = Vector3.Angle(targetDir, transform.forward);

			if (angle < 0.1f)
			{
				this.transform.rotation = targetRot;
				PandaTask.Succeed();
			}

			if (PandaTask.isInspected )
				PandaTask.debugInfo = string.Format("angle={0}", Vector3.Angle(targetDir, this.transform.forward));


		}
		#endregion

		[PandaTask]
		bool Explode()
		{
			ShooterGameController.instance.OnUnitDestroy(this);

			if (explosionPrefab != null)
			{
				var explosion = GameObject.Instantiate(explosionPrefab);
				explosion.transform.position = this.transform.position;
			}

			Destroy(this.gameObject);
			return true;
		}


	}
}
