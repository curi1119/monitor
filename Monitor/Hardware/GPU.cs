namespace Monitor.Hardware
{
    class GPU
    {
        private Nvidia _nvidia;
        public string Name { get { return _nvidia.name; } }
        public uint totalMemMb { get { return _nvidia.totalMemMb; } }
        public uint freeMemMb { get { return _nvidia.freeMemMb; } }
        public uint usedMemMb { get { return _nvidia.usedMemMb; } }
        public float loadMemPct { get { return _nvidia.loadMemPct; } }
        public float loadPct { get { return _nvidia.loadPct; } }
        public float temperature { get { return _nvidia.temperature; } }

        public GPU()
        {
            _nvidia = new Nvidia();

        }

        public void update()
        {
            _nvidia.update();

        }

    }
}
