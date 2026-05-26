using MAMONT.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages()
    .AddMvcOptions(options =>
    {
        var messages = options.ModelBindingMessageProvider;
        messages.SetAttemptedValueIsInvalidAccessor((value, fieldName) => $"Значение \"{value}\" некорректно для поля {fieldName}.");
        messages.SetMissingBindRequiredValueAccessor(fieldName => $"Поле {fieldName} обязательно для заполнения.");
        messages.SetMissingKeyOrValueAccessor(() => "Не указано значение.");
        messages.SetMissingRequestBodyRequiredValueAccessor(() => "Тело запроса не должно быть пустым.");
        messages.SetNonPropertyAttemptedValueIsInvalidAccessor(value => $"Значение \"{value}\" некорректно.");
        messages.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Указано некорректное значение.");
        messages.SetNonPropertyValueMustBeANumberAccessor(() => "Значение должно быть числом.");
        messages.SetUnknownValueIsInvalidAccessor(fieldName => $"Указано некорректное значение для поля {fieldName}.");
        messages.SetValueIsInvalidAccessor(value => $"Значение \"{value}\" некорректно.");
        messages.SetValueMustBeANumberAccessor(fieldName => $"Поле {fieldName} должно быть числом.");
        messages.SetValueMustNotBeNullAccessor(value => "Это поле обязательно для заполнения.");
    });
builder.Services.AddSession();

builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("NeonDb");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Connection string 'NeonDb' is not configured.");
    }

    return new NpgsqlDataSourceBuilder(connectionString).Build();
});
builder.Services.AddSingleton<RepairRequestService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    if (context.Request.Method == "POST" && context.Request.ContentLength > 20_000)
    {
        context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
        await context.Response.WriteAsync("Слишком большой запрос. Уменьшите количество текста в форме.");
        return;
    }

    await next();
});

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

await app.Services.GetRequiredService<RepairRequestService>().CreateTableAsync();

app.Run();
