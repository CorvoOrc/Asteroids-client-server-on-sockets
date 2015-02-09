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

        public const int borderId = -1;
        public const Byte objectDestroyed = 1;
        public const Byte objectSurvived = 0;

        public World()
        {
            this.ships = new Dictionary<int, SpaceShip>();
            this.bullets = new Dictionary<int, Bullet>();
            this.asteroids = new Dictionary<int, Asteroid>();

            int countAsteroids = 3;
            GenerateAsteroids(countAsteroids);
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

        public void GenerateAsteroids(Int32 max)
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
    }
}
