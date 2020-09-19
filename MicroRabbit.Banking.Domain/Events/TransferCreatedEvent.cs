using MicroRabbit.Domain.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroRabbit.Banking.Domain.Events
{
    public class TransferCreatedEvent:Event
    {
        public int From { get; set; }
        public int To { get; set; }
        public decimal Amout { get; set; }
        public TransferCreatedEvent(int from,int to,decimal amount)
        {
            From = from;
            To = to;
            Amout = amount;
        }
    }
}
