using TNT;

namespace CommonTestTools.Contracts
{
    public interface ISayAskContract
    {        
        [TntMessage(CordInterlocutorMock.SaySomething1Id)]
        void SaySomething1(int intParameter, string strParametr, double doubleParameter, float floatParametr, bool boolParametr);

        [TntMessage(CordInterlocutorMock.SaySomethingWithArrayId)]
        void SaySomethingWithArray(object[] objs, string[] strings);


        [TntMessage(CordInterlocutorMock.AskMessage1Id)]
        string AskSomething1(int intParameter, string strParametr, double doubleParameter, float floatParametr, bool boolParametr);

        [TntMessage(CordInterlocutorMock.AskSomethingWithArrayId)]
        bool AskSomethingWithArray(object[] objs, string[] strings);
    }
}