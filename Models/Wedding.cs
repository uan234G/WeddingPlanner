using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Models
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if ((DateTime)value <= DateTime.Now)
                return new ValidationResult("Date Incorrect, please enter a future date");
            return ValidationResult.Success;
        }
    }
    public class Wedding
    {
        [Key]
        public int WeddingId { get; set; }
        [Required]
        public string WedderOne { get; set; }
        [Required]
        public string WedderTwo { get; set; }
        [Required]
        [FutureDate(ErrorMessage = "Date Incorrect, please enter a future date")]
        public DateTime Date { get; set; }
        [Required]
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        // Foreign Key to Wedding Creator
        public int UserId { get; set; }
        public User Creator { get; set; }
        // association (Many to Many)
        public List<Association> Associations { get; set; }
    }
}