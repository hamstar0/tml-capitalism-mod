using System.Linq;
using System.Collections.Generic;
using Terraria;
using System;
using HamstarHelpers.Helpers.NPCHelpers;


namespace Capitalism.Logic {
	class VendorLogic {
		public int NpcType { get; private set; }
		public IDictionary<int, long> BasePrices { get; private set; }
		public IDictionary<int, float> TotalPurchases { get; private set; }
		public IDictionary<int, float> TotalSpendings { get; private set; }
		


		////////////////

		public static VendorLogic Create( int npcType ) {
			Chest shop = NPCTownHelpers.GetShop( npcType );
			if( shop == null ) { return null; }

			return new VendorLogic( npcType );
		}


		////////////////

		private VendorLogic( int npcType ) {
			this.NpcType = npcType;

			this.TotalPurchases = new Dictionary<int, float>();
			this.TotalSpendings = new Dictionary<int, float>();
			this.BasePrices = new Dictionary<int, long>();

			Chest shop = NPCTownHelpers.GetShop( this.NpcType );
			ISet<int> foundTypes = new HashSet<int>();

			for( int i=0; i<shop.item.Length; i++ ) {
				Item item = shop.item[i];
				if( item == null || item.type == 0 ) { break; }
				if( foundTypes.Contains( item.type ) ) { continue; }
				foundTypes.Add( item.type );

				item.SetDefaults( item.type );

				this.BasePrices[ item.type ] = item.value;
				this.TotalPurchases[item.type] = 0;
				this.TotalSpendings[item.type] = 0;
			}
		}

		////////////////

		public void LoadTotalPurchases( int[] totalPurchaseTypes, int[] totalSpendingsTypes, float[] totalPurchases, float[] totalSpendings ) {
			for( int i = 0; i < totalPurchases.Length; i++ ) {
				int itemType = totalPurchaseTypes[i];
				this.TotalPurchases[itemType] = (float)totalPurchases[i];
			}
			for( int i = 0; i < totalSpendings.Length; i++ ) {
				int itemType = totalSpendingsTypes[i];
				this.TotalSpendings[itemType] = (float)totalSpendings[i];
			}

			this.UpdateShop();
		}

		public void SaveTotalSpendings( out int[] totalPurchaseTypes, out int[] totalSpendingsTypes, out float[] totalPurchases, out float[] totalSpendings ) {
			totalPurchaseTypes = this.TotalPurchases.Keys.ToArray();
			totalPurchases = this.TotalPurchases.Values.ToArray();
			totalSpendingsTypes = this.TotalSpendings.Keys.ToArray();
			totalSpendings = this.TotalSpendings.Values.ToArray();
		}

		////////////////

		public void UpdateShop( Chest shop = null ) {
			if( shop == null ) {
				shop = NPCTownHelpers.GetShop( this.NpcType );
				if( shop == null ) { return; }
			}

			ISet<int> foundTypes = new HashSet<int>();

			for( int i=0; i<shop.item.Length; i++ ) {
				Item item = shop.item[i];
				if( item == null || item.type <= 0 || item.stack <= 0 ) { break; }

				// Update only the first instance of an item
				if( foundTypes.Contains(item.type) ) { continue; }
				foundTypes.Add( item.type );

				int value = this.UpdateShopItem( item, shop );
				double basePrice = (double)this.BasePrices[ item.type ];
				double markup = ((double)value - basePrice) / basePrice;

				item.value = value;

				var itemInfo = item.GetGlobalItem<CapitalismItemInfo>();
				itemInfo.MarkupPercentPlus = markup;
			}
		}

		private int UpdateShopItem( Item item, Chest shop = null ) {
			var mymod = CapitalismMod.Instance;

			// Compute new price
			int price = (int)this.GetPriceOf( item.type );

			// Female NPCs during a bloodmoon markup their prices
			bool isGrill = NPCTownHelpers.GetFemaleTownNpcTypes().Contains( this.NpcType );
			if( Main.bloodMoon && isGrill ) {
				price = (int)((float)price * mymod.Config.FemaleBloodMoonMarkupPercent);
			}

			NPC npc = NPCFinderHelpers.FindFirstNpcByType( this.NpcType );
			Player player = Main.player[Main.myPlayer];

			if( npc != null && player != null ) {
				// Stinky players markup prices
				if( player.FindBuffIndex( 120 ) >= 0 ) {
					price = (int)((float)price * mymod.Config.StinkyMarkupPercent);
				}

				// Love struck NPCs markdown prices
				if( npc.FindBuffIndex( 119 ) >= 0 ) {
					bool isGendered = !NPCTownHelpers.GetNonGenderedTownNpcTypes().Contains( this.NpcType );

					if( isGendered && (player.Male && isGrill) || (!player.Male && !isGrill) ) {
						price = (int)((float)price * mymod.Config.LovestruckMarkdownPercent);
					}
				}
			}

			return price;
		}

		////////////////

		public float GetPriceOf( int itemType ) {
			// Register the initial base price of an item once and for all
			if( !this.BasePrices.Keys.Contains( itemType ) ) {
				Item item = new Item();
				item.SetDefaults( itemType );
				this.BasePrices[itemType] = item.value;
			}
			if( !this.TotalSpendings.Keys.Contains( itemType ) ) {
				this.TotalPurchases[itemType] = 0;
				this.TotalSpendings[itemType] = 0;
			}

			long basePrice = this.BasePrices[itemType];
			float totalPurchases = this.TotalPurchases[itemType];
			float totalSpendings = this.TotalSpendings[itemType];
			return VendorLogic.ComputePrice( (float)basePrice, totalPurchases, totalSpendings );
		}


		public void AddPurchase( int itemType ) {
			float price = this.GetPriceOf( itemType );
			if( price == 0 ) { return; }

			this.TotalSpendings[ itemType ] += price;
			this.TotalPurchases[ itemType ] += 1;
//Main.NewText( "  ");
//Main.NewText( "item_type "+ item_type);
//Main.NewText( "base_price "+ base_price);
//Main.NewText( "purchases "+ total_purchases);
//Main.NewText( "price " + price);

			this.UpdateShop();
		}
		
		public void DecayPrices() {
			var mymod = CapitalismMod.Instance;
			float rate = mymod.Config.BiDailyDecayMarkdownPercent;

			foreach( int itemType in this.TotalPurchases.Keys.ToList() ) {
				this.TotalPurchases[itemType] *= rate;
				this.TotalSpendings[itemType] *= rate;
			}
		}
		
		public void Infuriate() {
			var mymod = CapitalismMod.Instance;
			float rate = mymod.Config.InfuriationMarkupPercent;

			foreach( int itemType in this.TotalPurchases.Keys.ToList() ) {
				this.TotalPurchases[itemType] *= rate;
				this.TotalSpendings[itemType] *= rate;
			}
		}

		////////////////

		private static float ComputePrice( float basePrice, float purchases, float spendings ) {
			// v1 formula: b + ( b * t * 0.1 * (0.996 ^ t) )
			// v2 formula: b + ( b * t * 0.02 * (0.996 ^ ((t/4) + (s/10000))) )
			// v3 formula: b + ( (b * t)^0.8 ) / 50

			var mymod = CapitalismMod.Instance;
			float exp = mymod.Config.MarkupExponent;
			float div = mymod.Config.MarkupDivisor;

			//if( spendings == 0 ) { spendings = float.Epsilon; }
			float markup = (float)Math.Pow( (basePrice * purchases), exp ) / div;

			if( NPC.taxCollector ) {
				markup *= mymod.Config.TaxMarkupPercent;  // Is it worth it?!?!?!
			}
			
			return basePrice + markup;
		}
	}
}
