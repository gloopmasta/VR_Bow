#Root
	// Blink the text by turning it
	// on and off 4 times.
	repeat(4)
		sequence
			EnableText(true)
			Wait(0.2)
			EnableText(false)
			Wait(0.2)
