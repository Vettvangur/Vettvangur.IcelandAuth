#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Vettvangur.IcelandAuth
{
    public static class ServiceCollectionExtensions
    {
        public static void AddIcelandAuth(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IcelandAuthService>();
        }
    }
}
#endif
