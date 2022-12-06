namespace TNT.SpeedTest.OutputBandwidth;

public class OutputBandwithTestResults
{
    private const double bpmsTombps = 1024d * 1024d / 1000d;
    public int Size { get; set; }
    public int Iterations { get; set; }
    public int TotalSent { get; set; }
    public int TotalReceived { get; set; }
    public double ElaspedMilisecondsForSendOnly { get; set; }
    public double ElaspedMilisecondsForSendAndReceive { get; set; }
    public double OutputBandwidthMbs => (TotalSent) / (ElaspedMilisecondsForSendOnly * bpmsTombps);
    public double TotalBandwidthMbs => ((TotalSent + TotalReceived)) / (ElaspedMilisecondsForSendAndReceive * bpmsTombps);

     
    public static string GetTabbedHeader()
    {
        return "\tout\ttotal [megaBYtes per sec]";
    }
    public string GetTabbedResults()
    {
        return $"\t{OutputBandwidthMbs:0.0} \t {TotalBandwidthMbs:0.0}";
    }

}