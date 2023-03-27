using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/loadImage", async (HttpContext context) =>
{
    IFormFileCollection files = context.Request.Form.Files;

    string path = builder.Configuration.GetSection("PathToImages").Value;

    var uploadPath = $"{Directory.GetCurrentDirectory()}{path}";

    Directory.CreateDirectory(uploadPath);

    foreach (var file in files)
    {

        string fullPath = $"{uploadPath}/{file.FileName}";


        using (var fileStream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }
    }
    await context.Response.WriteAsync("Файлы успешно загружены");
});

app.MapGet("/getImage", async (HttpContext context,string imgName) =>
{
    string path = builder.Configuration.GetSection("PathToImages").Value;
    string dataPath = $"{Directory.GetCurrentDirectory()}{path}/";

    string? fileName=Directory.GetFiles(dataPath).Where(f => f == dataPath + imgName).FirstOrDefault();
    if (fileName == null)
    {
        return ;
    }

    context.Response.Headers.ContentDisposition = $"attachment; filename={imgName}";
    await context.Response.SendFileAsync(fileName);
});
app.MapGet("/getRandomImage", async (HttpContext context) =>
{
    string path = builder.Configuration.GetSection("PathToImages").Value;
    string dataPath = $"{Directory.GetCurrentDirectory()}{path}/";

    string[] fileNames=Directory.GetFiles(dataPath);
    if (fileNames == null)
    {
        return;
    }
    string fileName = fileNames[new Random().Next(0, fileNames.Count())];

    using (FileStream file = new FileStream(fileName,FileMode.Open))
    {
        context.Response.Headers.ContentDisposition = $"attachment; filename={file.Name}";
        await context.Response.SendFileAsync(fileName);
    }

});

app.Map("/notFoundImage", () =>
{
    return "File not found";
});



app.Run();
