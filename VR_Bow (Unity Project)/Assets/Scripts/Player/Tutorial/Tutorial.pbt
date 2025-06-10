#Root race
	WaitUntilTutorialRestart
	#TutorialSequence

#TutorialSequence sequence
	#SteeringTutorial
	#JumpingTutorial
	#ShootingTutorial
	WaitUntilTutorialRestart //wait to prevent it from looping

#SteeringTutorial sequence
	WaitUntilSteerMessage
	FadeIn @steerInstruction
	StartSlowTime
	Wait 3.0
	StopSlowTime
	FadeOut @steerInstruction

#JumpingTutorial sequence
	WaitUntilJumpMessage
	FadeIn @jumpInstruction
	StartSlowTime
	race
		WaitUntilJump
		Wait 3.0
	StopSlowTime
	FadeOut @jumpInstruction

#ShootingTutorial sequence
	WaitUntilShootMessage
	FadeIn @shootInstruction
	WaitUntilBowVertical
	FadeOut @shootInstruction