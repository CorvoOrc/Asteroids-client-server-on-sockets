# Asteroids-client-server-on-sockets

Asteroids net game. Server (his name is Brain) written on C#, client (I call his Nerve) - ActionScript3, client for testing in console - C#.

Client for testing is oriented on check work event function.
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

Move of ship computes following:
- x += cos(angle) * c; ('+' forward, '-' - backward)
- y -= sin(angle) * c; ('-' forward, '+' - backward)
  
, where c - coefficient (hardcoded and equal 1.0)

Move of bullet equal move of ship.

Move of ship computes following:
- x += cos(angle) * c1;
- y -= sin(angle) * c1;
- 
, where c1 - coefficient (hardcoded and equal 1.3)

Move of asteroid computes following:
- x += cos(angle) * c; ('+' forward, '-' - backward)
- y -= sin(angle) * c; ('-' forward, '+' - backward)
- angle += min + (max - min) * Math.random()

, where c - coefficient (hardcoded and equal 1.0)

Nerve notification Brain about bullets and asteroids in when they hit in object:
- for bullet: hit in ship, asteroid, border
- for asteroid: hit in ship

Collision computes use function hitTestObject():

if(Object1.hitTestObject( Object2 ) {

    trace("Is Hit");
  
    // Send Brian objects id;
  
  }

All collision check in ENTER_FRAME event:

this.addEventListener(Event.ENTER_FRAME, handleCollision)

function handleCollision( e:Event ):void {

  // ...
  
  foreach(var bullet in ownBullets) {
  
    //move bullet
    
  }

  foreach(var bullet in alienBullets) {
    //move bullet
  }
  
  foreach(var asteroid in asteroids) {
  
    //move asteroid
    
  }
  
  // ...
  
}

Client on ActionScript3 have problem with Worker (analog Thread in C#, served for update data), sometimes he incorrectly closing.
