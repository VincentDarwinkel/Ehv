﻿using System;
using System.Data;
using System.Threading.Tasks;
using Event_Service.Logic;
using Event_Service.Models.HelperFiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Event_Service.Controllers
{
    [Route("event/step/user")]
    [ApiController]
    public class EventStepUserController : ControllerBase
    {
        private readonly EventStepUserLogic _eventStepUserLogic;
        private readonly ControllerHelper _controllerHelper;
        private readonly LogLogic _logLogic;

        public EventStepUserController(EventStepUserLogic eventStepUserLogic, ControllerHelper controllerHelper,
            LogLogic logLogic)
        {
            _eventStepUserLogic = eventStepUserLogic;
            _controllerHelper = controllerHelper;
            _logLogic = logLogic;
        }

        [HttpPost("{uuid}")]
        public async Task<ActionResult> Add(Guid uuid)
        {
            try
            {
                UserHelper requestingUser = _controllerHelper.GetRequestingUser(this);
                await _eventStepUserLogic.Add(uuid, requestingUser);
                return Ok();
            }
            catch (DuplicateNameException)
            {
                return StatusCode(StatusCodes.Status409Conflict);
            }
            catch (Exception e)
            {
                _logLogic.Log(e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete]
        public async Task<ActionResult> Remove(Guid uuid)
        {
            try
            {
                UserHelper requestingUser = _controllerHelper.GetRequestingUser(this);
                await _eventStepUserLogic.Remove(uuid, requestingUser);
                return Ok();
            }
            catch (Exception e)
            {
                _logLogic.Log(e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
