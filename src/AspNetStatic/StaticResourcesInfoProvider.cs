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
			IEnumerable<ResourceInfoBase>? resources,
			string? defaultFileName = default,
			string? defaultFileExtension = default,
			IEnumerable<string>? dffExclusions = default,
			bool skipPageResources = default,
			bool skipOtherResources = default)
			: base()
		{
			if (resources is not null)
			{
				var typeofPage = typeof(PageResource);
				this.pages.AddRange(resources.Where(r => r?.GetType() == typeofPage).Cast<PageResource>());
				this.otherResources.AddRange(resources.Where(r => r?.GetType() != typeofPage).Cast<NonPageResource>());
			}

			if (defaultFileName is not null) SetDefaultFileName(defaultFileName);
			if (defaultFileExtension is not null) SetDefaultFileExtension(defaultFileExtension);
			if (dffExclusions is not null) SetDefaultFileExclusions(dffExclusions.ToArray());

			this.SkipProcessingPageResources = skipPageResources;
			this.SkipProcessingOtherResources = skipOtherResources;
		}


		public void Add(PageResource page) => this.pages.Add(Throw.IfNull(page));
		public void Add(IEnumerable<PageResource> pages)
		{
			Throw.IfNull(pages);
			this.pages.AddRange(pages.Where(p => p is not null));
		}

		public void Add(CssResource resource) => this.otherResources.Add(Throw.IfNull(resource));
		public void Add(IEnumerable<CssResource> resources)
		{
			Throw.IfNull(resources);
			this.otherResources.AddRange(resources.Where(p => p is not null));
		}

		public void Add(JsResource resource) => this.otherResources.Add(Throw.IfNull(resource));
		public void Add(IEnumerable<JsResource> resources)
		{
			Throw.IfNull(resources);
			this.otherResources.AddRange(resources.Where(p => p is not null));
		}

		public void Add(BinResource resource) => this.otherResources.Add(Throw.IfNull(resource));
		public void Add(IEnumerable<BinResource> resources)
		{
			Throw.IfNull(resources);
			this.otherResources.AddRange(resources.Where(p => p is not null));
		}
	}
}
