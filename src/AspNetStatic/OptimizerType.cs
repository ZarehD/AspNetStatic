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
	public enum OptimizerType
	{
		/// <summary>
		///		Auto-select the optimizer based on the resource type and OutFile pathname.
		/// </summary>
		/// <remarks>
		///		Not used for <see cref="BinResource"/> entry types.
		/// </remarks>
		Auto,

		/// <summary>
		///		Perform no optimization on the resource.
		/// </summary>
		None,

		/// <summary>
		///		Use an HTML markup optimizer.
		/// </summary>
		Html,

		/// <summary>
		///		Use an XHTML markup optimizer.
		/// </summary>
		Xhtml,

		/// <summary>
		///		Use an XML markup optimizer.
		/// </summary>
		Xml,

		/// <summary>
		///		Use a CSS content optimizer.
		/// </summary>
		Css,

		/// <summary>
		///		Use a JS content optimizer.
		/// </summary>
		Js,

		/// <summary>
		///		Optimize using an instance of <see cref="IBinOptimizer" />.
		/// </summary>
		Bin
	}
}
