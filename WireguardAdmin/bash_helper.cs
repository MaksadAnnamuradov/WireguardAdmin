using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace WireguardAdmin
{
    public static class ShellHelper
    {
        public static Task<string> Bash(this string cmd, ILogger logger)
        {
            var source = new TaskCompletionSource<int>();
            var escapedArgs = cmd.Replace("\"", "\\\"");
            var output = "";
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };
            process.Exited += (sender, args) =>
            {
                logger.LogWarning(process.StandardError.ReadToEnd());
                logger.LogInformation(process.StandardOutput.ReadToEnd());

                if (process.ExitCode == 0)
                {
                    output = process.StandardOutput.ReadToEnd();
                }
                else
                {
                    source.SetException(new Exception($"Command `{cmd}` failed with exit code `{process.ExitCode}`"));
                }

                process.Dispose();
            };

            try
            {
                process.Start();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Command {} failed", cmd);
                source.SetException(e);
            }

            return Task.FromResult(output);
        }
    }
}
