using HamstarHelpers.Components.Config;
using HamstarHelpers.Helpers.DebugHelpers;
using System;
using Terraria.ModLoader;


namespace Capitalism {
	partial class CapitalismMod : Mod {
		public static CapitalismMod Instance { get; private set; }
		


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
				LogHelpers.Log( "Capitalism updated to " + CapitalismConfigData.ConfigVersion.ToString() );
				this.ConfigJson.SaveFile();
			}
		}

		public override void Unload() {
			CapitalismMod.Instance = null;
		}


		////////////////

		public override object Call( params object[] args ) {
			if( args.Length == 0 ) { throw new Exception( "Undefined call type." ); }

			string call_type = args[0] as string;
			if( args == null ) { throw new Exception( "Invalid call type." ); }

			var new_args = new object[args.Length - 1];
			Array.Copy( args, 1, new_args, 0, args.Length - 1 );

			return CapitalismAPI.Call( call_type, new_args );
		}
	}
}
