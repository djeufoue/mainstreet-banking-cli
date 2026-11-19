using BankingCli;

BankSession banking = new BankSession();
using HttpClient http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
EnglishCommands? english = null;
try
{
    if (OpenAiClient.FromEnvironment(http) is OpenAiClient api)
    {
        english = new EnglishCommands(api);
    }
}
catch (ArgumentException error)
{
    Console.Error.WriteLine(error.Message);
}

while (true)
{
    // Keep redirected output limited to command results so it can be compared line by line.
    if (!Console.IsInputRedirected)
    {
        Console.Write("bank> ");
    }

    string? input = Console.ReadLine();
    if (input is null)
    {
        break;
    }

    CommandResult result = english is null || EnglishCommands.IsDirectCommand(input)
        ? banking.Execute(input)
        : await english.ExecuteAsync(input, banking);
    if (result.Output.Length > 0)
    {
        Console.WriteLine(result.Output);
    }

    if (result.ShouldExit)
    {
        break;
    }
}
