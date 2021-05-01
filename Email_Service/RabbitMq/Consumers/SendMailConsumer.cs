﻿using System;
using System.Collections.Generic;
using System.Text;
using Email_Service.Logic;
using Email_Service.Models.Helpers;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Email_Service.RabbitMq.Consumers
{
    public class SendMailConsumer : IConsumer
    {
        private readonly IModel _channel;
        private readonly EmailLogic _emailLogic;
        private readonly LogLogic _logLogic;

        public SendMailConsumer(IServiceProvider serviceProvider, IModel channel)
        {
            _channel = channel;
            using var scope = serviceProvider.CreateScope();
            _emailLogic = scope.ServiceProvider.GetRequiredService<EmailLogic>();
            _logLogic = scope.ServiceProvider.GetRequiredService<LogLogic>();
        }

        /// <summary>
        /// This method listens for email messages on the message queue and sends an email if it receives a message
        /// </summary>
        public void Consume()
        {
            _channel.ExchangeDeclare(RabbitMqExchange.MailExchange, ExchangeType.Direct);
            _channel.QueueDeclare(RabbitMqQueues.MailQueue, true, false, false, null);
            _channel.QueueBind(RabbitMqQueues.MailQueue, RabbitMqExchange.MailExchange, RabbitMqRouting.SendMail);
            _channel.BasicQos(0, 10, false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (sender, e) =>
            {
                try
                {
                    byte[] body = e.Body.ToArray();
                    string json = Encoding.UTF8.GetString(body);
                    var emails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Email>>(json);

                    _emailLogic.SendMails(emails);
                }
                catch (Exception exception)
                {
                    _logLogic.Log(exception);
                }
            };

            _channel.BasicConsume(RabbitMqQueues.MailQueue, true, consumer);
        }
    }
}