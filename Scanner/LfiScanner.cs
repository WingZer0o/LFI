using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace LFI.Scanner
{
    public static class LfiScanner
    {
        
        private static readonly Regex[] Signatures = new[]
        {
            // Linux /etc/passwd
            new Regex(@"root:x:\d+:\d+:", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            // Linux /etc/shadow
            new Regex(@"root:[*!x]?:[0-9]*:[0-9]*:[0-9]*:[0-9]*:[0-9]*:[0-9]*:[0-9]*", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            // Linux /etc/hosts
            new Regex(@"127\.0\.0\.1\s+localhost", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            // Windows INI files
            new Regex(@"\[fonts\]|\[extensions\]|\[drivers\]", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            // SSH private key
            new Regex(@"-----BEGIN [A-Z ]+ PRIVATE KEY-----", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            // Common error messages
            new Regex(@"No such file or directory|failed to open stream|Permission denied", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            // HTML (error pages)
            new Regex(@"<html|<body|<title>.*error.*</title>", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        };
        public static void Run(ArgsValidationResult args)
        {
            Console.WriteLine($"Target      : {args.AddressToAttack}");
            Console.WriteLine($"Threads     : {args.NumberOfThreads}");
            Console.WriteLine($"Wordlist    : {args.WordlistPath}");

            if (!File.Exists(args.WordlistPath))
            {
                throw new FileNotFoundException("Wordlist not found.", args.WordlistPath);
            }

            // Read wordlist lazily
            var candidates = File.ReadLines(args.WordlistPath)
                                  .Select(l => l.Trim())
                                  .ToList();

            Console.WriteLine($"Candidates  : {candidates.Count}");

            Console.WriteLine("\n[Live Run] Generating candidate paths and issuing HTTP requests\n");

            // Normalize target address - ensure scheme is present
            string baseAddress = args.AddressToAttack.Trim();
            if (!baseAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !baseAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                baseAddress = "http://" + baseAddress;
            }

            // Prepare HttpClient (shared instance)
            using var httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(10)
            };

            // Do a baseline probe with a guaranteed-nonexistent path to compare response sizes
            string baselinePath = $"nonexistent-{Guid.NewGuid()}.txt";
            Uri baselineUri = new Uri(new Uri(baseAddress), baselinePath);
            int baselineLength = 0;
            try
            {
                var baselineResp = httpClient.GetAsync(baselineUri).GetAwaiter().GetResult();
                var baselineBytes = baselineResp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                baselineLength = baselineBytes?.Length ?? 0;
                Console.WriteLine($"Baseline probe -> {baselineUri} [{(int)baselineResp.StatusCode}] len={baselineLength}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Baseline probe failed: {ex.Message}. Proceeding with baseline length=0");
                baselineLength = 0;
            }

            var options = new ParallelOptions { MaxDegreeOfParallelism = args.NumberOfThreads };

            // Thread-safe collection for findings
            var findings = new ConcurrentBag<string>();

            long total = 0;
            int sampleLimit = Math.Min(25, candidates.Count);
            int printed = 0;

            Parallel.ForEach(candidates, options, candidate =>
            {
                string targetUri;
                try
                {
                    Interlocked.Increment(ref total);
                    string relative = candidate.TrimStart('/');
                    targetUri = baseAddress + relative;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERR] Processing candidate '{candidate}' -> {ex.Message}");
                    return; // skip to next candidate
                }

                if (Interlocked.Increment(ref printed) <= sampleLimit)
                {
                    Console.WriteLine($"  -> {targetUri}");
                }

                try
                {
                    var resp = httpClient.GetAsync(targetUri).GetAwaiter().GetResult();
                    var content = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    int len = content?.Length ?? 0;

                    bool likelyInteresting = false;
                    string snippet = string.Empty;

                    if (len > baselineLength + 50 && resp.IsSuccessStatusCode)
                    {
                        likelyInteresting = true;
                    }

                    // check textual signatures (safely try to decode)
                    try
                    {
                        
                        if (content.Contains("root:x:0:0") || content.Contains("uid="))
                        {
                            likelyInteresting = true;
                            snippet = content.Length > 500 ? content.Substring(0, 500) : content;
                        }
                    }
                    catch { /* ignore decoding errors */ }

                    if (likelyInteresting)
                    {
                        findings.Add($"{targetUri} [{(int)resp.StatusCode}] len={len} content=${content}");
                        Console.WriteLine($"[FOUND] {targetUri} -> status={(int)resp.StatusCode} len={len} content=${content}");
                    }
                }
                catch (Exception ex)
                {
                    // Keep going on network errors
                    Console.WriteLine($"[ERR] {targetUri} -> {ex.Message}");
                }
            });

            Console.WriteLine($"\nPlanned requests: {total}");
            if (total > sampleLimit)
            {
                Console.WriteLine($"Shown sample: {sampleLimit} of {total}");
            }

            Console.WriteLine($"\nFindings: {findings.Count}");
            foreach (var f in findings)
            {
                Console.WriteLine(f);
            }

            // Optionally write findings to a file
            if (findings.Count > 0)
            {
                var outPath = Path.Combine(Directory.GetCurrentDirectory(), "findings.txt");
                File.WriteAllLines(outPath, findings);
                Console.WriteLine($"Wrote findings to: {outPath}");
            }
        }
    }
}

