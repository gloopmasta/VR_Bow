#Root sequence
	retry 
		sequence
			SwitchTimeActivated
			SwitchTime
	#Shooting2
		
		

#Driving race
	WaitUntilSwitchtime //wait until something happens that lets you enter switchTime
	//if //only able to jump shoot if you're on the ground
		//IsGrounded
		//#JumpShooting


#Shooting2 sequence
	SetState PlayerState.Shooting //enable shooting script
	StartSlowTime
	race
		WaitUntilBowHorizontal
		sequence //after you hit the ground -> display too late message. This sequence can never return true, it just waits to tilt bow
			WaitUntilGrounded
			DisableArrows
			DisplaySwitchMessage

	SetState PlayerState.Driving //reenable driving script

#AirSlowTimeLogic race
	race
		sequence
			WaitUntilBowHorizontal
			StopSlowTime
		sequence //after you hit the ground -> display too late message. This sequence can never return true, it just waits to tilt bow
			WaitUntilGrounded
			StopSlowTime
			DisableArrows
			DisplaySwitchMessage

#GroundSlowTimeLogic race
	race
		WaitUntilBowHorizontal
		sequence //after you hit the ground -> display too late message. This sequence can never return true, it just waits to tilt bow
			WaitUntilGrounded
			DisableArrows
			DisplaySwitchMessage


#Shooting sequence
	SetState PlayerState.Shooting //enable shooting script
	SlowTime
	race
		WaitUntilBowHorizontal
		sequence //after you hit the ground -> display too late message. This sequence can never return true, it just waits to tilt bow
			WaitUntilGrounded
			DisableArrows
			DisplaySwitchMessage

	SetState PlayerState.Driving //reenable driving script


#JumpShooting repeat sequence //if you jump without switchTime -> enable shooting script but no extras
	WaitUntilJump
	SetState PlayerState.Shooting
	WaitUntilGrounded
	SetState PlayerState.Driving

	//#SwitchTime race //Wait 4 seconds to turn bow, otherwise fail and go back to Driving. If you turn bow on time -> go to shooting
//	sequence
//		SlowTime 4.0
//		Fail
//	race
//		sequence
//			Wait 0.8
//			Fail
//		WaitUntilBowVertical

//#Shooting sequence
//	SetState PlayerState.Shooting //enable shooting script
//	SlowTime 4.0 //free SlowTime
//
//	//if you switched in the air -> back to driving
//	//if you hit the ground while still holding like bow -> display message
//	race
//		WaitUntilBowHorizontal
//		sequence //after you hit the ground -> display too late message. This sequence can never return true, it just waits to tilt bow
//			WaitUntilGrounded
//			#SwitchHappenedTooLate
//
//	SetState PlayerState.Driving //reenable driving script


//#DriveAndSwitch sequence //go back to driving when switchtime fails
//	#Driving
//	#SwitchTime


//#SwitchHappenedTooLate race 
//	MoveForwardSlowly
//	DisplaySwitchMessage