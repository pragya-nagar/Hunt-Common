using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CorrelationId;
using Microsoft.AspNetCore.Http;
using Synergy.Common.Abstracts;

namespace Synergy.Common.AspNet
{
    public class OperationContextMiddleware
    {
        private readonly RequestDelegate _next;

        public OperationContextMiddleware(RequestDelegate next)
        {
            this._next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext context,
            IOperationContextFactory operationContextFactory,
            ICorrelationContextAccessor correlationContextAccessor)
        {
            if (context.User?.Identity is ClaimsIdentity identity)
            {
                var uName = context.User?.Identity?.Name;
                if (Guid.TryParse(identity.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)?.Value, out var uId) &&
                    Guid.TryParse(correlationContextAccessor.CorrelationContext.CorrelationId, out var correlationId))
                {
                    operationContextFactory.Create(new OperationContext(correlationId, uId, uName));
                }
            }

            await _next(context).ConfigureAwait(false);

            operationContextFactory.Dispose();
        }
    }
}