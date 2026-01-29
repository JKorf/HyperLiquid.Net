using System.Text.Json.Serialization;
using HyperLiquid.Net.Enums;

namespace HyperLiquid.Net.Objects.Models;

/// <summary>
/// Trigger condition
/// </summary>
public record HyperLiquidTriggerCondition
{
    /// <summary>
    /// Whether the Trigger is currently Active.
    /// True if has Direction + Price.
    /// False if no Direction + Price, or is Triggered.
    /// </summary>
    public bool Active { get; set; }
    /// <summary>
    /// Whether the Trigger has fired, Price and Direction are unavailable.
    /// </summary>
    public bool Triggered { get; set; }
    /// <summary>
    /// Direction of the Trigger, only available when Active == true
    /// </summary>
    public TriggerDirection? Direction { get; set; }
    /// <summary>
    /// Price of the Trigger, only available when Active == true
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Default ctor
    /// </summary>
    [JsonConstructor]
    public HyperLiquidTriggerCondition() { }

    /// <summary>
    /// Ctor for an Active Trigger Condition
    /// </summary>
    /// <param name="direction">Whether the price must be below or above the specified price</param>
    /// <param name="price">The price of the trigger</param>
    public HyperLiquidTriggerCondition(TriggerDirection direction, decimal price)
    {
        Active = true;
        Triggered = false;
        Direction = direction;
        Price = price;
    }
}