﻿using System.ComponentModel.DataAnnotations;


namespace OrderAPI.Application.DTOs
{
    public record ProductDTO(
        int Id,
        [Required] string Name,
        [Required, Range(1, int.MaxValue)] int Quantity,
        [Required, DataType(DataType.Currency)] decimal Price
        );
}
