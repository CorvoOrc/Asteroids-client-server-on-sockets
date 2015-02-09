using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nerve
{
    class World
    {
        Dictionary<int, SpaceShip> spaceShips = null;
        Dictionary<int, Asteroid> asteroids = null;
        Dictionary<int, Bullet> ownBullets = null;
        Dictionary<int, Bullet> alienBullets = null;

        public int myId;
        public int borderId = -1;
        public bool gameOver = false;

        public World()
        {
            spaceShips = new Dictionary<int, SpaceShip>();
            asteroids = new Dictionary<int, Asteroid>();
            ownBullets = new Dictionary<int, Bullet>();
            alienBullets = new Dictionary<int, Bullet>();
        }

        public Dictionary<int, SpaceShip> Ships
        {
            get
            {
                return spaceShips;
            }
        }

        public Dictionary<int, Asteroid> Asteroids
        {
            get
            {
                return asteroids;
            }
        }

        public Dictionary<int, Bullet> OwnBullets
        {
            get
            {
                return ownBullets;
            }
        }

        public Dictionary<int, Bullet> AlienBullets
        {
            get
            {
                return alienBullets;
            }
        }
    }
}
