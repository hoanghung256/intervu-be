using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Text.RegularExpressions;

namespace Intervu.API.Properties
{
    public class LowercaseControllerRouteConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            foreach (var selector in controller.Selectors)
            {
                if (selector.AttributeRouteModel != null)
                {
                    selector.AttributeRouteModel.Template =
                        Regex.Replace(selector.AttributeRouteModel.Template, @"\[controller\]", controller.ControllerName.ToLower());
                }
            }
        }
    }
}
