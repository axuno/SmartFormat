namespace SmartFormat.Core.Settings
{
	public class SmartSettings
	{
		internal SmartSettings()
		{
			CaseSensitivity = CaseSensitivityType.CaseSensitiv;
		}

		public CaseSensitivityType CaseSensitivity { get; set; }
	}
}