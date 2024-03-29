﻿using Authentication_Service.Logic;
using Authentication_Service.Models.Dto;
using Authentication_Service.Models.HelperFiles;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Authentication_Service.RabbitMq.Consumers
{
    public class UpdateUserConsumer : IConsumer
    {
        private readonly IModel _channel;
        private readonly UserLogic _userLogic;
        private readonly LogLogic _logLogic;

        public UpdateUserConsumer(IServiceProvider serviceProvider, IModel channel)
        {
            _channel = channel;
            using var scope = serviceProvider.CreateScope();
            _userLogic = scope.ServiceProvider.GetRequiredService<UserLogic>();
            _logLogic = scope.ServiceProvider.GetRequiredService<LogLogic>();
        }

        public void Consume()
        {
            _channel.ExchangeDeclare(RabbitMqExchange.AuthenticationExchange, ExchangeType.Direct);
            _channel.QueueDeclare(RabbitMqQueues.UpdateUserQueue, true, false, false, null);
            _channel.QueueBind(RabbitMqQueues.UpdateUserQueue, RabbitMqExchange.AuthenticationExchange, RabbitMqRouting.UpdateUser);
            _channel.BasicQos(0, 10, false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (sender, e) =>
            {
                try
                {
                    byte[] body = e.Body.ToArray();
                    string json = Encoding.UTF8.GetString(body);
                    var user = Newtonsoft.Json.JsonConvert.DeserializeObject<UserDto>(json);

                    await _userLogic.Update(user);
                }
                catch (Exception exception)
                {
                    _logLogic.Log(exception);
                }
            };

            _channel.BasicConsume(RabbitMqQueues.UpdateUserQueue, true, consumer);
        }
    }
}