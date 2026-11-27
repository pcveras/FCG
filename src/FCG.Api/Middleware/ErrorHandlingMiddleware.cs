using System.Diagnostics;
using FCG.Api.Domain;

namespace FCG.Api.Middleware;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await next(context);
        }
        catch (DomainException ex)
        {
            logger.LogWarning("Regra de negócio violada: {Message} ({Method} {Path})", ex.Message, Clean(context.Request.Method), Clean(context.Request.Path.Value));
            await Write(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro não tratado em {Method} {Path}", Clean(context.Request.Method), Clean(context.Request.Path.Value));
            await Write(context, StatusCodes.Status500InternalServerError, "Erro interno do servidor.");
        }
        finally
        {
            logger.LogInformation("{Method} {Path} => {StatusCode} em {ElapsedMs}ms",
                Clean(context.Request.Method), Clean(context.Request.Path.Value), context.Response.StatusCode, sw.ElapsedMilliseconds);
        }
    }

    private static string Clean(string? v) =>
        (v ?? string.Empty).Replace("\r", "").Replace("\n", "");

    private static Task Write(HttpContext ctx, int status, string message)
    {
        if (ctx.Response.HasStarted) return Task.CompletedTask;
        ctx.Response.Clear();
        ctx.Response.StatusCode = status;
        return ctx.Response.WriteAsJsonAsync(new { status, error = message });
    }
}
