/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023-2025 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace AspNetStatic
{
	public static class HttpContextExtensions
	{
		/// <summary>
		///		Determines whether the HTTP request was made 
		///		by AspNetStatic.
		/// </summary>
		/// <remarks>
		///		Examines the UA header in the request to look for 
		///		the presence of the AspNetStatic UA string.
		/// </remarks>
		/// <param name="request">The HTTP request object.</param>
		/// <returns>
		///		<c>True</c> if the request was made by AspNetStatic; 
		///		otherwise <c>False</c>.
		/// </returns>
		public static bool IsAspNetStaticRequest(this HttpRequest request) =>
			(request is not null) &&
			request.Headers.TryGetValue(HeaderNames.UserAgent, out var ua) &&
			ua.Contains(Consts.AspNetStatic);
	}
}
