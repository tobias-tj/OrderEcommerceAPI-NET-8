﻿using System.ComponentModel.DataAnnotations;

namespace OrderAPI.Application.DTOs
{
    public record AppUserDTO(
          int Id,
          [Required] string Name,
          [Required] string TelephoneNumber,
          [Required] string Address,
          [Required, EmailAddress] string Email,
          [Required] string Password,
          [Required] string Role
        );
}
 