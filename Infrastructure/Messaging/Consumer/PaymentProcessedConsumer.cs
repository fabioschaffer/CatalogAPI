using Application.Interfaces;
using Domain.Events;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Messaging.Consumer
{
    public class PaymentProcessedConsumer(ILibrarySevice librarySevice) : IConsumer<PaymentProcessedEvent>
    {
        public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
        {
            var message = context.Message;

            //Gravar order se aprovada
            await librarySevice.OrderPaymentProcessed(message);
        }
    }
}
