﻿using AutoMapper;
using Moq;
using User_Service.Logic;
using User_Service.Models.HelperFiles;
using User_Service.RabbitMq.Publishers;
using User_Service.UnitTests.MockedDals;

namespace User_Service.UnitTests.MockedLogics
{
    public class MockedUserLogic
    {
        public readonly UserLogic UserLogic;

        public MockedUserLogic()
        {
            var mockedUserDal = new MockedUserDal().Mock;
            var mockedUserProducer = new Mock<IUserPublisher>();
            UserLogic = new UserLogic(mockedUserDal, new Mapper(AutoMapperConfig.Config), mockedUserProducer.Object);
        }
    }
}