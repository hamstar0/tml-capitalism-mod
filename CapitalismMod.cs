using Capitalism.NetProtocol;
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

		public static string ConfigFileRelativePath {
			get { return ConfigurationDataBase.RelativePath + Path.DirectorySeparatorChar + CapitalismConfigData.ConfigFileName; }
		}
		public static void ReloadConfigFromFile() {
			if( Main.netMode != 0 ) {
				throw new Exception( "Cannot reload configs outside of single player." );
			}

			if( CapitalismMod.Instance != null ) {
				if( !CapitalismMod.Instance.ConfigJson.LoadFile() ) {
					CapitalismMod.Instance.ConfigJson.SaveFile();
				}
			}
		}


		////////////////

		public JsonConfig<CapitalismConfigData> ConfigJson { get; private set; }
		public CapitalismConfigData Config { get { return this.ConfigJson.Data; } }


		////////////////

		public CapitalismMod() {
			this.Properties = new ModProperties() {
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};

			string filename = "Capitalism Config.json";
			this.ConfigJson = new JsonConfig<CapitalismConfigData>( filename, ConfigurationDataBase.RelativePath, new CapitalismConfigData() );
		}

		////////////////

		public override void Load() {
			CapitalismMod.Instance = this;

			this.LoadConfig();
		}

		private void LoadConfig() {
			if( !this.ConfigJson.LoadFile() ) {
				this.ConfigJson.SaveFile();
			}

			if( this.Config.UpdateToLatestVersion() ) {
				ErrorLogger.Log( "Capitalism updated to " + CapitalismConfigData.ConfigVersion.ToString() );
				this.ConfigJson.SaveFile();
			}
		}

		public override void Unload() {
			CapitalismMod.Instance = null;
		}
	}
}
