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
	[Serializable]
	public class PageResource : ResourceInfoBase
	{
		public ChangeFrequency ChangeFrequency { get; init; } = ChangeFrequency.Never;

		public DateTime LastModified { get; init; } = DateTime.MinValue;

		public double IndexPriority { get; init; }


		public PageResource(string route) : base(route) { }
	}


	[Serializable]
	public class NonPageResource : ResourceInfoBase
	{
		public NonPageResource(string route) : base(route) { }
	}


	[Serializable]
	public class CssResource : NonPageResource
	{
		public CssResource(string route) : base(route) { }
	}


	[Serializable]
	public class JsResource : NonPageResource
	{
		public JsResource(string route) : base(route) { }
	}


	[Serializable]
	public class BinResource : NonPageResource
	{
		public BinResource(string route) : base(route) { }


		public new EncodingType OutputEncoding { get; init; } = EncodingType.Default;
		public new OptimizerType OptimizerType { get; } = OptimizerType.None;
		public new bool SkipOptimization => true;
	}
}