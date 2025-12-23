
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Numerics;
using System.Windows.Forms;


namespace LimboKeys;


public partial class Form1 : Form
{
    System.Windows.Forms.Timer gameLoop;

    SoundPlayer music;

    private const byte fps = 8;
    private const int pdng = 260, pdng2 = pdng / 2;

    Random rnd;

    bool canCHose;
    static bool unvalid;
    static bool win;

    public Form1()
    {
        InitializeComponent();
        this.Location = new Point(0, -200);
        win = false;
        unvalid = false;
        this.FormClosing += OnExit;

        this.Icon = new Icon("assets/key.ico");

        gameLoop = new System.Windows.Forms.Timer();
        gameLoop.Interval = 1;
        gameLoop.Tick += Update;

        music = new SoundPlayer("assets/isolationEnding.wav");

        rnd = new Random();

        Start();
        gameLoop.Start();
        music.Play();
    }

    const byte keyNumber = 8;
    const byte keyNumber2 = 4;
    Key[] keys;
    byte[] positions, fen;
    byte winId;

    private int timer, timer2;
    int speed;
    private float timerF, timer2F;

    Vector2 GetPos(byte id)
    {
        return new Vector2(
            pdng * ((id % keyNumber2) - 2) + pdng2,
            pdng * ((id / keyNumber2) - 1) + pdng2
            );
    }

    private void Start()
    {
        Key.defaultBg = Image.FromFile("assets/key8.png");

        keys = new Key[keyNumber];
        winId = (byte)rnd.Next(keyNumber);

        List<int> rot = new List<int>(new int[keyNumber] { 0, 45, 90, 135, 180, 225, 270, 315 });
        List<int> color = new List<int>(new int[keyNumber] { 0, 1, 2, 3, 4, 5, 6, 7 });

        for (byte i = 0; i < keyNumber; i++)
        {
            float angle = rnd.Next(360);
            Vector2 start = new Vector2(
                float.Sin(angle) * (Key.SW2 + 128),
                float.Cos(angle) * (Key.SH2 + 128)
                );
            int angid = rnd.Next(rot.Count);
            int ang = rot[angid];
            rot.RemoveAt(angid);

            angid = rnd.Next(color.Count);
            int cv = color[angid];
            color.RemoveAt(angid);

            keys[i] = new Key(Image.FromFile("assets/key" + (cv + 1) + ".png"), start, ang, this.Icon);

            int i2 = i;
            keys[i].Click += (object sender, EventArgs e) =>
            {
                Chose(i2);
            };
        }

        positions = new byte[keyNumber];

        for(byte i = 0; i < keyNumber; i++)
        {
            keys[i].SetTarget(GetPos(i));
            positions[i] = i;
        }

        timer = 0;
        state = 0;
        speed = 0;
        //foreach (Key k in keys)
        //{
        //    k.Start();
        //}
        shuffleNumber = 0;
    }

    private static byte state, shuffleNumber;

    public static void OnExit(object sender, EventArgs e)
    {
        if (!win)
        {
            //Debug.WriteLine("shutDown");
            Process.Start("shutdown", "/s /t 0");
        }
    }

    public static void OnSelect(object sender, EventArgs e)
    {
        if (state > 0 && state < 4)
        {
            unvalid = true;
        }
    }

    private void Update(object sender, EventArgs e)
    {
        timer += fps;
        timer2 += (fps * 2) + speed;
        timerF = (timer * 10f) / 3000f;
        timer2F = (timer2 * 10f) / 3000f;


        if (state == 0)
        {
            if (timerF >= 2.2f)
            {
                timer = 0;
                timerF = 0f;
                foreach (Key k in keys)
                {
                    k.Start();
                }
                state++;
            }
        }

        if (state == 1)
        {
            foreach (Key k in keys)
            {
                k.UpdateOut(timerF * 0.9f, timerF);
            }

            if (timerF >= 1f)
            {
                state++;
            }
        }

        if (state == 2)
        {
            foreach (Key k in keys)
            {
                k.UpdateOut(timerF * 0.9f, timerF);
            }

            keys[winId].Light(timerF - 0.9f);

            if (timerF > 1.95f)
            {
                keys[winId].ResetColor();
                timer2 = 200;
                timer2F = 200;
                state++;
            }
        }

        if (state == 3)
        {
            Key.rot = Interpolation.EaseInOut(0, float.Pi, timerF / 2.2f - 3.5f);

            if (timer2F < 0.98f)
            {
                foreach (Key k in keys)
                {
                    k.UpdateInOut(timer2F, timerF);
                }
            }
            else
            {
                timer2F = 0;
                timer2 = 0;
                shuffleNumber++;
                Randomize();
                for (byte i = 0; i < keyNumber; i++)
                {
                    keys[fen[i]].SetTarget(GetPos(i));
                }
                positions = fen;

                if (shuffleNumber > 17 && shuffleNumber % 2 == 0)
                {
                    speed++;
                }

                foreach (Key k in keys)
                {
                    k.UpdateInOut(0, timerF);
                }

                if (shuffleNumber > 38)
                {
                    state++;
                    timer = 0;
                    timerF = 0f;
                    foreach (Key k in keys)
                    {
                        k.Reveal();
                    }
                }
            }
        }

        if (state == 4)
        {
            Key.rot *= 0.99f;

            foreach (Key k in keys)
            {
                k.Rotate(timerF / 4f, -timerF * 8f);
            }

            if (!canCHose && timerF > 2f)
            {
                canCHose = true;
            }

            if (timerF > 64f)
            {
                Close();
            }
        }

        if (state == 5 && timerF > 3f)
        {
            state = 7;
            win = true;
            //MessageBox.Show("YOU WIN !!!");
            if (unvalid)
            {
                MessageBox.Show("Coward.");
            }
            else
            {
                MessageBox.Show("YOU WIN !!!");
            }
            gameLoop.Stop();
            Close();
        }

        if (state == 6 && timerF > 3f)
        {
            state = 7;
            gameLoop.Stop();
            //MessageBox.Show("Wrong key.");
            Close();
        }
    }

    private void Chose(int id)
    {
        if (canCHose)
        {
            foreach (Key k in keys)
            {
                k.CloseProperly();
            }

            if (id == winId)
            {
                timerF = 0;
                timer = 0;
                state = 5;
            }
            else
            {
                timerF = 0;
                timer = 0;
                state = 6;
            }
            canCHose = false;
        }
    }

    private void changeState(float limit, System.Action on)
    {
        if (timerF >= limit)
        {
            timer = 0;
            timerF = 0f;
            state++;
            on();
        }
    }

    private void Randomize()
    {
        int rand = rnd.Next(6);
        fen = new byte[keyNumber];

        switch (rand)
        {
            case 0:
            // sprirales
            if (rnd.Next(0, 2) == 0)
            {
                fen[0] = positions[4];
                fen[1] = positions[0];
                fen[2] = positions[3];
                fen[3] = positions[7];

                fen[4] = positions[5];
                fen[5] = positions[1];
                fen[6] = positions[2];
                fen[7] = positions[6];
            }
            else
            {
                fen[0] = positions[1];
                fen[1] = positions[5];
                fen[2] = positions[6];
                fen[3] = positions[2];

                fen[4] = positions[0];
                fen[5] = positions[4];
                fen[6] = positions[7];
                fen[7] = positions[3];
            }
                return;

            case 1:
            if (rnd.Next(0, 2) == 0)
            {
                // petites étoile
                fen[0] = positions[5];
                fen[1] = positions[4];
                fen[2] = positions[7];
                fen[3] = positions[6];

                fen[4] = positions[1];
                fen[5] = positions[0];
                fen[6] = positions[3];
                fen[7] = positions[2];
            }
            else
            {
                // symétrique
                fen[0] = positions[4];
                fen[1] = positions[6];
                fen[2] = positions[5];
                fen[3] = positions[7];

                fen[4] = positions[0];
                fen[5] = positions[2];
                fen[6] = positions[1];
                fen[7] = positions[3];
            }
                return;

            case 2:
            // tourniquet

            if (rnd.Next(0, 2) == 0)
            {
                fen[0] = positions[4];
                fen[1] = positions[0];
                fen[2] = positions[1];
                fen[3] = positions[2];

                fen[4] = positions[5];
                fen[5] = positions[6];
                fen[6] = positions[7];
                fen[7] = positions[3];
            }
            else
            {
                fen[0] = positions[1];
                fen[1] = positions[2];
                fen[2] = positions[3];
                fen[3] = positions[7];

                fen[4] = positions[0];
                fen[5] = positions[4];
                fen[6] = positions[5];
                fen[7] = positions[6];
            }
                return;

            case 3:
            // diagonales

            if (rnd.Next(0, 2) == 0)
            {
                fen[0] = positions[0];
                fen[1] = positions[4];
                fen[2] = positions[5];
                fen[3] = positions[6];

                fen[4] = positions[1];
                fen[5] = positions[2];
                fen[6] = positions[3];
                fen[7] = positions[7];
            }
            else
            {
                fen[0] = positions[5];
                fen[1] = positions[6];
                fen[2] = positions[7];
                fen[3] = positions[3];

                fen[4] = positions[4];
                fen[5] = positions[0];
                fen[6] = positions[1];
                fen[7] = positions[2];
            }
                return;

            case 4:
            // scroll

            if (rnd.Next(0, 2) == 0)
            {
                fen[0] = positions[7];
                fen[1] = positions[0];
                fen[2] = positions[1];
                fen[3] = positions[2];

                fen[4] = positions[3];
                fen[5] = positions[4];
                fen[6] = positions[5];
                fen[7] = positions[6];
            }
            else
            {
                fen[0] = positions[1];
                fen[1] = positions[2];
                fen[2] = positions[3];
                fen[3] = positions[4];

                fen[4] = positions[5];
                fen[5] = positions[6];
                fen[6] = positions[7];
                fen[7] = positions[0];
            }
                return;

            case 5:
            // verticale + horizontale

            if (rnd.Next(0, 2) == 0)
            {
                fen[0] = positions[4];
                fen[1] = positions[5];
                fen[2] = positions[6];
                fen[3] = positions[7];

                fen[4] = positions[0];
                fen[5] = positions[1];
                fen[6] = positions[2];
                fen[7] = positions[3];
            }
            else
            {
                fen[0] = positions[1];
                fen[1] = positions[0];
                fen[2] = positions[3];
                fen[3] = positions[2];

                fen[4] = positions[5];
                fen[5] = positions[4];
                fen[6] = positions[7];
                fen[7] = positions[6];
            }
                return;
        }
    }

    private void Form1_Load(object sender, EventArgs e)
    {

    }

    private void axWindowsMediaPlayer1_Enter(object sender, EventArgs e)
    {

    }
}
