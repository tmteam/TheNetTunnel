namespace TNT.Cords
{
    public delegate void TNTAction<T1,T2,T3,T4,T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public delegate void TNTAction<T1, T2, T3, T4, T5,T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,T6 arg6);
    public delegate void TNTAction<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,T6 arg6,T7 arg7);
    public delegate void TNTAction<T1, T2, T3, T4, T5, T6, T7, T8>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,T6 arg6,T7 arg7 ,T8 arg8);
    public delegate void TNTAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,T6 arg6 ,T7 arg7 ,T8 arg8 ,T9 arg9);
    public delegate void TNTAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);
    public delegate void TNTAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);
    public delegate void TNTAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12);

    public delegate TOut TNTFunc<T1, T2, T3, T4, T5, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public delegate TOut TNTFunc<T1, T2, T3, T4, T5, T6, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    public delegate TOut TNTFunc<T1, T2, T3, T4, T5, T6, T7, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    public delegate TOut TNTFunc<T1, T2, T3, T4, T5, T6, T7, T8, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
    public delegate TOut TNTFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);
    public delegate TOut TNTFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);
    public delegate TOut TNTFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);
    public delegate TOut TNTFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12);

}
