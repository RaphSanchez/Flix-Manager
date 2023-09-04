

Enable the 'dotnet watch' command using the Package Manager Console to rebuild the 
project	when changes in source code are detected.
	
- donet watch command
https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-watch#name
	
- Episode "6. Example Project Explained" of Udemy course Blazor WebAssembly Full
Stack Bootcamp with .Net 5 by Patrick God
https://www.udemy.com/course/blazor-webassembly/learn/lecture/26246060#content 

- YouTuve video "How to Kill/Stop the 'dotnet' Process | Blazor Shorts" by
Patrick God
https://youtu.be/RfQutsKrNl4?si=z9dYePsITjH7qr_e

~ Go to the Package Manager Console and ensure that the default project is the
	Application/Server-Api project.

~ use the 'dir' command to discover the directory tree for the complete solution.

~ use the 'cd .\BlazorMovies' command to move to the root directory of the solution.

~ use the 'dir' command to discover the directory tree for the current location.

~ use the 'cd .\Server' command to move to the Application/Server-Api directory

~ Once in the desired server directory, use the 'dotnet watch run' command 

~ you can use the 'cd ..\' command to go up (or back) one level in the directory

~ You can kill the process by:

	a) Clicking on the red square (stop button) on the top right corner of the
		Package Manager Console.
	
	b) Going to the package manager console or opening a terminal in the project and:

		- List all the current dotnet processes
			get-process -name "dotnet"

		- Stop the processes with
			get-process -name "dotnet" | stop-process

