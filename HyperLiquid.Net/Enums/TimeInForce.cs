﻿using CryptoExchange.Net.Attributes;

namespace HyperLiquid.Net.Enums
{
    /// <summary>
    /// Time in force
    /// </summary>
    public enum TimeInForce
    {
        /// <summary>
        /// Post only
        /// </summary>
        [Map("Alo")]
        PostOnly,
        /// <summary>
        /// Immediate or cancel
        /// </summary>
        [Map("Ioc")]
        ImmediateOrCancel,
        /// <summary>
        /// Good till canceled
        /// </summary>
        [Map("Gtc")]
        GoodTillCanceled
    }
}
