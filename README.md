# RoboCup-Agent-in-CSharp
RoboCup 2D Soccer Simulation Agent (the soccer player's "brain") in C# (C Sharp). Made with Visual Studio Express 2013.


This proyect attempts to become a RoboCup Soccer Agent (the "brains" of
the soccer player). So far it starts a connection with the server,
starts a player, interprets the whole received string (only the "see"
string), determines the position of the player with a simple method
based on the closest flag and knowledge of its position, and with that
information either moves to a desired position, waits for an amount of
time determined by the user, and asks for another position, or closes in
on the ball and kicks it to the goal (assuming they were both being
seen).

In our case it presented a severe sincronization issue that needs fixing
(the messages received in a determined moment are from a while back in
the past). We tried discarding the "see" strings that happened in a time
0 and a few other things to salvage this issue.

This was a proyect we started as a college assignment to practice UDP
connections. We're putting it up so that if anyone wants to work on
RoboCup in this language, they can use our past experience. We currently
do not plan on continuing work on it.

If you have any questions, hit us up!
