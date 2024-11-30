using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using SeaBattle.API.DependencyRegistration;
using SeaBattle.API.Extensions;
using SeaBattle.API.Hubs;
using SeaBattle.API.JWT;
using SeaBattle.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

Authentication.AddAuthentication(builder);

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<JwtMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PlayHub>("/play");

app.UseMiddleware<GlobalErrorHandlingMiddleware>();

app.Run();