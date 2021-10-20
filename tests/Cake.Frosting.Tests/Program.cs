using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Frosting;
using Cake.MarkdownToPdf;

public static class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
            .UseContext<BuildContext>()
            .Run(args);
    }
}

public class BuildContext : FrostingContext
{
    public BuildContext(ICakeContext context)
        : base(context)
    {

    }
}

[TaskName("GenerateDocument")]
public sealed class GenerateDocumentTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.Log.Information("Generating document...");
        context.MarkdownToPdf("# Hello World \r\nSample text", "output.pdf");
        context.MarkdownToPdf("# Hello World \r\nSample text", "output2.pdf", settings =>
        {
            settings.Theme = Themes.Github;
        });
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(GenerateDocumentTask))]
public class DefaultTask : FrostingTask
{
}
