using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetGRPC.Model
{
    [Table("contribution")]
    public class Contribution
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Column("db_author_id")]
        public long DbAuthorId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("evaluation")]
        public string? Evaluation { get; set; }

        [Column("view_count")]
        public long ViewCount { get; set; }

        [Column("semester_id")]
        public long SemesterId { get; set; }

        [Column("selected")]
        public bool? Selected { get; set; }

        [Column("is_anonymous")]
        public bool? IsAnonymous { get; set; }

        [Column("status")]
        public string Status { get; set; }
    }
}