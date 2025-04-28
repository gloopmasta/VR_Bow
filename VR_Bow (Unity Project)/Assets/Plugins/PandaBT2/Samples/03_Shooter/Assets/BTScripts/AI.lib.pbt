// This BT script contains common behaviours
// shared among the different units.

// Root behaviour for all unit: Be alive or die.
#Root parallel
	repeat mute #BeAlive
	repeat mute #Die

// While no enemy is spotted,
// Look around randomly.
#Idle while (!IsEnemySpotted & !Acquire_Enemy)
	repeat sequence
		random
			SetTarget_Angle(90.0)
			SetTarget_Angle(-90.0)
			SetTarget_Angle(135.0)
			SetTarget_Angle(-135.0)
		AimAt_Target
		WaitRandom(0.5, 2.0)

// Move to cover if threatened.
// Search for a spot not in the line of sight of the attacker,
// then move there.
// When no more threatened, wait a random timelapse before
// doing something else.
#RunForCover sequence
	#IsThreatened
	fallback
		// if no more ammo, have a wait penalty.
		HasAmmo
		Wait(1.0)
	repeat fallback
		// Got to a cover location
		// not in line of sight of the attacker.
		while #IsThreatened
			fallback
				while not IsThereLineOfSight_Attacker_Destination
					sequence
						MoveTo_Destination
						SetTarget_EnemyLastSeenPosition
						AimAt_Target

				SetDestination_Cover

		// Unpredictably quit cover.
		sequence
			not #IsThreatened
			WaitRandom(0.0, 1.5)
			Fail //Quit Cover


// Search for a location not in the line of sight of the enemy,
// then move there.

#Hide repeat fallback
	while not IsThereLineOfSight_Attacker_Destination
		MoveTo_Destination
	SetDestination_Cover

// A unit is under threat if HP is under a critical threshold
// while the enemy has ammo and at least one of the following conditions
// is satisfied:
// - The unit has just been shot.
// - The unit has just seen a bullet.
// - The unit has no more ammo.
#IsThreatened sequence
	IsHealth_PercentLessThan(60.0)
	HasAmmo_Ememy
	fallback
		LastShotTime_LessThan(3.0)
		LastBulletSeenTime_LessThan(1.0)
		not HasAmmo

// Die by exploding if no more HP.
#Die sequence
	IsHealthLessThan(0.1)
	Explode

// Chase the enemy when it is not visible.
// Shoot some bullet randomly to show agresivity.
// When the enemy is out of sight for a long time,
// search randomly around.
#ChaseEnemy	while IsEnemySpotted
	fallback
		while not IsVisible_Enemy
			parallel
				repeat sequence
					WaitRandom(0.5, 1.0)
					#Fire

				repeat sequence
					SetDestination_Enemy
					MoveTo_Destination

				sequence
					Wait(5.0)
					Fail
		#SearchAround

// While the enemy is not visible,
// move around randomly then forget about
// the enemy if not found.
#SearchAround while (!IsVisible_Enemy & Wait(0.5))
	sequence
		repeat(4) sequence
			SetDestination_Random(5.0)
			MoveTo_Destination
			Wait(0.5)
		Clear_Enemy

// Fire
#Fire sequence
	Wait(0.3)
	Fire
