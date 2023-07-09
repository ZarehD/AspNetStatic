using CommandLine;

namespace AspNetStatic.Models;

/// All booleans must default to false
/// https://stackoverflow.com/questions/35873835/command-line-parser-library-boolean-parameter
public class GenerateStaticPagesOptions
{

    ///  <summary>
    /// 		Parse commandline args. All returned object fields are not nullable.
    ///  </summary>
    ///  <param name="commandLineArgs">
    /// 		The commandline arguments passed to your web app. The arguments must be convertible to 
    /// 		<see cref="GenerateStaticPagesOptions"/> model.
    ///  </param>
    ///  <param name="destinationRootFallback">
    ///			The path to the root folder where generated static page files (and subfolders) will be placed.
    ///			This argument is only used if <see cref="commandLineArgs"/> does not contain "--destination-root".
    /// </param>
    public static GenerateStaticPagesOptions ParseOptions(string[] commandLineArgs, string? destinationRootFallback = null)
    {
        var options = new Parser().ParseArguments<GenerateStaticPagesOptions>(commandLineArgs);
        if (options.Errors.Any())
        {
            throw new ArgumentException($"{options.Errors.First().Tag.ToString()} error occured with parsing commandline arguments.");
        }

        if (options.Value.DestinationRoot == null)
        {
            if (destinationRootFallback ==  null)
            {
                throw new ArgumentException($"{nameof(DestinationRoot)} could not be assigned!");
            }
            options.Value.DestinationRoot = destinationRootFallback;
        }
        return options.Value;
    }

    /// <para>
    ///		The path to the root folder where generated static page 
    ///		files (and subfolders) will be placed.
    ///     This is nullable only so parser would not throw error.
    /// </para>
    [Option("destination-root", Required = false)]
    // when language gets changed to C# 11 and CommandLine supports try adding required attribute, currently this is actually nullable
    public string? DestinationRoot { get; set; } = null;

    
    ///	Specifies whether to exit the app (gracefully shut the web app down) 
    ///	after generating the static files.
    [Option("static-only", Required = false)]
    public bool ExitWhenDone { get; set; }


    /// <para>
    ///		Specifies whether to always create default files for pages 
    ///		(true) even if a route specifies a page name, or an html file 
    ///		bearing the page name (false).
    ///	</para>
    ///	<para>
    ///		Does not affect routes that end with a trailing forward slash.
    ///		A default file will always be generated for such routes.
    ///	</para>
    ///	<para>
    ///		Whereas /page/ will always produce /page/index.html, /page will 
    ///		produce /page/index.html (true) or /page.html (false).
    ///	</para>
    [Option("always-default", Required = false)]
    public bool AlwaysDefaultFile { get; set; } = default;
    
    
    ///	<para>
    ///		Indicates, when true, that the href value of [a] and [area] 
    ///		HTML tags should not be modified to refer to the generated 
    ///		static pages.
    ///	</para>
    ///	<para>
    ///		Href values will be modified such that a value of /page is 
    ///		converted to /page.html or /page/index.html depending on 
    ///		<see cref="AlwaysDefaultFile" />.
    ///	</para>
    [Option("dont-update-links", Required = false)]
    public bool DontUpdateLinks { get; set; } = default;

    ///	<para>
    ///		Specifies whether to omit optimizing the content of generated static files.
    ///	</para>
    ///	<para>
    ///		By default, when this parameter is <c>false</c>, content of the generated 
    ///		static file will be optimized. Specify <c>true</c> to omit the optimizations.
    ///	</para>
    [Option("dont-optimize-content", Required = false)]
    public bool DontOptimizeContent { get; set; } = default;
    
    
    
    /// <param>
    ///		Specifies whether periodic re-generation is enabled (non-null value),
    ///		and the interval in milliseconds between re-generation events.
    /// </param>
    [Option("regeneration-interval", Required = false)]
    public double? RegenerationInterval { get; set; } = null;
}