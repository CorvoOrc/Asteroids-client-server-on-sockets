# Asteroids-client-server-on-sockets

Asteroids net game. Server (his name is Brain) written on C#, client (I call his Nerve) - ActionScript3, client for testing - C#.

Simplify game:
- thrust is absent (spaceship will stop when gamer pop button to call of spaceship manage)
- asteroids not destroyed on smaller part
- absent collision between bullet
- absent collision between spaceShip

Simplify Brain:
- absent data base (if Brain was terminated, all data lost)
- don`t use encrypt data

Client on ActionScript3 in testing stage (problem with Worker (analog Thread in C#), sometimes he incorrectly closing).
