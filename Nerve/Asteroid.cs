using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nerve
{
    class Asteroid : GameObject
    {
        AsteroidType type;
        Health resource;

        public Asteroid(AsteroidType type_) :
            base()
        {
            this.type = type_;
        }

        public Asteroid(GameObject obj, AsteroidType type_) :
            base(obj)
        {
            this.type = type_;
        }

        public Asteroid(Point pos_, int angle_, double speed_, double angularSpeed_, Health health_, AsteroidType type_) :
            base(pos_, angle_, speed_, angularSpeed_, health_)
        {
            this.type = type_;
        }

        public Asteroid(int id, Point pos_, int angle_, double speed_, double angularSpeed_, Health health_, AsteroidType type_) :
            base(id, pos_, angle_, speed_, angularSpeed_, health_)
        {
            this.type = type_;
        }

        public AsteroidType Type
        {
            get
            {
                return type;
            }
        }
    }

    enum AsteroidType
    { 
        big,
        medium,
        small
    }
}
