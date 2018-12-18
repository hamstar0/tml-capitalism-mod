using HamstarHelpers.Components.Network;
using HamstarHelpers.Components.Network.Data;


namespace Capitalism.NetProtocol {
	class WorldDataProtocol : PacketProtocolRequestToServer {
		public string OldID;


		////////////////

		protected WorldDataProtocol( PacketProtocolDataConstructorLock ctor_lock ) : base( ctor_lock ) { }

		////////////////

		protected override void InitializeServerSendData( int to_who ) {
			var myworld = CapitalismMod.Instance.GetModWorld<CapitalismWorld>();

			this.OldID = myworld.ID;
		}

		////////////////

		protected override void ReceiveReply() {
			if( !string.IsNullOrEmpty(this.OldID) ) {
				var myworld = CapitalismMod.Instance.GetModWorld<CapitalismWorld>();
				myworld._ID = this.OldID;
			}
		}
	}
}
