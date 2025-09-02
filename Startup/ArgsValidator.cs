using System;

namespace LFI
{
    public static class ArgsValidator
    {
        public static void ArgumentsValidation(string[] args)
        {
            if (args.Length == 0)
            {
                throw new Exception("Usage: dotnet run <hostname> <Num of Threads> <Num of Traversals> <Path to wordlist>");
            }
            if (string.IsNullOrEmpty(args[0]))
            {
                throw new Exception("Please provide a hostname");
            }
            int numberOfThreads = 0;
            if (!int.TryParse(args[1], out numberOfThreads))
            {
                throw new Exception("Please provide the number of threads you want to execute");
            }
            int numberOfTraversals = 0;
            if (string.IsNullOrEmpty(args[2]))
            {
                throw new Exception("Please provide a number of traversals");
            }
            if (string.IsNullOrEmpty(args[3]))
            {
                throw new Exception("Please provide a path to the wordlist");
            }
            if (!File.Exists(args[3])) {
                throw new Exception("Please provide a wordlist that exists");
            }

                string addressToAttack = args[0];
        }
    }
}