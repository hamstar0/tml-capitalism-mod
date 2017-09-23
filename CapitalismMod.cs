using Capitalism.NetProtocol;
using HamstarHelpers.DebugHelpers;
using HamstarHelpers.Utilities.Config;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;


namespace Capitalism {
	public class CapitalismMod : Mod {
		public readonly static Version ConfigVersion = new Version(1, 3, 3);
		public JsonConfig<CapitalismConfigData> Config { get; private set; }


		public CapitalismMod() {
			try {
				this.Properties = new ModProperties() {
					Autoload = true,
					AutoloadGores = true,
					AutoloadSounds = true
				};

				string filename = "Capitalism Config.json";
				this.Config = new JsonConfig<CapitalismConfigData>( filename, "Mod Configs", new CapitalismConfigData() );
			} catch( Exception e ) {
				ErrorLogger.Log( e.ToString() );
				throw e;
			}
		}

		public override void Load() {
			var hamhelpmod = ModLoader.GetMod( "HamstarHelpers" );
			var min_vers = new Version( 1, 1, 0 );

			if( hamhelpmod.Version < min_vers ) {
				throw new Exception( "Hamstar's Helpers must be version " + min_vers.ToString() + " or greater." );
			}

			this.LoadConfig();
		}

		private void LoadConfig() {
			try {
				if( !this.Config.LoadFile() ) {
					this.Config.SaveFile();
				}
			} catch( Exception e ) {
				DebugHelpers.Log( e.Message );
				this.Config.SaveFile();
			}

			if( this.Config.Data.UpdateToLatestVersion() ) {
				ErrorLogger.Log( "Capitalism updated to " + CapitalismConfigData.ConfigVersion.ToString() );
				this.Config.SaveFile();
			}
		}


		////////////////

		public override void HandlePacket( BinaryReader reader, int player_who ) {
			if( Main.netMode == 1 ) {   // Client
				ClientPacketHandlers.HandlePacket( this, reader );
			} else if( Main.netMode == 2 ) {    // Server
				ServerPacketHandlers.HandlePacket( this, reader, player_who );
			}
		}
	}
}
