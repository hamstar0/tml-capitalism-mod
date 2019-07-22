using HamstarHelpers.Helpers.Debug;
using HamstarHelpers.Helpers.Items;
using HamstarHelpers.Helpers.Players;
using HamstarHelpers.Helpers.World;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader.IO;


namespace Capitalism.Logic {
	class CapitalismLogic {
		private static bool IsDay = false;

		public IDictionary<string, IDictionary<int, VendorLogic>> VendorWorlds { get; private set; }
		private IDictionary<long, double> SellPrices = new Dictionary<long, double>();
		
		private long LastMoney = 0;
		private Item LastBuyItem = null;
		private IDictionary<int, KeyValuePair<int, int>> PrevInventoryInfos = new Dictionary<int, KeyValuePair<int, int>>();
		private KeyValuePair<int, int> PrevMouseInfo;

		private int StartupDelay = 0;



		////////////////

		public CapitalismLogic() {
			this.VendorWorlds = new Dictionary<string, IDictionary<int, VendorLogic>>();
		}

		////////////////

		public void LoadVendorsForCurrentPlayer( CapitalismMod mymod, TagCompound tags, string worldId ) {
			try {
				int vendorCount = tags.GetInt( worldId + "_vendor_count" );

				IDictionary<int, VendorLogic> vendors;
				if( this.VendorWorlds.Keys.Contains( worldId ) && this.VendorWorlds[worldId] != null ) {
					vendors = this.VendorWorlds[worldId];
				} else {
					vendors = this.VendorWorlds[worldId] = new Dictionary<int, VendorLogic>( vendorCount );
				}

				for( int i = 0; i < vendorCount; i++ ) {
					if( !tags.ContainsKey( worldId + "_vendor_npc_types_" + i ) ) { continue; }

					int npcType = tags.GetInt( worldId + "_vendor_npc_types_" + i );
					int[] totalPurchaseTypes = tags.GetIntArray( worldId + "_vendor_total_purchase_types_" + i );
					int[] totalSpendingsTypes = tags.GetIntArray( worldId + "_vendor_total_spendings_types_" + i );
					string jsonTotalPurchases = tags.GetString( worldId + "_vendor_total_purchases_str_" + i );
					string jsonTotalSpendings = tags.GetString( worldId + "_vendor_total_spendings_str_" + i );

					float[] totalPurchases = JsonConvert.DeserializeObject<float[]>( jsonTotalPurchases );
					float[] totalSpendings = JsonConvert.DeserializeObject<float[]>( jsonTotalSpendings );
					
					//if( (DebugHelper.DEBUGMODE & 1) > 0 ) {
					//	ErrorLogger.Log( "    load " + world_id + "_vendor_npc_types_" + i + ": " + npc_type );
					//	ErrorLogger.Log( "    load " + world_id + "_vendor_total_purchase_types_" + i + ": " + string.Join( ",", total_purchase_types ) );
					//	ErrorLogger.Log( "    load " + world_id + "_vendor_total_spendings_types_" + i + ": " + string.Join( ",", total_spendings_types ) );
					//	ErrorLogger.Log( "    load " + world_id + "_vendor_total_purchases_str_" + i + ": " + json_total_purchases );
					//	ErrorLogger.Log( "    load " + world_id + "_vendor_total_spendings_str_" + i + ": " + json_total_spendings );
					//}

					vendors[npcType] = VendorLogic.Create( npcType );
					if( vendors[npcType] != null ) {
						vendors[npcType].LoadTotalPurchases( totalPurchaseTypes, totalSpendingsTypes, totalPurchases, totalSpendings );
					}
				}
			} catch( Exception e ) {
				LogHelpers.Warn( e.ToString() );
				this.VendorWorlds = new Dictionary<string, IDictionary<int, VendorLogic>>();
			}
		}

		public void SaveVendorsForCurrentPlayer( TagCompound tags ) {
			foreach( var worldVendors in this.VendorWorlds ) {
				string worldId = worldVendors.Key;
				IDictionary<int, VendorLogic> vendors = worldVendors.Value;

				tags.Set( worldId + "_vendor_count", vendors.Count );
				//if( (DebugHelper.DEBUGMODE & 1) > 0 ) {
				//	ErrorLogger.Log( "  save " + world_id + "_vendor_count: " + vendors.Count );
				//}

				int i = 0;
				foreach( var kv in vendors ) {
					if( kv.Key <= 0 ) { continue; }
					if( kv.Value == null ) { continue; }

					int npcType = kv.Key;
					VendorLogic vendor = kv.Value;
					int[] totalPurchaseTypes, totalSpendingsTypes;
					float[] totalPurchases, totalSpendings;

					vendor.SaveTotalSpendings( out totalPurchaseTypes, out totalSpendingsTypes, out totalPurchases, out totalSpendings );
					string jsonTotalPurchases = JsonConvert.SerializeObject( totalPurchases );
					string jsonTotalSpendings = JsonConvert.SerializeObject( totalSpendings );

					tags.Set( worldId + "_vendor_npc_types_" + i, npcType );
					tags.Set( worldId + "_vendor_total_purchase_types_" + i, totalPurchaseTypes );
					tags.Set( worldId + "_vendor_total_spendings_types_" + i, totalSpendingsTypes );
					tags.Set( worldId + "_vendor_total_purchases_str_" + i, jsonTotalPurchases );
					tags.Set( worldId + "_vendor_total_spendings_str_" + i, jsonTotalSpendings );

					//if( (DebugHelper.DEBUGMODE & 1) > 0 ) {
					//	ErrorLogger.Log( "    save " + world_id + "_vendor_npc_types_" + i + ": " + (int)npc_type );
					//	ErrorLogger.Log( "    save " + world_id + "_vendor_total_purchase_types_" + i + ": " + String.Join( ",", total_purchase_types ) );
					//	ErrorLogger.Log( "    save " + world_id + "_vendor_total_spendings_types_" + i + ": " + String.Join( ",", total_spendings_types ) );
					//	ErrorLogger.Log( "    save " + world_id + "_vendor_total_purchases_" + i + ": " + json_total_purchases );
					//	ErrorLogger.Log( "    save " + world_id + "_vendor_total_spendings_" + i + ": " + json_total_spendings );
					//}
					i++;
				}
			}
		}

		////////////////

		private bool IsReady( Player player ) {
			if( Main.netMode == 2 ) { return false; }	// Client of single only
			if( player.whoAmI != Main.myPlayer ) { return false; }	// Current player only
			if( this.StartupDelay++ < 60*2 ) { return false; }
			return true;
		}


		public void Update( Player player ) {
			if( !this.IsReady( player ) ) {
				CapitalismLogic.IsDay = Main.dayTime;
				return;
			}

			var mymod = CapitalismMod.Instance;

			if( player.talkNPC != -1 ) {
				long money = PlayerItemHelpers.CountMoney( player, false );
				long spent = this.LastMoney - money;

				this.LastMoney = money;

				if( spent > 0 ) {
					this.LastBuyItem = this.AccountForPurchase( player, spent, this.LastBuyItem );
				} else if( spent < 0 ) {
					this.AccountForSale( player, -spent );
				} else {
					this.LastBuyItem = null;
				}

				// Snapshot current inventory
				int len = player.inventory.Length;
				for( int i = 0; i < len; i++ ) {
					Item item = player.inventory[i];
					if( item != null ) {
						this.PrevInventoryInfos[i] = new KeyValuePair<int, int>( item.type, item.stack );
					} else {
						this.PrevInventoryInfos[i] = new KeyValuePair<int, int>( 0, 0 );
					}
				}

				if( Main.mouseItem != null ) {
					this.PrevMouseInfo = new KeyValuePair<int, int>( Main.mouseItem.type, Main.mouseItem.stack );
				} else {
					this.PrevMouseInfo = new KeyValuePair<int, int>( 0, 0 );
				}
			}

			// Advance day
			if( CapitalismLogic.IsDay != Main.dayTime ) {
				CapitalismLogic.IsDay = Main.dayTime;

				this.DecayAllVendorPrices( player );
			}
		}


		////////////////

		private Item AccountForPurchase( Player player, long spent, Item lastBuyItem ) {
			NPC talkNpc = Main.npc[player.talkNPC];
			if( talkNpc == null || !talkNpc.active ) {
				LogHelpers.Log( "AccountForPurchase - No shop npc." );
				return null;
			}

			var mymod = CapitalismMod.Instance;
			ISet<int> possiblePurchases = ItemFinderHelpers.FindPossiblePurchaseTypes( player.inventory, spent );
			Item item = null;
			int stack = 1;
			
			if( possiblePurchases.Count > 0 ) {
				var changesAt = PlayerItemFinderHelpers.FindInventoryChanges( player, this.PrevMouseInfo, this.PrevInventoryInfos );
				changesAt = ItemFinderHelpers.FilterByTypes( changesAt, possiblePurchases );

				if( changesAt.Count == 1 ) {
					foreach( var entry in changesAt ) {
						if( entry.Key == -1 ) {
							item = Main.mouseItem;
						} else {
							item = player.inventory[entry.Key];
						}

						if( item != null ) {
							// Must be a false positive?
							if( lastBuyItem != null && lastBuyItem.type != item.type ) {
								item = null;
							} else {
								//stack = entry.Value.Value;
								break;
							}
						}
					}
				}
			}
			if( item == null ) {
				if( lastBuyItem != null ) {
					var vendor = this.GetOrCreateVendor( WorldHelpers.GetUniqueIdForCurrentWorld(true), talkNpc.type );
					int value = (int)vendor.GetPriceOf( lastBuyItem.type );

					if( (spent % value) == 0 ) {
						item = lastBuyItem;
						stack = (int)(spent / value);
					}
				}
			}

			if( item != null ) {
				this.BoughtFrom( player, talkNpc, item, stack );
			}
			return item;
		}


		private void AccountForSale( Player player, long earned ) {
			// TODO
		}

		////////////////

		public void UpdateGivenShop( Player player, int npcType, Chest shop, ref int nextSlot ) {
			var mymod = CapitalismMod.Instance;
			var myworld = mymod.GetModWorld<CapitalismWorld>();

			var vendor = this.GetOrCreateVendor( myworld.ID, npcType );
			if( vendor == null ) {
				LogHelpers.Warn( "No such vendor of type " + npcType );
				return;
			}

			vendor.UpdateShop( shop );
		}

		public bool InfuriateVendor( int npcType ) {
			var mymod = CapitalismMod.Instance;
			var myworld = mymod.GetModWorld<CapitalismWorld>();

			var vendor = this.GetOrCreateVendor( myworld.ID, npcType );
			if( vendor == null ) {
				LogHelpers.Warn( "InfuriateVendor - No such vendor of type " + npcType );
				return false;
			}

			vendor.Infuriate();
			return true;
		}

		////////////////

		public IDictionary<int, VendorLogic> GetOrCreateWorldVendors( string worldId ) {
			if( !this.VendorWorlds.Keys.Contains( worldId ) ) {
				this.VendorWorlds.Add( worldId, new Dictionary<int, VendorLogic>() );
			}
			return this.VendorWorlds[ worldId ];
		}

		public VendorLogic GetOrCreateVendor( string worldId, int npcType ) {
			var vendors = this.GetOrCreateWorldVendors( worldId );

			if( !vendors.Keys.Contains( npcType ) ) {
				vendors[npcType] = VendorLogic.Create( npcType );
			}
			return vendors[npcType];
		}

		////////////////

		public void BoughtFrom( Player player, NPC npc, Item item, int stack ) {
			var mymod = CapitalismMod.Instance;
			var modworld = mymod.GetModWorld<CapitalismWorld>();

			var vendor = this.GetOrCreateVendor( modworld.ID, npc.type );
			if( vendor != null ) {
				for( int i=0; i<stack; i++ ) {
					vendor.AddPurchase( item.type );
				}
			}
		}

		
		/*public void SoldTo( Player player, NPC _, Item item, long earned ) {
			if( !this.SellPrices.Keys.Contains( item.type ) ) {
				this.SellPrices[item.type] = earned;
			} else {
				this.SellPrices[item.type] /= 1.1;
			}

			long quantity = earned / (long)item.value;
			long final_earned = quantity * (long)this.SellPrices[item.type];

			long deduct = earned - final_earned;
			this.Player.BuyItem( (int)deduct );
Main.NewText( "Sold "+ item.name + " to "+_.name+" for "+earned+", deducted " + deduct + ", final "+ (earned - deduct) );
		}*/


		public void DecayAllVendorPrices( Player player ) {
			var mymod = CapitalismMod.Instance;
			var modworld = mymod.GetModWorld<CapitalismWorld>();
			if( modworld.ID == null || modworld.ID.Length == 0 ) {
				LogHelpers.Warn( "No world id." );
				return;
			}
			
			foreach( var kv in this.GetOrCreateWorldVendors(modworld.ID) ) {
				if( kv.Value != null ) {
					kv.Value.DecayPrices();
				}
			}
		}
	}
}
