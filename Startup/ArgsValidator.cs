using System;

namespace LFI
{
    public static class ArgsValidator
    {
        public static ArgsValidationResult ArgumentsValidation(string[] args)
        {
            if (args == null || args.Length < 3)
            {
                throw new ArgumentException("Invalid arguments. Expecting 3 arguments.");
            }

            if (string.IsNullOrWhiteSpace(args[0]))
            {
                throw new ArgumentException("Please provide a hostname.");
            }
            string addressToAttack = args[0];

            if (!int.TryParse(args[1], out int numberOfThreads))
            {
                throw new ArgumentException("Please provide a valid integer for threads.");
            }
            if (numberOfThreads <= 0)
            {
                throw new ArgumentException("Threads must be greater than 0.");
            }

            if (string.IsNullOrWhiteSpace(args[2]))
            {
                throw new ArgumentException("Please provide a path to the wordlist.");
            }
            if (!File.Exists(args[2]))
            {
                throw new ArgumentException("Please provide a wordlist file that exists.");
            }
            string wordlistPath = args[2];

            return new ArgsValidationResult(addressToAttack, numberOfThreads, wordlistPath);
        }
    }
}
