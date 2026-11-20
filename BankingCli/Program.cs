using BankingCli;

BankSession banking = new BankSession();
using HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
EnglishCommands? englishCommands = null;
try
{
    if (OpenAiClient.CreateFromEnvironment(httpClient) is OpenAiClient apiClient)
    {
        englishCommands = new EnglishCommands(apiClient);
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

    CommandResult result = englishCommands is null || EnglishCommands.IsDirectCommand(input)
        ? banking.Execute(input)
        : await englishCommands.HandleEnglishRequestAsync(input, banking);
    if (result.Output.Length > 0)
    {
        Console.WriteLine(result.Output);
    }

    if (result.ShouldExit)
    {
        break;
    }
}
