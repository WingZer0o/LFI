

namespace LFI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MainStartup.DisplayHeader();

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true; // prevent abrupt termination while we clean up
            };

            ArgsValidationResult? validationArgs = null;
            try
            {
                validationArgs = ArgsValidator.ArgumentsValidation(args);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                MainStartup.DisplayUsage();
                MainStartup.DisplayFooter();
                Environment.ExitCode = 1;
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                MainStartup.DisplayFooter();
                Environment.ExitCode = 1;
                return;
            }

            // Scanner scaffold (no network I/O yet)
            try
            {
                Scanner.LfiScanner.Run(validationArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Scanner error: {ex.Message}");
                Environment.ExitCode = 1;
            }
            finally
            {
                MainStartup.DisplayFooter();
            }
        }
    }
}
