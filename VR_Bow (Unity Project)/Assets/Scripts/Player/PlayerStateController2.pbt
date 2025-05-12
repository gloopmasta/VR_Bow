#Root
	#HandleBowMode

#HandleBowMode
	fallback
		#Shooting
		#Driving
		#Switchtime
		

#Shooting // while the bow is in shooting mode and the switch time is not triggered
	while 
		sequence 
			IsShootingMode
			not IsSwitchTime
		// Do Shooting mode
		
#Driving
	while // while the bow is in driving mode and the switch time is not triggered
		sequence 
			IsDrivingMode
			not IsSwitchTime
		// Do Driving Mode

#Switchtime while IsSwitchTime // The player has 4 sec to turn the bow vertically and enter shooting mode, otherwize back to driving
	race
		sequence
			WaitBowVertical
			SetState PlayerState.Shooting
		sequence
			Wait 4.0
			SetState PlayerState.Driving