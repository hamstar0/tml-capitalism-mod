using HamstarHelpers.Components.Network;
using HamstarHelpers.Components.Network.Data;


namespace Capitalism.NetProtocol {
	class ModSettingsProtocol : PacketProtocolRequestToServer {
		public CapitalismConfigData Settings;


		////////////////

		protected ModSettingsProtocol( PacketProtocolDataConstructorLock ctor_lock ) : base( ctor_lock ) { }

		////////////////

		protected override void InitializeServerSendData( int to_who ) {
			this.Settings = CapitalismMod.Instance.Config;
		}

		////////////////

		protected override void ReceiveReply() {
			CapitalismMod.Instance.ConfigJson.SetData( this.Settings );
		}
	}
}
