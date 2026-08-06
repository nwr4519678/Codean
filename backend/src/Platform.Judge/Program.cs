using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Platform.Judge;

var builder = WebApplication.CreateBuilder(args);

// Add Platform.Judge Services and Background Worker
builder.Services.AddControllers();
builder.Services.AddPlatformJudge(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
