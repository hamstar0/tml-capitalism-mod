using System.Linq;
using System.Collections.Generic;
using Terraria;
using System;
using HamstarHelpers.NPCHelpers;


namespace Capitalism.Logic {
	public class VendorLogic {
		public int NpcType { get; private set; }
		public IDictionary<int, long> BasePrices { get; private set; }
		public IDictionary<int, float> TotalPurchases { get; private set; }
		public IDictionary<int, float> TotalSpendings { get; private set; }
		

		////////////////

		public static VendorLogic Create( int npc_type ) {
			Chest shop = NPCHelpers.GetShop( npc_type );
			if( shop == null ) { return null; }

			return new VendorLogic( npc_type );
		}


		////////////////

		private VendorLogic( int npc_type ) {
			this.NpcType = npc_type;

			this.TotalPurchases = new Dictionary<int, float>();
			this.TotalSpendings = new Dictionary<int, float>();
			this.BasePrices = new Dictionary<int, long>();

			Chest shop = NPCHelpers.GetShop( this.NpcType );
			ISet<int> found_types = new HashSet<int>();

			for( int i=0; i<shop.item.Length; i++ ) {
				Item item = shop.item[i];
				if( item == null || item.type == 0 ) { break; }
				if( found_types.Contains( item.type ) ) { continue; }
				found_types.Add( item.type );

				item.SetDefaults( item.type );

				this.BasePrices[ item.type ] = item.value;
				this.TotalPurchases[item.type] = 0;
				this.TotalSpendings[item.type] = 0;
			}
		}

		////////////////

		public void LoadTotalPurchases( CapitalismMod mymod, int[] total_purchase_types, int[] total_spendings_types, float[] total_purchases, float[] total_spendings ) {
			for( int i = 0; i < total_purchases.Length; i++ ) {
				int item_type = total_purchase_types[i];
				this.TotalPurchases[item_type] = (float)total_purchases[i];
			}
			for( int i = 0; i < total_spendings.Length; i++ ) {
				int item_type = total_spendings_types[i];
				this.TotalSpendings[item_type] = (float)total_spendings[i];
			}

			this.UpdateShop( mymod );
		}

		public void SaveTotalSpendings( out int[] total_purchase_types, out int[] total_spendings_types, out float[] total_purchases, out float[] total_spendings ) {
			total_purchase_types = this.TotalPurchases.Keys.ToArray();
			total_purchases = this.TotalPurchases.Values.ToArray();
			total_spendings_types = this.TotalSpendings.Keys.ToArray();
			total_spendings = this.TotalSpendings.Values.ToArray();
		}

		////////////////

		public void UpdateShop( CapitalismMod mymod, Chest shop = null ) {
			if( shop == null ) {
				shop = NPCHelpers.GetShop( this.NpcType );
				if( shop == null ) { return; }
			}

			ISet<int> found_types = new HashSet<int>();

			for( int i=0; i<shop.item.Length; i++ ) {
				Item item = shop.item[i];
				if( item == null || item.type <= 0 || item.stack <= 0 ) { break; }

				// Update only the first instance of an item
				if( found_types.Contains(item.type) ) { continue; }
				found_types.Add( item.type );

				int value = this.UpdateShopItem( mymod, item, shop );
				double base_price = (double)this.BasePrices[ item.type ];
				double markup = ((double)value - base_price) / base_price;

				item.value = value;

				var item_info = item.GetGlobalItem<CapitalismItemInfo>( mymod );
				item_info.MarkupPercentPlus = markup;
			}
		}

		private int UpdateShopItem( CapitalismMod mymod, Item item, Chest shop = null ) {
			// Compute new price
			int price = (int)this.GetPriceOf( mymod, item.type );

			// Female NPCs during a bloodmoon markup their prices
			bool is_grill = NPCHelpers.GetFemaleTownNpcTypes().Contains( this.NpcType );
			if( Main.bloodMoon && is_grill ) {
				price = (int)((float)price * mymod.Config.Data.FemaleBloodMoonMarkupPercent);
			}

			NPC npc = NPCFinderHelpers.FindFirstNpcByType( this.NpcType );
			Player player = Main.player[Main.myPlayer];

			if( npc != null && player != null ) {
				// Stinky players markup prices
				if( player.FindBuffIndex( 120 ) >= 0 ) {
					price = (int)((float)price * mymod.Config.Data.StinkyMarkupPercent);
				}

				// Love struck NPCs markdown prices
				if( npc.FindBuffIndex( 119 ) >= 0 ) {
					bool is_gendered = !NPCHelpers.GetNonGenderedTownNpcTypes().Contains( this.NpcType );

					if( is_gendered && (player.Male && is_grill) || (!player.Male && !is_grill) ) {
						price = (int)((float)price * mymod.Config.Data.LovestruckMarkdownPercent);
					}
				}
			}

			return price;
		}

		////////////////

		public float GetPriceOf( CapitalismMod mymod, int item_type ) {
			// Register the initial base price of an item once and for all
			if( !this.BasePrices.Keys.Contains( item_type ) ) {
				Item item = new Item();
				item.SetDefaults( item_type );
				this.BasePrices[item_type] = item.value;
			}
			if( !this.TotalSpendings.Keys.Contains( item_type ) ) {
				this.TotalPurchases[item_type] = 0;
				this.TotalSpendings[item_type] = 0;
			}

			long base_price = this.BasePrices[item_type];
			float total_purchases = this.TotalPurchases[item_type];
			float total_spendings = this.TotalSpendings[item_type];
			return VendorLogic.ComputePrice( mymod, (float)base_price, total_purchases, total_spendings );
		}


		public void AddPurchase( CapitalismMod mymod, int item_type ) {
			float price = this.GetPriceOf( mymod, item_type );
			if( price == 0 ) { return; }

			this.TotalSpendings[ item_type ] += price;
			this.TotalPurchases[ item_type ] += 1;
//Main.NewText( "  ");
//Main.NewText( "item_type "+ item_type);
//Main.NewText( "base_price "+ base_price);
//Main.NewText( "purchases "+ total_purchases);
//Main.NewText( "price " + price);

			this.UpdateShop( mymod );
		}
		
		public void DecayPrices( CapitalismMod mymod ) {
			float rate = mymod.Config.Data.BiDailyDecayMarkdownPercent;

			foreach( int item_type in this.TotalPurchases.Keys.ToList() ) {
				this.TotalPurchases[item_type] *= rate;
				this.TotalSpendings[item_type] *= rate;
			}
		}
		
		public void Infuriate( CapitalismMod mymod ) {
			float rate = mymod.Config.Data.InfuriationMarkupPercent;

			foreach( int item_type in this.TotalPurchases.Keys.ToList() ) {
				this.TotalPurchases[item_type] *= rate;
				this.TotalSpendings[item_type] *= rate;
			}
		}

		////////////////

		private static float ComputePrice( CapitalismMod mymod, float base_price, float purchases, float spendings ) {
			// v1 formula: b + ( b * t * 0.1 * (0.996 ^ t) )
			// v2 formula: b + ( b * t * 0.02 * (0.996 ^ ((t/4) + (s/10000))) )
			// v3 formula: b + ( (b * t)^0.8 ) / 50

			float exp = mymod.Config.Data.MarkupExponent;
			float div = mymod.Config.Data.MarkupDivisor;

			//if( spendings == 0 ) { spendings = float.Epsilon; }
			float markup = (float)Math.Pow( (base_price * purchases), exp ) / div;

			if( NPC.taxCollector ) {
				markup *= mymod.Config.Data.TaxMarkupPercent;  // Is it worth it?!?!?!
			}
			
			return base_price + markup;
		}
	}
}
