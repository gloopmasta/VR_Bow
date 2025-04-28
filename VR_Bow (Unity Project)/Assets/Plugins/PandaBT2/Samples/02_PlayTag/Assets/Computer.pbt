//Play Tag, check for collision and Talk at the same time.
#Root parallel
	repeat mute #PlayTag
	repeat mute #CheckCollision
	repeat mute #Talk

//Tag when we collide with the player.
//Leave 1 sec of cooldown between each "Tag".
#CheckCollision	sequence
	IsColliding_Player
	Tag
	Wait 1.0

//Say something when changing.
#Talk fallback
	#Talk_WhenIt
	#Talk_WhenNotIt

#Talk_WhenIt while IsIt	sequence
	Say "!"
	Wait 0.5
	Say "I'm it."
	Wait 1.0
	Say "I gonna get you!"
	repeat sequence
		random
			sequence Say "You can run..." Wait 2.0 Say "... but you can't hide!"
			Say "Stop moving!"
			Say "I'm getting tired now."
			Say "Oh! Come on!"
			Say ""
		Wait 2.0

#Talk_WhenNotIt	while not IsIt sequence
	Say "Tag!"
	Wait 1.0
	Say "You're it!"
	Wait 1.0
	repeat sequence
		random
			Say "Come and get me!"
			Say "That's all you can do?"
			Say "Come on!"
			Say "Ah Ah Ah!"
			Say ""
		Wait 2.0