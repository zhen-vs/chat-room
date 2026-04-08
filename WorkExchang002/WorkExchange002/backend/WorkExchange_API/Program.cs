using Elfie.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Diagnostics.Metrics;
using System.Security.Claims;
using System.Text;
using WorkExchange.Hubs;
using WorkExchange_API.Interface;
using WorkExchange_API.Middlewares;
using WorkExchange_API.Models;
using WorkExchange_API.Services;
using WorkExchange_API.Services.Baleen;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;


var builder = WebApplication.CreateBuilder(args);

// --- 1. 基本服務註冊 ---
builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//=====================================

//-------------------------------------
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WorkExchange_API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "1234567890"
    });
    //------------------------------------

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddDbContext<WorkExchangeDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=WorkExchangeDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False"));

builder.Services.AddSignalR();
builder.Services.AddHttpClient();
builder.Services.AddScoped<MailService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<IWorkerProfileService, WorkerMemberService>();
// 在 Program.cs 的 builder.Services 區塊加入這兩行：
builder.Services.AddMemoryCache(); // 啟用記憶體快取
builder.Services.AddScoped<IAiChatService, AiChatService>(); // 註冊你的 Service
// --- 2. 驗證服務配置 (修正重點：保持鏈式調用) ---
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    // 1. 預設使用 JWT 檢查 Token
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

    // 2. 關鍵修正：當驗證失敗時，直接回傳 401/403 (JWT 行為)，不要去跳轉 Google
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    // 3. 只有 Google 登入過程「內部」需要 Cookie
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // 讓它自動適應 http 或 https
    options.Cookie.SameSite = SameSiteMode.Lax; // 跨站跳轉必備


    options.Events.OnRedirectToLogin = context =>
    {
        // 如果是 API 請求，改回傳 401，不要 Redirect (302)
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        // 如果是 API 請求但權限不足 (Role不對)，改回傳 403
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }
        return Task.CompletedTask;
    };

})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        //IssuerSigningKey = new SymmetricSecurityKey(key),
        //RoleClaimType = "role",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
        ClockSkew = TimeSpan.FromMinutes(5)
    };

    // 讓 JWT 支援從 Cookie 讀取 Token (對應你的 Controller 寫法)
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            // 這行會直接印出：是過期了？密鑰不對？還是 Issuer 不對？
            Console.WriteLine("--- JWT 驗證失敗詳細資訊 ---");
            Console.WriteLine(context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("--- JWT 驗證成功！ ---");
            return Task.CompletedTask;
        },



        OnMessageReceived = context =>
        {
            // 從請求的 Cookies 中尋找 "AuthToken"
            var token = context.Request.Cookies["AuthToken"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
        //    context.Token = context.Request.Cookies["AuthToken"];
        //    return Task.CompletedTask;
        //},
        //OnAuthenticationFailed = context =>
        //{
        //    Console.WriteLine("JWT 驗證失敗: " + context.Exception.Message);
            return Task.CompletedTask;
        }
    };
})
.AddGoogle(options =>
{
    var googleConfig = builder.Configuration.GetSection("Authentication:Google");
    options.ClientId = googleConfig["ClientId"];
    options.ClientSecret = googleConfig["ClientSecret"];

    if (string.IsNullOrEmpty(options.ClientId) || string.IsNullOrEmpty(options.ClientSecret))
    {
        throw new Exception("Google ClientID 或 Secret 讀取失敗，請確認 appsettings.json 層級！");
    }
});


//註冊ChatService.cs
builder.Services.AddScoped<ChatService>();

builder.Services.AddScoped<BlacklistService>();


// 1. 註冊 CORS 服務
builder.Services.AddCors(options =>
{
    options.AddPolicy("MySpecificOrigin", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://dry-truth-32d8.victor900403.workers.dev") // 明確指定 Vue 的網址
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // 允許跨域傳輸 Cookie
    });
});

builder.Services.Configure<ECPaySettings>(builder.Configuration.GetSection("ECPay"));
builder.Services.AddScoped<CheckMacValue>();

var app = builder.Build();

// --- 4. 中間件順序 (順序非常重要) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//註冊ECPay service


//全開
//app.UseCors("AllowAll"); 

app.UseStaticFiles();

app.UseRouting(); // 1. 先啟動路由

app.UseCors("MySpecificOrigin");

app.UseMiddleware<ExceptionMiddleware>();

//app.UseHttpsRedirection();

// 3. 啟用驗證中間件 (注意順序：UseAuthentication 必須在 UseAuthorization 之前)
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/chathub");  //設定SignalR的程式進入點

app.MapControllers();

app.Run();
public partial class Program { }