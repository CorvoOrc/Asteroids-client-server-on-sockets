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

        int myId;
        public const int borderId = -1;
        public bool gameOver = false;

        public World()
        {
            spaceShips = new Dictionary<int, SpaceShip>();
            asteroids = new Dictionary<int, Asteroid>();
            ownBullets = new Dictionary<int, Bullet>();
            alienBullets = new Dictionary<int, Bullet>();
        }

        public int MyId
        {
            get
            {
                return myId;
            }
            set
            {
                myId = value;
            }
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

        public void OrganizeArmageddon(Int32 id, Asteroid asteroid)
        {
            Asteroids.Add(id, asteroid);
        }

        public bool IsMyShip(Int32 id)
        {
            if (id == MyId)
            {
                return true;
            }

            return false;
        }

        public bool IsActuallyExist(Int32 bulletId, Int32 burnObjectId)
        {
            if (IsOwnBullet(bulletId) && (IsBorder(burnObjectId) || IsShip(burnObjectId) || IsAsteroid(burnObjectId)))
            {
                return true;
            }

            return false;
        }

        public bool IsShip(Int32 objectId)
        {
            if (Ships.ContainsKey(objectId))
            {
                return true;
            }

            return false;
        }

        public bool IsOwnBullet(Int32 objectId)
        {
            if (OwnBullets.ContainsKey(objectId))
            {
                return true;
            }

            return false;
        }

        public bool IsAlienBullet(Int32 objectId)
        {
            if (AlienBullets.ContainsKey(objectId))
            {
                return true;
            }

            return false;
        }

        public bool IsAsteroid(Int32 objectId)
        {
            if (Asteroids.ContainsKey(objectId))
            {
                return true;
            }

            return false;
        }

        public bool IsBorder(Int32 objectId)
        {
            if (objectId == borderId)
            {
                return true;
            }

            return false;
        }

        public void CrushAsteroid(Int32 asteroidId, Int32 countBurn)
        {
            if (IsAsteroid(asteroidId))
            {
                Asteroid oldAsteroid = Asteroids[asteroidId];

                switch (Asteroids[asteroidId].Type)
                {
                    case AsteroidType.big:
                        {
                            for (int i = 0; i < countBurn; ++i)
                            {
                                Asteroid newAsteroid = new Asteroid(oldAsteroid.Pos, oldAsteroid.Angle, oldAsteroid.Speed,
                                oldAsteroid.AngularSpeed, new Health(), AsteroidType.medium);
                                Asteroids.Add(newAsteroid.Id, newAsteroid);
                            }

                            break;
                        }
                    case AsteroidType.medium:
                        {
                            for (int i = 0; i < countBurn; ++i)
                            {
                                Asteroid newAsteroid = new Asteroid(oldAsteroid.Pos, oldAsteroid.Angle, oldAsteroid.Speed,
                                oldAsteroid.AngularSpeed, new Health(), AsteroidType.small);
                                Asteroids.Add(newAsteroid.Id, newAsteroid);
                            }

                            break;
                        }
                }

                Asteroids.Remove(asteroidId);
            }
        }

        public void ExplodeShip(Int32 shipId)
        {
            if (IsShip(shipId))
            {
                Ships.Remove(shipId);
            }
        }

        public void UseBullet(Int32 bulletId)
        {
            if (IsOwnBullet(bulletId))
            {
                OwnBullets.Remove(bulletId);
            }
            else if (IsAlienBullet(bulletId))
            {
                AlienBullets.Remove(bulletId);
            }
        }

        public void HarmShip(Int32 shipId, Double damage)
        {
            if (IsShip(shipId))
            {
                Ships[shipId].Health.Point -= damage;
            }
        }

        public void HarmAsteroid(Int32 asteroidId, Double damage)
        {
            if (IsAsteroid(asteroidId))
            {
                Asteroids[asteroidId].Health.Point -= damage;
            }
        }

        public void HarmOwnBullet(Int32 bulletId, Double damage)
        {
            if (IsOwnBullet(bulletId))
            {
                OwnBullets[bulletId].Health.Point -= damage;
            }
        }

        public void HarmAlienBullet(Int32 bulletId, Double damage)
        {
            if (IsAlienBullet(bulletId))
            {
                AlienBullets[bulletId].Health.Point -= damage;
            }
        }

        public void ShotOwn(Int32 id, Bullet bullet)
        {
            OwnBullets.Add(id, bullet);
        }

        public void ShotAlien(Int32 id, Bullet bullet)
        {
            AlienBullets.Add(id, bullet);
        }

        public void AddShip(Int32 id, SpaceShip ship)
        {
            Ships.Add(id, ship);
        }
    }
}
