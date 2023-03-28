using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
List<string> paths = new List<string>
    {
        "https://avatars.mds.yandex.net/i?id=da6419f589ba74179cc2ce52b2d26f38bba626a1-9223904-images-thumbs&n=13",
        "https://avatars.mds.yandex.net/i?id=5ff5db7c4d5385fe4479d12329a14d0be86964c9-4592776-images-thumbs&n=13",
        "https://avatars.mds.yandex.net/i?id=2aa2f47b01ac323be2afafe7c080b48a8f705a33-8497900-images-thumbs&n=13"
    };


ITelegramBotClient bot = new TelegramBotClient("5707186409:AAHAKXMYYOLR1dqBxCBdAy12ae1BFThPiZ4");
async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    //List<string> paths = new List<string>
    //{
    //    "https://vitoria.bsite.net/getImage?imgName=9ef540468aaac1e2c63eaaffb3b1254a.jpg",
    //    "https://vitoria.bsite.net/getImage?imgName=monika.png"
    //};






    // Некоторые действия
    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
    if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
    {
        var message = update.Message;
        if (message.Text != null && message.Text.ToLower() == "/start")
        {
            await botClient.SendTextMessageAsync(message.Chat, "Добро пожаловать на борт, добрый путник!");
            return;
        }
        else if (message.Text != null && message.Text.ToLower() == "/getrandomimage")
        {
            await botClient.SendPhotoAsync(
                    chatId: message.Chat.Id,
                    photo: new InputOnlineFile(paths[new Random().Next(0, paths.Count())])
                );
            return;
        }
        else if (message.Text != null && message.Text.ToLower() == "/getimage")
        {
            await botClient.SendPhotoAsync(
                    chatId: message.Chat.Id,
                    photo: new InputOnlineFile(paths[0])
                );
            return;
        }
        else if (message.From != null && message.From.FirstName == "Даниил")
        {
            await botClient.SendTextMessageAsync(message.Chat, "Замолчи свой рот!!!");

            return;
        }
        await botClient.SendTextMessageAsync(message.Chat, "Привет-привет!!");
    }
}

static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    // Некоторые действия
    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
}

Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

var cts = new CancellationTokenSource();
var cancellationToken = cts.Token;
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = { }, // receive all update types
};
bot.StartReceiving(
    HandleUpdateAsync,
    HandleErrorAsync,
    receiverOptions,
    cancellationToken
);


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


app.MapPost("/loadImageUrl", async (HttpContext context, string imgUrl) =>
{
    paths.Add(imgUrl);
});

app.MapGet("/getImage", async (HttpContext context,string imgName) =>
{
    string path = builder.Configuration.GetSection("PathToImages").Value;
    string dataPath = $"{Directory.GetCurrentDirectory()}{path}/";

    string? fileName=Directory.GetFiles(dataPath).Where(f => f == dataPath + imgName).FirstOrDefault();
    if (fileName == null)
    {
        return "Image not found";
    }

    context.Response.Headers.ContentDisposition = $"attachment; filename={imgName}";
    await context.Response.SendFileAsync(fileName);
    return "";
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
