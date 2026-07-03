var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapPost("/api/users/auth/login", () => 
{
    var response = new
    {
        status = "Success",
        data = new
        {
            token_type = "Bearer",
            access_token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJkdW5nYmFua2luZyIsImV4cCI6NDA3Njk2MDAwMH0.AriFinDemoToken2026",
            refresh_token = "rfr_92jsK_AriFinPlatformSecureKey2026"
        }
    };

    return Results.Ok(response); // Trả về HTTP 200 OK kèm JSON body
});

// Endpoint chức năng để kiểm tra xem Token đã tự động ăn theo chưa
app.MapGet("/api/users/vouchers/redeem", (HttpContext context) =>
{
    // Đọc header Authorization gửi lên từ Postman
    if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
    {
        return Results.Json(new { error = "Missing Authorization Header" }, statusCode: 401);
    }

    return Results.Ok(new { 
        message = "Xác thực Token thành công!", 
        your_header = authHeader.ToString() 
    });
});

app.Run();
