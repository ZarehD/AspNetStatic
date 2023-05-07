using System.Text;

namespace AspNetStatic
{
	// HACK: EncodingType is a quick & cheap way to get around System.Text.Encoding serialization errors.

	public enum EncodingType { ASCII, BigEndianUnicode, Default, Latin1, Unicode, UTF7, UTF8, UTF32 }

	public static class EncodingConverter
	{
		public static Encoding ToSystemEncodng(this EncodingType encodingType) =>
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
