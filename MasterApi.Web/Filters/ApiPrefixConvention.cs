using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace MasterApi.Web.Filters
{
    public class ApiPrefixConvention : IApplicationModelConvention
    {
        private readonly AttributeRouteModel _apiPrefix;
        private readonly AttributeRouteModel _apiCulturePrefix;
        private readonly AttributeRouteModel _apiRouteWithController;
        private readonly AttributeRouteModel _apiCultureRouteWithController;

        public ApiPrefixConvention()
        {
            //These are meant to be combined with existing route attributes
            _apiPrefix = new AttributeRouteModel(
                new RouteAttribute("api/"));
            _apiCulturePrefix = new AttributeRouteModel(
                new RouteAttribute("api/{language:regex(^[[a-z]]{{2}}(?:-[[A-Z]]{{2}})?$)}/"));

            //These are meant to be added as routes for api controllers that do not specify any route attribute
            _apiRouteWithController = new AttributeRouteModel(
                new RouteAttribute("api/[controller]"));
            _apiCultureRouteWithController = new AttributeRouteModel(
                new RouteAttribute("api/{language:regex(^[[a-z]]{{2}}(?:-[[A-Z]]{{2}})?$)}/[controller]"));
        }

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers.Where(c => c.ControllerName.EndsWith("Api")))
            {
                ApplyControllerConvention(controller);
            }
        }

        private void ApplyControllerConvention(ControllerModel controller)
        {
            //Remove the "Api" suffix from the controller name 
            //The "Controller" suffix is already removed by default conventions
            controller.ControllerName =
                controller.ControllerName.Substring(0, controller.ControllerName.Length - 3);

            //Either update existing route attributes or add new ones
            if (controller.Selectors.Any(x => x.AttributeRouteModel != null))
            {
                AddPrefixesToExistingRoutes(controller);
            }
            else
            {
                AddNewRoutes(controller);
            }
        }

        private void AddPrefixesToExistingRoutes(ControllerModel controller)
        {
            foreach (var selectorModel in controller.Selectors.Where(x => x.AttributeRouteModel != null).ToList())
            {
                var originalAttributeRoute = selectorModel.AttributeRouteModel;
                //Merge controller selector with the api prefix
                selectorModel.AttributeRouteModel =
                    AttributeRouteModel.CombineAttributeRouteModel(_apiPrefix,
                        originalAttributeRoute);

                //Add another selector with the culture api prefix
                var cultureSelector = new SelectorModel(selectorModel)
                {
                    AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(_apiCulturePrefix,
                        originalAttributeRoute)
                };
                controller.Selectors.Add(cultureSelector);
            }
        }

        private void AddNewRoutes(ControllerModel controller)
        {
            //The controller has no route attributes, lets add a default api convention 
            var defaultSelector = controller.Selectors.First(s => s.AttributeRouteModel == null);
            defaultSelector.AttributeRouteModel = _apiRouteWithController;
            //Lets add another selector for the api with culture convention
            controller.Selectors.Add(
                new SelectorModel { AttributeRouteModel = _apiCultureRouteWithController });
        }
    }

}