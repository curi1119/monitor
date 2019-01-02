using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using Monitor.Hardware;

namespace Monitor.GUI
{
    public partial class MainForm : Form
    {
        private CPU _cpu = new CPU();
        private GPU _gpu = new GPU();
        private Memory _memory = new Memory();

        private ProgressBar _ramBar;
        private ProgressBar _vramBar;
        private ProgressBar _gpuBar;
        private ProgressBar _cpuBar;
        private ProgressBar[] _cpuCoreBars;

        public MainForm()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            InitializeComponent();
            this.MouseDown += new MouseEventHandler(mouseDownHandler);
            this.MouseMove += new MouseEventHandler(mouseMoveHandler);

            _ramBar = new ProgressBar(70, 37, 79, "Monitor.Embed.bar09.png");
            _vramBar = new ProgressBar(70, 46, 261, "Monitor.Embed.bar09.png");
            _gpuBar = new ProgressBar(64, 42, 208, "Monitor.Embed.bar10.png");
            _cpuBar = new ProgressBar(64, 42, 23, "Monitor.Embed.bar00.png");
            _cpuCoreBars = new ProgressBar[_cpu.coreCnt];
            int y = 97;
            string imgName;
            for (int i=0; i < _cpu.coreCnt; i++)
            {
                imgName = "Monitor.Embed.bar0" + (i + 1).ToString() + ".png";
                _cpuCoreBars[i] = new ProgressBar(70, 28, y, imgName);
                y += 10;
            }
            this.timer.Enabled = true;
        }

        private static Font _fntTitle = new Font("Arial", 10, FontStyle.Bold);
        private static Font _fntUsage = new Font("Arial", 8, FontStyle.Bold);
        private static Font _fntLabel = new Font("Arial", 7);
        private static Image _bg = Util.getImage("Monitor.Embed.bg.png");
        private static Image _logoCPU = Util.getImage("Monitor.Embed.logo_intel.png");
        private static Image _logoGPU = Util.getImage("Monitor.Embed.logo_nvidia.png");
        private static Image _horizontalLine = Util.getImage("Monitor.Embed.hr.png");

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.Clear(Color.Transparent);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawImageUnscaled(_bg, 0, 0);
            g.SmoothingMode = SmoothingMode.None;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(_horizontalLine, 8, 92, 120, 1);
            drawCPU(g);
            drawRAM(g);
            drawGPU(g);
        }

        private void drawCPU(Graphics g)
        {
            g.DrawImageUnscaled(_logoCPU, 5, 5);
            g.DrawString("CPU Usage", _fntTitle, Brushes.White, 40, 5);
            g.DrawString(((uint)_cpu.loadTotal).ToString() + "%", _fntUsage, Brushes.White, 115, 18);

            _cpuBar.draw(g, (uint)_cpu.loadTotal);
            g.DrawString(_cpu.Name, _fntLabel, Brushes.Gold, 2, 40);

            string pct;
            for (int i = 0; i < _cpu.coreCnt; i++)
            {
                g.DrawString((i + 1).ToString(), _fntLabel, Brushes.White, 10, (95 + i * 10));
                _cpuCoreBars[i].draw(g, (uint)_cpu.loadCores[i]);
                pct = (_cpu.loadCores[i] > 10) ? ((uint)_cpu.loadCores[i]).ToString(): "  " + ((uint)_cpu.loadCores[i]).ToString();
                pct += "%";
                g.DrawString(pct, _fntLabel, Brushes.White, 114, (95 + i * 10));
            }
        }

        private void drawRAM(Graphics g)
        {
            g.DrawString("Used",  _fntLabel, Brushes.AliceBlue, 12, 52);
            g.DrawString("Free",  _fntLabel, Brushes.AliceBlue, 57, 52);
            g.DrawString("Total", _fntLabel, Brushes.AliceBlue, 100, 52);
            g.DrawString("RAM",   _fntLabel, Brushes.AliceBlue, 10, 76);

            g.DrawString(_memory.usedMb.ToString() + "Mb",  _fntLabel, Brushes.AliceBlue, 8, 62);
            g.DrawString(_memory.freeMb.ToString() + "Mb",  _fntLabel, Brushes.AliceBlue, 50, 62);
            g.DrawString(_memory.totalMb.ToString() + "Mb", _fntLabel, Brushes.AliceBlue, 93, 62);

            _ramBar.draw(g, _memory.loadPct);
        }

        private void drawGPU(Graphics g)
        {
            g.DrawImageUnscaled(_logoGPU, 5, 190);
            g.DrawString("GPU Usage", _fntTitle, Brushes.White, 40, 188);
            g.DrawString(((uint)_gpu.loadPct).ToString() + "%", _fntUsage, Brushes.White, 115, 203);
            g.DrawString(((uint)_gpu.temperature).ToString() + "°", _fntUsage, Brushes.Pink, 115, 190);

            g.DrawString(_gpu.Name, _fntLabel, Brushes.MediumSpringGreen, 5, 221);

            g.DrawString("Used",  _fntLabel, Brushes.AliceBlue, 12, 233);
            g.DrawString("Free",  _fntLabel, Brushes.AliceBlue, 57, 233);
            g.DrawString("Total", _fntLabel, Brushes.AliceBlue, 100, 233);
            g.DrawString("VRAM",  _fntLabel, Brushes.AliceBlue, 10, 258);

            g.DrawString(_gpu.usedMemMb.ToString() + "Mb",  _fntLabel, Brushes.AliceBlue, 8, 243);
            g.DrawString(_gpu.freeMemMb.ToString() + "Mb",  _fntLabel, Brushes.AliceBlue, 50, 243);
            g.DrawString(_gpu.totalMemMb.ToString() + "Mb", _fntLabel, Brushes.AliceBlue, 93, 243);
            _gpuBar.draw(g, (uint)_gpu.loadPct);
            _vramBar.draw(g, (uint)_gpu.loadMemPct);
        }


        private Point _mousePoint;
        private void mouseDownHandler(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                _mousePoint = new Point(e.X, e.Y);
            }
        }

        private void mouseMoveHandler(object sender,
            System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                this.Location = new Point(
                    this.Location.X + e.X - _mousePoint.X,
                    this.Location.Y + e.Y - _mousePoint.Y);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            _memory.update();
            _cpu.update();
            _gpu.update();
            this.Invalidate();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
