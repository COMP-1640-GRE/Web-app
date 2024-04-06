using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetGRPC.Model
{
    [Table("notification")]
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        [Column("user_id")]
        public long UserId { get; set; }


        [Required]
        [Column("with_email")]
        public bool WithEmail { get; set; }

        [Required]
        [Column("content")]
        public string Content { get; set; }

        [Column("url")]
        public string? Url { get; set; }

        [Required]
        [Column("seen")]
        public bool Seen { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}