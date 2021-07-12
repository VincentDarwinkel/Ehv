﻿using System;
using System.Diagnostics;

namespace File_Service.Models.HelperFiles
{
    public static class SystemHelper
    {
        public static void ExecuteOsCommand(string command)
        {
            var escapedArgs = command.Replace("\"", "\\\"");
            Console.WriteLine(escapedArgs);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            process.WaitForExit();
        }
    }
}
