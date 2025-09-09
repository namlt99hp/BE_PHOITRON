using BE_PHOITRON.Infrastructure.Configuration;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddInfrastructure(builder.Configuration);

// ====== Swagger ======
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

// ====== CORS ======
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("Default", p =>
        p.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod());
});

// ====== Controllers ======
builder.Services.AddControllers();



var app = builder.Build();

// ====== Middleware pipeline ======
app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();
app.UseCors("Default");

app.UseAuthorization(); // Không có Authentication nên chỉ check [Authorize] nếu bạn gắn

app.MapControllers();

app.Run();
