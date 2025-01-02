/*--------------------------------------------------------------------------------------------------------------------------------
Copyright 2023-2025 Zareh DerGevorkian

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License. 
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
--------------------------------------------------------------------------------------------------------------------------------*/

using System.Text;

namespace AspNetStatic
{
	// HACK: EncodingType is a quick & cheap way to get around System.Text.Encoding serialization errors.

	public enum EncodingType { ASCII, BigEndianUnicode, Default, Latin1, Unicode, UTF7, UTF8, UTF32 }

	public static class EncodingConverter
	{
		public static Encoding ToSystemEncoding(this EncodingType encodingType) =>
			encodingType switch
			{
				EncodingType.ASCII => Encoding.ASCII,
				EncodingType.BigEndianUnicode => Encoding.BigEndianUnicode,
				EncodingType.Default => Encoding.Default,
				EncodingType.Latin1 => Encoding.Latin1,
				EncodingType.Unicode => Encoding.Unicode,
				EncodingType.UTF7 => Encoding.UTF7,
				EncodingType.UTF8 => Encoding.UTF8,
				EncodingType.UTF32 => Encoding.UTF32,
				_ => Encoding.Default
			};
	}
}
