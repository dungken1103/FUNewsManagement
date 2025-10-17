using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Models;

public partial class Tag
{
    public int TagId { get; set; }

    [Required, StringLength(50)]
    public string? TagName { get; set; }

    [StringLength(400)]
    public string? Note { get; set; }

    public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();
}
