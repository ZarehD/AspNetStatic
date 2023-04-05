namespace AspNetStatic
{
	public static class Helpers
	{
		public static string ToDefaultFileFallback(
			this string route, string[] exclusions,
			string defaultFileName, string pageFileExtension)
		{
			if (string.IsNullOrWhiteSpace(route))
			{
				throw new ArgumentException(
					Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace,
					nameof(route));
			}
			if (string.IsNullOrWhiteSpace(defaultFileName))
			{
				throw new ArgumentException(
					Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace,
					nameof(defaultFileName));
			}
			if (string.IsNullOrWhiteSpace(pageFileExtension))
			{
				throw new ArgumentException(
					Properties.Resources.Err_ValueCannotBeNullEmptyWhitespace,
					nameof(pageFileExtension));
			}

			return
				((exclusions is null) || !exclusions.Any(
					x => route.AssureNotEndsWith(RouteConsts.FwdSlash)?
					.EndsWith(x, StringComparison.InvariantCultureIgnoreCase) ?? false))
				? route.AssureEndsWith(RouteConsts.FwdSlash) + defaultFileName
				: route.AssureNotEndsWith(RouteConsts.FwdSlash) + pageFileExtension
				;
		}
	}
}
