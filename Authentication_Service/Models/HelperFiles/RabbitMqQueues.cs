﻿namespace Authentication_Service.Models.HelperFiles
{
    public static class RabbitMqQueues
    {
        public static readonly string FindUserQueue = "find_user_queue";
        public static readonly string AddUserQueue = "add_user_queue";
        public static readonly string UpdateUserQueue = "update_user_queue";
        public static readonly string DeleteUserQueue = "auth_delete_user_queue";
        public static readonly string DisabledExistsUserQueue = "exists_disabled_queue";
        public static readonly string ValidateUserPasswordQueue = "validate_password_user_queue";
    }
}