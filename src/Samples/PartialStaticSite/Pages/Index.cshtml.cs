using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PartialStaticSite.Pages
{
	public class IndexModel : PageModel
	{
		private readonly ILogger<IndexModel> _logger;

		public IndexModel(ILogger<IndexModel> logger)
		{
			_logger = logger;
		}


		public class Data
		{
			public string? RouteId { get; set; }

			public string? Parm1 { get; set; }
		}

		public Data VmData { get; } = new();


		public void OnGet(string? id, [FromQuery] string? p1)
		{
			this.VmData.RouteId = id;
			this.VmData.Parm1 = p1;
		}
	}
}