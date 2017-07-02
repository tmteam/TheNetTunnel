using System;

namespace EmitExperiments
{
    class ContractExamplar : ISayingContract
    {
        private readonly IOutputCordApi _transporter;
        private Func<int, DateTime, string> _onAsk;

        public ContractExamplar(IOutputCordApi transporter)
        {
            _transporter = transporter;
            _transporter.Subscribe<string>(52, OnAsk_Callback);
            
        }
        public void SaySomething(int intParameter)
        {
        }

        public void SaySomething2(int intParameter, double doubleParameter)
        {
        }

        public string AskSomething(int intParameter, double doubleParameter)
        {
            return null;
        }

        public int GiveMe42()
        {
            return 0;
        }

        public void TheProcedure()
        {
        }

        public void SaySomething3(int intParameter, string str, double doubleParameter)
        {
        }

        public void SaySomething4(int intParameter, string str, double doubleParameter, object lalal, object[] objs)
        {
        }

        public Action OnProcedureEvent { get; set; }
        public Action<int, DateTime> OnEvent { get; set; }

        public Func<int, DateTime, string> OnAsk { get; set; }

        private string OnAsk_Callback(object[] objects)
        {
            var ask = _onAsk;
            if (ask != null)
                return ask((int) objects[0], (DateTime) objects[1]);

            return null;
        }

        public Func<int> GiveMe42Ask { get; set; }
    }
}