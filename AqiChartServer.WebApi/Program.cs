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

// ���� Serilog
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


    builder.Host.UseSerilog(); // ʹ�� Serilog

    //SqlSugarHelper.ConnectionString = builder.Configuration.GetConnectionString("ConnectionStrings");
    SqlSugarHelper.ConnectionString = builder.Configuration["ConnectionStrings:conn"];


    // ���� JWT ����
    var jwtKey = builder.Configuration["Jwt:Key"];
    var jwtIssuer = builder.Configuration["Jwt:Issuer"];
    var jwtAudience = builder.Configuration["Jwt:Audience"];
    builder.Services.AddSingleton(new JwtService(jwtKey, jwtIssuer, jwtAudience));

    // ���� JWT ��֤
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

    // ���ӿ�����
    builder.Services.AddControllers(options =>
    {
        //options.Filters.Add(typeof(ModelValidateActionFilterAttribute));
        options.Filters.Add(typeof(MyResultMiddleWare));
    })
    .AddNewtonsoftJson(options =>
    {
        //����ѭ������
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        //options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
    });

    // ����ҵ�����
    builder.Services.AddScoped<IUserBiz, UserBiz>();
    builder.Services.AddScoped<IPrivateChatBiz, PrivateChatBiz>();
    builder.Services.AddScoped<IFriendshipsBiz, FriendshipsBiz>();

    // ���� SignalR
    builder.Services.AddSignalR();

    // ǿ��JSON��ʽ
    builder.Services.AddMvc().AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNamingPolicy = null;
        opt.JsonSerializerOptions.WriteIndented = true;
    });


    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    var app = builder.Build();

    app.UseSerilogRequestLogging(); // �Զ���¼HTTP����

    // �����м��
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

    app.UseMiddleware<ExceptionFilter>();//�Զ����쳣����

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
