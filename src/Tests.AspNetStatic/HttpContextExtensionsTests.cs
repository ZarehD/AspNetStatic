using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Moq;

namespace Tests.AspNetStatic
{
	[TestClass]
	public class HttpContextExtensionsTests
	{
		[TestMethod]
		public void IsAspNetStaticRequest_No()
		{
			var request = new Mock<HttpRequest>();
			request.Setup(x => x.Headers).Returns(new HeaderDictionary());

			var actual = request.Object.IsAspNetStaticRequest();

			Assert.IsFalse(actual);
		}

		[TestMethod]
		public void IsAspNetStaticRequest_Yes()
		{
			var headerDictionary =
				new HeaderDictionary
				{
					{ HeaderNames.UserAgent, Consts.AspNetStatic }
				};

			var request = new Mock<HttpRequest>();
			request.Setup(x => x.Headers).Returns(headerDictionary);

			var actual = request.Object.IsAspNetStaticRequest();

			Assert.IsTrue(actual);
		}
	}
}
