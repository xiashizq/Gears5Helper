namespace Gears5Helper;

static class Theme
{
    public static readonly Font Face = new("Microsoft YaHei UI", 9f, FontStyle.Regular);

    public static readonly Font FaceBold = new("Microsoft YaHei UI", 9f, FontStyle.Bold);

    public static readonly Font Title = new("Microsoft YaHei UI", 14f, FontStyle.Bold);

    public static readonly Color Canvas = Color.FromArgb(236, 238, 241);
    public static readonly Color Card = Color.White;
    public static readonly Color Line = Color.FromArgb(214, 218, 224);
    public static readonly Color Ink = Color.FromArgb(24, 24, 24);
    public static readonly Color Muted = Color.FromArgb(92, 96, 102);
    public static readonly Color Header = Color.FromArgb(247, 248, 250);
    public static readonly Color Bar = Color.FromArgb(22, 24, 28);
    public static readonly Color BarText = Color.White;
    public static readonly Color Accent = Color.FromArgb(15, 108, 189);
    public static readonly Color AccentHover = Color.FromArgb(16, 94, 163);
    public static readonly Color Hover = Color.FromArgb(244, 246, 248);
    public static readonly Color Selected = Color.FromArgb(224, 238, 252);

    public static Panel CardHost(int padding) =>
        new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(padding),
            BackColor = Canvas,
        };

    public static Panel CardBody()
    {
        var body = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Card,
            Padding = new Padding(1),
        };
        body.Paint += (_, e) =>
        {
            using var pen = new Pen(Line);
            var box = new Rectangle(0, 0, body.Width - 1, body.Height - 1);
            e.Graphics.DrawRectangle(pen, box);
        };
        return body;
    }
}
