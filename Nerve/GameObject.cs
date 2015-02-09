using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nerve
{
    abstract class GameObject
    {
        static int lastId = 0;

        int id;
        Point pos;
        int angle;
        double speed;
        double angularSpeed;
        Health health;

        public GameObject(int id_, Point pos_, int angle_, double speed_, double angularSpeed_, Health health_)
        {
            this.id = id_;
            this.pos = pos_;
            this.angle = angle_;
            this.speed = speed_;
            this.angularSpeed = angularSpeed_;
            this.health = health_;
        }

        public GameObject(Point pos_, int angle_, double speed_, double angularSpeed_, Health health_)
        {
            this.id = lastId;
            this.pos = pos_;
            this.angle = angle_;
            this.speed = speed_;
            this.angularSpeed = angularSpeed_;
            this.health = health_;

            ++lastId;
        }

        public GameObject(GameObject obj)
        {
            this.id = obj.Id;
            this.pos = obj.Pos;
            this.angle = obj.Angle;
            this.speed = obj.Speed;
            this.angularSpeed = obj.AngularSpeed;
            this.health = obj.Health;

            //++lastId; // psevdo copy
        }

        public GameObject()
        {
            this.id = lastId;
            this.pos = new Point();
            this.angle = 0;
            this.speed = 0.0;
            this.angularSpeed = 0.0;
            this.health = new Health();

            ++lastId;
        }

        public string ToString(String separator)
        {
            String data = this.Id.ToString() + separator + this.Pos.x +
                separator + this.Pos.y + separator + this.Angle +
                separator + this.Speed + separator + this.AngularSpeed;

            return data;
        }

        public string ToString(String separator, bool placeOnly)
        {
            if (!placeOnly)
            {
                return ToString(separator);
            }

            String data = this.Id.ToString() + separator + this.Pos.x +
                separator + this.Pos.y + separator + this.Angle;

            return data;
        }

        public int Id
        {
            get
            {
                return id;
            }
        }

        public Point Pos
        {
            get
            {
                return pos;
            }
            set
            {
                pos = value;
            }
        }

        public int Angle
        {
            get
            {
                return angle;
            }
            set
            {
                angle = value;
            }
        }

        public double Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
            }
        }

        public double AngularSpeed
        {
            get
            {
                return angularSpeed;
            }
            set
            {
                angularSpeed = value;
            }
        }

        public Health Health
        {
            get
            {
                return health;
            }
            set
            {
                health = value;
            }
        }
    }
}
