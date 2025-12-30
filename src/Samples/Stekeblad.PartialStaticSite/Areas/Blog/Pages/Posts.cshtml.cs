using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PartialStaticSite.Areas.Blog.Pages
{
	public class PostsModel : PageModel
	{
		public PostsModel() { }

		public int PostId { get; set; }

		public void OnGet(int postId)
		{
			this.PostId = postId;
		}
	}
}
