using KKIHUB.ContentSync.Web.Service;
using Microsoft.Extensions.DependencyInjection;

namespace KKIHUB.ContentSync.Web.Infrastructure.Dependency
{
    public class CoreDependencyRegistry : IDependency
    {
        public void Register(IServiceCollection services)
        {
            services.AddScoped<IContentService, ContentService>();
            services.AddScoped<IAcousticService, AcousticService>();

        }
    }
}
