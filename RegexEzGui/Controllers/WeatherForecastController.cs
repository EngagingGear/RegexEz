using Microsoft.AspNetCore.Mvc;

namespace RegexExGui.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    const string pattern = @"
// This is a comment
test: ^$(username)@$(domain)\.$(tld)$
username: $name
domain: $name
tld: $name
name: [a-zA-Z0-9_]+

$match: fraser@yahoo.com
$match: fraser_orr@yahoo.com
$noMatch: fraser@yahoo
$noMatch: fraser-orr@yahoo.com
$field.username: fraser@yahoo.com $= fraser
$field.domain: fraser@yahoo.com $= yahoo
$field.tld: fraser@yahoo.com $= com
";

    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
    
}
