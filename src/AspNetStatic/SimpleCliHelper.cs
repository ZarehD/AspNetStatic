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
	public static class SimpleCliHelper
	{
		public static bool HasExitWhenDoneArg(this string[] args) =>
			(args is not null) && args.Any(
				a =>
				a.HasSameText(SSG) ||
				a.HasSameText(SSG_ONLY) ||
				a.HasSameText(STATIC_ONLY) ||
				a.HasSameText(EXIT_WHEN_DONE));

		public static bool HasOmitSsgArg(this string[] args) =>
			(args is not null) && args.Any(
				a =>
				a.HasSameText(NO_SSG));

		private const string SSG = "ssg";
		private const string SSG_ONLY = "ssg-only";
		private const string STATIC_ONLY = "static-only";
		private const string EXIT_WHEN_DONE = "exit-when-done";

		private const string NO_SSG = "no-ssg";
	}
}
