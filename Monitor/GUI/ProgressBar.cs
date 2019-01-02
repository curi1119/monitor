using System.Drawing;

namespace Monitor.GUI
{
    class ProgressBar
    {
        private static Image _barBg = Util.getImage("Monitor.Embed.bar_bg.png");
        private Image _barFore;
        public const int HEIGHT = 6;
        private float _width;
        private int _x;
        private int _y;

        public ProgressBar(int width, int x, int y, string foreImage)
        {
            _width = width;
            _x = x;
            _y = y;
            _barFore = Util.getImage(foreImage);
        }

        public void draw(Graphics g, uint pct)
        {
            g.DrawImage(_barBg, _x, _y, _width, 6);
            float foreWidth = _width / 100.0f * pct;
            g.DrawImage(_barFore, _x, _y, foreWidth, 6);
        }
    }
}
