using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Converters.SystemTextJson.MessageConverters;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using HyperLiquid.Net.Objects.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Clients.MessageHandlers
{
    internal class HyperLiquidRestMessageHandler : JsonRestMessageHandler
    {
        private readonly ErrorMapping _errorMapping;

        public override JsonSerializerOptions Options { get; } = HyperLiquidExchange._serializerContext;

        public HyperLiquidRestMessageHandler(ErrorMapping errorMapping)
        {
            _errorMapping = errorMapping;
        }

        public override async ValueTask<Error?> CheckForErrorResponse(RequestDefinition request, object? state, HttpResponseHeaders responseHeaders, Stream responseStream)
        {
            var (parseError, document) = await GetJsonDocument(responseStream, state).ConfigureAwait(false);
            if (parseError != null)
                return parseError;

            if (document!.RootElement.ValueKind != JsonValueKind.Object)
                return null;

            var status = document!.RootElement.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : null;
            if (status?.Equals("err") == true)
            {
                var errorCode = document!.RootElement.TryGetProperty("response", out var responseProp) ? responseProp.GetString() : null;
                return new ServerError(errorCode!, new ErrorInfo(ErrorType.Unknown, errorCode!));

            }

            return null;
        }

        public override Error? CheckDeserializedResponse<T>(HttpResponseHeaders responseHeaders, T result)
        {
            if (result is not HyperLiquidResponse hyperResponse)
                return null;

            if (hyperResponse.Status.Equals("ok", StringComparison.Ordinal))
                return null;

            return new ServerError(ErrorInfo.Unknown with { Message = hyperResponse.Status });
        }

        public override ValueTask<Error> ParseErrorResponse(int httpStatusCode, object? state, HttpResponseHeaders responseHeaders, Stream responseStream)
        {
            return new ValueTask<Error>(new ServerError(ErrorInfo.Unknown));
        }
    }
}
