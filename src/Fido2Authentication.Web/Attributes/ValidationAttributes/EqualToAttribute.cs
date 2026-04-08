using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Fido2Authentication.Web.Attributes.ValidationAttributes;

public class EqualToAttribute(string otherValue) : ValidationAttribute//, IClientModelValidator
{
    private readonly string _otherValue = otherValue;

    // public void AddValidation(ClientModelValidationContext context)
    // {
    //     MergeAttribute(context.Attributes, "data-val", "true");
    //     MergeAttribute(context.Attributes, "data-val-equaltocustom", "error!");

    //     MergeAttribute(context.Attributes, "data-val-equaltocustom-othervalue", "123");
    // }

    protected override ValidationResult IsValid(
        object? value,
        ValidationContext validationContext)
    {
        var property = validationContext.ObjectType.GetProperty(_otherValue);
        
        if (property == null)
        {
            return new ValidationResult(
                string.Format("Unknown property: {0}", _otherValue)
            );
        }

        var otherValue = property.GetValue(validationContext.ObjectInstance, null);

        if (!object.Equals(value, otherValue))
        {
            return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
        }

        return ValidationResult.Success!;
    }

    private static bool MergeAttribute(
        IDictionary<string, string> attributes,
        string key,
        string value)
    {
        if (attributes.ContainsKey(key))
        {
            return false;
        }

        attributes.Add(key, value);
        return true;
    }
}