using CryptoExchange.Net.Objects.Errors;

namespace HyperLiquid.Net
{
    internal static class HyperLiquidErrors
    {
        internal static ErrorMapping Errors { get; } = new ErrorMapping([
                            
            ],
            [
                new ErrorEvaluator("Order", (code, msg) => {
                    if (!(msg?.Length > 0))
                        return new ErrorInfo(ErrorType.Unknown, false, "Unknown error", code);

                    if (msg.StartsWith("Order must have minimum value of"))
                        return new ErrorInfo(ErrorType.InvalidQuantity, false, "Order value too low", code);

                    if (msg.StartsWith("Order has invalid size"))
                        return new ErrorInfo(ErrorType.InvalidQuantity, false, "Order quantity invalid", code);

                    if (msg.StartsWith("Price must be divisible by tick size"))
                        return new ErrorInfo(ErrorType.InvalidPrice, false, "Price decimal places invalid", code);

                    if (msg.Equals("Order has invalid price."))
                        return new ErrorInfo(ErrorType.InvalidPrice, false, "Price invalid", code);

                    if (msg.StartsWith("Insufficient margin to place order"))
                        return new ErrorInfo(ErrorType.InsufficientBalance, false, "Insufficient margin", code);

                    if (msg.StartsWith("Insufficient spot balance"))
                        return new ErrorInfo(ErrorType.InsufficientBalance, false, "Insufficient balance", code);

                    if (msg.StartsWith("Post only order would have immediately matched"))
                        return new ErrorInfo(ErrorType.RejectedOrderConfiguration, false, "PostOnly order would have filled immediately", code);

                    if (msg.StartsWith("Order could not immediately match against any resting orders"))
                        return new ErrorInfo(ErrorType.RejectedOrderConfiguration, false, "ImmediateOrCancel order could not fill immediately", code);

                    if (msg.StartsWith("Invalid TP/SL price"))
                        return new ErrorInfo(ErrorType.InvalidStopParameters, false, "Invalid TP/SL price", code);

                    if (msg.StartsWith("No liquidity available for market order"))
                        return new ErrorInfo(ErrorType.RejectedOrderConfiguration, false, "No liquidity available for market order", code);

                    if (msg.StartsWith("Order price cannot be more than "))
                        return new ErrorInfo(ErrorType.InvalidPrice, false, "Order price deviates too far from current price", code);
                    
                    if (msg.StartsWith("Order would increase open interest while open interest is capped") ||
                        msg.StartsWith("Order would cause position to exceed margin tier limit at current leverage"))
                        return new ErrorInfo(ErrorType.MaxPosition, false, "Max position size exceeded", code);

                    return new ErrorInfo(ErrorType.Unknown, false, msg, code);
                }),
                new ErrorEvaluator("Subscription", (code, msg) => {
                    if (!(msg?.Length > 0))
                        return new ErrorInfo(ErrorType.Unknown, false, "Unknown error", code);

                    if (msg.StartsWith("Invalid subscription "))
                        return new ErrorInfo(ErrorType.UnknownSymbol, false, "Invalid symbol", code);

                    return new ErrorInfo(ErrorType.Unknown, false, msg, code);
                })
                ]
            );

    }
}