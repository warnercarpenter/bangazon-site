using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace Bangazon.Models
{
    public class ProductLike
    {
        [Key]
        public int ProductLikeId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string ProductId { get; set; }

        public ApplicationUser User { get; set; }
        public Product Product { get; set; }

        [Required]
        public bool IsLiked { get; set; }
    }
}