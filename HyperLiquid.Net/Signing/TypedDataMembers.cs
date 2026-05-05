namespace HyperLiquid.Net.Signing
{
    internal class MemberDescription
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    internal class MemberValue
    {
        public string TypeName { get; set; } = string.Empty;
        public object? Value { get; set; }
    }
}
