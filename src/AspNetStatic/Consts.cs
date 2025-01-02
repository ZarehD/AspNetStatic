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
	internal static class Consts
	{
		public static readonly Type TypeOfPageResource = typeof(PageResource);

		public static readonly char BakSlash = '\\';
		public static readonly char FwdSlash = '/';
		public static readonly string BSlash = BakSlash.ToString();
		public static readonly string FSlash = FwdSlash.ToString();

		public static readonly string AspNetStatic = nameof(AspNetStatic);

		public static readonly string Ext_Unk = ".unknown";
		public static readonly string Ext_Htm = ".html";
		public static readonly string Ext_Css = ".css";
		public static readonly string Ext_Js = ".js";
		public static readonly string Ext_Bin = ".bin";

		public static readonly string DefaultIndexFile = "index";
		public static readonly string DefaultIndexFileFullName = $"{DefaultIndexFile}{Ext_Htm}";

		public static readonly string[] DefaultFileExclusions = { DefaultIndexFile, "default" };
	}
}
