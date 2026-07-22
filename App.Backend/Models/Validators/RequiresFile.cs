using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using App.Backend.Domain.Values.Misc;

namespace App.Backend.Models.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
sealed public class RequiresFile(string filename) : ValidationAttribute
{

    public override bool IsValid(object? value)
    {
        if (value is null)
            return false;
        if (value is not IEnumerable<CommitFile> files)
            return false;

        filename = filename.Trim();
        if (string.IsNullOrWhiteSpace(filename))
            return false;

        return files.Any(file =>
            string.Equals(Path.GetFileName(file.Path), filename, StringComparison.OrdinalIgnoreCase));
    }
}
