using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MongoDelta.AspNetCore3
{
    /// <summary>
    /// Contains extension methods that can be used in the startup of an ASP.NET Core app
    /// </summary>
    public static class StartupExtensions
    {
        /// <summary>
        /// Registers middleware that makes a fresh instance of the unit of work available for each request
        /// </summary>
        /// <param name="app">The application builder in the Startup class</param>
        /// <typeparam name="TUnitOfWork">The type of unit of work to provide</typeparam>
        /// <returns>The application builder</returns>
        public static IApplicationBuilder UseUnitOfWork<TUnitOfWork>(this IApplicationBuilder app) where TUnitOfWork : UnitOfWorkBase
        {
            return app.UseMiddleware<UnitOfWorkMiddleware<TUnitOfWork>>();
        }

        /// <summary>
        /// Registers the unit of work with the DI container. This requires the UseUnitOfWork method to be called on the IApplicationBuilder
        /// </summary>
        /// <param name="services">The services collection for the DI container</param>
        /// <typeparam name="TInterface">The interface that will be used to inject the unit of work</typeparam>
        /// <typeparam name="TUnitOfWork">The concrete type that is being used by the middleware</typeparam>
        /// <returns>The service collection</returns>
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
