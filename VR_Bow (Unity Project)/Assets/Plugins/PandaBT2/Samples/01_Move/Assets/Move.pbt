// Move around and rotate at the same time
#Root parallel
	#Move
	#Rotate

// Move around
#Move sequence
	MoveTo @bottomLeft
	MoveTo @bottomRight
	MoveTo @topRight
	MoveTo @topLeft

// Rotate
#Rotate	random
	Rotate -45.0 0.5
	Rotate  45.0 1.0
	Rotate -90.0 2.0
	Rotate -90.0 3.0
