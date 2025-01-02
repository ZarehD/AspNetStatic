/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023-2025 Zareh DerGevorkian

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
		public StaticResourcesInfoProvider() { }

		public StaticResourcesInfoProvider(
			IEnumerable<ResourceInfoBase>? resources = default,
			string? defaultFileName = default,
			string? defaultFileExtension = default,
			IEnumerable<string>? defaultFileExclusions = default)
			: base()
		{
			if (resources is not null)
			{
				var typeofPage = typeof(PageResource);
				this.resources.AddRange(resources.Where(r => r?.GetType() == typeofPage).Cast<PageResource>());
				this.resources.AddRange(resources.Where(r => r?.GetType() != typeofPage).Cast<NonPageResource>());
			}

			if (defaultFileName is not null) SetDefaultFileName(defaultFileName);
			if (defaultFileExtension is not null) SetDefaultFileExtension(defaultFileExtension);
			if (defaultFileExclusions is not null) SetDefaultFileExclusions(defaultFileExclusions.ToArray());
		}


		public new StaticResourcesInfoProvider SetDefaultFileName(string name)
		{
			base.SetDefaultFileName(name);
			return this;
		}
		public new StaticResourcesInfoProvider SetDefaultFileExtension(string extension)
		{
			base.SetDefaultFileExtension(extension);
			return this;
		}
		public new StaticResourcesInfoProvider SetDefaultFileExclusions(string[] newExclusions)
		{
			base.SetDefaultFileExclusions(newExclusions);
			return this;
		}

		public StaticResourcesInfoProvider Add(PageResource page)
		{
			this.resources.Add(Throw.IfNull(page));
			return this;
		}
		public StaticResourcesInfoProvider Add(IEnumerable<PageResource> pages)
		{
			Throw.IfNull(pages);
			this.resources.AddRange(pages.Where(p => p is not null));
			return this;
		}

		public StaticResourcesInfoProvider Add(CssResource resource)
		{
			this.resources.Add(Throw.IfNull(resource));
			return this;
		}
		public StaticResourcesInfoProvider Add(IEnumerable<CssResource> resources)
		{
			Throw.IfNull(resources);
			this.resources.AddRange(resources.Where(p => p is not null));
			return this;
		}

		public StaticResourcesInfoProvider Add(JsResource resource)
		{
			this.resources.Add(Throw.IfNull(resource));
			return this;
		}
		public StaticResourcesInfoProvider Add(IEnumerable<JsResource> resources)
		{
			Throw.IfNull(resources);
			this.resources.AddRange(resources.Where(p => p is not null));
			return this;
		}

		public StaticResourcesInfoProvider Add(BinResource resource)
		{
			this.resources.Add(Throw.IfNull(resource));
			return this;
		}
		public StaticResourcesInfoProvider Add(IEnumerable<BinResource> resources)
		{
			Throw.IfNull(resources);
			this.resources.AddRange(resources.Where(p => p is not null));
			return this;
		}
	}
}
