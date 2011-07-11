﻿namespace SassAndCoffee.AspNet
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.IO;
    using System.Web;
    using SassAndCoffee.Core;
    using SassAndCoffee.Core.Caching;

    public class CompilableFileModule : IHttpModule, ICompilerHost
    {
        public ICompiledCache Cache { get; private set; }

        public IContentCompiler Compiler { get; private set; }

        public void Init(HttpApplication context)
        {
            var cacheType = ConfigurationManager.AppSettings["SassAndCoffee.Cache"];
            var cachePath = ConfigurationManager.AppSettings["SassAndCoffee.CachePath"];
            if (string.IsNullOrWhiteSpace(cachePath))
                cachePath = "~/App_Data/.sassandcoffeecache";

            Cache = CreateCache(cacheType, HttpContext.Current.Server.MapPath(cachePath));
            Compiler = new ContentCompiler(this);

            var handler = new CompilableFileHandler(this);
            context.PostResolveRequestCache += (o, e) => {
                var app = o as HttpApplication;

                if (Compiler.CanCompile(app.Request.PhysicalPath)) {
                    app.Context.RemapHandler(handler);
                }
            };
        }

        static ICompiledCache CreateCache(string cacheType, string cachePath)
        {
            if (string.Equals("NoCache", cacheType, StringComparison.InvariantCultureIgnoreCase))
                return new NoCache();

            if (string.Equals("InMemoryCache", cacheType, StringComparison.InvariantCultureIgnoreCase))
                return new InMemoryCache();

            if (!Directory.Exists(cachePath))
                Directory.CreateDirectory(cachePath);

            return new FileCache(cachePath);
        }

        public void Dispose()
        {
        }
    }
}
