namespace IPLookUp
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            // Declare variables.
            var urlCheck = false;
            string ipRangesLocation = string.Empty;
            string ipsForLookUp = string.Empty;

            // Get the file ocation of IP ranges.
            while (urlCheck == false)
            {
                Console.Write("Location of IP ranges (CIDR):");
                ipRangesLocation = Console.ReadLine() ?? string.Empty;

                urlCheck = File.Exists(ipRangesLocation);
            }

            // Get the file location of IPs to be looked up.
            urlCheck = false;
            while (urlCheck == false)
            {
                Console.Write("Location of the IP address to be looked up:");
                ipsForLookUp = Console.ReadLine() ?? string.Empty;

                urlCheck = File.Exists(ipsForLookUp);
            }

            var timer = new Stopwatch();

            Console.WriteLine("Reading files...");
            timer.Start();

            // Get IP Ranges/CIDR separated by new line.
            string[] cidrRanges = await File.ReadAllLinesAsync(ipRangesLocation);

            // Get IPs to be looked up.
            string[] ipAddressesToLookup = await File.ReadAllLinesAsync(ipsForLookUp);

            timer.Stop();
            Console.WriteLine($"Files loaded in {timer.ElapsedMilliseconds} ms.");

            Console.WriteLine($"Running lookup in {ipAddressesToLookup.Count()} IP(s).");
            timer.Restart();

            foreach (string ipAddress in ipAddressesToLookup)
            {
                var matched = false;
                foreach (string cidrNotation in cidrRanges)
                {
                    if (IsIpInCidrRange(ipAddress, cidrNotation))
                    {
                        Console.WriteLine($"IP Address: {ipAddress} matches the CIDR notation: {cidrNotation}");
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    Console.WriteLine($"IP Address: {ipAddress} did not match a CIDR notation.");
                }
            }

            timer.Stop();
            Console.WriteLine($"Look up completed in {timer.ElapsedMilliseconds} ms.");
            Console.ReadLine();
        }

        public static bool IsIpInCidrRange(string ipAddress, string cidrNotation)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);

            string[] parts = cidrNotation.Split('/');
            IPAddress networkAddress = IPAddress.Parse(parts[0]);
            int prefixLength = int.Parse(parts[1]);

            byte[] ipBytes = ip.GetAddressBytes();
            byte[] networkBytes = networkAddress.GetAddressBytes();

            if (ipBytes.Length != networkBytes.Length)
            {
                return false;
            }

            int numBits = ipBytes.Length * 8;

            if (prefixLength > numBits)
            {
                return false;
            }

            int numBytesToCompare = prefixLength / 8;
            int numBitsRemaining = prefixLength % 8;

            bool isEqual = true;

            for (int i = 0; i < numBytesToCompare; i++)
            {
                if (ipBytes[i] != networkBytes[i])
                {
                    isEqual = false;
                    break;
                }
            }

            if (isEqual)
            {
                if (numBitsRemaining > 0)
                {
                    byte mask = (byte)(0xFF << (8 - numBitsRemaining));

                    if ((ipBytes[numBytesToCompare] & mask) != (networkBytes[numBytesToCompare] & mask))
                    {
                        isEqual = false;
                    }
                }
            }

            return isEqual;
        }
    }
}