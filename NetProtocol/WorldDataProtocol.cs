using HamstarHelpers.Classes.Protocols.Packet.Interfaces;
using Terraria.ModLoader;

namespace Capitalism.NetProtocol {
	class WorldDataProtocol : PacketProtocolRequestToServer {
		public static void QuickRequest() {
			PacketProtocolRequestToServer.QuickRequest<WorldDataProtocol>( -1 );
		}



		////////////////

		public string OldID;



		////////////////

		private WorldDataProtocol() { }

		////////////////

		protected override void InitializeServerSendData( int toWho ) {
			var myworld = ModContent.GetInstance<CapitalismWorld>();

			this.OldID = myworld.ID;
		}

		////////////////

		protected override void ReceiveReply() {
			if( !string.IsNullOrEmpty(this.OldID) ) {
				var myworld = ModContent.GetInstance<CapitalismWorld>();
				myworld._ID = this.OldID;
			}
		}
	}
}
