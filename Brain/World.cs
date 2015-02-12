using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brain
{
    class World
    {
        Dictionary<int, SpaceShip> ships = null;
        Dictionary<int, Bullet> bullets = null;
        Dictionary<int, Asteroid> asteroids = null;

        Dictionary<int, int> bulletOwner = null;

        public const int borderId = -1;
        public const Byte objectDestroyed = 1;
        public const Byte objectSurvived = 0;

        public World()
        {
            this.ships = new Dictionary<int, SpaceShip>();
            this.bullets = new Dictionary<int, Bullet>();
            this.asteroids = new Dictionary<int, Asteroid>();
            this.bulletOwner = new Dictionary<int, int>();

            int countAsteroids = 3;
            OrganizeArmageddon(countAsteroids);
        }

        public Dictionary<int, SpaceShip> Ships
        {
            get
            {
                return ships;
            }
        }

        public Dictionary<int, Bullet> Bullets
        {
            get
            {
                return bullets;
            }
        }

        public Dictionary<int, Asteroid> Asteroids
        {
            get
            {
                return asteroids;
            }
        }

        public void OrganizeArmageddon(Int32 max)
        {
            int turnCircle = 360;
            double angularSpeedReduction = 3.0;

            for (int i = 0; i < max; ++i)
            {
                Random random = new Random();
                double x = random.NextDouble() * 100.0;
                double y = random.NextDouble() * 100.0;
                int angle = random.Next(turnCircle);
                double speed = random.NextDouble() * 10;
                double angularSpeed = random.NextDouble() * 10.0 - angularSpeedReduction;
                AsteroidType type = AsteroidType.big;

                Asteroid asteroid = new Asteroid(new Point(x, y), angle, speed, angularSpeed, new Health(), type);
                asteroids.Add(asteroid.Id, asteroid);
            }
        }

        public bool IsActuallyExist(Int32 bulletId, Int32 burnObjectId)
        {
            if (IsBullet(bulletId) && (IsBorder(burnObjectId) || IsShip(burnObjectId) || IsAsteroid(burnObjectId)))
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

        public bool IsBullet(Int32 objectId)
        {
            if (Bullets.ContainsKey(objectId))
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

        public bool IsBulletOwner(Int32 objectId, Int32 ownerId)
        {
            if (bulletOwner.ContainsKey(objectId) && bulletOwner[objectId] == ownerId)
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
            if(IsShip(shipId))
            {
                Ships.Remove(shipId);
            }
        }

        public void UseBullet(Int32 bulletId)
        {
            if (IsBullet(bulletId))
            {
                Bullets.Remove(bulletId);
                bulletOwner.Remove(bulletId);
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

        public void HarmBullet(Int32 bulletId, Double damage)
        {
            if (IsBullet(bulletId))
            {
                Bullets[bulletId].Health.Point -= damage;
            }
        }

        public void Shot(Int32 bulletId, Int32 ownerId, Bullet bullet)
        {
            Bullets.Add(bulletId, bullet);
            bulletOwner.Add(bulletId, ownerId);
        }

        public void AddShip(Int32 id, SpaceShip ship)
        {
            Ships.Add(id, ship);
        }
    }
}
