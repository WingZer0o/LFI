namespace LFI 
{
    public static class MainStartup
    {
        public static void DisplayHeader()
        {
            Console.WriteLine("******LFI Scanner***********");
        }
        public static void DisplayUsage()
        {
            Console.WriteLine("Usage: dotnet run <hostname> <threads> <traversals> <wordlist>");
            Console.WriteLine("  <hostname>   : Target base URL or host (e.g., http://example.com)");
            Console.WriteLine("  <threads>    : Number of concurrent workers (e.g., 10)");
            Console.WriteLine("  <wordlist>   : Path to file containing candidate paths");
        }
        public static void DisplayFooter()
        {
            Console.WriteLine("****************************");
        }
    }
}
