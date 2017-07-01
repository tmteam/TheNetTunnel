using System;

namespace EmitExperiments
{
    public class SayingContract : ISayingContract, ICordMessageHandler
    {
        private readonly  IOutputCordApi _rawContract;

        public SayingContract(IOutputCordApi rawContract)
        {
            _rawContract = rawContract;
            /*
             * SayingContract..ctor:
               IL_0000:  ldarg.0     
               IL_0001:  call        System.Object..ctor
               IL_0006:  nop         
               IL_0007:  nop         
               IL_0008:  ldarg.0     
               IL_0009:  ldarg.1     
               IL_000A:  stfld       UserQuery+SayingContract._rawContract
               IL_000F:  ret
             */
        }

        public void SaySomething(int intParameter)
        {
            _rawContract.Say(2, new object[] { intParameter });
            /*
             *  IL_0000:  nop         
                IL_0001:  ldarg.0     
                IL_0002:  ldfld       UserQuery+SayingContract._rawContract
                IL_0007:  ldc.i4.2    
                IL_0008:  ldc.i4.1    
                IL_0009:  newarr      System.Object
                IL_000E:  dup         
                IL_000F:  ldc.i4.0    
                IL_0010:  ldarg.1     
                IL_0011:  box         System.Int32
                IL_0016:  stelem.ref  
                IL_0017:  callvirt    UserQuery+IRawOutputContract.Say
                IL_001C:  nop         
                IL_001D:  ret         
             */

        }

        public void SaySomething2(int intParameter, double doubleParameter)
        {
            _rawContract.Say(2, new object []{ intParameter, doubleParameter });
            /*
             *  IL_0000:  nop         
                IL_0001:  ldarg.0     
                IL_0002:  ldfld       UserQuery+SayingContract._rawContract
                IL_0007:  ldc.i4.2    
                IL_0008:  ldc.i4.2    
                IL_0009:  newarr      System.Object
                IL_000E:  dup         
                IL_000F:  ldc.i4.0    
                IL_0010:  ldarg.1     
                IL_0011:  box         System.Int32
                IL_0016:  stelem.ref  
                IL_0017:  dup         
                IL_0018:  ldc.i4.1    
                IL_0019:  ldarg.2     
                IL_001A:  box         System.Double
                IL_001F:  stelem.ref  
                IL_0020:  callvirt    UserQuery+IRawOutputContract.Say
                IL_0025:  nop         
                IL_0026:  ret         
             */

                //for int, string, double, object:
                /*IL_0000:  nop         
                IL_0001:  ldarg.0     
                IL_0002:  ldfld       UserQuery+SayingContract._rawContract
                IL_0007:  ldc.i4.3    
                IL_0008:  ldc.i4.4    
                IL_0009:  newarr      System.Object
                IL_000E:  dup         
                IL_000F:  ldc.i4.0    
                IL_0010:  ldarg.1     
                IL_0011:  box         System.Int32
                IL_0016:  stelem.ref  
                IL_0017:  dup         
                IL_0018:  ldc.i4.1    
                IL_0019:  ldarg.2     

                IL_001A:  stelem.ref  
                IL_001B:  dup         
                IL_001C:  ldc.i4.2    
                IL_001D:  ldarg.3     

                IL_001E:  box         System.Double
                IL_0023:  stelem.ref  

                IL_0024:  dup         
                IL_0025:  ldc.i4.3    
                IL_0026:  ldarg.s     04
                 
                IL_0028:  stelem.ref  
                IL_0029:  callvirt    UserQuery+IRawOutputContract.Say
                IL_002E:  nop         
                IL_002F:  ret         
                */
}

public string AskSomething(int intParameter, double doubleParameter)
{
return (string)_rawContract.Ask(3, new object[] {intParameter, doubleParameter});
}

public Delegate Handle(ushort tellCordId)
{
if (tellCordId == 4)
    return OnNewMessage;
if (tellCordId == 5)
    return OnNewMessageWithCallBack;
return null;
}

public Action<string, DateTime, ushort> OnNewMessage { get; set; }
public Func<string, DateTime, ushort> OnNewMessageWithCallBack { get; set; }
}
}
 