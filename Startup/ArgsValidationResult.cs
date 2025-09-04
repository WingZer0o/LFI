namespace LFI
{
    public class ArgsValidationResult
    {
        public string AddressToAttack { get; set; }
        public int NumberOfThreads { get; set; }
        public int NumberOfTraversals { get; set; }
        public string WordlistPath { get; set; }

        public ArgsValidationResult(string addressToAttack, int numberOfThreads, int numberOfTraversals, string wordlistPath)
        {
            AddressToAttack = addressToAttack;
            NumberOfThreads = numberOfThreads;
            NumberOfTraversals = numberOfTraversals;
            WordlistPath = wordlistPath;
        }
    }
}