﻿namespace Event_Service.Models.HelperFiles
{
    public static class RabbitMqQueues
    {
        public static readonly string FindUserQueue = "find_user_queue";
        public static readonly string ExistsEventQueue = "exists_event_queue";
        public static readonly string FindDatepickerQueue = "find_datepicker_queue";
    }
}