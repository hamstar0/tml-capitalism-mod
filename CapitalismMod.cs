using HamstarHelpers.Helpers.TModLoader.Mods;
using System;
using Terraria.ModLoader;


namespace Capitalism {
	partial class CapitalismMod : Mod {
		public static CapitalismMod Instance { get; private set; }
		


		////////////////

		public CapitalismConfig Config { get; private set; }



		////////////////

		public CapitalismMod() {
			this.Config = new CapitalismConfig();
		}

		////////////////

		public override void Load() {
			CapitalismMod.Instance = this;
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
