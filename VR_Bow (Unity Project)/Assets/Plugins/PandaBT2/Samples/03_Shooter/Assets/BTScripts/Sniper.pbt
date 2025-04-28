// This Behaviour Tree controls the 'Sniper' unit.

// This unit shoots the enemy on sight from a distance.
// If the enemy is not in sight, chase it.
// When there is no enemy, this unit just idles.
#BeAlive fallback
	#Snipe
	#ChaseEnemy
	#Idle

// When the enemy becomes visible,
// shoot a burst of bullet at it.
// Then hide for a while.
#Snipe sequence
	IsVisible_Enemy
	Stop
	Wait 0.5
	SetTarget_Enemy
	AimAt_Target
	Wait 0.1
	repeat 3 sequence
		fallback
			Fire
			Wait 0.5 //if fire failed, have a wait penalty.
		Wait 0.05
	Wait 1.0
	race
		#Hide
		Wait 5.0



