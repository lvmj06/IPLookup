namespace IPLookUp.CIDR
{
    using System.Net;
    using System.Net.Sockets;

    public class Trie
    {
        private TrieNode root;

        public Trie()
        {
            root = new TrieNode();
        }

        public void Insert(string cidrRange)
        {
            string[] cidrParts = cidrRange.Split('/');
            string ipAddressString = cidrParts[0];
            int subnetMaskBits = int.Parse(cidrParts[1]);

            IPAddress networkAddress = IPAddress.Parse(ipAddressString);
            IPAddress subnetMask = GetSubnetMask(subnetMaskBits, networkAddress.AddressFamily);

            byte[] networkBytes = networkAddress.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            TrieNode node = root;

            for (int i = 0; i < networkBytes.Length; i++)
            {
                byte currentByte = networkBytes[i];
                byte maskByte = subnetMaskBytes[i];

                for (int j = 7; j >= 0; j--)
                {
                    bool isBitSet = (currentByte & (1 << j)) != 0;

                    if (isBitSet)
                    {
                        if (node.Right == null)
                        {
                            node.Right = new TrieNode();
                        }

                        node = node.Right;
                    }
                    else
                    {
                        if (node.Left == null)
                        {
                            node.Left = new TrieNode();
                        }

                        node = node.Left;
                    }
                }

                if (i == networkBytes.Length - 1)
                {
                    root.Ranges.Add(cidrRange);
                }
            }
        }

        public bool ContainsIPAddress(string ipAddress)
        {
            IPAddress ipToLookup = IPAddress.Parse(ipAddress);
            byte[] ipBytes = ipToLookup.GetAddressBytes();
            TrieNode node = this.root;

            foreach (byte b in ipBytes)
            {
                for (int i = 7; i >= 0; i--)
                {
                    bool isBitSet = (b & (1 << i)) != 0;

                    if (isBitSet)
                    {
                        if (node.Right != null)
                        {
                            node = node.Right;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (node.Left != null)
                        {
                            node = node.Left;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    if (node.Ranges.Count > 0)
                    {
                        foreach (string cidrRange in node.Ranges)
                        {
                            if (IsIPInCIDRRange(ipToLookup, cidrRange))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool IsIPInCIDRRange(IPAddress ipAddress, string cidrRange)
        {
            string[] cidrParts = cidrRange.Split('/');
            string ipAddressString = cidrParts[0];
            int subnetMaskBits = int.Parse(cidrParts[1]);

            IPAddress networkAddress = IPAddress.Parse(ipAddressString);
            IPAddress subnetMask = GetSubnetMask(subnetMaskBits, networkAddress.AddressFamily);

            byte[] networkBytes = networkAddress.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();
            byte[] ipBytes = ipAddress.GetAddressBytes();

            if (networkBytes.Length != ipBytes.Length)
            {
                return false;
            }

            for (int i = 0; i < networkBytes.Length; i++)
            {
                if ((networkBytes[i] & subnetMaskBytes[i]) != (ipBytes[i] & subnetMaskBytes[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private IPAddress GetNetworkAddress(IPAddress ipAddress, IPAddress subnetMask)
        {
            byte[] ipBytes = ipAddress.GetAddressBytes();
            byte[] maskBytes = subnetMask.GetAddressBytes();

            byte[] networkBytes = new byte[ipBytes.Length];

            for (int i = 0; i < ipBytes.Length; i++)
            {
                networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
            }

            return new IPAddress(networkBytes);
        }

        private IPAddress GetSubnetMask(int subnetMaskBits, AddressFamily addressFamily)
        {
            byte[] maskBytes = new byte[addressFamily == AddressFamily.InterNetworkV6 ? 16 : 4];

            for (int i = 0; i < maskBytes.Length; i++)
            {
                if (subnetMaskBits >= 8)
                {
                    maskBytes[i] = 255;
                    subnetMaskBits -= 8;
                }
                else if (subnetMaskBits > 0)
                {
                    maskBytes[i] = (byte)(255 - (Math.Pow(2, 8 - subnetMaskBits) - 1));
                    subnetMaskBits = 0;
                }
            }

            return new IPAddress(maskBytes);
        }
    }
}
