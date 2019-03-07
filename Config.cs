using HamstarHelpers.Components.Config;
using System;


namespace Capitalism {
	public class CapitalismConfigData : ConfigurationDataBase {
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
			var mymod = CapitalismMod.Instance;
			var newConfig = new CapitalismConfigData();
			var versSince = this.VersionSinceUpdate != "" ?
				new Version( this.VersionSinceUpdate ) :
				new Version();

			if( versSince >= mymod.Version ) {
				return false;
			}

			this.VersionSinceUpdate = mymod.Version.ToString();

			return true;
		}
	}
}
