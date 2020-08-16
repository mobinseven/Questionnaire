﻿using BlazorBoilerplate.Server.Aop;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using System;

namespace BlazorBoilerplate.Server
{
    public static class AopServicesFactory
    {
        public static readonly ServiceProvider ServiceProvider;

        static AopServicesFactory()
        {
            ServiceProvider = new ServiceCollection()
                .AddLogging()
                .AddLocalization()
                .Configure<RequestLocalizationOptions>(options =>
                {
                    options.DefaultRequestCulture = new RequestCulture(Localization.Settings.SupportedCultures[0]);
                    options.AddSupportedCultures(Localization.Settings.SupportedCultures);
                    options.AddSupportedUICultures(Localization.Settings.SupportedCultures);
                })
                .AddTransient<ApiResponseExceptionAspect>()
                .AddSingleton<ILoggerFactory>(services => new SerilogLoggerFactory())
                .BuildServiceProvider();
        }

        public static object GetInstance(Type type) => ServiceProvider.GetRequiredService(type);
    }
}
