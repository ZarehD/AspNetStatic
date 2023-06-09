﻿/*--------------------------------------------------------------------------------------------------------------------------------
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
	public class StaticPagesInfoProvider : StaticPagesInfoProviderBase
	{
		public StaticPagesInfoProvider(
			IEnumerable<PageInfo> pages,
			string? defaultFileName = default,
			string? defaultFileExtension = default,
			IEnumerable<string>? dffExclusions = default)
			: base()
		{
			this.pages.AddRange(Throw.IfNull(pages, nameof(pages)));

			if (defaultFileName is not null) SetDefaultFileName(defaultFileName);
			if (defaultFileExtension is not null) SetDefaultFileExtension(defaultFileExtension);
			if (dffExclusions is not null) SetDefaultFileExclusions(dffExclusions.ToArray());
		}
	}
}
