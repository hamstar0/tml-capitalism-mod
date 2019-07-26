using HamstarHelpers.Helpers.User;
using System;
using System.ComponentModel;
using Terraria;
using Terraria.ModLoader.Config;


namespace Capitalism {
	public class CapitalismConfig : ModConfig {
		public override ConfigScope Mode => ConfigScope.ServerSide;



		////////////////

		[DefaultValue( true )]
		public bool Enabled = true;


		[Tooltip( "A factor for computing markup." )]
		[Range( Single.MinValue, Single.MaxValue )]
		[DefaultValue( 0.8f )]
		public float MarkupExponent = 0.8f;

		[Tooltip( "A factor for computing markup." )]
		[Range(Single.Epsilon, Single.MaxValue)]
		[DefaultValue( 50f )]
		public float MarkupDivisor = 50f;

		[Tooltip("% markup if a Tax Collector NPC has moved in.")]
		[Range( Single.Epsilon, Single.MaxValue )]
		[DefaultValue( 1.02f )]
		public float TaxMarkupPercent = 1.02f;

		[Tooltip("% markup after an NPC has died.")]
		[Range( Single.Epsilon, Single.MaxValue )]
		[DefaultValue( 1.5f )]
		public float InfuriationMarkupPercent = 1.5f;


		[Tooltip("% that markup prices 'decay' twice per day.")]
		[Range( Single.Epsilon, Single.MaxValue )]
		[DefaultValue( 0.95f )]
		public float BiDailyDecayMarkdownPercent = 0.95f;


		[Tooltip("% scale during a blood moon from female NPCs.")]
		[Range( Single.Epsilon, Single.MaxValue )]
		[DefaultValue( 1.1f )]
		public float FemaleBloodMoonSellPricePercent = 1.1f;

		[Tooltip( "% scale from a 'lovestruck' NPC." )]
		[Range( Single.Epsilon, Single.MaxValue )]
		[DefaultValue( 0.9f )]
		public float LovestruckSellPricePercent = 0.9f;

		[Tooltip( "% scale from a 'stinky' NPC." )]
		[Range( Single.Epsilon, Single.MaxValue )]
		[DefaultValue( 1.1f )]
		public float StinkySellPricePercent = 1.1f;



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
