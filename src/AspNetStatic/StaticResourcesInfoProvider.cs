/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

namespace AspNetStatic
{
	public class StaticResourcesInfoProvider : StaticResourcesInfoProviderBase
	{
		public StaticResourcesInfoProvider(
			IEnumerable<PageResource>? pages,
			IEnumerable<NonPageResource>? otherResources,
			string? defaultFileName = default,
			string? defaultFileExtension = default,
			IEnumerable<string>? dffExclusions = default,
			bool skipProcessingPageResources = default,
			bool skipProcessingOtherResources = default)
			: base()
		{
			if (pages?.Any() ?? false) this.pages.AddRange(pages);
			if (otherResources?.Any() ?? false) this.otherResources.AddRange(otherResources);
			if (defaultFileName is not null) SetDefaultFileName(defaultFileName);
			if (defaultFileExtension is not null) SetDefaultFileExtension(defaultFileExtension);
			if (dffExclusions is not null) SetDefaultFileExclusions(dffExclusions.ToArray());
			this.SkipProcessingPageResources = skipProcessingPageResources;
			this.SkipProcessingOtherResources = skipProcessingOtherResources;
		}
	}
}
