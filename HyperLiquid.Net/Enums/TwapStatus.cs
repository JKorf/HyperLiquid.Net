﻿using CryptoExchange.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace HyperLiquid.Net.Enums
{
    /// <summary>
    /// TWAP order status
    /// </summary>
    public enum TwapStatus
    {
        /// <summary>
        /// Activated
        /// </summary>
        [Map("activated")]
        Activated,
        /// <summary>
        /// Terminated
        /// </summary>
        [Map("terminated")]
        Terminated,
        /// <summary>
        /// Finished
        /// </summary>
        [Map("finished")]
        Finished,
        /// <summary>
        /// Error
        /// </summary>
        [Map("error")]
        Error
    }
}
