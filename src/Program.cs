using Amazon.S3;
using ImageService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddDbContext<ImagesDbContext>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDefaultAWSOptions(
        builder.Configuration.GetAWSOptions());
}

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();