namespace Capitalism {
	public static class CapitalismAPI {
		public static CapitalismConfigData GetModSettings() {
			return CapitalismMod.Instance.Config.Data;
		}
	}
}
