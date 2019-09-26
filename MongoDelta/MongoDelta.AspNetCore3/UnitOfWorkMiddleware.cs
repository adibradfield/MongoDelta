using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MongoDelta.AspNetCore3
{
    [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
    internal class UnitOfWorkMiddleware<TUnitOfWork> where TUnitOfWork : UnitOfWorkBase
    {
        public static readonly object ContextItemKey = new object();

        private readonly RequestDelegate _next;

        public UnitOfWorkMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IServiceProvider serviceProvider)
        {
            var unitOfWork = serviceProvider.GetService<TUnitOfWork>();
            context.Items[ContextItemKey] = unitOfWork;

            await _next.Invoke(context);
        }
    }
}
