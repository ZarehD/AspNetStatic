using CommandLine;

namespace AspNetStatic.Models;

/// All booleans must default to false
/// https://stackoverflow.com/questions/35873835/command-line-parser-library-boolean-parameter
public class GenerateStaticPagesOptions
{

    /// <para>
    ///		The path to the root folder where generated static page 
    ///		files (and subfolders) will be placed.
    /// </para>
    [Option("destination-root", Required = true, HelpText = "Root directory path where generated files are placed")]
    // when language gets changed to C# 11 and CommandLine supports try adding required attribute, currently this is actually nullable
    public string DestinationRoot { get; set; } = default!;

    
    ///	Specifies whether to exit the app (gracefully shut the web app down) 
    ///	after generating the static files.
    [Option("static-only", Required = false, HelpText = "Exit when done generating static files")]
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
    [Option("always-default", Required = false, HelpText = "Treat every path as a directory")]
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
    [Option("dont-update-links", Required = false, HelpText = "Updates links to match generated content, appends filename extension")]
    public bool DontUpdateLinks { get; set; } = default;

    ///	<para>
    ///		Specifies whether to omit optimizing the content of generated static files.
    ///	</para>
    ///	<para>
    ///		By default, when this parameter is <c>false</c>, content of the generated 
    ///		static file will be optimized. Specify <c>true</c> to omit the optimizations.
    ///	</para>
    [Option("dont-optimize-content", Required = false, HelpText = "Optimize generated content")]
    public bool DontOptimizeContent { get; set; } = default;
    
    
    
    /// <param>
    ///		Specifies whether periodic re-generation is enabled (non-null value),
    ///		and the interval in milliseconds between re-generation events.
    /// </param>
    [Option("regeneration-interval", Required = false, HelpText = "Regenerate static content periodically while the app is running")]
    public long? RegenerationInterval { get; set; } = null;
}