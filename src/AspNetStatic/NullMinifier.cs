using System.Text;
using WebMarkupMin.Core;

namespace AspNetStatic
{
	public class NullMinifier : IMarkupMinifier
	{
		public MarkupMinificationResult Minify(string content) => new(content);
		public MarkupMinificationResult Minify(string content, string fileContext) => new(content);
		public MarkupMinificationResult Minify(string content, bool generateStatistics) => new(content);
		public MarkupMinificationResult Minify(string content, Encoding encoding) => new(content);
		public MarkupMinificationResult Minify(string content, string fileContext, Encoding encoding, bool generateStatistics) => new(content);
	}
}
