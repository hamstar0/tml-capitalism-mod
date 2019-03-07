using HamstarHelpers.Components.Config;
using HamstarHelpers.Helpers.DebugHelpers;
using HamstarHelpers.Helpers.TmlHelpers.ModHelpers;
using System;
using Terraria.ModLoader;


namespace Capitalism {
	partial class CapitalismMod : Mod {
		public static CapitalismMod Instance { get; private set; }
		


		////////////////

		public JsonConfig<CapitalismConfigData> ConfigJson { get; private set; }
		public CapitalismConfigData Config => this.ConfigJson.Data;


		////////////////

		public CapitalismMod() {
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
				LogHelpers.Log( "Capitalism updated to " + this.Version.ToString() );
				this.ConfigJson.SaveFile();
			}
		}

		public override void Unload() {
			CapitalismMod.Instance = null;
		}


		////////////////

		public override object Call( params object[] args ) {
			return ModBoilerplateHelpers.HandleModCall( typeof(CapitalismAPI), args );
		}
	}
}
