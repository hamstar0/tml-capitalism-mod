using HamstarHelpers.Components.Network;
using HamstarHelpers.Components.Network.Data;


namespace Capitalism.NetProtocol {
	class ModSettingsProtocol : PacketProtocolRequestToServer {
		public CapitalismConfigData Settings;


		////////////////

		private ModSettingsProtocol() { }

		////////////////

		protected override void InitializeServerSendData( int toWho ) {
			this.Settings = CapitalismMod.Instance.Config;
		}

		////////////////

		protected override void ReceiveReply() {
			CapitalismMod.Instance.ConfigJson.SetData( this.Settings );
		}
	}
}
