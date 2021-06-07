using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Kelson.Common.Route.Web;
using static Kelson.Common.Route.CoreArgs<Microsoft.AspNetCore.Http.HttpContext>;
using static Kelson.Common.Route.Web.WebArgs;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using System.IO;

var scores = new HighscoreCollection();
var opts = new JsonSerializerOptions()
{
    PropertyNameCaseInsensitive = true,
    WriteIndented = true
};

var router = WebRouter.Start
    .When(Method(HttpMethod.Get) & Path("echo"))
        .On(REMAINING)
        .Do(EchoToBody)
    .When(Method(HttpMethod.Post) & Path("highscores"))
        .On(Json<Highscore>(opts))
        .Do(PostHighscore)
    .When(Method(HttpMethod.Get) & Path("highscores"))
        .Do(GetHighscores);

await Host.CreateDefaultBuilder(args)
    .ConfigureWebHost(webBuilder => webBuilder
        .Configure(app => app.RunRequestRouter(router))
        .UseKestrel())
    .Build()
    .RunAsync();

Task EchoToBody(HttpContext context, string text) => context.Response.WriteAsync(text);

Task PostHighscore(HttpContext context, Highscore score)
{
    void AddScore(SortedList<ulong, Highscore> scores)
    {
        scores.Add(score.Score, score);
        while (scores.Count > 10)
            scores.RemoveAt(10);
    }
    scores.Mutate(AddScore);
    return Task.CompletedTask;
}
async Task GetHighscores(HttpContext context) => await context.Response.Body.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(scores.Get()));


public record Highscore(string User, ulong Score);

class HighscoreCollection
{
    private readonly SortedList<ulong, Highscore> scores = new();
    private readonly object lockObject = new();

    public ImmutableArray<Highscore> Get()
    {
        lock(lockObject)
        {
            return scores.Values.ToImmutableArray();
        }
    }

    public void Mutate(Action<SortedList<ulong, Highscore>> mutate)
    {
        lock (lockObject)
        {
            mutate(scores);
        }
    }
}