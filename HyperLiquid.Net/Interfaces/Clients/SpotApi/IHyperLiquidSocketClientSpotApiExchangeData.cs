using CryptoExchange.Net.Objects;
using System;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects.Sockets;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Interfaces.Clients.BaseApi;

namespace HyperLiquid.Net.Interfaces.Clients.SpotApi
{
    /// <summary>
    /// HyperLiquid spot WebSocket exchange data endpoints and streams. Exchange data includes market data (tickers, order books, etc) and system status.
    /// </summary>
    /// <see cref="IHyperLiquidSocketClientApiExchangeData"/>
    public interface IHyperLiquidSocketClientSpotApiExchangeData : IHyperLiquidSocketClientApiExchangeData
    {
        /// <summary>
        /// Get exchange info
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/spot#retrieve-spot-metadata" /><br />
        /// Endpoint:<br />
        /// POST /info (type: spotMeta)
        /// </para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidSpotExchangeInfo>> GetExchangeInfoAsync(CancellationToken ct = default);

        /// <summary>
        /// Get exchange and ticker info
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/spot#retrieve-spot-asset-contexts" /><br />
        /// Endpoint:<br />
        /// POST /info (type: spotMetaAndAssetCtxs)
        /// </para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidExchangeInfoAndTickers>> GetExchangeInfoAndTickersAsync(CancellationToken ct = default);

        /// <summary>
        /// Get information on an asset
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/spot#retrieve-information-about-a-token" /><br />
        /// Endpoint:<br />
        /// POST /info (type: tokenDetails)
        /// </para>
        /// </summary>
        /// <param name="assetId">["<c>tokenId</c>"] The asset id</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidAssetInfo>> GetAssetInfoAsync(string assetId, CancellationToken ct = default);


        /// <summary>
        /// Get HIP-4 questions and outcomes info
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/spot#retrieve-outcome-metadata" /><br />
        /// Endpoint:<br />
        /// POST /info (type: outcomeMeta)
        /// </para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidQuestionsAndOutcomesInfo>> GetQuestionsAndOutcomesInfoAsync(CancellationToken ct = default);

        /// <summary>
        /// Get HIP-4 settled outcome info
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/spot#retrieve-information-about-a-settled-outcome" /><br />
        /// Endpoint:<br />
        /// POST /info (type: settledOutcome)
        /// </para>
        /// </summary>
        /// <param name="outcomeId">The outcome id</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidSettledOutcome>> GetSettledOutcomeAsync(long outcomeId, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to spot symbol updates
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: activeAssetCtx)
        /// </para>
        /// </summary>
        /// <param name="symbol">Symbol name, for example `HYPE/USDC`</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToSymbolUpdatesAsync(string symbol, Action<DataEvent<HyperLiquidTicker>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to HIP-4 outcome updates 
        /// </summary>
        /// <param name="onOutcomeCreateUpdate">Outcome created data handler</param>
        /// <param name="onOutcomeSettleUpdate">Outcome settled data handler</param>
        /// <param name="onQuestionUpdate">Question updated data handler</param>
        /// <param name="onQuestionSettleUpdate">Question settled data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToOutcomeInfoUpdatesAsync(
            Action<DataEvent<long>>? onOutcomeCreateUpdate = null,
            Action<DataEvent<HyperLiquidOutcomeInfo>>? onOutcomeSettleUpdate = null,
            Action<DataEvent<HyperLiquidQuestion>>? onQuestionUpdate = null,
            Action<DataEvent<long>>? onQuestionSettleUpdate = null,
            CancellationToken ct = default);
    }
}
