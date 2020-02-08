using System;
using System.Drawing;

namespace Monitor.GUI
{
    class CoreProgressBar
    {
        private int _i;
        private int _x;
        private int _y;
        private int _barWidth;
        //private string _coreNum;
        private string _pct;
        //bool _showPct;
        ProgressBar _bar;
        Font _font;

        public CoreProgressBar(int i, int barWidth, int x, int y, Font font)
        {
            _i = i;
            _x = x;
            _y = y;
            _barWidth = barWidth;
            //_coreNum = String.Format("{0:D2}", i + 1);
            _font = font;
            string imageName = String.Format("Monitor.Embed.bar{0:D2}.png", i);
            _bar = new ProgressBar(barWidth, x, y + 3, imageName);
        }

        public void Draw(uint load, Graphics g)
        {
            //g.DrawString(_coreNum, _font, Brushes.White, _x, _y);
            _bar.draw(g, load);
            _pct = load.ToString() + "%";
            g.DrawString(_pct, _font, Brushes.White, _x + _barWidth + 3, _y);
        }
    }
}
