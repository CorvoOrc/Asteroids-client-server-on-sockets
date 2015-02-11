# Asteroids-client-server-on-sockets

Asteroids net game. Server (his name is Brain) written on C#, client (I call his Nerve) - ActionScript3, client for testing in console - C#.

Client for testing is ooriented on check work event function.
Event functions:
- newspace (appearence new space ship)
- sendtoall (send simebody message to all nerve)
- poschange (user push on control buttom and move space ship)
- angchange (user push on control buttom and turn space ship)
- shot (user shot, appearence new bullet)
- hit (user hit in somebody object: another space ship, asteroid, border)
- collision (user`s space ship collision with asteroid (in future with another space ship))
- close (user terminated game and close session, need free space ship object and own bullets, also close sockets and streams)

Simplify game:
- thrust is absent (spaceship will stop when gamer pop control button to controll of spaceship)
- asteroids not crushes on smaller part
- absent collision between bullet
- absent collision between spaceShip

Simplify Brain:
- absent data base (if Brain was terminated, all data lost)
- don`t use encrypt data

Client on ActionScript3 have problem with Worker (analog Thread in C#, served for update data), sometimes he incorrectly closing.
