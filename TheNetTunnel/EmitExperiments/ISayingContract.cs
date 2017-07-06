using System;

namespace EmitExperiments
{
    public interface ISayingContract
    {
        [ContractMessage(1)]
        void SaySomething(int intParameter);

        [ContractMessage(2)]
        void SaySomething2(int intParameter, double doubleParameter);

        [ContractMessage(84)]
        string AskSomething(int intParameter, double doubleParameter);

        [ContractMessage(87)]
        int GiveMe42();

        [ContractMessage(88)]
        void TheProcedure();

        [ContractMessage(3)]
        void SaySomething3(int intParameter, string str, double doubleParameter);

        [ContractMessage(4)]
        void SaySomething4(int intParameter, string str, double doubleParameter, object lalal, object[] objs);

        [ContractMessage(50)]
        Action OnProcedureEvent { get; set; }

        [ContractMessage(51)]
        Action<int, DateTime> OnEvent { get; set; }

        [ContractMessage(53)]
        Func<int> GiveMe42Ask { get; set; }

        [ContractMessage(54)]
        Func<double, double, double> SummAsk { get; set; }
    }
}
