
using Microsoft.EntityFrameworkCore;
using MvcUmbraco.Controllers;
using MvcUmbraco.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// This method will be called when making EF commands like migrations + update database
builder.Services.AddDbContext<MvcAppDbContext>(

    options => options.UseSqlite(

      builder.Configuration.GetConnectionString("efConnection")
      
  ));  


builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()

     // 13-11-2025 - Most likely not needed in Version 17
     // Setting from Version 13
     // .AddDeliveryApi()

    .AddComposers()
    .Build();


WebApplication app = builder.Build();

// 16-11-2025 - When runtime mode is set to Production in appsettings.json do the below:
// Remove the codeblock with the Items in csproj file "RazorCompileOnBuild and RazorCompileOnPublish"
// Note: Added to make the Umbraco Site more secure
app.Use(async (context, next) =>
{
    // Click-Jacking Protection
    // Checks if your site is allowed to be IFRAMEd by another site and 
    // then could be susceptible to click-jacking
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");

    // Content/MIME Sniffing Protection
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

    // Note: This header will make the Browser block the Bootstrap files 
    // and the layout of the Umbraco site will be lost
    // context.Response.Headers.Append("Content-Security-Policy", "img-src 'self' our.umbraco.com data: dashboard.umbraco.com; default-src 'self' our.umbraco.com marketplace.umbraco.com; script-src 'self'; style-src 'unsafe-inline' 'self'; font-src 'self'; connect-src 'self'; frame-src 'self'; ");
    
    await next();
});

// Note: Added to make the Umbraco Site more secure
// Protects for Cookie hijacking and protocol downgrade attacks
// Strict-Transport-Security Header (HSTS) is only used for Production when using HTTPS
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
    
}

await app.BootUmbracoAsync();

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        // 13-11-2025 - Needs to be removed for run / build in verion 17
        //  u.UseInstallerEndpoints();

        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
       
    });

await app.RunAsync();
