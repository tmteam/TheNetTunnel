using System;
using TNT;

namespace CommonTestTools.Contracts;

public interface ICallContract2
{
    [TntMessage(CordInterlocutorMock.ProcedureCallId)]
    Action ProcedureCall { get; set; }

    [TntMessage(CordInterlocutorMock.ArgumentProcedureCallId)]
    Action<int, string> ArgumentProcedureCall { get; set; }

    [TntMessage(CordInterlocutorMock.ArrayArgumentProcedureCallId)]
    Action<int[], string[]> ArrayArgumentProcedureCall { get; set; }

    [TntMessage(CordInterlocutorMock.FuncCallReturnsIntId)]
    Func<int> FuncCallReturnsInt { get; set; }

    [TntMessage(CordInterlocutorMock.FuncCallReturnsBoolId)]
    Func<bool> FuncCallReturnsBool { get; set; }


    [TntMessage(CordInterlocutorMock.FuncCallReturnsRefTypeId)]
    Func<string> FuncCallReturnsRefType { get; set; }

    [TntMessage(CordInterlocutorMock.ArgumentFuncCallId)]
    Func<int,string,int> ArgumentFuncCall { get; set; }

    [TntMessage(CordInterlocutorMock.ArrayArgumentFuncCallId)]
    Func<int[], string[], int> ArrayArgumentFuncCall { get; set; }

    [TntMessage(CordInterlocutorMock.FuncReturnsArrayCallId)]
    Func<int[]> FuncReturnsArrayCall { get; set; }
}