using System;
using SharpWnfDump.Handler;

namespace SharpWnfDump
{
    class SharpWnfDump
    {
        static void Main()
        {
            // This is a minimally modified version of the original SharpWnfDump by daem0nc0re specifically designed to print the value of WNF_SHEL_QUIET_MOMENT_SHELL_MODE_CHANGED
            // Aside from forcing those particular arguments and adding a 10 second delay to allow the user to get into the desired Do Not Disturb state and a pause at the end,
            // the only other change is if our OS version is too new to be supported by SharpWnfDump, we just assume the latest supported version is fine
            // In any case, WNF_SHEL_QUIET_MOMENT_SHELL_MODE_CHANGED has not changed since it was introduced, so this is almost certainly a safe assumption
            // And if it's not, then Do Not Disturb Fix wouldn't work anyways and we'd have to update everything at some point

            string[] args = new string[] { "-r", "WNF_SHEL_QUIET_MOMENT_SHELL_MODE_CHANGED" };

            Console.WriteLine("Do Not Disturb state will be read in 10 seconds...");
            System.Threading.Thread.Sleep(10000);

            var options = new CommandLineParser();

            try
            {
                options.SetTitle("SharpWnfDump - Diagnostics Tool for Windows Notification Facility");
                options.AddFlag(false, "h", "help", "Displays this help message.");
                options.AddFlag(false, "i", "info", "Displays given state name. Can use with -s, -r or -v option.");
                options.AddFlag(false, "d", "dump", "Displays information on all non-temporary state names. Can use with -s, -r or -v option.");
                options.AddFlag(false, "b", "brut", "Displays information on all temporary state names. Can use with -r or -v option.");
                options.AddFlag(false, "r", "read", "Reads the current data stored in the given state name.");
                options.AddFlag(false, "w", "write", "Writes data into the given state name.");
                options.AddFlag(false, "v", "value", "Dump the value of each name.");
                options.AddFlag(false, "s", "sid", "Show the security descriptor for each name.");
                options.AddFlag(false, "u", "used", "Show only used state name information. Use with -d or -b option.");
                options.AddArgument(false, "WNF_NAME", "WNF State Name. Use with -i, -r or -w option.");
                options.AddArgument(false, "FILE_NAME", "Data source file path. Use with -w option.");
                options.Parse(args);
                Execute.Run(options);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (ArgumentException ex)
            {
                options.GetHelp();
                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();
        }
    }
}
