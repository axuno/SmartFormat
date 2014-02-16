using System;

using SmartFormat.Core.Settings;

namespace SmartFormat.Tests.Utilities
{
	public class SmartSettingOverride : IDisposable
	{
		private readonly Action<SmartSettings> after;

		public SmartSettingOverride(Action<SmartSettings> before, Action<SmartSettings> after)
		{
			this.after = after;
			before(Smart.Settings);
		}

		public void Dispose()
		{
			after(Smart.Settings);
		}
	}
}