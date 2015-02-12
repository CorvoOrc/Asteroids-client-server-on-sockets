using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brain
{
    class SpaceShip : GameObject
    {
        Gun gun;

        public SpaceShip(Gun gun_) :
            base()
        {
            this.gun = gun_;
        }

        public SpaceShip(GameObject obj, Gun gun_) :
            base(obj)
        {
            this.gun = gun_;
        }

        public SpaceShip(Point pos_, int angle_, double speed_, double angularSpeed_, Health health_, Gun gun_) :
            base(pos_, angle_, speed_, angularSpeed_, health_)
        {
            this.gun = gun_;
        }

        public SpaceShip(int id, Point pos_, int angle_, double speed_, double angularSpeed_, Health health_, Gun gun_) :
            base(id, pos_, angle_, speed_, angularSpeed_, health_)
        {
            this.gun = gun_;
        }

        public Gun Gun
        {
            get
            {
                return gun;
            }
        }
    }
}
