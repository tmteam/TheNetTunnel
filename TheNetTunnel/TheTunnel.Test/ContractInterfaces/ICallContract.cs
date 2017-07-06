using System;
using EmitExperiments;

namespace TheTunnel.Test.ContractInterfaces
{
    public interface ICallContract
    {
        [ContractMessage(CordMock.ProcedureCallId)]
        Action ProcedureCall { get; set; }

        [ContractMessage(CordMock.ArgumentProcedureCallId)]
        Action<int, string> ArgumentProcedureCall { get; set; }

        [ContractMessage(CordMock.ArrayArgumentProcedureCallId)]
        Action<int[], string[]> ArrayArgumentProcedureCall { get; set; }

        [ContractMessage(CordMock.FuncCallReturnsIntId)]
        Func<int> FuncCallReturnsInt { get; set; }

        [ContractMessage(CordMock.FuncCallReturnsBoolId)]
        Func<bool> FuncCallReturnsBool { get; set; }


        [ContractMessage(CordMock.FuncCallReturnsRefTypeId)]
        Func<string> FuncCallReturnsRefType { get; set; }

        [ContractMessage(CordMock.ArgumentFuncCallId)]
        Func<int,string,int> ArgumentFuncCall { get; set; }

        [ContractMessage(CordMock.ArrayArgumentFuncCallId)]
        Func<int[], string[], int> ArrayArgumentFuncCall { get; set; }

        [ContractMessage(CordMock.FuncReturnsArrayCallId)]
        Func<int[]> FuncReturnsArrayCall { get; set; }
    }
}