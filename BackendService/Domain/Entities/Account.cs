using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Account
{
    public int AccountId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public bool? IsActive { get; set; }

    public int RoleId { get; set; }

    public string? ImagePath { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<ConversationParticipant> ConversationParticipants { get; set; } = new List<ConversationParticipant>();

    public virtual ICollection<ConversationsBot> ConversationsBots { get; set; } = new List<ConversationsBot>();

    public virtual ICollection<CustomerDetail> CustomerDetails { get; set; } = new List<CustomerDetail>();

    public virtual ICollection<Dispatch> Dispatches { get; set; } = new List<Dispatch>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Import> Imports { get; set; } = new List<Import>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ReplyFeedback> ReplyFeedbacks { get; set; } = new List<ReplyFeedback>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<ShippingAddress> ShippingAddresses { get; set; } = new List<ShippingAddress>();

    public virtual ICollection<ShopManagerDetail> ShopManagerDetails { get; set; } = new List<ShopManagerDetail>();

    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();

    public virtual ICollection<StaffDetail> StaffDetails { get; set; } = new List<StaffDetail>();

    public virtual ICollection<WareHouseStockAudit> WareHouseStockAudits { get; set; } = new List<WareHouseStockAudit>();
}
