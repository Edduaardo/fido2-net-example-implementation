using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Fido2Authentication.Web.ExtensionMethods;

public class AlertExtensionDecorator(
    IActionResult result,
    string messageType,
    string message,
    string title) : IActionResult
{
    private readonly IActionResult _result = result;
    private readonly string _messageType = messageType;
    private readonly string _message = message;
    private readonly string _title = title;

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var factory = context.HttpContext.RequestServices.GetService<ITempDataDictionaryFactory>();

        var tempData = factory!.GetTempData(context.HttpContext);
        tempData["_alert.type"] = _messageType;
        tempData["_alert.title"] = _title;
        tempData["_alert.message"] = _message;

        await _result.ExecuteResultAsync(context);
    }
}
