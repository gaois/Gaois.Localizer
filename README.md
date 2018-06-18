# Fiontar.Localization

[![NuGet](https://img.shields.io/badge/nuget-0.6.0--alpha-blue.svg)](https://www.nuget.org/packages/Fiontar.Localization/)

A toolkit for creating multilingual web applications on ASP.NET Core. It offers a suite of configurable localization middleware, including request culture validators, cookie management, exception handlers, and URL rewriting rules, that work alongside the framework's native [globalisation and localisation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-2.1) functions to take the pain out of building localised websites.

The library was developed at [Fiontar & Scoil na Gaeilge](https://www.gaois.ie), Dublin City University, Ireland and reflects our own experience producing multilingual web applications.

**Note:** This is a **prerelease version** for testing purposes. Expect some breaking changes and renamed API methods before we reach a 1.0 release.

- [What problem does it solve?](#what-problem-does-it-solve)
- [Features](#features)
- [Install and setup](#install-and-setup)
  - [Basic configuration](#basic-configuration)
- [Getting the request culture](#getting-the-request-culture)
  - [Configure the culture path parameter](#configure-the-culture-path-parameter)
  - [A word about SEO](#a-word-about-seo)
- [Unsupported cultures](#unsupported-cultures)
  - [Return a 404 Not Found status code](#return-a-404-not-found-status-code)
  - [Redirect the user to a page in the default culture](#redirect-the-user-to-a-page-in-the-default-culture)
    - [Configure the rerouter](#configure-the-rerouter)
- [Excluding routes](#excluding-routes)
- [Localisation cookies](#localisation-cookies)
  - [Configure the localisation cookies](#configure-the-localisation-cookies)
- [Landing page redirection](#landing-page-redirection)
  - [Another word about SEO](#another-word-about-seo)
- [Language tag choice](#language-tag-choice)
- [Migrating between language tags in a URL scheme](#migrating-between-language-tags-in-a-url-scheme)
- [Is there a sample application?](#is-there-a-sample-application)
- [Who is using this?](#who-is-using-this)
- [Credits](#credits)

## What problem does it solve?

The library complements native localisation tools like `IStringLocalizer`, `IHtmlLocalizer`, `IViewLocalizer`, resource files and the `CultureInfo` class. If you are not already familiar with these aspects of globalisation and localisation in ASP.NET Core you will benefit greatly from [reading up on them](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-2.1) first. Particularly, it will explain why we use terms like request *cultures* and target *cultures*, rather than just target languages, in this document. Fiontar.Localization is mainly concerned with how the target culture is inferred from the HTTP request, i.e. how do we know which form of localized contents to display to the user.

ASP.NET Core does offer a [number of tools](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-2.1#implement-a-strategy-to-select-the-languageculture-for-each-request) to this end but they are somewhat rigid, and do not account for a number of scenarios. Notably, and for various well-intentioned reasons, the `RouteDataRequestCultureProvider` does not work out-of-the-box and requires additional configuration if you want to ascertain the request culture from URL parameters like `/fr-FR/about/`. Furthermore, it is left to the developer to cater for scenarios where the user attempts to access a page in a culture that is not supported by the application and to implement solutions that persist user language preferences across sessions.

This library offers common-sense solutions to a number of common localization requirements. As a result, it is moderately opinionated in its approach, but we've tried to leave each individual tool as open to configuration as possible.

## Features

The library provides out-of-the-box functionality that allows you to:

- Get the request culture from URL path parameters such as `www.mymultilingualapp.com/en-GB/about/` and `www.mymultilingualapp.com/ga-IE/about/`
- Use cookies and HTTP Accept-Language headers to infer the user's desired culture when they visit the website homepage, e.g. `www.mymultilingualapp.com`, where no path parameters are present.
- Handle requests to unsupported cultures, either by returning a 404 error page or redirecting the user to a page in the default language
- Exclude certain routes from being affected by the localization middleware
- Manage and configure settings related to localization cookies, so that the user's language preferences can be persisted across browsing sessions
- Decide if users should be redirected to a localized landing page after they first reach the homepage of your site
- Handle scenarios where you wish to internally map a two- or three-letter ISO language code in the URL to a region or an extended language subtag

Most of these features are configurable: sensible defaults are supplied, but you get to specify which types of redirects to use, how long before cookies expire, etc. The library was also designed very much with SEO in mind, and the default setup offers an optimal localization solution when it comes to being indexed by major search engines.

## Install and setup

Add the NuGet package [Fiontar.Localization](https://www.nuget.org/packages/Fiontar.Localization/) to any ASP.NET Core 2.1+ project.

```cmd
dotnet add package Fiontar.Localization
```

Then in **Startup.cs**, add `using Fiontar.Localization;` to the top of the file. All configuration of the package is done in this file. The specific configuration for your application will depend on your own needs.

### Basic configuration

1. Add the `app.UseRequestLocalization()`, `app.UseRequestCultureValidation()`, and `app.UseRequestCultureExceptionHandler()` to the `Configure` method anywhere after `app.UseStaticFiles` (if present) and before `app.UseMvc()`, like so:

```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseStatusCodePagesWithReExecute("/Error/{0}");
    }

    app.UseStaticFiles();

    var localizationOptions = app.ApplicationServices
        .GetService<IOptions<RequestLocalizationOptions>>().Value;

    app.UseRequestCultureExceptionHandler();
    app.UseRequestCultureValidation();
    app.UseRequestLocalization(localizationOptions);

    app.UseMvc(routes =>
    {
        routes.MapRoute(
            name: "default",
            template: "{controller=Home}/{action=Index}/{id?}");
    });
}
```

`app.UseRequestLocalization()` activates ASP.NET Core's native middleware to automatically set culture information for requests based on information provided by the client. `app.UseRequestCultureValidation()` verifies that the requested culture is supported by the application. `app.UseRequestCultureExceptionHandler()` implements an exception handler that will return a 404 Not Found status code in the HTTP response if the client attempts to access a page in a culture that is unsupported or unavailable.

2. Modify the *ConfigureServices* method:

```csharp
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
            options.Conventions.AddPageRoute("/Index", route: "{culture?}");
            options.Conventions.AddPageRoute("/About", route: "{culture}/about/{id?}");
            options.Conventions.AddPageRoute("/Error", route: "/Error/{0}");
        })
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
}
```

The majority of the above code implements native ASP.NET Core localization functions. The `RouteCultureProvider` object, however, is responsible for inferring the target culture from the HTTP request. This functionality is discussed in more detail below.

## Getting the request culture

The primary goal of this package is to make it easier to understand what language the user intends to use when accessing your website so that the application can set the correct culture and localise the contents for the user. To this end, Fiontar.Localization provides a `RouteCultureProvider` that interprets the HTTP request. Set it up using the *ConfigureServices* method in **Startup.cs**:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...

    services.Configure<RequestLocalizationOptions>(options =>
    {
        var supportedCultures = new[]
        {
            new CultureInfo("en-GB"),
            new CultureInfo("ga-IE"),
            new CultureInfo("ru-RU")
        };
        options.DefaultRequestCulture = new RequestCulture(culture: "en-GB", uiCulture: "en-GB");
        options.SupportedCultures = supportedCultures;
        options.SupportedUICultures = supportedCultures;
        options.RequestCultureProviders.Clear();
        options.RequestCultureProviders.Add(new RouteCultureProvider(options.SupportedCultures, options.DefaultRequestCulture));
    });
}
```

The `supportedCultures` variables lists the languages and culture types we wish to support in the application. The `DefaultRequestCulture` provides the ultimate fallback if the desired culture cannot be inferred from the HTTP request. We call `Clear()` on the list of `RequestCultureProvdiers` as the `RouteCultureProvider` overrides their functionality.

The `RouteCultureProvider` parses the HTTP request and returns a target culture according to the following criteria:

1. The presence of a culture path parameter, i.e. an [IETF language tag](https://en.wikipedia.org/wiki/IETF_language_tag), in the request URL (e.g. the 'ga-IE' parameter in `www.mymultilingualapp.com/ga-IE/`)
2. The request contains a culture cookie previously obtained from the website (see [below](#localisation-cookies))
3. The user has specified a desired language in their browser (obtained via the HTTP `Accept-Language` header) that matches one of the application's supported cultures
4. The default language specified in the `RequestLocalizationOptions` service

The first criterion to return a non-null result will be used. Thus, a user who accesses `www.mymultilingualapp.com/ru-RU/` will be shown a page in Russian, regardless of their browser settings. A user who has selected `en` as their preferred language in their browser and visits `www.mymultilingualapp.com` will be shown a page in English, etc.

These checks are performed once the `app.UseRequestLocalization()` method is reached in the request execution pipeline. All middleware and application logic called subsequent to this can access the request culture via the `CultureInfo.CurrentCulture` object.

### Configure the culture path parameter

By default, Fiontar.Localization assumes that the first request path parameter, i.e. the `ga-IE` parameter in the URL `www.mymultilingualapp.com/ga-IE/about/our-story/`, contains the target culture information, as is common practice. You can configure the application to use a different parameter, however. Owing to the nature of the ASP.NET Core request execution pipeline you will need to specify the `CultureParameterIndex` in *two* places:

1. Add a `RouteCultureOptions` service to the `ConfigureServices()` method in **Startup.cs** and set the `CultureParameterIndex`:

```csharp
    services.Configure<RouteCultureOptions>(options =>
    {
        options.CultureParameterIndex = 2
    });
```

2. The `RouteCultureProvider` accepts the `CultureParameterIndex` as an optional overload:

```csharp
    ...
    options.RequestCultureProviders.Add(new RouteCultureProvider(options.SupportedCultures, 
        options.DefaultRequestCulture, 2));
    ...
```

Now, the application will attempt to use the second path parameter in the request when evaluating the request culture, meaning that a URL such as `www.mymultilingualapp.com/shop/fr-FR/products/` will work just fine.

### A word about SEO

In what is probably the most opinionated feature of this library, preference is given to a culture obtained from a URL path parameter over client cookie or HTTP header settings. This means that if a user visits `www.mymultilingualapp.com/en-GB/` and their preferred browser language is US English (en-US) they will still receive the page in United Kingdom English (en-GB), provided this is a supported culture within the application. This means that: (1) users get the page they expected to open when they clicked the URL, and; (2) search crawlers can reliably associate URLs with localized content. This is optimal for SEO and for user experience. Having opened the page, users should be able to voluntarily switch languages via a dedicated language switcher in the UI should they wish.

## Unsupported cultures

What happens when a user inputs a URL that contains an unsupported culture? For instance, if your site supports Spanish and Portugese content, but the user supplies an `fr-FR` region subtag in the URL. By default, the `RequestCultureValidation` middleware will cause a  `CultureNotFoundException` to be thrown in this scenario. Fiontar.Localization offers two ways to handle this error: both assume that you have already applied the request culture validation middleware, `app.UseRequestCultureValidation()` to the `Configure()` method in **Startup.cs**.

### Return a 404 Not Found status code

Add `app.UseRequestCultureExceptionHandler()` to the request execution pipeline *before* the request culture validation middleware. The exception handlers in Fiontar.Localization use an async method that listen for exceptions thrown as the request makes its way through the pipeline. Therefore, if the exception handlers are applied after after the error is thrown they will have no effect.

```csharp
    app.UseRequestCultureExceptionHandler();
    app.UseRequestCultureValidation();
    app.UseRequestLocalization(localizationOptions);
```

With this setting in place, a request to an unsupported culture will cause a 404 Not Found HTTP status code to be returned in the response and the user will be shown an appropriate message, provided that an error page route has been configured. This may be the ideal approach in terms of SEO. Search engines will be in no doubt that content is not available at the requested URL.

### Redirect the user to a page in the default culture

This may, in some cases, be the preferred option for user experience. When a URL containing an unsupported culture is accessed the client will be redirected to an equivalent page in the default culture of the application. To implement this, place `app.UseRequestCultureExceptionRerouter()` *before* the request culture validation middleware in the `Configure()` method in **Startup.cs**. By default, the response will return a 302 redirect to the same path but with an update culture parameter.

```csharp
    app.UseRequestCultureExceptionRerouter();
    app.UseRequestCultureValidation();
    app.UseRequestLocalization(localizationOptions);
```

#### Configure the rerouter

The `UseRequestCultureExceptionRerouter()` method accepts an instance of `RequestCultureRerouterOptions` as an optional argument. You can use this to send a different HTTP status code in the response or to have redirects routed to a particular path.

```csharp
    app.UseRequestCultureExceptionRerouter(new RequestCultureRerouterOptions
    {
        StatusCode = 301,
        ResponsePath = "/en-GB/lost/"
    });
```

## Excluding routes

While you might use the first parameter of your request path to represent the target culture most of the time, you may wish to exclude certain routes from the localisation pipeline, for example `www.mymultilingualapp.com/api/v1.2/`. Using the out-of-the-box configuration, a URL such as this will throw a `CultureNotFoundException`. However, you can prevent this by adding any number of 'non-cultures' to your service configuration:

```csharp
services.Configure<RouteCultureOptions>(options =>
{
    options.NonCultures.Add("api");
    options.NonCultures.Add("static");
});
```

Routes beginning with `/error`, e.g. `/Error` or `/error/{0}/`, are added by default to the exlusion list in order to prevent circular routing issues when a `CultureNotFoundException` is thrown.

**Note:** This aspect of the API is the most likely to change before we reach a 1.0 release. We are currently considering whether excluding routes on the basis of regex-matched paths rather than specific culture parameters might offer more flexibility.

## Localisation cookies

When a user visits your website, they may decide to select another language via a language switcher or similar UI facility. It can be useful to store the user's preference in a cookie so that the application will 'remember' their choice and the user can pick up where they left off on their next visit. Fortunately, ASP.NET Core has a built-in [provider](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-2.1#set-the-culture-programmatically) to append culture cookies to the HTTP response. Fiontar.Localization's `app.UseLocalizationCookies()` middleware wraps this provider in some useful logic that lives in the request execution pipeline: thus foregoing the need for POST requests or additional controllers to programatically set and update the user's language preferences. All you need to do is add the middleware to the `Configure()` method in **Startup.cs**:

```csharp
    app.UseRequestCultureNotFoundExceptionPage();
    app.UseRequestCultureValidation();
    app.UseRequestLocalization(localizationOptions);
    app.UseLocalizationCookies();
```

When used in conjunction with Fiontar.Localization's `RouteCultureProvider` (see [above](#getting-the-request-culture)) this middleware will ensure that your site is displayed in the user's preferred culture every time they visit the homepage. Don't forget, however, that the `RouteCultureProvider` [gives precedence to the culture in the URL path parameters over any other settings](#a-word-about-seo), so, if the user follows a link to a page in a particular culture, localisation cookies will not be taken into account.

### Configure the localisation cookies

The `UseLocalizationCookies()` method takes an instance of the `LocalizationCookiesOptions` class as an optional argument, allowing you to specify both the cookie expiration date and whether the cookie is essential for the application to function. The default values are `1 year` and `false` respectively. The `IsEssential` property should be configured with respect to your privacy and data protection policies. If true then consent policy checks may be bypassed.

```csharp
    app.UseLocalizationCookies(new LocalizationCookiesOptions
    {
        Expires = DateTime.UtcNow.AddDays(5),
        IsEssential = true
    });
```

## Landing page redirection

When a user visits a website's homepage, e.g. `www.mymultilingualapp.com`, it may sometimes be desirable to automatically redirect them to the URL for a localised version of that page, e.g `www.mymultilingualapp.com/es`. This is not the default behaviour when using Fiontar.Localization for reasons of SEO (see below) but we recongise that it is a common use case. To turn on landing page redirection, just add `app.UseRequireCulturePathParameter()` to the `Configure()` method in **Startup.cs**. It should be the last localisation method applied.

```csharp
    app.UseRequestCultureExceptionHandler();
    app.UseRequestCultureValidation();
    app.UseRequestLocalization(localizationOptions);
    app.UseLocalizationCookies();
    app.UseRequireCulturePathParameter();
```

The redirect URL automatically respects settings configured in the [`Microsoft.AspNetCore.Routing.RouteOptions`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.routing.routeoptions?view=aspnetcore-2.1) service as regards whether a trailing slash should be appended to the generated URL.

### Another word about SEO

Numerous approaches can be taken when localising a website's homepage. Some websites will evaluate a user's preferred language and redirect them to a new URL, such as moving from `example.com` to `example.com/es`. This can be problematic, however, due to the nature of HTTP redirects and how they interact with the browser:

- Some websites (for example, [mozilla.org](https://www.mozilla.org/)) examine the browser's language preferences and implement a 301 (permanent) redirect. This is fine if you are reasonably certain that users will mostly access the application in one language. However, many browsers indefinitely cache 301 redirects, meaning that even if the user later updates their browser settings—or they access the computer in a public location such as a school or a library— the browser will always take them to the first localised version of the site that was opened in that browser.
- Many other websites implement 302 redirects, likely for the reasons described above. However, this is both semantically incorrect (302 redirects define pages that have "Moved Temporarily") and bad for SEO as it is unclear whether web crawlers pay attention to pages with 302 redirects.

That is why the default approach using the Fiontar.Localization library is not to redirect the user (i.e. the user stays on `example.com`) though the culture information will still be localized according to the criteria outlined [above](#getting-the-request-culture). We feel this gives optimal results both in terms of SEO and user experience.

## Language tag choice

The library is agnostic as to which type of [IETF language tag](https://en.wikipedia.org/wiki/IETF_language_tag) you use in your URL to signify the target culture. The examples in this documentation use region subtags such as `ga-IE`, `en-GB`, `pt-BR`, etc. Many applications prefer ISO two-letter language codes like `ga`, `en`, `fr`. You can specify either type of tag in the supported cultures variable of your `RequestLocalizationOptions` in Startup.cs.

If you do opt for two-letter language codes it can often be helpful to store a set of region or extended language subtags that map to your language codes—for example, when supplying locale data in [Open Graph](http://ogp.me/) meta tags. Fiontar.Localization facilitates this by allowing you to configure the route culture options in the `Configure()` method of **Startup.cs**:

```csharp
services.Configure<RouteCultureOptions>(options =>
{
    options.LanguageLocaleMap.Add("en", "en-GB");
    options.LanguageLocaleMap.Add("ga", "ga-IE");
});
```

The mapped tags can then be accessed elswhere in the application, and a `InferLocaleFromLanguage()` convenience function is provided to infer the correct locale or region subtag (the code shown below is taken from a Razor view):

```csharp
@using Fiontar.Localization
@inject IOptions<RequestLocalizationOptions> LocalizationOptions
@inject IOptions<RouteCultureOptions> CultureOptions

...

@foreach (var culture in LocalizationOptions.Value.SupportedUICultures)
{
    if (culture.Name != CultureInfo.CurrentCulture.Name)
    {
        string lang = @culture.TwoLetterISOLanguageName;
        string locale = CultureOptions.Value.InferLocaleFromLanguage(@lang);
        <link rel="alternate" hreflang="@lang" href="https://www.mymultilingualapp.com/(@lang)">
        <meta property="og:locale:alternate" content="@locale">
    }
}
```

## Migrating between language tags in a URL scheme

If you wish to migrate from a URL scheme that used two-letter language tags (such as `example.com/es`) to a schema that uses regional locales (such as `example.com/es-ES`), Fiontar.Localization includes a helpful redirection protocol that leverages ASP.NET Core's native URL rewriting middleware to take care of the hard work for you.

First, configure the `RouteCultureOptions` service as described in the [previous section](#language-tag-choice):

```csharp
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
```

You can then pass the `LanguageLocaleMap` to a `RedirectLanguageToLocale` object in the URL rewriting middleware:

```csharp
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

Now, all requests to `www.mymultilingualapp.com/ga/about/` will be automatically redirected to `www.mymultilingualapp.com/ga-IE/about/`. The `RedirectLanguageToLocale` object implements a 302 redirect by default, but you can specify a 301 or other redirect type in an optional argument.

## Is there a sample application?

Not yet, but one is coming in version 0.7.

## Who is using this?

Fiontar.Localization has been implemented on one of Fiontar & Scoil na Gaeilge's project websites, [www.dúchas.ie](https://www.duchas.ie/) and will be rolled out across the rest of our websites over the coming year.

## Credits

The Fiontar.Localization library is authored by [Ronan Doherty](https://github.com/rodoch), but much credit is due to his colleagues [Michal Boleslav Měchura](https://github.com/michmech) and [Brian Ó Raghallaigh](https://github.com/oraghalb). The former's book *[An Ríomhaire Ilteangach](http://www.lexiconista.com/arit/)* is a font of quality guidance in these matters.