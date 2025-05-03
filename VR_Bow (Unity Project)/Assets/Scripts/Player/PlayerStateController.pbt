#Root sequence
	#Driving
	#SwitchTime //go back to driving when switchtime fails
	#Shooting
		
		

#Driving sequence
	WaitUntilSwitchtime //wait until something happens that lets you enter switchTime

#SwitchTime parallel //Wait 4 seconds to turn bow, otherwise fail and go back to Driving. If you turn bow on time -> go to shooting
	sequence
		Wait 4.0
		Fail
	WaitUntilBowVertical

#Shooting sequence
	SetState PlayerState.Shooting //enable shooting script
	SlowTime 4.0 //free SlowTime

	//if you switched in the air -> back to driving
	//if you hit the ground while still holding like bow
	race
		WaitUntilBowHorizontal
		sequence //after you hit the ground -> display too late message. This sequence can never return true, it just waits to tilt bow
			WaitUntilGrounded
			#SwitchHappenedTooLate

		#SwitchHappenedTooLate

	SetState PlayerState.Driving //reenable driving script

#SwitchHappenedTooLate race
	MoveForwardSlowly
	DisplaySwitchMessage


