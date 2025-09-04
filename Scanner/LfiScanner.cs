using System.Collections.Concurrent;

namespace LFI.Scanner
{
    public static class LfiScanner
    {
        public static void Run(ArgsValidationResult args)
        {
            Console.WriteLine($"Target      : {args.AddressToAttack}");
            Console.WriteLine($"Threads     : {args.NumberOfThreads}");
            Console.WriteLine($"Traversals  : {args.NumberOfTraversals}");
            Console.WriteLine($"Wordlist    : {args.WordlistPath}");

            if (!File.Exists(args.WordlistPath))
            {
                throw new FileNotFoundException("Wordlist not found.", args.WordlistPath);
            }

            // Read wordlist lazily
            var candidates = File.ReadLines(args.WordlistPath)
                                  .Where(l => !string.IsNullOrWhiteSpace(l))
                                  .Select(l => l.Trim())
                                  .ToList();

            Console.WriteLine($"Candidates  : {candidates.Count}");

            // Build traversal prefix like ../../.. based on count
            string traversalPrefix = string.Concat(Enumerable.Repeat("../", args.NumberOfTraversals));

            Console.WriteLine("\n[Dry Run] Generating candidate paths (no network requests)\n");

            var options = new ParallelOptions { MaxDegreeOfParallelism = args.NumberOfThreads };

            // Use a thread-safe counter and print a small sample to avoid flooding the console
            int printed = 0;
            int sampleLimit = Math.Min(25, candidates.Count);
            long total = 0;

            Parallel.ForEach(candidates, options, candidate =>
            {
                Interlocked.Increment(ref total);
                string path = $"{traversalPrefix}{candidate.TrimStart('/') }";
                if (Interlocked.Increment(ref printed) <= sampleLimit)
                {
                    Console.WriteLine($"  -> {args.AddressToAttack} | {path}");
                }
            });

            Console.WriteLine($"\nPlanned requests (dry-run): {total}");
            if (total > sampleLimit)
            {
                Console.WriteLine($"Shown sample: {sampleLimit} of {total}");
            }

            Console.WriteLine("\nNext step: implement HTTP requests and findings reporting.");
        }
    }
}

