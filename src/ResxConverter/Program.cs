using PleOps.Il28n.ResxConverter.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(config => {
    config.AddCommand<Resx2PoCommand>("resx2po");
    config.AddCommand<Resx2PotCommand>("resx2pot");
});
return await app.RunAsync(args);
