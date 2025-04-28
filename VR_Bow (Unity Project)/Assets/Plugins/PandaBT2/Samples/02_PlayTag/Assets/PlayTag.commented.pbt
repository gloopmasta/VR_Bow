// Do whatever is appropriate:
// Chase the player, avoid the player or just Idle.
#PlayTag fallback
	#ChasePlayer
	#AvoidPlayer
	#Idle


// Chase the player while we are "it".
// Use the current player position as destination,
// Then move straight to that destination.
#ChasePlayer while IsIt	repeat sequence
	SetDestination @playerPosition
	MoveTo @destination

// Avoid the player while we are not "it".
// If the player is near by, pick a destination at random.
// If that destination is safe  player is not in the way,
// move straight to it.
// Otherwise the tree fails.
#AvoidPlayer while not IsIt	sequence
	IsPlayerNear
	SetDestination @randomPosition
	IsDirectionSafe
	MoveTo @destination

// Idle while we are not "it" and the player
// is far away.
#Idle while (!IsIt & !IsPlayerNear)
	repeat Succeed // Just repeat doing nothing.
