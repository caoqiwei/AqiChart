using AqiChartServer.DB;
using AqiChartServer.DB.Business;
using AqiChartServer.DB.Interface;
using AqiChartServer.WebApi.Helper;
using AqiChartServer.WebApi.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog;
using System.Text;

#region MyRegion

var builder = WebApplication.CreateBuilder(args);

// 配置 Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentUserName()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        shared: true)
    .CreateLogger();

try
{


    builder.Host.UseSerilog(); // 使用 Serilog

    //SqlSugarHelper.ConnectionString = builder.Configuration.GetConnectionString("ConnectionStrings");
    SqlSugarHelper.ConnectionString = builder.Configuration["ConnectionStrings:conn"];


    // 添加 JWT 服务
    var jwtKey = builder.Configuration["Jwt:Key"];
    var jwtIssuer = builder.Configuration["Jwt:Issuer"];
    var jwtAudience = builder.Configuration["Jwt:Audience"];
    builder.Services.AddSingleton(new JwtService(jwtKey, jwtIssuer, jwtAudience));

    // 配置 JWT 认证
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey))
            };
        });

    // 添加控制器
    builder.Services.AddControllers(options =>
    {
        //options.Filters.Add(typeof(ModelValidateActionFilterAttribute));
        options.Filters.Add(typeof(MyResultMiddleWare));
    })
    .AddNewtonsoftJson(options =>
    {
        //忽略循环引用
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        //options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
    });

    // 添加业务服务
    builder.Services.AddScoped<IUserBiz, UserBiz>();
    builder.Services.AddScoped<IPrivateChatBiz, PrivateChatBiz>();
    builder.Services.AddScoped<IFriendshipsBiz, FriendshipsBiz>();

    // 添加 SignalR
    builder.Services.AddSignalR();

    // 强制JSON格式
    builder.Services.AddMvc().AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNamingPolicy = null;
        opt.JsonSerializerOptions.WriteIndented = true;
    });


    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    var app = builder.Build();

    app.UseSerilogRequestLogging(); // 自动记录HTTP请求

    // 配置中间件
    //if (app.Environment.IsDevelopment())
    //{
    //    app.UseDeveloperExceptionPage();
    //}
    //app.UseHttpsRedirection();

    app.UseRouting();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<ExceptionFilter>();//自定义异常处理

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseStaticFiles();
    app.UseEndpoints(endpoints =>
    {
        _ = endpoints.MapControllers();
        _ = endpoints.MapHub<ChatHub>("/chatHub");
    });

    //app.MapControllers();
    //app.MapHub<ChatHub>("/chatHub");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

#endregion
