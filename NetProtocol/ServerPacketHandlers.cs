using HamstarHelpers.DebugHelpers;
using System.IO;
using Terraria;
using Terraria.ModLoader;


namespace Capitalism.NetProtocol {
	public static class ServerPacketHandlers {
		public static void HandlePacket( CapitalismMod mymod, BinaryReader reader, int player_who ) {
			NetProtocolTypes protocol = (NetProtocolTypes)reader.ReadByte();

			switch( protocol ) {
			case NetProtocolTypes.RequestModSettings:
				ServerPacketHandlers.ReceiveModSettingsRequestOnServer( mymod, reader, player_who );
				break;
			default:
				/*if( mymod.IsDebugInfoMode() ) {*/ DebugHelpers.Log( "RouteReceivedServerPackets ...? " + protocol ); //}
				break;
			}
		}


		
		////////////////
		// Server Senders
		////////////////

		public static void SendModSettingsFromServer( CapitalismMod mymod, Player player ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.ModSettings );
			packet.Write( (string)mymod.Config.SerializeMe() );

			packet.Send( (int)player.whoAmI );
		}


		
		////////////////
		// Server Receivers
		////////////////

		private static void ReceiveModSettingsRequestOnServer( CapitalismMod mymod, BinaryReader reader, int player_who ) {
			if( Main.netMode != 2 ) { return; } // Server only

			ServerPacketHandlers.SendModSettingsFromServer( mymod, Main.player[player_who] );
		}
	}
}
