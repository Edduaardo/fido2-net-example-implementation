using Microsoft.AspNetCore.Mvc;

namespace Fido2Authentication.Web.ExtensionMethods;

public static class AlertExtension
{
    public static IActionResult SuccessMessage(
        this IActionResult result,
        string message,
        string title = "Success!")
        {
            return result.Alert("success", title, message);
        }

        public static IActionResult ErrorMessage(
            this IActionResult result,
            string message,
            string title = "Error")
        {
            return result.Alert("error", title, message);
        }

        public static IActionResult InformationMessage(
            this IActionResult result,
            string message,
            string title = "Information")
        {
            return result.Alert("information", title, message);
        }

        public static IActionResult AlertMessage(
            this IActionResult result,
            string message,
            string title = "Alert")
        {
            return result.Alert("warning", title, message);
        }

        private static IActionResult Alert(
            this IActionResult result,
            string messageType,
            string title,
            string message)
        {
            return new AlertExtensionDecorator(result, messageType, message, title);
        }
}
