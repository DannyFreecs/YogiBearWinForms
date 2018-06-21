using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace YogiBear.Model
{
    //Vadőrök típusa
    public class Ranger
    {
        private Point position; //Játékbeli pozíció
        private bool direction; //Vertikális/Horizontális irányban járőrözik-e
        private Int32 velocity; //sebességvektor iránya

        //Properties
        public Point Position { get { return position; } }
        public bool Direction { get { return direction; } }
        public Int32 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        //Konstruktor
        public Ranger(Int32 row, Int32 col, bool dir)
        {
            position = new Point(row, col);
            direction = dir;
            velocity = 1;
        }

        //Public Setters
        public void SetXPos(Int32 val) { position.X = val; }
        public void SetYPos(Int32 val) { position.Y = val; }
    }
}
