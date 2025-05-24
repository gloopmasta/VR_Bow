#Root sequence
	#SteeringTutorial

#SteeringTutorial
	WaitUntilSteerMessage
	FadeIn steerInstruction
	StartSlowTime
	Wait 3.0
	StopSlowTime
	FadeOut steerInstruction

#JumpingTutorial
	WaitUntilJumpMessage
	FadeIn jumpInstruction
	StartSlowTime
	race
		WaitUntilJump
		Wait 3.0
	StopSlowTime
	FadeOut jumpInstruction

#ShootingTutorial
	WaitUntilShootMessage
	FadeIn shootInstruction
	WaiWaitUntilBowVertical
	FadeOut shootInstruction