using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MongoDelta.AspNetCore3
{
    public static class StartupExtensions
    {
        public static IApplicationBuilder UseUnitOfWork<TUnitOfWork>(this IApplicationBuilder app) where TUnitOfWork : UnitOfWorkBase
        {
            return app.UseMiddleware<UnitOfWorkMiddleware<TUnitOfWork>>();
        }

        public static IServiceCollection AddUnitOfWork<TInterface, TUnitOfWork>(this IServiceCollection services) where TUnitOfWork : UnitOfWorkBase, TInterface where TInterface : class
        {
            return services
                .AddTransient(provider =>
                {
                    var contextAccessor = provider.GetService<IHttpContextAccessor>();
                    return (TInterface) contextAccessor.HttpContext.Items[UnitOfWorkMiddleware<TUnitOfWork>.ContextItemKey];
                })
                .AddTransient<TUnitOfWork>()
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }
    }
}
