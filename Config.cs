using HamstarHelpers.Utilities.Config;
using System;


namespace Capitalism {
	public class CapitalismConfigData : ConfigurationDataBase {
		public readonly static Version ConfigVersion = new Version( 1, 2, 0 );
		public readonly static string ConfigFileName = "Capitalism Config.json";


		////////////////

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



		////////////////

		public bool UpdateToLatestVersion() {
			var new_config = new CapitalismConfigData();
			var vers_since = this.VersionSinceUpdate != "" ?
				new Version( this.VersionSinceUpdate ) :
				new Version();

			if( vers_since >= CapitalismConfigData.ConfigVersion ) {
				return false;
			}

			if( vers_since < new Version(1, 2, 1) ) {
				this.FemaleBloodMoonMarkupPercent = new_config.FemaleBloodMoonMarkupPercent;
				this.LovestruckMarkdownPercent = new_config.LovestruckMarkdownPercent;
				this.StinkyMarkupPercent = new_config.StinkyMarkupPercent;
			}

			this.VersionSinceUpdate = CapitalismConfigData.ConfigVersion.ToString();

			return true;
		}
	}
}
