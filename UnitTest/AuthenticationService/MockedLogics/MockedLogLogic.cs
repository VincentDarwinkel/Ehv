﻿using Authentication_Service.Logic;
using Authentication_Service.RabbitMq.Publishers;
using Moq;

namespace UnitTest.AuthenticationService.MockedLogics
{
    public class MockedLogLogic
    {
        public readonly LogLogic LogLogic;

        public MockedLogLogic()
        {
            var mockedPublisher = new Mock<IPublisher>().Object;
            LogLogic = new LogLogic(mockedPublisher);
        }
    }
}