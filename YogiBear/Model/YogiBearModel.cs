using System;
using YogiBear.Persistence;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace YogiBear.Model
{
    //Maci Laci játék típusa
    public class YogiBearModel
    {
        #region Fields

        private List<List<Int32>> map; //pálya
        private Point playerpos; //maci laci pozíciója
        private List<Ranger> rangers; //vadőrök pozíciói
        private Int32 baskets; // piknik kosarak száma
        private System.Windows.Forms.Timer patrolling; //vadőrök járőrözésének ideje

        #endregion

        #region Properties
        
        //Getters
        public List<List<Int32>> Map { get { return map; } set { this.map = value; } }
        public Point PlayerPos { get { return playerpos; } }
        public List<Ranger> Rangers { get { return rangers; } }
        public Int32 Baskets { get { return baskets; } }
        public System.Windows.Forms.Timer Patrolling { get { return patrolling; } }

        #endregion

        #region Constructor

        //A modell példányosítása
        public YogiBearModel()
        {
            map = new List<List<Int32>>();
            playerpos = new Point(0, 0);
            rangers = new List<Ranger>();
            baskets = 0;
            patrolling = new System.Windows.Forms.Timer();
            patrolling.Interval = 500;
            patrolling.Tick += Timer_Elapsed;
        }

        #endregion

        #region Public Methods

        //Új játék kezdése "filename" fájlban tárolt adatokból
        public void NewGame()
        {
            YogiBearData data = new YogiBearData();
            map = data.LoadFromFile();

            if(map == null)
            {
                map = data.LoadFirstLevel(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\Resources\\Map\\level1.map");
            }

            //Vadőrök lista feltöltése, piknikkosarak megszámolása
            for (Int32 i = 0; i < map.Count; i++)
                for (Int32 j = 0; j < map.Count; j++)
                {
                    if (map[i][j] == 4)
                        rangers.Add(new Ranger(i, j, Convert.ToBoolean(rangers.Count % 2)));

                    if (map[i][j] == 2)
                        baskets++;
                }
        }

        //Maci Laci lépésének érvényességének vizsgálata
        //Ha "bent" van a pályán és nem akadály/vadőr
        public bool IsFloor(int row, int col)
        {
            return (row >= 0 && row < map.Count && col >= 0 && col < map.Count && (map[row][col] == 0 || map[row][col] == 2 || map[row][col] == 3));
        }

        //Felfelé lépés 1 mezővel, vizsgáljuk kosarat vettünk-e fel, és ha nincs akkor győzelem esemény
        public void Up()
        {
            if (map[playerpos.X - 1][playerpos.Y] == 2)
            {
                map[playerpos.X - 1][playerpos.Y] = 0;
                baskets--;
                OnGotBasket();
            }

            playerpos.X--;
            if (baskets == 0) OnGameOver(new GameOverEventArgs(true));
        }

        //Balra lépés 1 mezővel, vizsgáljuk kosarat vettünk-e fel, és ha nincs akkor győztünk
        public void Left()
        {
            if (map[playerpos.X][playerpos.Y - 1] == 2)
            {
                map[playerpos.X][playerpos.Y - 1] = 0;
                baskets--;
                OnGotBasket();
            }
            playerpos.Y--;
            if (baskets == 0) OnGameOver(new GameOverEventArgs(true));
        }

        //Lefelé lépés 1 mezővel, vizsgáljuk kosarat vettünk-e fel, és ha nincs akkor győztünk
        public void Down()
        {
            if (map[playerpos.X + 1][playerpos.Y] == 2)
            {
                map[playerpos.X + 1][playerpos.Y] = 0;
                baskets--;
                OnGotBasket();
            }
            playerpos.X++;
            if (baskets == 0) OnGameOver(new GameOverEventArgs(true));
        }

        //Jobbra lépés 1 mezővel, vizsgáljuk kosarat vettünk-e fel, és ha nincs akkor győztünk
        public void Right()
        {
            if (map[playerpos.X][playerpos.Y + 1] == 2)
            {
                map[playerpos.X][playerpos.Y + 1] = 0;
                baskets--;
                OnGotBasket();
            }
            playerpos.Y++;
            if (baskets == 0) OnGameOver(new GameOverEventArgs(true));
        }

        #endregion

        #region Private Methods

        //Vadőrök szempontjából (row, col) érvényes mező-e
        //Ha bent van a pályán, és nem akadály/kosár/Maci Laci
        private bool IsPatrolField(Int32 row, Int32 col)
        {
            return (row >= 0 && row < map.Count && col >= 0 && col < map.Count && map[row][col] == 0);
        }

        //Egy vadőr következő lépése
        private void RangerMove(Ranger ranger)
        {
            //Ha függőlegesen járőrözik
            if (ranger.Direction)
            {
                if (IsPatrolField(ranger.Position.X, ranger.Position.Y + ranger.Velocity))
                {
                    map[ranger.Position.X][ranger.Position.Y] = 0;
                    map[ranger.Position.X][ranger.Position.Y + ranger.Velocity] = 2;
                    ranger.SetYPos(ranger.Position.Y + ranger.Velocity);
                }else //ha érvénytelen mező jönne, akkor megfordul
                {
                    ranger.Velocity *= -1;
                }
            }else //Ha vízszintesen járőrözik
            {
                if(IsPatrolField(ranger.Position.X + ranger.Velocity, ranger.Position.Y))
                {
                    map[ranger.Position.X][ranger.Position.Y] = 0;
                    map[ranger.Position.X + ranger.Velocity][ranger.Position.Y] = 2;
                    ranger.SetXPos(ranger.Position.X + ranger.Velocity);
                }
                else //ha érvénytelen mező jönne, akkor megfordul
                {
                    ranger.Velocity *= -1;
                }
            }
        }

        //Adott vadőr elkapja-e Maci Lacit
        private bool RangerCatch(Ranger ranger)
        {
            for(Int32 i=-1; i<2; i++)
                for(Int32 j=-1; j<2; j++)
                {
                    Int32 row = ranger.Position.X + i;
                    Int32 col = ranger.Position.Y + j;

                    if (playerpos.X == row && playerpos.Y == col)
                        return true;
                }

            return false;
        }

        //Bármelyik vadőr elkapta-e Maci Lacit?
        //Ha igen, akkor Vereség esemény kiváltása
        private void IsCaught()
        {
            for (Int32 i = 0; i < rangers.Count; i++)
                if (RangerCatch(rangers[i]))
                {
                    OnGameOver(new GameOverEventArgs(false));
                    return;
                }
        }

        

        #endregion

        #region Events

        //Kosár felvételének eseménye
        public event EventHandler GotBasket;

        //Játék vége eseménye
        public event EventHandler<GameOverEventArgs> GameOver;

        //Vadőrök járőrözésének az előrehaladásának eseménye
        public event EventHandler<AdvancePatrolEventArgs> AdvancePatrol;

        #endregion

        #region Event Methods

        //Kosár felvételének kiváltása
        protected virtual void OnGotBasket()
        {
            EventHandler handler = GotBasket;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        //Játék vége esemény kiváltása
        protected virtual void OnGameOver(GameOverEventArgs e)
        {
            EventHandler<GameOverEventArgs> handler = GameOver;
            if (handler != null)
                handler(this, e);
        }

        //Járőrözés előrehaladásának eseményének kiváltása
        protected virtual void OnAdvancePatrol(AdvancePatrolEventArgs e)
        {
            EventHandler<AdvancePatrolEventArgs> handler = AdvancePatrol;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        #region Event handlers

        //patrolling Timer Tick eseményének a kezelője
        //Adott időközönként léptetjük az összes vadőrt
        private void Timer_Elapsed(object sender, EventArgs e)
        {
            List<Point> origins = new List<Point>(rangers.Count); //Eredeti pozíciók
            
            for (Int32 i = 0; i < rangers.Count; i++)
            {
                origins.Add(new Point(rangers[i].Position.X, rangers[i].Position.Y));
                RangerMove(rangers[i]);  //mindenkit léptetünk
            }

            //kiváltjuk a járőrözés előrehaladását
            OnAdvancePatrol(new AdvancePatrolEventArgs(rangers, origins));
            IsCaught(); //megvizsgáljuk nem kapta-e valaki el Maci Lacit
        }

        #endregion

    }

    #region EventArgs

    //Játék vége eseményargumentum típusa
    public class GameOverEventArgs : EventArgs
    {
        public Boolean result { get; set; }
        public GameOverEventArgs(Boolean res) { result = res; }
    }

    //Járőrözés előrehaladásának eseményargumentum típusa
    public class AdvancePatrolEventArgs : EventArgs
    {
        public List<Ranger> newpos { get; set; } //vadőrök új pozíciói
        public List<Point> oldpos { get; set; } //vadőrök régi pozíciói

        public AdvancePatrolEventArgs(List<Ranger> rngs, List<Point> old)
        {
            newpos = rngs;
            oldpos = old;
        }
    }

    #endregion
}
