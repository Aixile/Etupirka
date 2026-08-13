using System;
using System.Windows.Markup;

namespace Etupirka.Localization
{
	[MarkupExtensionReturnType(typeof(string))]
	public sealed class LocExtension : MarkupExtension
	{
		public LocExtension(string key)
		{
			Key = key;
		}

		public string Key { get; private set; }

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return Strings.Get(Key);
		}
	}
}
