using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nerve
{
    class Health
    {
        double point;
        double maxPoint;
        static double maxPointDefault = 100.0; 

        public Health(double point_, double maxPoint_)
        {
            this.point = point_;
            this.maxPoint = maxPoint_;
        }

        public Health(double point_)
        {
            this.point = point_;
            this.maxPoint = point_;
        }

        public Health()
        {
            this.point = maxPointDefault;
            this.maxPoint = maxPointDefault;
        }

        public bool isDead()
        {
            if (this.point == 0)
                return true;

            return false;
        }

        public double Point
        {
            get
            {
                return point;
            }
            set
            {
                point = value;
            }
        }

        public double MaxPoint
        {
            get
            {
                return point;
            }
        }
    }
}
