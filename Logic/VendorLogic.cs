using System.Linq;
using System.Collections.Generic;
using Terraria;
using Utils;
using System;


namespace Capitalism.Logic {
	public class VendorLogic {
		public int NpcType { get; private set; }
		public IDictionary<int, long> BasePrices { get; private set; }
		public IDictionary<int, float> TotalSpendings { get; private set; }
		

		////////////////

		public static VendorLogic Create( int npc_type ) {
			Chest shop = NPCHelper.GetShop( npc_type );
			if( shop == null ) { return null; }

			return new VendorLogic( npc_type );
		}


		////////////////

		private VendorLogic( int npc_type ) {
			this.NpcType = npc_type;

			this.TotalSpendings = new Dictionary<int, float>();
			this.BasePrices = new Dictionary<int, long>();

			Chest shop = NPCHelper.GetShop( this.NpcType );
			ISet<int> found_types = new HashSet<int>();

			for( int i=0; i<shop.item.Length; i++ ) {
				Item item = shop.item[i];
				if( item == null || item.type == 0 ) { break; }
				if( found_types.Contains( item.type ) ) { continue; }
				found_types.Add( item.type );

				item.SetDefaults( item.type );

				this.BasePrices[ item.type ] = item.value;
				this.TotalSpendings[item.type] = 0;
			}
		}

		////////////////

		public void LoadTotalPurchases( CapitalismMod mymod, int[] total_purchase_types, float[] total_purchases ) {
			for( int i = 0; i < total_purchases.Length; i++ ) {
				int item_type = total_purchase_types[i];
				float total_spendings = total_purchases[i];

				this.TotalSpendings[item_type] = (float)total_spendings;
			}

			this.UpdateShop( mymod );
		}

		public void SaveTotalSpendings( out int[] total_spendings_types, out float[] total_spendings ) {
			total_spendings_types = this.TotalSpendings.Keys.ToArray();
			total_spendings = this.TotalSpendings.Values.ToArray();
		}

		////////////////

		public void UpdateShop( CapitalismMod mymod, Chest shop = null ) {
			if( shop == null ) {
				shop = NPCHelper.GetShop( this.NpcType );
				if( shop == null ) { return; }
			}

			ISet<int> found_types = new HashSet<int>();

			for( int i=0; i<shop.item.Length; i++ ) {
				Item item = shop.item[i];
				if( item == null || item.type <= 0 || item.stack <= 0 ) { break; }

				// Update only the first instance of an item
				if( found_types.Contains(item.type) ) { continue; }
				found_types.Add( item.type );

				// Register the initial base price of an item once and for all
				if( !this.BasePrices.Keys.Contains(item.type) ) {
					item.SetDefaults( item.type );
					this.BasePrices[item.type] = item.value;
				}
				if( !this.TotalSpendings.Keys.Contains( item.type ) ) {
					this.TotalSpendings[item.type] = 0;
				}

				// Compute new price
				long base_price = this.BasePrices[item.type];
				float spendings = this.TotalSpendings[item.type];
				long price = VendorLogic.ComputePrice( mymod, base_price, spendings );

				// Female NPCs during a bloodmoon markup their prices
				bool is_grill = NPCHelper.GetFemaleTownNpcTypes().Contains( this.NpcType );
				if( Main.bloodMoon && is_grill ) {
					price = (long)((float)price * mymod.Config.Data.FemaleBloodMoonMarkupPercent);
				}

				NPC npc = NPCHelper.FindFirstNpcByType( this.NpcType );
				Player player = Main.player[Main.myPlayer];

				if( npc != null && player != null ) {
					// Stinky players markup prices
					if( player.FindBuffIndex(120) >= 0 ) {
						price = (long)((float)price * mymod.Config.Data.StinkyMarkupPercent);
					}
					
					// Love struck NPCs markdown prices
					if( npc.FindBuffIndex(119) >= 0 ) {
						bool is_gendered = !NPCHelper.GetNonGenderedTownNpcTypes().Contains( this.NpcType );

						if( is_gendered && (player.Male && is_grill) || (!player.Male && !is_grill) ) {
							price = (long)((float)price * mymod.Config.Data.LovestruckMarkdownPercent);
						}
					}
				}

				item.value = (int)price;
			}
		}
		
		////////////////
		
		public void AddPurchase( CapitalismMod mymod, int item_type ) {
			if( !this.BasePrices.Keys.Contains( item_type ) ) { return; }
			
			this.TotalSpendings[ item_type ] = VendorLogic.ComputePrice( mymod, this.BasePrices[item_type], this.TotalSpendings[item_type] );

			this.UpdateShop( mymod );
		}
		
		public void DecayPrices( CapitalismMod mymod ) {
			double rate = mymod.Config.Data.BiDailyDecayPercent;

			foreach( int item_type in this.TotalSpendings.Keys.ToList() ) {
				this.TotalSpendings[item_type] = (int)((double)this.TotalSpendings[item_type] * rate);	// - 1?
				if( this.TotalSpendings[item_type] < 0 ) {
					this.TotalSpendings[item_type] = 0;
				}
			}
		}
		
		public void Infuriate( CapitalismMod mymod ) {
			foreach( int item_types in this.TotalSpendings.Keys.ToList() ) {
				this.TotalSpendings[item_types] *= mymod.Config.Data.InfuriateMultiplier;
			}
		}

		////////////////

		private static long ComputePrice( CapitalismMod mymod, long base_price, float total_spendings ) {
			// Old formula: b + ( b * t * 0.1 * (0.996 ^ t) )
			// New formula: b + ( b * (s/10000) * (0.996 ^ (s/1000)) )


			double markup_cap = Math.Pow( mymod.Config.Data.MarkupCapExponentBase, (total_spendings / 1000f) );
			double markup = base_price * (total_spendings / 10000f) * markup_cap * mymod.Config.Data.MarkupMultiplier;
			
			long price = base_price + (int)markup;
			if( NPC.taxCollector ) {
				price = (long)( (double)price * (double)mymod.Config.Data.TaxMarkupPercent );  // Is it worth it?!?!?!
			}

			return price;
		}
	}
}
