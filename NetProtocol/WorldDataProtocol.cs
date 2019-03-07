using HamstarHelpers.Components.Network;


namespace Capitalism.NetProtocol {
	class WorldDataProtocol : PacketProtocolRequestToServer {
		public string OldID;


		////////////////

		private WorldDataProtocol() { }

		////////////////

		protected override void InitializeServerSendData( int toWho ) {
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
