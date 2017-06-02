using Utils;
using Utils.JsonConfig;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria.ModLoader;


namespace Capitalism {
	public class ConfigurationData {
		public string VersionSinceUpdate = "";

		public bool Enabled = true;

		public float MarkupExponent = 0.8f;
		public float MarkupDivisor = 50f;
		public float TaxMarkupPercent = 1.02f;
		public float InfuriationMarkupPercent = 1.5f;

		public float BiDailyDecayMarkdownPercent = 0.95f;

		public float FemaleBloodMoonMarkupPercent = 1.1f;
		public float LovestruckMarkdownPercent = 0.9f;
		public float StinkyMarkupPercent = 1.1f;
	}

	

	public class CapitalismMod : Mod {
		public readonly static Version ConfigVersion = new Version(1, 3, 2);
		public JsonConfig<ConfigurationData> Config { get; private set; }


		public CapitalismMod() {
			try {
				this.Properties = new ModProperties() {
					Autoload = true,
					AutoloadGores = true,
					AutoloadSounds = true
				};

				string filename = "Capitalism Config.json";
				this.Config = new JsonConfig<ConfigurationData>( filename, "Mod Configs", new ConfigurationData() );
			} catch( Exception e ) {
				ErrorLogger.Log( e.ToString() );
				throw e;
			}
		}

		public override void Load() {
			var old_config = new JsonConfig<ConfigurationData>( "Capitalism 1.1.0.json", "", new ConfigurationData() );
			// Update old config to new location
			if( old_config.LoadFile() ) {
				old_config.DestroyFile();
				old_config.SetFilePath( this.Config.FileName, "Mod Configs" );
				this.Config = old_config;
			} else if( !this.Config.LoadFile() ) {
				this.Config.SaveFile();
			}

			Version vers_since = this.Config.Data.VersionSinceUpdate != "" ?
				new Version( this.Config.Data.VersionSinceUpdate ) :
				new Version();

			if( vers_since < CapitalismMod.ConfigVersion ) {
				ErrorLogger.Log( "Capitalism config updated to " + CapitalismMod.ConfigVersion.ToString() );

				if( vers_since < new Version( 1, 2, 1 ) ) {
					this.Config.Data.FemaleBloodMoonMarkupPercent = new ConfigurationData().FemaleBloodMoonMarkupPercent;
					this.Config.Data.LovestruckMarkdownPercent = new ConfigurationData().LovestruckMarkdownPercent;
					this.Config.Data.StinkyMarkupPercent = new ConfigurationData().StinkyMarkupPercent;
				}

				this.Config.Data.VersionSinceUpdate = CapitalismMod.ConfigVersion.ToString();
				this.Config.SaveFile();
			}
		}

		////////////////

		public override void HandlePacket( BinaryReader reader, int whoAmI ) {
			try {
				CapitalismNetProtocol.RoutePacket( this, reader );
			} catch( Exception e ) {
				ErrorLogger.Log( e.ToString() );
				throw e;
			}
		}

		////////////////

		public override void PostDrawInterface( SpriteBatch sb ) {
			if( !this.Config.Data.Enabled ) { return; }

			DebugHelper.PrintToBatch( sb );
		}
	}
}
