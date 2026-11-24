using WebApplication1.Domain.Orders;
using WebApplication1.Models;

namespace WebApplication1.ViewModels.Shared;

/// <summary>What a page should say about a parcel shipment.</summary>
public enum ShipmentDisplay
{
    /// <summary>The shop recorded a carrier and tracking number.</summary>
    Tracked,

    /// <summary>Parcel order that left the shop before tracking numbers existed (no row in order_shipments).</summary>
    LegacyWithoutTracking
}

/// <summary>
/// Shipment details prepared for the customer's order card and the accounting order list.
/// One decision for both pages, so they can never disagree about what to show.
/// </summary>
public sealed record ShipmentInfo(ShipmentDisplay Display, string? CarrierName, string? TrackingNumber, DateTime? ShippedAt)
{
    /// <summary>
    /// Returns null when there is nothing to show: rider orders, orders without shop data, cancelled orders
    /// and parcel orders the shop has not shipped yet.
    /// </summary>
    public static ShipmentInfo? For(bool? riderDelivery, OrderStatus status, OrderShipment? shipment)
    {
        if (riderDelivery is not false)
            return null;

        if (shipment is not null)
        {
            var details = shipment.Details;
            return new ShipmentInfo(ShipmentDisplay.Tracked, details.CarrierDisplayName, details.TrackingNumber, shipment.ShippedAt);
        }

        return status is OrderStatus.Delivering or OrderStatus.Delivered
            ? new ShipmentInfo(ShipmentDisplay.LegacyWithoutTracking, null, null, null)
            : null;
    }

    public static ShipmentInfo? For(Order order) => For(order.Marketplace?.RiderDelivery, order.Status, order.Shipment);
}
