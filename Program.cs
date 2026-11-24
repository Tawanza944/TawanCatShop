using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using WebApplication1.Services.Storage;
using WebApplication1.Data;
using WebApplication1.Extensions;
using WebApplication1.Middleware;

// ลงทะเบียนบริการที่แอปต้องใช้ แล้วกำหนดลำดับการประมวลผลคำขอ HTTP
// โครงสร้างฐานข้อมูลและข้อมูลตั้งต้นอยู่ในโฟลเดอร์ database สำหรับใช้กับ MySQL Command Line

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.")));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(120);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddLoveCatApplication(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error/Status/500");
    app.UseHsts();
}

// ใช้วัฒนธรรมคงที่เมื่ออ่านเลขและวันที่จากฟอร์ม เพื่อให้ค่า ISO เป็น ค.ศ. เหมือนกันทุกเครื่อง
// ส่วนวันที่ภาษาไทยที่ผู้ใช้เห็นกำหนด CultureInfo("th-TH") ใน View
var appCulture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = appCulture;
CultureInfo.DefaultThreadCurrentUICulture = appCulture;
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(appCulture),
    SupportedCultures = [appCulture],
    SupportedUICultures = [appCulture]
});

app.UseStatusCodePagesWithReExecute("/Error/Status/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();

// เก็บรูปอัปโหลดใน App_Data/uploads นอก wwwroot แล้วเปิดให้อ่านผ่าน /uploads
var uploadsRoot = LocalImageStorage.RootFor(app.Environment);
Directory.CreateDirectory(uploadsRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot),
    RequestPath = LocalImageStorage.RequestPath
});
app.UseRouting();
app.UseSession();
app.UseMiddleware<MembershipRefreshMiddleware>();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action}")
    .WithStaticAssets();

app.Run();
