﻿using System;
using System.Collections.Generic;

namespace Datepicker_Service.Models.ToFrontend
{
    public class DatepickerViewmodel
    {
        public Guid Uuid { get; set; } = Guid.NewGuid();
        public bool CanBeRemoved { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime Expires { get; set; }
        public List<DatePickerDate> Dates { get; set; } = new List<DatePickerDate>();
    }
}