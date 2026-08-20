// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Models.Validators;

public class OptionalStringLengthAttribute(int maximumLength) : StringLengthAttribute(maximumLength)
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IOptional optional)
        {
            if (!optional.HasValue)
                return ValidationResult.Success;
            value = optional.BoxedValue;
        }

        return base.IsValid(value, validationContext);
    }
}