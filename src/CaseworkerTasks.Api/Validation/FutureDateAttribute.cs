using System.ComponentModel.DataAnnotations;

namespace CaseworkerTasks.Api.Validation;

public class FutureDateAttribute : ValidationAttribute
{
    public FutureDateAttribute()
    {
        ErrorMessage = "Due date must be in the future";
    }

    public override bool IsValid(object? value)
    {
        // Null values are valid (optional due date)
        if (value is null)
            return true;

        if (value is DateTime dateTime)
        {
            // Allow dates that are at least now or in the future
            return dateTime > DateTime.UtcNow;
        }

        return false;
    }
}