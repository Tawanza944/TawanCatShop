using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Domain.Orders;

namespace WebApplication1.Models
{
    /// <summary>
    /// โมเดลข้อมูลคำสั่งซื้อจริง (ตาราง orders ในฐานข้อมูล MySQL csi402db)
    /// บันทึกคำสั่งซื้อของสมาชิก พร้อมรายละเอียดผู้รับ ช่องทางการชำระเงิน และสถานะจัดส่ง
    /// </summary>
    public class Order
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// รหัสคำสั่งซื้อ เช่น CAT-20261113-5821
        /// </summary>
        [Required]
        [StringLength(64)]
        public string OrderNo { get; set; } = string.Empty;

        /// <summary>
        /// รหัสสมาชิกผู้สั่งซื้อ (เชื่อมกับ users.id)
        /// </summary>
        public int UserId { get; set; }

        [Required]
        [StringLength(255)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "PromptPay";

        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// โค้ดส่วนลดที่ใช้ในคำสั่งซื้อนี้ (ถ้ามี)
        /// </summary>
        [StringLength(50)]
        public string? CouponCode { get; set; }

        /// <summary>
        /// สถานะคำสั่งซื้อ เก็บใน DB เป็นรหัสตัวพิมพ์เล็ก (ดู OrderStatusCodes)
        /// </summary>
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        /// <summary>
        /// สถานะการหักค่าคอมมิชชั่น 10% โดยฝ่ายบัญชี
        /// </summary>
        public bool CommissionDeducted { get; set; } = false;

        public OrderShop? Marketplace { get; set; }

        public ShopPayout? ShopPayout { get; set; }

        /// <summary>Carrier and tracking number, once the shop has shipped a parcel order (null for rider orders).</summary>
        public OrderShipment? Shipment { get; set; }

        [StringLength(255)]
        public string? TrackingNote { get; set; }

        /// <summary>
        /// ไรเดอร์ที่รับงานนี้ (users.id) ว่างจนกว่าจะมีไรเดอร์กดรับ
        /// </summary>
        public int? RiderId { get; set; }

        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// รายการสินค้าในออเดอร์นี้
        /// </summary>
        public List<OrderItem> Items { get; set; } = new();

        // Helper properties สำหรับแสดงผลในหน้า View
        [NotMapped]
        public string StatusThai => Status.ToThai();

        [NotMapped]
        public string StatusBadgeClass => Status switch
        {
            OrderStatus.Pending => "bg-warning text-dark",
            OrderStatus.Packing or OrderStatus.Ready => "bg-info text-white",
            OrderStatus.Delivering => "bg-primary text-white",
            OrderStatus.Delivered => "bg-success text-white",
            OrderStatus.Cancelled => "bg-danger text-white",
            _ => "bg-secondary text-white"
        };

        [NotMapped]
        public int StepIndex => Status.ToStepIndex();
    }

    /// <summary>
    /// โมเดลข้อมูลรายการสินค้าในคำสั่งซื้อ (ตาราง order_items ใน MySQL)
    /// </summary>
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        [Required]
        [StringLength(255)]
        public string ProductName { get; set; } = string.Empty;

        public string? ProductImage { get; set; }

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; } = 1;

        public decimal TotalPrice { get; set; }

        // Navigation property
        public Order? Order { get; set; }
    }
}

