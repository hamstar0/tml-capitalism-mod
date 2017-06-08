using System.IO;
using Terraria;
using Terraria.ModLoader;


namespace Capitalism {
	public enum CapitalismNetProtocolTypes : byte {
		RequestModSettings,
		ModSettings
	}



	public static class CapitalismNetProtocol {
		public static void RoutePacket( CapitalismMod mymod, BinaryReader reader ) {
			CapitalismNetProtocolTypes protocol = (CapitalismNetProtocolTypes)reader.ReadByte();

			switch( protocol ) {
			case CapitalismNetProtocolTypes.RequestModSettings:
				CapitalismNetProtocol.ReceiveSettingsRequestOnServer( mymod, reader );
				break;
			case CapitalismNetProtocolTypes.ModSettings:
				CapitalismNetProtocol.ReceiveSettingsOnClient( mymod, reader );
				break;
			default:
				ErrorLogger.Log( "Invalid packet protocol: " + protocol );
				break;
			}
		}



		////////////////////////////////
		// Senders (Client)
		////////////////////////////////

		public static void SendSettingsRequestFromClient( CapitalismMod mymod, Player player ) {
			if( Main.netMode != 1 ) { return; } // Clients only

			ModPacket packet = mymod.GetPacket();
			packet.Write( (byte)CapitalismNetProtocolTypes.RequestModSettings );
			packet.Write( (int)player.whoAmI );
			packet.Send();
		}

		////////////////////////////////
		// Senders (Server)
		////////////////////////////////

		public static void SendSettingsFromServer( CapitalismMod mymod, int who ) {
			if( Main.netMode != 2 ) { return; } // Server only

			ModPacket packet = mymod.GetPacket();
			packet.Write( (byte)CapitalismNetProtocolTypes.ModSettings );
			packet.Write( (string)mymod.Config.SerializeMe() );

			packet.Send( who );
		}



		////////////////////////////////
		// Recipients (Server)
		////////////////////////////////

		private static void ReceiveSettingsRequestOnServer( CapitalismMod mymod, BinaryReader reader ) {
			if( Main.netMode != 2 ) { return; } // Server only

			int who = reader.ReadInt32();
			if( who < 0 || who >= Main.player.Length || Main.player[who] == null ) {
				ErrorLogger.Log( "CapitalismNetProtocol.ReceiveSettingsRequestOnServer - Invalid player whoAmI. " + who );
				return;
			}

			CapitalismNetProtocol.SendSettingsFromServer( mymod, who );
		}

		////////////////////////////////
		// Recipients (Client)
		////////////////////////////////

		private static void ReceiveSettingsOnClient( CapitalismMod mymod, BinaryReader reader ) {
			if( Main.netMode != 1 ) { return; } // Clients only
			
			mymod.Config.DeserializeMe( reader.ReadString() );
		}
	}
}
