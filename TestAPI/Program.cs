namespace TestAPI
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //builder.Services.AddHttpClient(client =>
            //{
            //    client.BaseAddress = new Uri("https://dev.azure.com/");
            //});

            builder.Services.AddHealthChecks();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            // Enable health check endpoint at /health
            app.Use(async (ctx, next) =>
            {
                if (ctx.Request.Path == "/health")
                    Console.WriteLine("Health endpoint requested");
                await next();
            });
            app.MapHealthChecks("/health");

            app.MapControllers();

            app.Run();
        }
    }

    //public partial class Program { }
}
