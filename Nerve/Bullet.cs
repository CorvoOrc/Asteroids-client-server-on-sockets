using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nerve
{
    class Bullet : GameObject
    {
        double damage;

        public Bullet(double damage_) :
            base()
        {
            this.damage = damage_;
        }

        public Bullet(GameObject obj, double damage_) :
            base(obj)
        {
            this.damage = damage_;
        }

        public Bullet(Point pos_, int angle_, double speed_, double angularSpeed_, Health health_, double damage_) :
            base(pos_, angle_, speed_, angularSpeed_, health_)
        {
            this.damage = damage_;
        }

        public Bullet(int id, Point pos_, int angle_, double speed_, double angularSpeed_, Health health_, double damage_) :
            base(id, pos_, angle_, speed_, angularSpeed_, health_)
        {
            this.damage = damage_;
        }

        public double Damage
        {
            get
            {
                return damage;
            }
        }
    }
}
