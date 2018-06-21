using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YogiBear.Model;
using YogiBear.Persistence;

namespace YogiBear.View
{
    //Játékablak típusa
    public partial class GameForm : Form
    {
        #region Fields

        private YogiBearModel model; //játékmodell
        private Label[,] label_grid; //címkerács
        private List<Image> imgs; //játékot alkotó képek
        private Int32 gametime; //játékidő
        private Timer timer; //időzítő
        private bool paused; //szünet
        private bool gameover; //játék vége

        #endregion

        #region Constructor

        //Játékablak példányosítása
        public GameForm()
        {
            InitializeComponent();

            paused = false;
            gameover = false;

            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += new EventHandler(Timer_Tick);
            gametime = 0;

            imgs = new List<Image>();
            imgs.Add(Image.FromFile(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\Resources\\Image\\floor.png"));
            imgs.Add(Image.FromFile(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\Resources\\Image\\barrier.png"));
            imgs.Add(Image.FromFile(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\Resources\\Image\\basket.png"));
            imgs.Add(Image.FromFile(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\Resources\\Image\\player.png"));
            imgs.Add(Image.FromFile(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\Resources\\Image\\ranger.png"));
        }

        #endregion

        #region Private Methods

        //Új pálya létrehozása
        private void CreateMap()
        {
            panel1.Controls.Clear();
            label_grid = new Label[model.Map.Count, model.Map.Count];

            for (Int32 i = 0; i < model.Map.Count; i++)
                for (Int32 j = 0; j < model.Map.Count; j++)
                {
                    label_grid[i, j] = new Label();
                    label_grid[i, j].Location = new Point(32 * j, 32 * i);
                    label_grid[i, j].Size = new Size(imgs[0].Width, imgs[0].Height);

                    panel1.Controls.Add(label_grid[i, j]);
                }
        }

        //Pálya beállítása
        private void SetupMap()
        {
            for (Int32 i = 0; i < label_grid.GetLength(0); i++)
                for (Int32 j = 0; j < label_grid.GetLength(1); j++)
                    switch (model.Map[i][j])
                    {
                        case 0:
                            label_grid[i, j].Image = imgs[0]; //sima mező
                            break;
                        case 1:
                            label_grid[i, j].Image = imgs[1]; //akadály
                            break;
                        case 2:
                            label_grid[i, j].Image = imgs[2]; //piknik kosár
                            break;
                        case 3:
                            label_grid[i, j].Image = imgs[3]; //Maci Laci
                            break;
                        case 4:
                            label_grid[i, j].Image = imgs[4]; //Vadőr
                            break;
                        default:
                            break;
                    }
        }

        #endregion

        #region Form event handlers

        //Új játék menüpont eseménykezelője
        private void NewGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Ha már futott játék, akkor leíratkozunk az eseményekről, míg nem indul új
            if (model != null)
            {
                model.GotBasket -= Model_GotBasket;
                model.AdvancePatrol -= Model_AdvancePatrol;
                model.GameOver -= Model_GameOver;
            }

            model = new YogiBearModel(); //új játékmodell példányosítása

            //Pálya létrehozása/beállítása
            try
            { 
                model.NewGame();
            }
            catch (YogiBearDataException)
            { 
                MessageBox.Show("Pálya betöltése sikertelen!" + Environment.NewLine + "Hibás az elérési út, vagy a fájlformátum." + Environment.NewLine + "Első pálya indítása", "Hiba!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            CreateMap();
            SetupMap();

            scorelabel.Text = model.Baskets.ToString();
            gameover = false;
            paused = false;

            //játékeseményekre feliratkozás
            model.GotBasket += Model_GotBasket;
            model.AdvancePatrol += Model_AdvancePatrol;
            model.GameOver += Model_GameOver;

            //Játékidő inicializálás, időzítők indítása
            gametime = 0;
            timer.Start();
            model.Patrolling.Start();
        }

        //Billentyű lenyomás eseménykezelője
        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            //ha nincs szünet, és tart a játék...
            if (!paused && !gameover)
            {
                switch (e.KeyCode) // megkapjuk a billentyűt
                {
                    case Keys.W: //fel
                        if (model.IsFloor(model.PlayerPos.X - 1, model.PlayerPos.Y))
                        {
                            label_grid[model.PlayerPos.X, model.PlayerPos.Y].Image = imgs[0];
                            label_grid[model.PlayerPos.X - 1, model.PlayerPos.Y].Image = imgs[3];
                            model.Up();
                        }
                        break;
                    case Keys.A: //balra
                        if (model.IsFloor(model.PlayerPos.X, model.PlayerPos.Y - 1))
                        {
                            label_grid[model.PlayerPos.X, model.PlayerPos.Y].Image = imgs[0];
                            label_grid[model.PlayerPos.X, model.PlayerPos.Y - 1].Image = imgs[3];
                            model.Left();
                        }
                        break;
                    case Keys.S: //le
                        if (model.IsFloor(model.PlayerPos.X + 1, model.PlayerPos.Y))
                        {
                            label_grid[model.PlayerPos.X, model.PlayerPos.Y].Image = imgs[0];
                            label_grid[model.PlayerPos.X + 1, model.PlayerPos.Y].Image = imgs[3];
                            model.Down();
                        }
                        break;
                    case Keys.D: //jobbra
                        if (model.IsFloor(model.PlayerPos.X, model.PlayerPos.Y + 1))
                        {
                            label_grid[model.PlayerPos.X, model.PlayerPos.Y].Image = imgs[0];
                            label_grid[model.PlayerPos.X, model.PlayerPos.Y + 1].Image = imgs[3];
                            model.Right();
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        //Szünet menüpont kiválasztásának eseménykezelője
        private void PauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!gameover)
            {
                paused = !paused;
                if (paused)
                {
                    timer.Stop(); //játékidő időzítő
                    model.Patrolling.Stop(); //vadőr járőrözés időzítő
                    timelabel.Text = "Szünet";
                }
                else
                {
                    timer.Start();
                    model.Patrolling.Start();
                    model.AdvancePatrol += Model_AdvancePatrol;
                }
            }
        }

        //Kilépés menüpont eseménykezelője
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

        #region Timer event handler

        //Időzítő eseménykezelője
        private void Timer_Tick(Object sender, EventArgs e)
        {
            gametime += timer.Interval;
            if (gametime > 0)
            { 
                label1.Text = "Idő";
                label2.Text = "Kosarak:";
            }

            timelabel.Text = TimeSpan.FromMilliseconds(gametime).ToString(@"mm\:ss");
        }

        #endregion

        #region Game event handlers

        //Kosár felvétel eseménykezelője
        //Ha új kosarat vesz fel a játékos, akkor frissítjük a hátralévő kosarak számát
        private void Model_GotBasket(object sender, EventArgs e)
        {
            scorelabel.Text = model.Baskets.ToString();
        }

        //Vadőrök járőrőzésének előrehaladásának eseménykezelője
        //ha tart a játék, akkor frissítjük a vadőrök helyzetét a pályán
        private void Model_AdvancePatrol(object sender, AdvancePatrolEventArgs e)
        {
            if (!paused && !gameover)
            {
                for (Int32 i = 0; i < e.newpos.Count; i++)
                {
                    label_grid[e.oldpos[i].X, e.oldpos[i].Y].Image = imgs[0];
                    label_grid[e.newpos[i].Position.X, e.newpos[i].Position.Y].Image = imgs[4];
                }
            }
        }

        //Játék vége eseménykezelője
        private void Model_GameOver(object sender, GameOverEventArgs e)
        {
            timer.Stop();
            model.Patrolling.Stop();
            gameover = true;

            if(e.result)
                MessageBox.Show("Gratulálok, győztél!", "Győzelem", MessageBoxButtons.OK);
            else
                MessageBox.Show("Sajnos vesztettél :(", "Kudarc", MessageBoxButtons.OK);

        }

        //private void Model_PlayerWin(object sender, EventArgs e)
        //{
        //    timer.Stop();
        //    model.Patrolling.Stop();
        //    gameover = true;
        //    MessageBox.Show("Gratulálok, győztél!", "Győzelem", MessageBoxButtons.OK);
        //}

        ////Vesztés eseménykezelője
        //private void Model_PlayerLost(object sender, EventArgs e)
        //{
        //    timer.Stop();
        //    model.Patrolling.Stop();
        //    gameover = true;
        //    MessageBox.Show("Sajnos vesztettél :(", "Kudarc", MessageBoxButtons.OK);
        //}

        #endregion
    }
}
