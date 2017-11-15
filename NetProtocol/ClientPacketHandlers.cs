using HamstarHelpers.DebugHelpers;
using System.IO;
using Terraria;
using Terraria.ModLoader;


namespace Capitalism.NetProtocol {
	static class ClientPacketHandlers {
		public static void HandlePacket( CapitalismMod mymod, BinaryReader reader ) {
			NetProtocolTypes protocol = (NetProtocolTypes)reader.ReadByte();

			switch( protocol ) {
			case NetProtocolTypes.ModSettings:
				ClientPacketHandlers.ReceiveModSettingsOnClient( mymod, reader );
				break;
			default:
				/*if( mymod.IsDebugInfoMode() ) {*/ DebugHelpers.Log( "RouteReceivedClientPackets ...? "+protocol ); //}
				break;
			}
		}



		////////////////
		// Client Senders
		////////////////

		public static void SendModSettingsRequestFromClient( CapitalismMod mymod ) {
			if( Main.netMode != 1 ) { return; } // Clients only

			ModPacket packet = mymod.GetPacket();
			packet.Write( (byte)NetProtocolTypes.RequestModSettings );
			packet.Send();
		}



		////////////////
		// Client Receivers
		////////////////

		private static void ReceiveModSettingsOnClient( CapitalismMod mymod, BinaryReader reader ) {
			if( Main.netMode != 1 ) { return; } // Clients only
			
			mymod.Config.DeserializeMe( reader.ReadString() );
		}
	}
}
