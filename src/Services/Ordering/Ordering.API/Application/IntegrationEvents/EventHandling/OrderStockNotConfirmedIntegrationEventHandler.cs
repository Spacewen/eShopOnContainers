﻿using System.Linq;
using Ordering.API.Application.IntegrationCommands.Commands;

namespace Ordering.API.Application.IntegrationEvents.EventHandling
{
    using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions;
    using System;
    using System.Threading.Tasks;
    using Events;
    using Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate;
    using Domain.Exceptions;

    public class OrderStockNotConfirmedIntegrationEventHandler : IIntegrationEventHandler<OrderStockNotConfirmedIntegrationEvent>
    {
        private readonly IOrderRepository _orderRepository;

        public OrderStockNotConfirmedIntegrationEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task Handle(OrderStockNotConfirmedIntegrationEvent @event)
        {
            var orderToUpdate = await _orderRepository.GetAsync(@event.OrderId);
            CheckValidSagaId(orderToUpdate);

            var orderStockNotConfirmedItems = @event.OrderStockItems
                .FindAll(c => !c.Confirmed)
                .Select(c => c.ProductId);

            orderToUpdate.SetStockConfirmedStatus(orderStockNotConfirmedItems);
        }

        private void CheckValidSagaId(Order orderSaga)
        {
            if (orderSaga is null)
            {
                throw new OrderingDomainException("Not able to process order saga event. Reason: no valid orderId");
            }
        }
    }
}