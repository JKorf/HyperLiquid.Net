using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Objects;

namespace HyperLiquid.Net
{
    /// <summary>
    /// HyperLiquid environments
    /// </summary>
    public class HyperLiquidEnvironment : TradeEnvironment
    {
        /// <summary>
        /// Rest API address
        /// </summary>
        public string RestClientAddress { get; }

        /// <summary>
        /// Socket API address
        /// </summary>
        public string SocketClientAddress { get; }

        internal HyperLiquidEnvironment(
            string name,
            string restAddress,
            string streamAddress) :
            base(name)
        {
            RestClientAddress = restAddress;
            SocketClientAddress = streamAddress;
        }

        /// <summary>
        /// ctor for DI, use <see cref="CreateCustom"/> for creating a custom environment
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public HyperLiquidEnvironment() : base(TradeEnvironmentNames.Live)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        { }

        /// <summary>
        /// Get the HyperLiquid environment by name
        /// </summary>
        public static HyperLiquidEnvironment? GetEnvironmentByName(string? name)
         => name switch
         {
             TradeEnvironmentNames.Live => Live,
             TradeEnvironmentNames.Testnet => Testnet,
             "" => Live,
             null => Live,
             _ => default
         };

        /// <summary>
        /// Available environment names
        /// </summary>
        /// <returns></returns>
        public static string[] All => [Live.Name, Testnet.Name];

        /// <summary>
        /// Live environment
        /// </summary>
        public static HyperLiquidEnvironment Live { get; }
            = new HyperLiquidEnvironment(TradeEnvironmentNames.Live,
                                     HyperLiquidApiAddresses.Default.RestClientAddress,
                                     HyperLiquidApiAddresses.Default.SocketClientAddress);

        /// <summary>
        /// Testnet environment
        /// </summary>
        public static HyperLiquidEnvironment Testnet { get; }
            = new HyperLiquidEnvironment(TradeEnvironmentNames.Testnet,
                                     HyperLiquidApiAddresses.Testnet.RestClientAddress,
                                     HyperLiquidApiAddresses.Testnet.SocketClientAddress);

        /// <summary>
        /// Create a custom environment
        /// </summary>
        /// <param name="name"></param>
        /// <param name="spotRestAddress"></param>
        /// <param name="spotSocketStreamsAddress"></param>
        /// <returns></returns>
        public static HyperLiquidEnvironment CreateCustom(
                        string name,
                        string spotRestAddress,
                        string spotSocketStreamsAddress)
            => new HyperLiquidEnvironment(name, spotRestAddress, spotSocketStreamsAddress);
    }
}
