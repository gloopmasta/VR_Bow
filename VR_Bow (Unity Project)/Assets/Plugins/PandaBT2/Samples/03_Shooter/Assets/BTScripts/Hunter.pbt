// This Behaviour Tree controls the 'Hunter' unit.

// The highest priority action is to run for cover in critical situations.
// If there is no critical threat, this unit will chase the enemy and
// shoot at it on sight.
// When there is nothing to run away from or to shoot at, just idle.
#BeAlive fallback
	#RunForCover
	while not #IsThreatened
		fallback
			#FireOnSight
			#HideAndReload
			#ChaseEnemy
			#Idle

// Shoot at the enemy while it is visible.
#FireOnSight sequence
	IsVisible_Enemy
	Stop
	while IsVisible_Enemy
		repeat sequence
			SetTarget_Enemy
			AimAt_Target
			#Fire
			//When no more ammo, run for cover sometimes
			random
				Succeed
				HasAmmo

// Hide and reload if ammo is less than 3
#HideAndReload while Ammo_LessThan(3)
	#Hide
