using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Trackers.UserData;

namespace HyperLiquid.Net.Interfaces
{
    /// <summary>
    /// Tracker factory
    /// </summary>
    public interface IHyperLiquidTrackerFactory: ITrackerFactory
    {
        /// <summary>
        /// Create a new user data tracker
        /// </summary>
        /// <param name="config"></param>
        /// <param name="userIdentifier"></param>
        /// <param name="credentials"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        IUserSpotDataTracker CreateUserSpotDataTracker(string userIdentifier, UserDataTrackerConfig config, ApiCredentials credentials, HyperLiquidEnvironment? environment = null);
        IUserSpotDataTracker CreateUserSpotDataTracker(UserDataTrackerConfig config);
        IUserFuturesDataTracker CreateUserFuturesDataTracker(string userIdentifier, UserDataTrackerConfig config, ApiCredentials credentials, HyperLiquidEnvironment? environment = null);
        IUserFuturesDataTracker CreateUserFuturesDataTracker(UserDataTrackerConfig config);
    }
}
