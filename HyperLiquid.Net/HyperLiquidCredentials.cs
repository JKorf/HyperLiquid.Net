using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Web3;
using System;

namespace HyperLiquid.Net
{
    /// <summary>
    /// HyperLiquid API credentials
    /// </summary>
    public class HyperLiquidCredentials : ApiCredentials
    {
        /// <summary>
        /// API credentials
        /// </summary>
        public HyperLiquidCredential Credential { get; set; }

        /// <summary>
        /// Create new credentials
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public HyperLiquidCredentials() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.


        /// <summary>
        /// Create new credentials
        /// </summary>
        /// <param name="credential">Credentials</param>
        public HyperLiquidCredentials(HyperLiquidCredential credential)
        {
            Credential = credential ?? throw new ArgumentNullException(nameof(credential));
        }

        /// <summary>
        /// Specify the HyperLiquid credentials
        /// </summary>
        /// <param name="wallet">The wallet to connect to HyperLiquid</param>
        /// <param name="signer">Optional signing key</param>
        public HyperLiquidCredentials With(EthKey wallet, EthKey? signer = null)
        {
            if (Credential != null) throw new InvalidOperationException("Credentials already set");

            Credential = new HyperLiquidCredential(wallet, signer);
            return this;
        }

        /// <inheritdoc />
        public override ApiCredentials Copy() => 
            new HyperLiquidCredentials(new HyperLiquidCredential(Credential.Wallet, Credential.Signer));

        /// <inheritdoc />
        public override void Validate()
        {
            if (Credential == null)
                throw new ArgumentException("Credential not set");

            Credential.Validate();
        }
    }
}
