using WebApplication1.Domain.Shipping;

namespace WebApplication1.Models;

/// <summary>
/// Row of <c>order_shipments</c>: the parcel a shop handed to a carrier for an order outside the rider area.
/// Written once by <see cref="Services.Orders.ShipmentService"/>; never edited.
/// </summary>
public sealed class OrderShipment
{
    public int OrderId { get; set; }

    public Carrier Carrier { get; set; }

    /// <summary>Carrier name typed by the shop when <see cref="Carrier"/> is <see cref="Carrier.Other"/>.</summary>
    public string? OtherCarrierName { get; set; }

    public string TrackingNumber { get; set; } = string.Empty;

    public int ShippedBy { get; set; }

    /// <summary>UTC.</summary>
    public DateTime ShippedAt { get; set; }

    /// <summary>The stored values as the validated domain value (carrier display name, normalized number).</summary>
    public ShipmentDetails Details => ShipmentDetails.FromStored(Carrier, OtherCarrierName, TrackingNumber);
}
