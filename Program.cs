using MAMONT.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

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

var connectionString = builder.Configuration.GetConnectionString("NeonDb");

if (string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddSingleton<IRepairRequestService, JsonRepairRequestService>();
}
else
{
    builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).Build());
    builder.Services.AddSingleton<IRepairRequestService, RepairRequestService>();
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
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

await app.Services.GetRequiredService<IRepairRequestService>().CreateTableAsync();

app.Run();
