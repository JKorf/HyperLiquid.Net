using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperLiquid.Net
{
    /// <summary>
    /// HyperLiquid credentials
    /// </summary>
    public class HyperLiquidCredential : CredentialSet
    {
        /// <summary>
        /// The wallet connecting to HyperLiquid
        /// </summary>
        public EthKey Wallet { get; }
        /// <summary>
        /// Optionally approved signer
        /// </summary>
        public EthKey? Signer { get; }

        /// <summary>
        /// Create new HyperLiquid credentials
        /// </summary>
        /// <param name="wallet">The wallet to connect to HyperLiquid</param>
        /// <param name="signer">Optional signing key</param>
        public HyperLiquidCredential(EthKey wallet, EthKey? signer = null) : base(wallet.PublicAddress)
        {
            Wallet = wallet;
            Signer = signer;
        }

        /// <inheritdoc />
        public override ApiCredentials Copy() => new HyperLiquidCredential(Wallet, Signer);

        /// <inheritdoc />
        public override void Validate()
        {
            if (Signer != null && string.IsNullOrEmpty(Signer.PrivateKey))
                throw new ArgumentException($"Private key required on signer if signer is set", nameof(Signer));

            if (Signer == null && string.IsNullOrEmpty(Wallet.PrivateKey))
                throw new ArgumentException($"Private wallet key not set on {GetType().Name}", nameof(Wallet));

            base.Validate();
        }
    }
}
