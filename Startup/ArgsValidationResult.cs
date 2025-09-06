namespace LFI
{
    public class ArgsValidationResult
    {
        public string AddressToAttack { get; set; }
        public int NumberOfThreads { get; set; }
        public string WordlistPath { get; set; }

        public ArgsValidationResult(string addressToAttack, int numberOfThreads, string wordlistPath)
        {
            AddressToAttack = addressToAttack;
            NumberOfThreads = numberOfThreads;
            WordlistPath = wordlistPath;
        }
    }
}