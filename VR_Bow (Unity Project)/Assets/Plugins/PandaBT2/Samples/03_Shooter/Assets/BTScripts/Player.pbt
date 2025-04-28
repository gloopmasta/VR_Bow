// This Behaviour Tree controls a unit using the player inputs.

// Controll movement, aiming, firing and dying.
#Root parallel 
	repeat mute #Move
	repeat mute #Aim
	repeat mute #Fire
	repeat mute #Die

// Move to the destination.
// Set the mouse cursor as destination when right click,
// Or set the destination in the direction given by the WASD keys.
#Move parallel
	repeat MoveTo_Destination
	repeat mute fallback
		SetDestination_WASD
		if IsMouseButtonPressed(1) SetDestination_Mouse

// Fire when left click, and wait for cooldown.
#Fire sequence 
	IsMouseButtonPressed(0)
	Fire
	Wait(0.2)

// Aim at the mouse cursor.
#Aim sequence
	SetTarget_Mouse
	AimAt_Target

// Die when there is nore more HP.
#Die sequence 
	Eval "health < 0.1"
	Explode
