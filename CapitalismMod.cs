using Capitalism.NetProtocol;
using HamstarHelpers.DebugHelpers;
using HamstarHelpers.Utilities.Config;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;


namespace Capitalism {
	class CapitalismMod : Mod {
		public static CapitalismMod Instance { get; private set; }

		public static string GithubUserName { get { return "hamstar0"; } }
		public static string GithubProjectName { get { return "tml-capitalism-mod"; } }

		public static string ConfigRelativeFilePath {
			get { return ConfigurationDataBase.RelativePath + Path.DirectorySeparatorChar+ CapitalismConfigData.ConfigFileName; }
		}
		public static void ReloadConfigFromFile() {
			if( Main.netMode != 0 ) {
				throw new Exception( "Cannot reload configs outside of single player." );
			}
			CapitalismMod.Instance.Config.LoadFile();
		}


		////////////////

		public JsonConfig<CapitalismConfigData> Config { get; private set; }


		////////////////

		public CapitalismMod() {
			this.Properties = new ModProperties() {
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};

			string filename = "Capitalism Config.json";
			this.Config = new JsonConfig<CapitalismConfigData>( filename, ConfigurationDataBase.RelativePath, new CapitalismConfigData() );
		}

		////////////////

		public override void Load() {
			CapitalismMod.Instance = this;

			var hamhelpmod = ModLoader.GetMod( "HamstarHelpers" );
			var min_vers = new Version( 1, 1, 0 );

			if( hamhelpmod.Version < min_vers ) {
				throw new Exception( "Hamstar Helpers must be version " + min_vers.ToString() + " or greater." );
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

		public override void Unload() {
			CapitalismMod.Instance = null;
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
