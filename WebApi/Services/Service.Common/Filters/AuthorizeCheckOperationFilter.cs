using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Service.Common.Filters
{
    //參考:https://ithelp.ithome.com.tw/articles/10299318
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        private readonly Microsoft.AspNetCore.Routing.EndpointDataSource _endpointDataSource;

        public AuthorizeCheckOperationFilter(Microsoft.AspNetCore.Routing.EndpointDataSource endpointDataSource)
        {
            _endpointDataSource = endpointDataSource;
        }
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            //取得所有controller內的action
            var Descriptor = _endpointDataSource.Endpoints.FirstOrDefault(x =>
                x.Metadata.GetMetadata<ControllerActionDescriptor>() == context.ApiDescription.ActionDescriptor);
            //取得包含Authorize的Attribute
            var Authorize = Descriptor.Metadata.GetMetadata<AuthorizeAttribute>() != null;
            //取得包含AllowAnonymous的Attribute
            var AllowAnonymous = Descriptor.Metadata.GetMetadata<AllowAnonymousAttribute>() != null;
            //如果不需要鎖頭則return回去
            if (!Authorize || AllowAnonymous)
                return;
            //需要鎖頭則在swagger-UI中定義出來
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new()
                {
                    [
                        new OpenApiSecuritySchemeReference("Bearer", null, null)
                    ] = new List<string>()
                }
            };
        }
    }
}

