using System;
using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Entities
{
    public class User
    {
        [Key]
        public Guid Gd { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public byte[] PasswordHash { get; set; }

        public byte[] PasswordSalt { get; set; }

        public bool IsActivated { get; set; }

        public string ActivationPin { get; set; }
    }
}