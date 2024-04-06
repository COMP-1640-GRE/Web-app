using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetGRPC.Model
{
    [Table("template")]
    public class Template
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("template_code")]
        public string TemplateCode { get; set; }

        [Required]
        [Column("template_name")]
        public string TemplateName { get; set; }

        [Required]
        [Column("template_description")]
        public string TemplateDescription { get; set; }

        [Required]
        [Column("template_content")]
        public string TemplateContent { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}