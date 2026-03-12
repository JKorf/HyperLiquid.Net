using CryptoExchange.Net.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Objects
{
    public class HyperLiquidCredentials : ApiCredentials
    {
        public HyperLiquidCredentials(string apiKey, string secretKey)
            : this(new ECDSACredential(apiKey, secretKey)) { }

        public HyperLiquidCredentials(ECDSACredential credential)
            : base(credential) { }

        /// <inheritdoc />
        public override ApiCredentials Copy() => new HyperLiquidCredentials(GetCredential<ECDSACredential>()!);
    }
}
