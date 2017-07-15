using System;
using TNT.Presentation;
using TNT.Presentation.Proxy;

namespace TNT.Tests.Presentation.Proxy.ContractInterfaces
{
    public interface ICallContract
    {
        [ContractMessage(CordInterlocutorMock.ProcedureCallId)]
        Action ProcedureCall { get; set; }

        [ContractMessage(CordInterlocutorMock.ArgumentProcedureCallId)]
        Action<int, string> ArgumentProcedureCall { get; set; }

        [ContractMessage(CordInterlocutorMock.ArrayArgumentProcedureCallId)]
        Action<int[], string[]> ArrayArgumentProcedureCall { get; set; }

        [ContractMessage(CordInterlocutorMock.FuncCallReturnsIntId)]
        Func<int> FuncCallReturnsInt { get; set; }

        [ContractMessage(CordInterlocutorMock.FuncCallReturnsBoolId)]
        Func<bool> FuncCallReturnsBool { get; set; }


        [ContractMessage(CordInterlocutorMock.FuncCallReturnsRefTypeId)]
        Func<string> FuncCallReturnsRefType { get; set; }

        [ContractMessage(CordInterlocutorMock.ArgumentFuncCallId)]
        Func<int,string,int> ArgumentFuncCall { get; set; }

        [ContractMessage(CordInterlocutorMock.ArrayArgumentFuncCallId)]
        Func<int[], string[], int> ArrayArgumentFuncCall { get; set; }

        [ContractMessage(CordInterlocutorMock.FuncReturnsArrayCallId)]
        Func<int[]> FuncReturnsArrayCall { get; set; }
    }
}