using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
sealed public class PublicKeyAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is not string key)
            return true;
        var parts = key.Trim().Split(' ', 3);
        return parts.Length >= 2;
    }
}
