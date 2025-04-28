//This Behaviour controls the game.
#Root parallel
	#DisplayGameStart
	race
		//The game finishes either on Game Over or
		//when the level is completed.

		//It's Game over when the player is dead.
		sequence
			retry IsPlayerDead
			#DisplayGameOver 
			ReloadLevel

		//The level is completed when all enemies are dead.
		sequence
			retry IsLevelCompleted
			#DisplaLevelCompleted
			ReloadLevel


#DisplayGameStart sequence
	//Indicates that the game is starting
	//by blinking a text.
	DebugLog "GAME START"
	repeat 3 sequence
		Display "GAME START"
		Wait 0.5
		Display ""
		Wait 0.5

#DisplayGameOver sequence
	DebugLog "GAME OVER"
	Wait 2.0
	Display "GAME OVER"
	Wait 3.0

#DisplaLevelCompleted sequence
	Display "LEVEL COMPLETED"
	DebugLog "LEVEL COMPLETED"
	Wait 3.0
