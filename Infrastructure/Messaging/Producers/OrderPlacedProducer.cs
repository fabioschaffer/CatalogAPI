using Application.Interfaces;
using Domain.Events;
using MassTransit;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Messaging.Producers
{
    public class OrderPlacedProducer(ISendEndpointProvider sendEndpointProvider, IConfiguration cfg) : IOrderPlacedProducer
    {
        public async Task OrderPlacedPublish(OrderPlacedEvent evento)
        {
            var endpoint = await sendEndpointProvider.GetSendEndpoint(
                    new Uri($"queue:{cfg["RabbitMQ:Queues:FCG_Payment"]}"));            

                await endpoint.Send(evento);            
        }

    }
}
