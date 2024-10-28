using Microsoft.EntityFrameworkCore;
using Novibet.Application.Interfaces;
using Novibet.Application.Services;
using Novibet.Models;
using Novibet.Repositories.Interface;
using Novibet.Repositories.Interfaces;
using Novibet.Repositories.Repositories;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.Configure<CacheConfiguration>(builder.Configuration.GetSection("CacheConfiguration"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IIpApplication, IpApplication>();
builder.Services.AddScoped<IIpService, IpService>();
builder.Services.AddScoped<IIpRepository, IpRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IUpdateIpJobService, UpdateIpJobService>();

builder.Services.AddHttpClient<IpApplication>();

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    var jobKey = new JobKey("UpdateIpJobService");

    q.AddJob<Novibet.Application.Services.UpdateIpJobApplication>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("UpdateIpJobServiceTrigger")
        .WithSimpleSchedule(x => x            
            .WithIntervalInHours(1)
            .RepeatForever()));
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
