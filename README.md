# Fiontar.Localization

A small, opinionated library that extends [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/)'s native [globalisation and localisation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-2.1) functions.

The library was developed at [Fiontar & Scoil na Gaeilge](https://www.gaois.ie), Dublin City University, Ireland and reflects what we believe to be some optimal approaches to producing multilingual web applications.

## Version: 0.5.0-alpha

This is a **prerelease version** for testing purposes. Expect breaking changes and renamed API functions before we reach a 1.0 release.

## Features

Fiontar.Localization is a [Nuget library](https://www.nuget.org/packages/Fiontar.Localization/) that you can add to your project. Add `using Fiontar.Localization;` to your Startup.cs file and you can access the following out-of-the-box configuration:

- The application will handle requests to both `www.mymultilingualapp.com/en-GB/about/` and `www.mymultilingualapp.com/ga-IE/about/` (and to any other number of supported cultures)
- For each request the application will set the appropriate culture based on the request URL
- When the user visits the site homepage, `www.mymultilingualapp.com`, the application will use the following criteria (in order) to set the application and UI culture. The first criterion to be met will be used:
    1. The request URL contains a language code (e.g. `www.mymultilingualapp.com/ga-IE/`)
    2. The request contains a language cookie previously obtained from the website
    3. The user has specified a desired language in their browser (obtained via the HTTP `Accept-Language` header) that matches one of the application's supported cultures
    4. The default language specified in Startup.cs
- If the request URL contains a language code users will not be directed to another version of the page, regardless of their own language settings. This is good for SEO and means that the users gets the page they thought they were opening when they clicked the link. Users should be able to voluntarily switch languages via a dedicated language switcher in the UI.
- If the user attempts to access a page in an unsupported language (e.g. `www.mymultilingualapp.com/fr/`) this will throw a `CultureNotFoundException` within the application and they will be shown a 404 Not Found error page. (Note: you must configure a viable MVC or Razor Pages route to an error page and specify this within a `app.UseStatusCodePagesWithReExecute()` method in Startup.cs)
- The user's language preferences will be remembered on subsequent visits: if they last visited the Irish-language version of the websites and they have agreed to the website's privacy settings, they will be shown the Irish-language site the next time they visit the homepage.

A sample Startup.cs class is given below:

```c#
namespace MyMultilingualApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddRazorPagesOptions(options =>
                {
                    options.RootDirectory = "/Pages";
                    options.Conventions.AddPageRoute("/Index", route: "{lang?}/");
                    options.Conventions.AddPageRoute("/About", route: "{lang}/about/{slug?}/");
                    options.Conventions.AddPageRoute("/Error", route: "error/{0}");
                }
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix, options =>
                {
                    options.ResourcesPath = "Resources";
                })
                .AddDataAnnotationsLocalization();

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-GB"),
                    new CultureInfo("ga-IE")
                };
                options.DefaultRequestCulture = new RequestCulture(culture: "en-GB", uiCulture: "en-GB");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.RequestCultureProviders.Clear();
                options.RequestCultureProviders.Add(new RouteCultureProvider(options.SupportedCultures, options.DefaultRequestCulture));
            });

            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            {
                if (env.IsDevelopment())
                {
                    app.UseBrowserLink();
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Error");
                    app.UseStatusCodePagesWithReExecute("/Error/{0}");
                }

                app.UseHsts();
                app.UseHttpsRedirection();

                app.UseStaticFiles();

                var localizationOptions = app.ApplicationServices
                    .GetService<IOptions<RequestLocalizationOptions>>().Value;

                app.UseRequestCultureNotFoundExceptionPage();
                app.UseRequestCultureValidation();
                app.UseRequestLocalization(localizationOptions);
                app.UseLocalizationCookies();

                app.UseMvc();
            }
        }
    }
}
```

## Application homepage

Numerous approaches can be taken when localising a website's homepage. Some websites will evaluate a user's preferred language and redirect them to a new URL, such as moving from `example.com` to `example.com/es`. This can be problematic, however, due to the nature of HTTP redirects and how they interact with the browser:

- Some websites (for example, [mozilla.org](https://www.mozilla.org/)) examine the browser's language preferences and implement a 301 (permanent) redirect. This is fine if you are reasonably certain that users will mostly access the application in one language. However, many browser's indefinitely cache 301 redirects, meaning that even if the user later updates their browser settings—or they access the computer in a public location such as a school or a library— the browser will always take them to the first localised version of the site that was opened in that browser.
- Many other websites implement 302 redirects, likely for the reasons described above. However, this is both semantically incorrect (302 redirects defined pages that have "Moved Temporarily" ) and bad for SEO as web crawlers tend to penalise pages with 302 redirects.

Therefore, the default approach using the Fiontar.Localization library is not to redirect the user (i.e. the user stays on `example.com`) but the page UI and contents will be localized following criteria 2–4 described in Features above. We feel this gives the optimum results both in terms of SEO and user experience.

**Add option to require default culture redirect**

## Language tag choice

The library is agnostic as to which type of [IETF language tag](https://en.wikipedia.org/wiki/IETF_language_tag) you use in your URL to signify the target culture. The examples in this documentation use locale-type tags such as `ga-IE`, `en-GB`, `pt-BR` etc. Many applications prefer ISO two-letter language codes like `ga`, `en`, `fr`. You can specify either type of tag in the supported cultures variable of your `RequestLocalizationOptions` in Startup.cs.

If you do opt for two-letter language codes, for historical reasons or otherwise, it can often be helpful to store a set of locale tags that map to your language codes—for example, when supplying locale data in [Open Graph](http://ogp.me/) meta tags. Fiontar.Localization facilitates this by allowing you to configure the route culture options in the `Configure()` method of Startup.cs:

```c#
services.Configure<RouteCultureOptions>(options =>
{
    options.LanguageLocaleMap.Add("en", "en-GB");
    options.LanguageLocaleMap.Add("ga", "ga-IE");
});
```

The mapped tags can then be accessed elswhere in the application, and a convenience function is even provided to infer the correct locale:

```c#
@foreach (var culture in LocOptions.Value.SupportedUICultures) 
{
    if (culture.Name != CultureInfo.CurrentCulture.Name)
    {
        string lang = @culture.TwoLetterISOLanguageName;
        string locale = CultOptions.Value.InferLocaleFromLanguage(@lang);
        <link rel="alternate" hreflang="@lang" href="https://www.mymultilingualapp.com/(@lang)">
        <meta property="og:locale:alternate" content="@locale">
    }
}
```

## Options

The default settings Fiontar.Localization provides can be further configured in the following ways:

### Configure 'non-cultures'

While you might use the first parameter of your URL to denote your language code 90% of the time, there will be some situations where culture information is unimportant, for example `mymultilingualapp.com/api/v1.2/`. Out of the box, a URL such as this will cause a `CultureNotFoundException` to be thrown within the application. However, you can prevent this by adding any number of 'non-cultures' to your service configuration:

```c#
services.Configure<RouteCultureOptions>(options =>
{
    options.NonCultures.Add("api");
    options.LanguageLocaleMap.Add("en", "en-GB");
    options.LanguageLocaleMap.Add("ga", "ga-IE");
});
```

### Redirect language tags to locale tags

If you want to migrate from a URL schema that used two-letter language tags (such as `example.com/es`) to a schema that uses regional locales (such as `example.com/es-ES`) Fiontar.Localization includes a helpful redirection protocol that leverages .NET Core's native URL rewriting middleware and takes care of the hard work for you.

The `RedirectLanguageToLocale` object accepts a dictionary that maps the two-letter codes onto your regional locales. In fact, if you had previously created `LanguageLocaleMap` (see Language tag choice above) you can recycle this object here:

```c#
public void ConfigureServices(IServiceCollection services)
{
    ...

    services.Configure<RouteCultureOptions>(options =>
    {
        options.LanguageLocaleMap.Add("en", "en-GB");
        options.LanguageLocaleMap.Add("ga", "ga-IE");
    });

    ...
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    ...

    var routeCultureOptions = app.ApplicationServices
        .GetService<IOptions<RouteCultureOptions>>().Value;

    var rewriteOptions = new RewriteOptions()
        .Add(new RedirectLanguageToLocale(routeCultureOptions.LanguageLocaleMap));

    app.UseRewriter(rewriteOptions);

    ...

    app.UseMvc();
}
```

The `RedirectLanguageToLocale` object implements a 302 redirect by default, but you can specify a 301 or other redirect type in an optional argument.