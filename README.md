# Moanna

This is probably about my 4th or 5th game I started working on. 
Here i aimed at learning how to make procedural maps and randomly 
place items such as trees. 

I also put work in to creating AI with prototype actions that 
would make things easier to build on the AI for when i add more
to the game such as food, drink and i was going to aim for sailing
at some point. This game is absolutely still on my list to work on
so it might be updated at some point. 

How To Play:
Movement: point and click. (If you are having control issues hit ESC
This will reset you to movement mode)

Note!: the action button only has one action that it will use right now
so no list appears. It just sets to cut task which is used to cut down 
trees.


What is programmed:
-Procedural Generation:
I have made procedurally generated maps that randomise each time.
It will firstly chose points to place islands then create a noise map
for the island and finally edit it with a falloff map to make an island.
Then over that it will run poisson disc sampling to place trees on the 
grass areas. 

-Procedural Tiles:
Upon the creation of the map and the player render distance being decided
the visual tiles will be called to be placed. For each tile it will check
neighbours and add to the sprite name string to create the name of the needed 
tile. So when it builds the visuals they will connect together seamlessly.
I had to create these tiles myself for this operation. So apologies if
they don't look that good.

-Player render distance:
As the player moves about the world it will create the needed tiles from
the map info. it will then turn off the tiles that fall out of the render distance.
This way only the needed tiles are loaded for the game session to save on
performance.

-Threaded Pathfinding:
I have been adding AI to this game so i added threading to the pathfinding
as there will be multiple agents asking for a path. I have run multiple tests on
the path finding and the speed is capable of handling more AI agents.

-AI:
Right now the agents i have created will wait for the player to create a task 
or construction jobs. When the player creates a task it is placed in a queue
that the agents will read from every few seconds. The agent will take the task 
and carry out the list of actions if they are able or place it back in the 
queue. 

When the player places a job it has its own list of jobs and a list of agents
to work on it. As each agent picks up the job they are added to the list of agents
and assigned a task to carry out. If there are no more tasks they are removed from 
the job and once all tasks are complete the job is complete and removed. 

Normally the final step of the job is constructing the building.

The AI works based on a switch system so they can be set in to different modes 
based on this system. They change from idle, moving, waitingForPath and Execute
which is what is used to carry out tasks and jobs.

-Building/TilemapUpdating:
So this game will be about making a successful village to start with.
To achieve this i have created a system with prototype objects so
more buildings can be added easily. All of these will update the TileMap
on completion to make sure no one walks through walls. 

