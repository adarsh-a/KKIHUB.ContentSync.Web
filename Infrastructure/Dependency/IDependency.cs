using Microsoft.Extensions.DependencyInjection;

namespace KKIHUB.ContentSync.Web.Infrastructure.Dependency
{
    public interface IDependency
    {
        void Register(IServiceCollection services);
    }
}
