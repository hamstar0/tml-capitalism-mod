using HamstarHelpers.Classes.UI.ModConfig;
using HamstarHelpers.Helpers.User;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using Terraria;
using Terraria.ModLoader.Config;


namespace Capitalism {
	class MyFloatInputElement : FloatInputElement { }




	public class CapitalismConfig : ModConfig {
		public override ConfigScope Mode => ConfigScope.ServerSide;



		////////////////

		[DefaultValue( true )]
		public bool Enabled { get; set; } = true;


		[Header( "Formula for computing markup:\n"+
			"  b=base value\n" +
			"  t=total purchases\n"+
			"  a=exponent value (default 0.8)\n"+
			"  c=divsor value (default 50)\n \n"+
			"b + (b * t)^a / c" )]
		[JsonIgnore]
		[Label("Markup (+copper) for item (cost: 10 silv), bought 20x")]
		public float MarkupResult1000 { get {
			return 1000 + ( (float)Math.Pow(1000 * 20, this.MarkupExponent) / this.MarkupDivisor );
		} }
		[JsonIgnore]
		[Label("Markup (+copper) for item (cost: 1 plat), bought 20x")]
		public float MarkupResult1000000 { get {
			return 1000000 + ( (float)Math.Pow(1000000 * 20, this.MarkupExponent) / this.MarkupDivisor );
		} }


		[Tooltip( "A factor for computing markup." )]
		[Range( 0f, 5f )]
		[DefaultValue( 0.8f )]
		[CustomModConfigItem( typeof( MyFloatInputElement ) )]
		public float MarkupExponent { get; set; } = 0.8f;

		[Tooltip( "A factor for computing markup." )]
		[Range( Single.Epsilon, 1000f )]
		[DefaultValue( 50f )]
		[CustomModConfigItem( typeof( MyFloatInputElement ) )]
		public float MarkupDivisor { get; set; } = 50f;

		[Tooltip("% markup if a Tax Collector NPC has moved in.")]
		[Range( 0f, 10f )]
		[DefaultValue( 1.02f )]
		[CustomModConfigItem( typeof( MyFloatInputElement ) )]
		public float TaxMarkupPercent { get; set; } = 1.02f;

		[Tooltip("% markup after an NPC has died.")]
		[Range( 0f, 10f )]
		[DefaultValue( 1.5f )]
		[CustomModConfigItem( typeof( MyFloatInputElement ) )]
		public float InfuriationMarkupPercent { get; set; } = 1.5f;


		[Tooltip("% that markup prices 'decay' to, twice per day.")]
		[Range( 0f, 10f )]
		[DefaultValue( 0.95f )]
		[CustomModConfigItem( typeof( MyFloatInputElement ) )]
		public float BiDailyDecayMarkdownPercent { get; set; } = 0.95f;


		[Tooltip("% scale during a blood moon from female NPCs.")]
		[Range( 0f, 10f )]
		[DefaultValue( 1.1f )]
		[CustomModConfigItem( typeof( MyFloatInputElement ) )]
		public float FemaleBloodMoonSellPricePercent { get; set; } = 1.1f;

		[Tooltip( "% scale from a 'lovestruck' NPC." )]
		[Range( 0f, 10f )]
		[DefaultValue( 0.9f )]
		[CustomModConfigItem( typeof( MyFloatInputElement ) )]
		public float LovestruckSellPricePercent { get; set; } = 0.9f;

		[Tooltip( "% scale from a 'stinky' NPC." )]
		[Range( 0f, 10f )]
		[DefaultValue( 1.1f )]
		[CustomModConfigItem( typeof( MyFloatInputElement ) )]
		public float StinkySellPricePercent { get; set; } = 1.1f;



		////////////////

		public override bool AcceptClientChanges( ModConfig pendingConfig, int whoAmI, ref string message ) {
			if( !UserHelpers.HasBasicServerPrivilege(Main.player[whoAmI]) ) {
				message = "Insufficient privilege.";
				return false;
			}
			return true;
		}
	}
}
