namespace Monitor.Hardware
{
    class GPU
    {
        private Nvidia _nvidia;
        public string Name { get { return _nvidia.Name; } }
        public uint TotalMemMb { get { return _nvidia.TotalMemMb; } }
        public uint FreeMemMb { get { return _nvidia.FreeMemMb; } }
        public uint UsedMemMb { get { return _nvidia.UsedMemMb; } }
        public float LoadMemPct { get { return _nvidia.LoadMemPct; } }
        public float LoadPct { get { return _nvidia.LoadPct; } }
        public float Temperature { get { return _nvidia.Temperature; } }

        public GPU()
        {
            _nvidia = new Nvidia();
        }

        public void Update()
        {
            _nvidia.Update();

        }
    }
}
