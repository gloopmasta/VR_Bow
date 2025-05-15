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
		SetState PlayerState.Shooting
		RaiseSlowTime
		Wait @playerSlowTime
		
#Driving
	while // while the bow is in driving mode and the switch time is not triggered
		sequence 
			IsDrivingMode
			not IsSwitchTime
		// Do Driving Mode
		SetState PlayerState.Shooting
		WaitUntilSwitchtime
		ActivateSwitchTime


#Switchtime while IsSwitchTime // The player has 4 sec to turn the bow vertically and enter shooting mode, otherwize back to driving
    RaiseSlowTime
	race
		sequence
			WaitBowVertical
			SetState PlayerState.Shooting
		sequence
			Wait 4.0
			SetState PlayerState.Driving

	StopSlowTime
	DisableSwitchTime