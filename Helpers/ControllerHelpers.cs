using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace crewbackend.Helpers
{
    public static class ControllerHelpers
    {
        // Chat GPT suggested this method to be used 
        // public static IActionResult ToBadRequestIfInvalid(this ControllerBase controller)
        // {
        //     if (controller.ModelState.IsValid)
        //         return null;

        //     var errors = controller.ModelState
        //         .Where(x => x.Value != null && x.Value.Errors.Count > 0)
        //         .ToDictionary(
        //             kvp => kvp.Key,
        //             kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
        //         );

        //     return new BadRequestObjectResult(new { errors });
        // }

        public static IActionResult HandleModelStateErrors(ModelStateDictionary modelState)
        {
            if (!modelState.IsValid)
            {
                var errors = modelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    );

                return new BadRequestObjectResult(new { errors });
            }

            return null!;
        }
    }
}