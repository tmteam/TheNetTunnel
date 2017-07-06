using System;

namespace TheTunnel
{
    public class InAttribute : Attribute
    {
        public InAttribute(Int16 Id)
        {
            if (Id == 0 || Id > 16383 || Id < -16383)
                throw new ArgumentException(
                    "Cord id shold be in the range [-16383:16383] and not equal 0. These numbers are reserved in technical purposes");
            this.CordId = Id;
        }

        public readonly Int16 CordId;
    }

    public class OutAttribute : Attribute
    {
        public OutAttribute(Int16 Id, UInt32 MaxAnswerAwaitInterval = 60000)
        {
            if (Id == 0 || Id > 16383 || Id < -16383)
                throw new ArgumentException(
                    "Cord id shold be in the range [-16383:16383] and not equal 0. These numbers are reserved in technical purposes");
            this.CordId = Id;
            this.MaxAnswerAwaitInterval = MaxAnswerAwaitInterval;
        }

        public readonly Int16 CordId;
        public readonly UInt32 MaxAnswerAwaitInterval;
    }
}

