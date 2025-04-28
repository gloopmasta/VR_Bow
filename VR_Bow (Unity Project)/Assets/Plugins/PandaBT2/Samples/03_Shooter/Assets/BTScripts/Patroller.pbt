// This Behaviour Tree controls the 'Patroller' unit.

// This unit attacks when the enemy is visible,
// otherwise it patrols a predefined path.
#BeAlive
	fallback
		#Attack
		#Patroll

// When the enemy is visible, attack it,
// Otherwise forget about it.
#Attack	fallback
	// Attack the enemy if visible.
	repeat sequence
		IsVisible_Enemy
		Stop
		Wait(0.5)
		SetTarget_Enemy
		AimAt_Target
		#Fire

	// Otherwise forget about it.
	sequence
		Clear_Enemy
		Fail

// While no enemy is spotted,
// follow the assigned waypoints.
#Patroll while (!IsEnemySpotted & !Acquire_Enemy)	repeat sequence
	SetDestination_Waypoint
	SetTarget_Destination
	AimAt_Target
	MoveTo_Destination
	Wait(0.3)
	NextWaypoint
