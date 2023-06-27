namespace IPLookUp.CIDR
{
    public class TrieNode
    {
        public TrieNode? Left { get; set; } = null;
        public TrieNode? Right { get; set; } = null;
        public List<string> Ranges { get; set; } = new List<string>();
    }
}