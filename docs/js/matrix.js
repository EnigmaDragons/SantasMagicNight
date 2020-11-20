var c = document.getElementById("c");

const targetHeight = 380;

const resize = () => {
	c.width = window.innerWidth;
	c.height = targetHeight;
}

if (c) {
	var ctx = c.getContext("2d");

	resize();

	var characters = [0, 1]

	var font_size = 10;
	var columns = c.width/font_size; 
	var drops = [];
	for(var x = 0; x < columns; x++)
		drops[x] = 1; 

	function draw()
	{
		ctx.fillStyle = "rgba(0, 0, 0, 0.05)";
		ctx.fillRect(0, 0, c.width, c.height);
		ctx.fillStyle = "#0980CC"; 
		ctx.font = font_size + "px arial";

		for(var i = 0; i < drops.length; i++)
		{
			var text = characters[Math.floor(Math.random()*characters.length)];
			var x = i*font_size;
			var y = drops[i]*font_size;

			ctx.fillText(text, x, y);
			
			//sending the drop back to the top randomly after it has crossed the screen
			//adding a randomness to the reset to make the drops scattered on the Y axis
			if(y > c.height && Math.random() > 0.975)
				drops[i] = 0;
			
			//incrementing Y coordinate
			drops[i]++;
		}
	}

	setInterval(draw, 100);
}
