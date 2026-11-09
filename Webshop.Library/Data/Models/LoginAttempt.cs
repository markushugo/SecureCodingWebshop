using System;
using System.ComponentModel.DataAnnotations;

namespace Webshop.Data.Models
{
    public class LoginAttempt
    {
        [Key]
        public int Id { get; set; }

        public string UserName { get; set; }

        public DateTime AttemptTime { get; set; }

        public bool Success { get; set; }
    }
}