using HamstarHelpers.Components.Network;
using HamstarHelpers.Components.Network.Data;


namespace Capitalism.NetProtocol {
	class ModSettingsProtocol : PacketProtocol {
		public CapitalismConfigData Settings;


		////////////////

		private ModSettingsProtocol( PacketProtocolDataConstructorLock ctor_lock ) { }

		////////////////

		protected override void SetServerDefaults( int to_who ) {
			this.Settings = CapitalismMod.Instance.Config;
		}

		////////////////

		protected override void ReceiveWithClient() {
			CapitalismMod.Instance.ConfigJson.SetData( this.Settings );
		}
	}
}
