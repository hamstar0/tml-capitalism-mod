using HamstarHelpers.ItemHelpers;
using HamstarHelpers.PlayerHelpers;
using HamstarHelpers.Utilities.Config;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
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



		////////////////

		public CapitalismLogic() {
			this.VendorWorlds = new Dictionary<string, IDictionary<int, VendorLogic>>();
		}

		////////////////

		public void LoadVendorsForCurrentPlayer( CapitalismMod mymod, TagCompound tags, string world_id ) {
			try {
				int vendor_count = tags.GetInt( world_id + "_vendor_count" );

				IDictionary<int, VendorLogic> vendors;
				if( this.VendorWorlds.Keys.Contains( world_id ) && this.VendorWorlds[world_id] != null ) {
					vendors = this.VendorWorlds[world_id];
				} else {
					vendors = this.VendorWorlds[world_id] = new Dictionary<int, VendorLogic>( vendor_count );
				}

				for( int i = 0; i < vendor_count; i++ ) {
					if( !tags.ContainsKey( world_id + "_vendor_npc_types_" + i ) ) { continue; }

					int npc_type = tags.GetInt( world_id + "_vendor_npc_types_" + i );
					int[] total_purchase_types = tags.GetIntArray( world_id + "_vendor_total_purchase_types_" + i );
					int[] total_spendings_types = tags.GetIntArray( world_id + "_vendor_total_spendings_types_" + i );
					string json_total_purchases = tags.GetString( world_id + "_vendor_total_purchases_str_" + i );
					string json_total_spendings = tags.GetString( world_id + "_vendor_total_spendings_str_" + i );

					float[] total_purchases = JsonConfig<float[]>.Deserialize( json_total_purchases );
					float[] total_spendings = JsonConfig<float[]>.Deserialize( json_total_spendings );
					
					//if( (DebugHelper.DEBUGMODE & 1) > 0 ) {
					//	ErrorLogger.Log( "    load " + world_id + "_vendor_npc_types_" + i + ": " + npc_type );
					//	ErrorLogger.Log( "    load " + world_id + "_vendor_total_purchase_types_" + i + ": " + string.Join( ",", total_purchase_types ) );
					//	ErrorLogger.Log( "    load " + world_id + "_vendor_total_spendings_types_" + i + ": " + string.Join( ",", total_spendings_types ) );
					//	ErrorLogger.Log( "    load " + world_id + "_vendor_total_purchases_str_" + i + ": " + json_total_purchases );
					//	ErrorLogger.Log( "    load " + world_id + "_vendor_total_spendings_str_" + i + ": " + json_total_spendings );
					//}

					vendors[npc_type] = VendorLogic.Create( npc_type );
					if( vendors[npc_type] != null ) {
						vendors[npc_type].LoadTotalPurchases( mymod, total_purchase_types, total_spendings_types, total_purchases, total_spendings );
					}
				}
			} catch( Exception e ) {
				ErrorLogger.Log( e.ToString() );
				this.VendorWorlds = new Dictionary<string, IDictionary<int, VendorLogic>>();
			}
		}

		public void SaveVendorsForCurrentPlayer( TagCompound tags ) {
			foreach( var world_vendors in this.VendorWorlds ) {
				string world_id = world_vendors.Key;
				IDictionary<int, VendorLogic> vendors = world_vendors.Value;

				tags.Set( world_id + "_vendor_count", vendors.Count );
				//if( (DebugHelper.DEBUGMODE & 1) > 0 ) {
				//	ErrorLogger.Log( "  save " + world_id + "_vendor_count: " + vendors.Count );
				//}

				int i = 0;
				foreach( var kv in vendors ) {
					if( kv.Key <= 0 ) { continue; }
					if( kv.Value == null ) { continue; }

					int npc_type = kv.Key;
					VendorLogic vendor = kv.Value;
					int[] total_purchase_types, total_spendings_types;
					float[] total_purchases, total_spendings;

					vendor.SaveTotalSpendings( out total_purchase_types, out total_spendings_types, out total_purchases, out total_spendings );
					string json_total_purchases = JsonConfig<float[]>.Serialize( total_purchases );
					string json_total_spendings = JsonConfig<float[]>.Serialize( total_spendings );

					tags.Set( world_id + "_vendor_npc_types_" + i, npc_type );
					tags.Set( world_id + "_vendor_total_purchase_types_" + i, total_purchase_types );
					tags.Set( world_id + "_vendor_total_spendings_types_" + i, total_spendings_types );
					tags.Set( world_id + "_vendor_total_purchases_str_" + i, json_total_purchases );
					tags.Set( world_id + "_vendor_total_spendings_str_" + i, json_total_spendings );

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

		private bool IsReady( CapitalismMod mymod, Player player ) {
			if( Main.netMode == 2 ) { return false; }	// Client of single only
			if( player.whoAmI != Main.myPlayer ) { return false; }	// Current player only
			if( mymod.GetModWorld<CapitalismWorld>().ID == "" ) { return false; }
			if( this.StartupDelay++ < 60*2 ) { return false; }
			return true;
		}
		private int StartupDelay = 0;


		public void Update( CapitalismMod mymod, Player player ) {
			if( !this.IsReady( mymod, player ) ) {
				CapitalismLogic.IsDay = Main.dayTime;
				return;
			}

			if( player.talkNPC != -1 ) {
				long money = PlayerItemHelpers.CountMoney( player );
				long spent = this.LastMoney - money;

				this.LastMoney = money;

				if( spent > 0 ) {
					this.LastBuyItem = this.AccountForPurchase( mymod, player, spent, this.LastBuyItem );
				} else if( spent < 0 ) {
					this.AccountForSale( mymod, player, -spent );
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

				this.DecayAllVendorPrices( mymod, player );
			}
		}


		////////////////

		private Item AccountForPurchase( CapitalismMod mymod, Player player, long spent, Item last_buy_item ) {
			var modworld = mymod.GetModWorld<CapitalismWorld>();
			NPC talk_npc = Main.npc[player.talkNPC];
			if( talk_npc == null || !talk_npc.active ) {
				ErrorLogger.Log( "AccountForPurchase - No shop npc." );
				return null;
			}
			ISet<int> possible_purchases = PlayerItemHelpers.FindPossiblePurchaseTypes( player, spent );
			Item item = null;
			int stack = 1;
			
			if( possible_purchases.Count > 0 ) {
				var changes_at = PlayerItemHelpers.FindInventoryChanges( player, this.PrevMouseInfo, this.PrevInventoryInfos );
				changes_at = ItemFinderHelpers.FilterByTypes( changes_at, possible_purchases );

				if( changes_at.Count == 1 ) {
					foreach( var entry in changes_at ) {
						if( entry.Key == -1 ) {
							item = Main.mouseItem;
						} else {
							item = player.inventory[entry.Key];
						}

						if( item != null ) {
							// Must be a false positive?
							if( last_buy_item != null && last_buy_item.type != item.type ) {
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
				if( last_buy_item != null ) {
					var vendor = this.GetOrCreateVendor( modworld.ID, talk_npc.type );
					int value = (int)vendor.GetPriceOf( mymod, last_buy_item.type );

					if( (spent % value) == 0 ) {
						item = last_buy_item;
						stack = (int)(spent / value);
					}
				}
			}

			if( item != null ) {
				this.BoughtFrom( mymod, player, talk_npc, item, stack );
			}
			return item;
		}


		private void AccountForSale( CapitalismMod mymod, Player player, long earned ) {
			// TODO
		}

		////////////////

		public void UpdateGivenShop( CapitalismMod mymod, Player player, int npc_type, Chest shop, ref int nextSlot ) {
			var modworld = mymod.GetModWorld<CapitalismWorld>();
			if( modworld.ID.Length == 0 ) {
				return;
			}

			var vendor = this.GetOrCreateVendor( modworld.ID, npc_type );
			if( vendor == null ) {
				ErrorLogger.Log( "UpdateGivenShop - No such vendor of type " + npc_type );
				return;
			}

			vendor.UpdateShop( mymod, shop );
		}

		public bool InfuriateVendor( CapitalismMod mymod, int npc_type ) {
			var modworld = mymod.GetModWorld<CapitalismWorld>();
			if( modworld.ID.Length == 0 ) {
				ErrorLogger.Log( "InfuriateVendor - No world id set." );
				return false;
			}

			var vendor = this.GetOrCreateVendor( modworld.ID, npc_type );
			if( vendor == null ) {
				ErrorLogger.Log( "InfuriateVendor - No such vendor of type " + npc_type );
				return false;
			}

			vendor.Infuriate( mymod );
			return true;
		}

		////////////////

		public IDictionary<int, VendorLogic> GetOrCreateWorldVendors( string world_id ) {
			if( !this.VendorWorlds.Keys.Contains( world_id ) ) {
				this.VendorWorlds.Add( world_id, new Dictionary<int, VendorLogic>() );
			}
			return this.VendorWorlds[ world_id ];
		}

		public VendorLogic GetOrCreateVendor( string world_id, int npc_type ) {
			var vendors = this.GetOrCreateWorldVendors( world_id );

			if( !vendors.Keys.Contains( npc_type ) ) {
				vendors[npc_type] = VendorLogic.Create( npc_type );
			}
			return vendors[npc_type];
		}

		////////////////

		public void BoughtFrom( CapitalismMod mymod, Player player, NPC npc, Item item, int stack ) {
			var modworld = mymod.GetModWorld<CapitalismWorld>();
			if( modworld.ID.Length == 0 ) {
				ErrorLogger.Log( "BoughtFrom - No world id." );
				return;
			}

			var vendor = this.GetOrCreateVendor( modworld.ID, npc.type );
			if( vendor != null ) {
				for( int i=0; i<stack; i++ ) {
					vendor.AddPurchase( mymod, item.type );
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


		public void DecayAllVendorPrices( CapitalismMod mymod, Player player ) {
			var modworld = mymod.GetModWorld<CapitalismWorld>();
			if( modworld.ID == null || modworld.ID.Length == 0 ) {
				ErrorLogger.Log( "DecayAllVendorPrices - No world id." );
				return;
			}
			
			foreach( var kv in this.GetOrCreateWorldVendors(modworld.ID) ) {
				if( kv.Value != null ) {
					kv.Value.DecayPrices( mymod );
				}
			}
		}
	}
}
