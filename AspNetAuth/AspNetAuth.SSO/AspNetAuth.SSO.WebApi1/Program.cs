using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Security.Cryptography;

namespace AspNetAuth.SSO.WebApi1;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.Services.AddControllers();

        var jwtTokenOptions = new JwtTokenOptions();
        builder.Configuration.Bind("JwtToken", jwtTokenOptions);

        // 注册jwt验证, 请求头中需要带上Authorization: Bearer {token}
        // 注意：若使用minimal api，需指定使用的鉴权方案，比如.RequireAuthorization(new AuthorizeAttribute{ AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme });，否则不会使用，默认401不通过

        // 对称加密
        {
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,// 是否验证Issuer
                        ValidateAudience = true,// 是否验证Audience
                        ValidateLifetime = true,// 是否验证失效时间
                        ClockSkew = TimeSpan.Zero,// 过期后立即失效
                        ValidateIssuerSigningKey = true,// 是否验证SecurityKey
                        ValidIssuer = jwtTokenOptions.Issuer,
                        ValidAudience = jwtTokenOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtTokenOptions.SecurityKey))
                    };

                    // 自定义响应
                    //options.Events = new JwtBearerEvents()
                    //{
                    //    // Token解析失败（如过期、签名错误）
                    //    OnAuthenticationFailed = context =>
                    //    {
                    //        context.Response.StatusCode = 200;
                    //        context.Response.ContentType = "application/json";
                    //        if (context.Exception is SecurityTokenExpiredException)
                    //        {
                    //            return context.Response.WriteAsJsonAsync(new { code = 401, message = "Token已过期" });
                    //        }
                    //        if (context.Exception is SecurityTokenInvalidSignatureException)
                    //        {
                    //            return context.Response.WriteAsJsonAsync(new { code = 401, message = "Token签名无效" });
                    //        }

                    //        return context.Response.WriteAsJsonAsync(new { code = 401, message = "Token验证失败" });
                    //    },
                    //    // 请求未携带Token或Token无效
                    //    OnChallenge = context =>
                    //    {
                    //        context.HandleResponse();
                    //        if (!context.Response.HasStarted)
                    //        {
                    //            context.Response.StatusCode = 200;
                    //            context.Response.ContentType = "application/json";
                    //            return context.Response.WriteAsJsonAsync(new { code = 401, message = "请登录" });
                    //        }

                    //        return Task.CompletedTask;
                    //    },
                    //    // Token有效但权限不足
                    //    OnForbidden = context =>
                    //    {
                    //        context.Response.StatusCode = 200;
                    //        context.Response.ContentType = "application/json";
                    //        return context.Response.WriteAsJsonAsync(new { code = 403, message = "无权限访问" });
                    //    }
                    //};
                });
        }

        // 非对称加密
        //{
        //    var currentDir = Directory.GetCurrentDirectory();
        //    var parentDir = Directory.GetParent(currentDir)!.FullName;
        //    var publicKeyPem = File.ReadAllText(Path.Combine(parentDir, "AspNetAuth.SSO.AuthorizationServer", "public.pem"));
        //    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //        .AddJwtBearer(options =>
        //        {
        //            options.TokenValidationParameters = new TokenValidationParameters
        //            {
        //                ValidateIssuer = true,// 是否验证Issuer
        //                ValidateAudience = true,// 是否验证Audience
        //                ValidateLifetime = true,// 是否验证失效时间
        //                ClockSkew = TimeSpan.Zero,// 过期后立即失效
        //                ValidateIssuerSigningKey = true,// 是否验证SecurityKey
        //                ValidIssuer = jwtTokenOptions.Issuer,
        //                ValidAudience = jwtTokenOptions.Audience,
        //                IssuerSigningKey = new RsaSecurityKey(RsaKeyGenerator.ConvertStringToRSAParameters(publicKeyPem))
        //            };
        //        });
        //}

        var app = builder.Build();

        app.MapOpenApi();
        app.MapScalarApiReference();
        app.MapControllers();

        app.Run();
    }
}

public static class RsaKeyGenerator
{
    public static RSAParameters ConvertStringToRSAParameters(string publicKeyPem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        return rsa.ExportParameters(false);
    }
}