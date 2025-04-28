// Play tag
#PlayTag fallback
	#ChasePlayer
	#AvoidPlayer 
	#Idle

// Move to the player while we are 'it'.
#ChasePlayer while IsIt sequence
	Set &destination @playerPosition
	MoveTo @destination
	Wait 1.0

// Avoid the player while we are not 'it'.
#AvoidPlayer while not IsIt sequence
	IsPlayerNear
	Set &destination @randomPosition
	IsDirectionSafe @destination
	MoveTo @destination

// Idle when we are not 'it' and the player is far.
#Idle while (!IsIt & !IsPlayerNear)
	Running // Repeat doing nothing.
