using TNT.Contract;
using TNT.Tests.Presentation;

namespace TNT.Tests.Contracts
{
    public interface ISayAskContract
    {        
        [ContractMessage(CordInterlocutorMock.SaySomething1Id)]
        void SaySomething1(int intParameter, string strParametr, double doubleParameter, float floatParametr, bool boolParametr);

        [ContractMessage(CordInterlocutorMock.SaySomethingWithArrayId)]
        void SaySomethingWithArray(object[] objs, string[] strings);


        [ContractMessage(CordInterlocutorMock.AskMessage1Id)]
        string AskSomething1(int intParameter, string strParametr, double doubleParameter, float floatParametr, bool boolParametr);

        [ContractMessage(CordInterlocutorMock.AskSomethingWithArrayId)]
        bool AskSomethingWithArray(object[] objs, string[] strings);
    }
}