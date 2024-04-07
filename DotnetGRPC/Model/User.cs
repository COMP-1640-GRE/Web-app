using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DotnetGRPC.Model
{
    [Table("user")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("username")]
        public string Username { get; set; }

        [Required]
        [Column("password")]
        public string Password { get; set; }

        [Required]
        [Column("secret")]
        public string Secret { get; set; }

        [Required]
        [Column("role")]
        public string Role { get; set; }

        // public enum RoleType
        // {
        //     Administrator, Student, Guest
        // }

        [Required]
        [Column("email")]
        public string Email { get; set; }

        [Column("first_name")]
        public string? FirstName { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }

        [Required]
        [Column("account_status")]
        public string AccountStatus { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [ForeignKey("FacultyId")]
        public Faculty Faculty { get; set; }

        [Column("faculty_id")]
        public long? FacultyId { get; set; }
    }
}
