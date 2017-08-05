namespace TNT.SpeedTest.TransactionBandwidth
{
    public class TransactionBandwidthTestResults
    {
        private const double bpmsTombps = 1024d * 1024d / 1000d;
        public int Size { get; set; }
        public int Iterations { get; set; }
        public int TotalSent { get; set; }
        public int TotalReceived { get; set; }
        public double ElaspedMiliseconds { get; set; }
        public double OutputBandwidthMbs => (TotalSent) / (ElaspedMiliseconds * bpmsTombps);
        public double InputBandwidthMbs => ((TotalReceived)) / (ElaspedMiliseconds * bpmsTombps);
        public double TotalBandwidthMbs => ((TotalReceived + TotalSent)) / (ElaspedMiliseconds * bpmsTombps);

        public string GetStringResults()
        {
            return $"IO/total: {OutputBandwidthMbs:0.0} / {TotalBandwidthMbs:0.0} [MBpS]";
        }

    }
}
