﻿using System;
using System.Collections.Generic;
using User_Service.Enums;

namespace User_Service.Models.ToFrontend
{
    public class UserViewModel
    {
        public Guid Uuid { get; set; }
        public string Username { get; set; }
        public string About { get; set; }
        public string Email { get; set; }
        public Gender Gender { get; set; }
        public AccountRole AccountRole { get; set; }
        public DateTime BirthDate { get; set; }
        public bool ReceiveEmail { get; set; }
        public List<UserHobbyViewModel> Hobbies { get; set; }
        public List<FavoriteArtistViewModel> FavoriteArtists { get; set; }
    }
}