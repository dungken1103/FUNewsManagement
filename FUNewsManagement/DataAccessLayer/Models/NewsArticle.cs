using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Models;

public partial class NewsArticle
{
    public string NewsArticleId { get; set; } = null!;

    [Required, StringLength(400)]
    public string? NewsTitle { get; set; }

    [Required, StringLength(150)]
    public string Headline { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    [StringLength(4000)]
    public string? NewsContent { get; set; }

    [StringLength(400)]
    public string? NewsSource { get; set; }

    [Required]
    public short? CategoryId { get; set; }

    public bool? NewsStatus { get; set; }

    public short? CreatedById { get; set; }

    public short? UpdatedById { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual Category? Category { get; set; }

    public virtual SystemAccount? CreatedBy { get; set; }

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
