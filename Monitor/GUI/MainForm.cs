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
        private CoreProgressBar[] _cpuCoreBars;

        private static readonly Font _fntTitle = new Font("Arial", 10, FontStyle.Bold);
        private static readonly Font _fntUsage = new Font("Arial", 8, FontStyle.Bold);
        private static readonly Font _fntLabel = new Font("Arial", 7);
        private static readonly Image _bg = Util.getImage("Monitor.Embed.bg.png");
        private static Image _logoCPU;
        private static readonly Image _logoGPU = Util.getImage("Monitor.Embed.logo_nvidia.png");
        private static readonly Image _horizontalLine = Util.getImage("Monitor.Embed.hr.png");


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
            _cpuCoreBars = new CoreProgressBar[_cpu.CoreCnt];
            BuildCoreProgressBar();
            this.timer.Enabled = true;
        }

        const int CORE_BAR_X_LEFT = 5;
        const int CORE_BAR_X_RIGHT = 70;
        const int CORE_BAR_Y = 95;
        const int CORE_BAR_Y_SPAN = 10;
        const int CORE_BAR_WIDTH_FULL = 110;
        const int CORE_BAR_WIDTH_HALF = 43;

        private void BuildCoreProgressBar()
        {
            int x = CORE_BAR_X_LEFT;
            int y = CORE_BAR_Y;
            if (_cpu.CoreCnt <= 8)
            {
                for (int i = 0; i < _cpu.CoreCnt; i++)
                {
                    _cpuCoreBars[i] = new CoreProgressBar(i, CORE_BAR_WIDTH_FULL, x, y, _fntLabel);
                    y += CORE_BAR_Y_SPAN;
                }
            }
            else
            {
                for (int i = 0; i < _cpu.CoreCnt; i++)
                {
                    _cpuCoreBars[i] = new CoreProgressBar(i, CORE_BAR_WIDTH_HALF, x, y, _fntLabel);
                    y += CORE_BAR_Y_SPAN;
                    if(i == 7)
                    {
                        x = CORE_BAR_X_RIGHT;
                        y = CORE_BAR_Y;
                    }
                }
            }
        }

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
            ApplyCpuLog();
            DrawCPU(g);
            DrawRAM(g);
            DrawGPU(g);
        }

        private void ApplyCpuLog()
        {
            Image logo;
            switch (_cpu.Manufacture)
            {
                case CPU.ManufactureEnum.Intel:
                    logo = Util.getImage("Monitor.Embed.logo_intel.png");
                    break;
                case CPU.ManufactureEnum.AMD:
                    logo = Util.getImage("Monitor.Embed.logo_ryzen.png");
                    break;
                default:
                    logo = Util.getImage("Monitor.Embed.logo_unknown.png");
                    break;
            }
            _logoCPU = logo;
        }

        private void DrawCPU(Graphics g)
        {
            // total
            g.DrawImageUnscaled(_logoCPU, 5, 5);
            g.DrawString("CPU Usage", _fntTitle, Brushes.White, 40, 5);
            g.DrawString(((uint)_cpu.LoadTotal).ToString() + "%", _fntUsage, Brushes.White, 115, 18);
            _cpuBar.draw(g, (uint)_cpu.LoadTotal);
            g.DrawString(_cpu.Name, _fntLabel, Brushes.Gold, 2, 40);

            for (int i = 0; i < _cpu.CoreCnt; i++)
            {
                _cpuCoreBars[i].Draw((uint)_cpu.LoadCores[i], g);
            }
        }

        private void DrawRAM(Graphics g)
        {
            g.DrawString("Used",  _fntLabel, Brushes.AliceBlue, 12, 52);
            g.DrawString("Free",  _fntLabel, Brushes.AliceBlue, 57, 52);
            g.DrawString("Total", _fntLabel, Brushes.AliceBlue, 100, 52);
            g.DrawString("RAM",   _fntLabel, Brushes.AliceBlue, 10, 76);

            g.DrawString(_memory.UsedMb.ToString() + "Mb",  _fntLabel, Brushes.AliceBlue, 8, 62);
            g.DrawString(_memory.FreeMb.ToString() + "Mb",  _fntLabel, Brushes.AliceBlue, 50, 62);
            g.DrawString(_memory.TotalMb.ToString() + "Mb", _fntLabel, Brushes.AliceBlue, 93, 62);

            _ramBar.draw(g, _memory.LoadPct);
        }

        private void DrawGPU(Graphics g)
        {
            g.DrawImageUnscaled(_logoGPU, 5, 190);
            g.DrawString("GPU Usage", _fntTitle, Brushes.White, 40, 188);
            g.DrawString(((uint)_gpu.LoadPct).ToString() + "%", _fntUsage, Brushes.White, 115, 203);
            g.DrawString(((uint)_gpu.Temperature).ToString() + "°", _fntUsage, Brushes.Pink, 115, 190);

            g.DrawString(_gpu.Name, _fntLabel, Brushes.MediumSpringGreen, 5, 221);

            g.DrawString("Used",  _fntLabel, Brushes.AliceBlue, 12, 233);
            g.DrawString("Free",  _fntLabel, Brushes.AliceBlue, 57, 233);
            g.DrawString("Total", _fntLabel, Brushes.AliceBlue, 100, 233);
            g.DrawString("VRAM",  _fntLabel, Brushes.AliceBlue, 10, 258);

            g.DrawString(_gpu.UsedMemMb.ToString() + "Mb",  _fntLabel, Brushes.AliceBlue, 8, 243);
            g.DrawString(_gpu.FreeMemMb.ToString() + "Mb",  _fntLabel, Brushes.AliceBlue, 50, 243);
            g.DrawString(_gpu.TotalMemMb.ToString() + "Mb", _fntLabel, Brushes.AliceBlue, 93, 243);
            _gpuBar.draw(g, (uint)_gpu.LoadPct);
            _vramBar.draw(g, (uint)_gpu.LoadMemPct);
        }


        private Point _mousePoint;
        private void mouseDownHandler(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                _mousePoint = new Point(e.X, e.Y);
            }
        }

        private void mouseMoveHandler(object sender, System.Windows.Forms.MouseEventArgs e)
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
            _memory.Update();
            _cpu.Update();
            _gpu.Update();
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
