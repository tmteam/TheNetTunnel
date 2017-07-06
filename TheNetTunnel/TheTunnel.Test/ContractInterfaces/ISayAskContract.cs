using EmitExperiments;

namespace TheTunnel.Test.ContractInterfaces
{
    public interface ISayAskContract
    {        
        [ContractMessage(CordMock.SaySomething1Id)]
        void SaySomething1(int intParameter, string strParametr, double doubleParameter, float floatParametr, bool boolParametr);

        [ContractMessage(CordMock.SaySomethingWithArrayId)]
        void SaySomethingWithArray(object[] objs, string[] strings);


        [ContractMessage(CordMock.AskMessage1Id)]
        string AskSomething1(int intParameter, string strParametr, double doubleParameter, float floatParametr, bool boolParametr);

        [ContractMessage(CordMock.AskSomethingWithArrayId)]
        bool AskSomethingWithArray(object[] objs, string[] strings);
    }
}