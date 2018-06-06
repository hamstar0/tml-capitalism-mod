namespace Capitalism {
	public static partial class CapitalismAPI {
		public static CapitalismConfigData GetModSettings() {
			return CapitalismMod.Instance.Config;
		}
	}
}
