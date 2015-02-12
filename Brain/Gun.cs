using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brain
{
    class Gun : GameObject // exactly need inheritance?
    {
        Health prepareTime;

        public Gun(Health prepareTime_) :
            base()
        {
            this.prepareTime = prepareTime_;
        }

        public Gun(GameObject obj, Health prepareTime_) :
            base(obj)
        {
            this.prepareTime = prepareTime_;
        }

        public Gun(Point pos_, int angle_, double speed_, double angularSpeed_, Health health_, Health prepareTime_) :
            base(pos_, angle_, speed_, angularSpeed_, health_)
        {
            this.prepareTime = prepareTime_;
        }

        public Health PrepareTime
        {
            get
            {
                return prepareTime;
            }
        }

        public void Tick()
        {
            if(prepareTime.Point >= 1)
                --prepareTime.Point;
        }

        public bool Ready()
        {
            return prepareTime.isDead();
        }

        public Bullet Fire(Bullet cartridge) // Can I do that?
        {
            if (Ready())
            {
                Resurrection();
                return cartridge;
            }

            throw new Exception("Gun isn`t ready");
        }

        private void Resurrection()
        {
            prepareTime.Point = PrepareTime.MaxPoint;
        }
    }
}
