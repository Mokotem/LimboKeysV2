
using System.Numerics;
using System.Windows.Forms;
using System.Drawing;

namespace LimboKeys;

class Key : Form
{
    public static readonly int SW = Screen.PrimaryScreen.Bounds.Width;
    public static readonly int SH = Screen.PrimaryScreen.Bounds.Height;

    public static readonly int SW2 = Screen.PrimaryScreen.Bounds.Width / 2;
    public static readonly int SH2 = Screen.PrimaryScreen.Bounds.Height / 2;

    private const byte w = 150, w2 = 75;

    private Vector2 p1, p2;
    private readonly Image bg;

    public static Image defaultBg;

    private readonly byte id;
    readonly int rotOffset;

    public static float rot = 0f;

    public Key(byte id, Image bg, Vector2 startPos, int rot) : base()
    {
        this.id = id;
        this.Text = "Key " + (id + 1);
        this.bg = bg;
        this.p1 = startPos;
        this.p2 = startPos;
        AutoSize = false;
        FormBorderStyle = FormBorderStyle.None;
        Width = w;
        Height = w;
        this.BackColor = Color.FromArgb(0, 0, 1);
        this.TransparencyKey = Color.FromArgb(0, 0, 1);
        rotOffset = rot;
        Show();
        Update(0f, startPos);
    }

    public void Hide()
    {
        BackgroundImage = defaultBg;
    }

    public void Reveal()
    {
        BackgroundImage = bg;
    }

    public void Start()
    {
        this.BackgroundImage = defaultBg;
    }

    public void SetTarget(Vector2 value)
    {
        p1 = p2;
        p2 = value;
    }

    public void UpdateInOut(float dt, float timer)
    {
        Vector2 pos = Interpolation.EaseInOut(p1, p2, dt);
        Update(timer, pos);
    }

    public void Light(float dt)
    {
        int value = (int)float.Round(Interpolation.Parable(0f, 255f, dt));
        BackColor = Color.FromArgb(0, value, 1);
    }

    public void Rotate(float dt, float rot)
    {
        float a = ((rot + rotOffset) * float.Pi) / 180f;
        Vector2 pos = new Vector2(
            float.Sin(a) * 480,
            float.Cos(a) * 240
            );

        p2 += (pos - p2) * 0.01f;
        Update(0f, p2);
    }

    public void ResetColor()
    {
        this.BackColor = Color.FromArgb(0, 0, 1);
    }

    public void UpdateOut(float dt, float timer)
    {
        Vector2 pos = Interpolation.CubeOut(p1, p2, dt, exp:5);
        Update(timer, pos);
    }

    private void Update(float t, Vector2 pos)
    {
        float a1 = float.Atan(pos.Y / pos.X);
        if (pos.X < 0)
            a1 -= float.Pi;
        float d = float.Sqrt((pos.X * pos.X) + (pos.Y * pos.Y));

        Location = new Point(
            (int)float.Round((float.Cos(a1 - rot) * d) - w2 + SW2),
            (int)float.Round((float.Sin(a1 - rot) * d) - w2 + SH2 + float.Sin(t * float.Pi) * 16)
            );
    }
}