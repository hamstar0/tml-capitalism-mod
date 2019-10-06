using HamstarHelpers.Helpers.TModLoader.Mods;
using System;
using Terraria.ModLoader;


namespace Capitalism {
	partial class CapitalismMod : Mod {
		public static CapitalismMod Instance { get; private set; }



		////////////////

		public CapitalismConfig Config => ModContent.GetInstance<CapitalismConfig>();



		////////////////

		public CapitalismMod() { }

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
