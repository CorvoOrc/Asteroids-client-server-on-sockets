using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brain
{
    class Point
    {
        public double x;
        public double y;

        public Point(double x_, double y_)
        {
            this.x = x_;
            this.y = y_;
        }

        public Point()
        {
            this.x = 0.0;
            this.y = 0.0;
        }
    }
}
