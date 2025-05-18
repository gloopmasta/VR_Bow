#Root sequence
	#ShootMode
	#DriveMode

#ShootMode sequence
	WaitUntilBowVertical
	SetState PlayerState.Shooting
	StartSlowTime
	Wait 2.0
	StopSlowTime
	WaitUntilBowHorizontal

#DriveMode sequence
	WaitUntilBowHorizontal
	SetState PlayerState.Driving