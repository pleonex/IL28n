using PleOps.Il28n.LocalizationLinter.LanguageTool;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(config => {
  #if DEBUG
    config.PropagateExceptions();
  #endif

    config.AddBranch("po", poConfig => {
        poConfig.SetDescription("Quality assurance checks for PO files");
        poConfig.AddCommand<LanguageToolPoLinterCommand>("langtool");
    });
});
return await app.RunAsync(args);
