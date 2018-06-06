using HamstarHelpers.Utilities.Network;


namespace Capitalism.NetProtocol {
	class ModSettingsProtocol : PacketProtocol {
		public CapitalismConfigData Settings;


		public override void SetServerDefaults() {
			this.Settings = CapitalismMod.Instance.Config;
		}

		protected override void ReceiveWithClient() {
			CapitalismMod.Instance.ConfigJson.SetData( this.Settings );
		}
	}
}
