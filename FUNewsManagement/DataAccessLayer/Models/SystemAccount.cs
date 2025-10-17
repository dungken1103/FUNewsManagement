using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Models;

public partial class SystemAccount
{
    public short AccountId { get; set; }

    [Required, StringLength(100)]
    public string? AccountName { get; set; }

    [Required, EmailAddress, StringLength(70)]
    public string? AccountEmail { get; set; }

    public int? AccountRole { get; set; }

    [Required, StringLength(70)]
    public string? AccountPassword { get; set; }

    public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();
}
