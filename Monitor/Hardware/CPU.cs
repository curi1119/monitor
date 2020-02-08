using System;
using System.Diagnostics;
using System.Text;

namespace Monitor.Hardware
{
    class CPU
    {
        public enum ManufactureEnum
        {
            Intel,
            AMD,
            Unknown,
        };

        private PerformanceCounter _cpu;
        private PerformanceCounter[] _cores;
        public int CoreCnt { get; private set; }
        public float LoadTotal { get; private set; }
        public float[] LoadCores { get; private set; }
        public string Name { get; private set; }
        public ManufactureEnum Manufacture { get; private set; }

        public CPU()
        {
            CoreCnt = Environment.ProcessorCount;
            _cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _cores = new PerformanceCounter[CoreCnt];
            LoadTotal = 0;
            LoadCores = new float[CoreCnt];

            for (var i=0; i < CoreCnt; i++)
            {
                _cores[i] = new PerformanceCounter("Processor", "% Processor Time", i.ToString());
            }

            Microsoft.Win32.RegistryKey registrykeyHKLM = Microsoft.Win32.Registry.LocalMachine;
            string keyPath = @"HARDWARE\DESCRIPTION\System\CentralProcessor\0";
            Microsoft.Win32.RegistryKey registrykeyCPU = registrykeyHKLM.OpenSubKey(keyPath, false);
            string MHz = registrykeyCPU.GetValue("~MHz").ToString();
            string processorName = (string)registrykeyCPU.GetValue("ProcessorNameString");
            processorName = processorName.Trim();
            SetCpuManufacture(processorName);
            SetBuildCpuName(processorName);
            registrykeyCPU.Close();
            registrykeyHKLM.Close();
        }

        private void SetCpuManufacture(string processorName)
        {
            if (processorName.Contains("Intel"))
            {
                this.Manufacture = ManufactureEnum.Intel;
            }
            else if (processorName.Contains("AMD"))
            {
                this.Manufacture = ManufactureEnum.AMD;
            }
            else
            {
                this.Manufacture = ManufactureEnum.Unknown;
            }
        }

        private void SetBuildCpuName(string processorName)
        {
            StringBuilder txBuild = new StringBuilder(processorName);
            switch (this.Manufacture)
            {
                case ManufactureEnum.Intel:
                    txBuild.Replace("Intel", "");
                    txBuild.Replace("(R)", "");
                    txBuild.Replace("(TM)", "");
                    break;
                case ManufactureEnum.AMD:
                    txBuild.Replace("AMD", "");
                    txBuild.Replace("Processor", "");
                    break;
            }
            this.Name = txBuild.ToString();
        }

        public void Update()
        {
            LoadTotal = _cpu.NextValue();
            for(int i=0; i < _cores.Length; i++)
            {
                LoadCores[i] = _cores[i].NextValue();
            }
        }
    }
}
