﻿using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Internal
{
    internal class HyperLiquidSocketUpdate<T>
    {
        [JsonPropertyName("channel")]
        public string Channel { get; set; } = string.Empty;
        [JsonPropertyName("data")]
        public T Data { get; set; } = default!;
    }
}
