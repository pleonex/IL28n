using PleOps.Il28n.LocalizationLinter.LanguageTool;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(config => {
  #if DEBUG
    config.PropagateExceptions();
  #endif

    config.AddCommand<LanguageToolPoLinterCommand>("po-langtool");
});
return await app.RunAsync(args);
