using System.Collections.Generic;

namespace HyperLiquid.Net
{
    internal class TypedDataRaw 
    {
        public IDictionary<string, MemberDescription[]> Types { get; set; }

        public string PrimaryType { get; set; }

        public MemberValue[] Message { get; set; }

        public MemberValue[] DomainRawValues { get; set; }
    }
}