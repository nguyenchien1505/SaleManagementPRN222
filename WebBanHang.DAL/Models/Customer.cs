using System;
using System.Collections.Generic;

namespace WebBanHang.DAL.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public int UserId { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User User { get; set; } = null!;
}
