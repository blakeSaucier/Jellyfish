# Jellyfish

To run:
- Install dotnet 5.0 sdk 
- `dotnet fsi Jellyfish.fsx`

I implemened this as `fsx` script rather than build a console application. I find the fsx scripts easier to work with and much more lightweight.

You'll find the input inside `jellyfish.txt`. The script outputs to console.

My general strategy was to start with the domain and model the types involved. Once I had those I went ahead and created the use cases. To highlight, I have an `updateGame` function which takes a `Game` and a `Command` and then computes the next state of the world. 

The remainder of the code is focused on reading the input text file, constructing the game state, running all the commands, and printing some output.

Note: Due to time constraints and commitments, I wasn't able to add a feature where a Jellyfish 'scent' will prevent another jellyfish from falling off the edge of grid.
