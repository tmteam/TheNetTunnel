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



        /**
         * private string HandleOnTick(object[] arguments)
	        {
		        var tick = OnTick;
		        if(tick == null)
			        return default(string);
		
			        return tick((int)  arguments[0], 
					        (DateTime) arguments[1], 
					        (object)   arguments[2],
					        (string)   arguments[3]);
	        }
         */

        /*
IL_0000:  nop         
IL_0001:  ldarg.0     
IL_0002:  call        UserQuery+SayingContract.get_OnTick
IL_0007:  stloc.0     // tick
IL_0008:  ldloc.0     // tick
IL_0009:  ldnull      
IL_000A:  ceq         
IL_000C:  stloc.1     
IL_000D:  ldloc.1     
IL_000E:  brfalse.s   IL_0014
IL_0010:  ldnull      
IL_0011:  stloc.2     
IL_0012:  br.s        IL_0038
IL_0014:  ldloc.0     // tick
IL_0015:  ldarg.1     
IL_0016:  ldc.i4.0    
IL_0017:  ldelem.ref  
IL_0018:  unbox.any   System.Int32
IL_001D:  ldarg.1     
IL_001E:  ldc.i4.1    
IL_001F:  ldelem.ref  
IL_0020:  unbox.any   System.DateTime
IL_0025:  ldarg.1     
IL_0026:  ldc.i4.2    
IL_0027:  ldelem.ref  
IL_0028:  ldarg.1     
IL_0029:  ldc.i4.3    
IL_002A:  ldelem.ref  
IL_002B:  castclass   System.String
IL_0030:  callvirt    System.Func<System.Int32,System.DateTime,System.Object,System.String,System.String>.Invoke
IL_0035:  stloc.2     
IL_0036:  br.s        IL_0038
IL_0038:  ldloc.2     
IL_0039:  ret         

         */
    }
}