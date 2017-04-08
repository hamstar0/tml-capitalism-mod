using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Utils;
using Utils.JsonConfig;

namespace Capitalism.Logic {
	class CapitalismLogic {
		private static bool IsDay = false;

		private CapitalismMod MyMod;

		public IDictionary<string, IDictionary<int, VendorLogic>> VendorWorlds { get; private set; }
		private IDictionary<long, double> SellPrices = new Dictionary<long, double>();

		private long LastMoney = 0;
		private IDictionary<int, KeyValuePair<int, int>> PrevInventoryInfos = new Dictionary<int, KeyValuePair<int, int>>();
		private KeyValuePair<int, int> PrevMouseInfo;



		////////////////

		public CapitalismLogic( CapitalismMod mymod ) {
			this.VendorWorlds = new Dictionary<string, IDictionary<int, VendorLogic>>();
			this.MyMod = mymod;
		}

		////////////////

		public void LoadVendorsForCurrentPlayer( TagCompound tags, string world_id ) {
			try {
				int vendor_count = tags.GetInt( world_id + "_vendor_count" );
				if( (Debug.DEBUGMODE & 1) > 0 ) {
					ErrorLogger.Log( "  load vendor count: " + vendor_count );
				}

				IDictionary<int, VendorLogic> vendors;
				if( this.VendorWorlds.Keys.Contains( world_id ) && this.VendorWorlds[world_id] != null ) {
					vendors = this.VendorWorlds[world_id];
				} else {
					vendors = this.VendorWorlds[world_id] = new Dictionary<int, VendorLogic>( vendor_count );
				}

				for( int i = 0; i < vendor_count; i++ ) {
					if( !tags.ContainsKey( world_id + "_vendor_npc_type_s_" + i ) ) { continue; }

					int npc_type = tags.GetInt( world_id + "_vendor_npc_type_s_" + i );
					int[] total_purchase_types = tags.GetIntArray( world_id + "_vendor_total_purchases_types_s_" + i );
					string json_total_purchases = tags.GetString( world_id + "_vendor_total_purchases_s_" + i );
					float[] total_purchases = JsonConfig< float[] >.Deserialize( json_total_purchases );

					if( (Debug.DEBUGMODE & 1) > 0 ) {
						ErrorLogger.Log( "    load vendor_npc_type_" + i+": " + npc_type );
						ErrorLogger.Log( "    load vendor_total_purchases_s_types_" + i+": " + string.Join(",",total_purchase_types) );
						ErrorLogger.Log( "    load vendor_total_purchases_s_" + i+": " + string.Join(",",total_purchases) );
					}

					vendors[npc_type] = VendorLogic.Create( npc_type );
					if( vendors[npc_type] != null ) {
						vendors[npc_type].LoadTotalPurchases( this.MyMod, total_purchase_types, total_purchases );
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
				if( (Debug.DEBUGMODE & 1) > 0 ) {
					ErrorLogger.Log( "  save " + world_id + "_vendor_count: " + vendors.Count );
				}

				int i = 0;
				foreach( var kv in vendors ) {
					if( kv.Key <= 0 ) { continue; }
					if( kv.Value == null ) { continue; }

					VendorLogic vendor = kv.Value;
					int[] total_purchase_types;
					float[] total_purchases;

					vendor.SaveTotalPurchases( out total_purchase_types, out total_purchases );
					string json_total_purchases = JsonConfig< float[] >.Serialize( total_purchases );

					tags.Set( world_id + "_vendor_npc_type_s_" + i, kv.Key );
					tags.Set( world_id + "_vendor_total_purchases_types_s_" + i, total_purchase_types );
					tags.Set( world_id + "_vendor_total_purchases_s_" + i, json_total_purchases );

					if( (Debug.DEBUGMODE & 1) > 0 ) {
						ErrorLogger.Log( "    save " + world_id + "_vendor_npc_type_s_" + i + ": " + (int)kv.Key );
						ErrorLogger.Log( "    save " + world_id + "_vendor_total_purchases_types_s_" + i + ": " + String.Join( ",", total_purchase_types ) );
						ErrorLogger.Log( "    save " + world_id + "_vendor_total_purchases_s_" + i + ": " + String.Join( ",", total_purchases ) );
					}
					i++;
				}
			}
		}

		////////////////

		private bool IsReady( Player player ) {
			if( Main.netMode == 2 ) { return false; }	// Client of single only
			if( player.whoAmI != Main.myPlayer ) { return false; }	// Current player only
			if( this.MyMod.GetModWorld<CapitalismWorld>().ID == "" ) { return false; }
			if( this.StartupDelay++ < 60*2 ) { return false; }
			return true;
		}
		private int StartupDelay = 0;


		public void Update( Player player ) {
			if( !this.IsReady( player ) ) {
				CapitalismLogic.IsDay = Main.dayTime;
				return;
			}

			if( player.talkNPC != -1 ) {
				long money = PlayerHelper.CountMoney( player );
				long spent = this.LastMoney - money;

				if( spent > 0 ) {
					this.AccountForPurchase( player, spent );
				} else if( spent < 0 ) {
					this.AccountForSale( player, -spent );
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

				this.LastMoney = money;
			}

			// Advance day
			if( CapitalismLogic.IsDay != Main.dayTime ) {
				CapitalismLogic.IsDay = Main.dayTime;

				this.DecayAllVendorPrices( player );
			}
		}


		////////////////

		private void AccountForPurchase( Player player, long spent ) {
			ISet<int> possible_purchases = PlayerHelper.FindPossiblePurchaseTypes( player, spent );

			if( possible_purchases.Count > 0 ) {
				var changes_at = PlayerHelper.FindInventoryChanges( player, this.PrevMouseInfo, this.PrevInventoryInfos );
				changes_at = ItemHelper.FilterByTypes( changes_at, possible_purchases );

				if( changes_at.Count == 1 ) {
					foreach( var entry in changes_at ) {
						Item item;
						if( entry.Key == -1 ) {
							item = Main.mouseItem;
						} else {
							item = player.inventory[entry.Key];
						}

						if( item != null ) {
							this.BoughtFrom( player, Main.npc[player.talkNPC], item, entry.Value.Value );
						}
					}
				}
			}
		}

		private void AccountForSale( Player player, long earned ) {
			// TODO
		}

		////////////////

		public void UpdateGivenShop( Player player, int npc_type, Chest shop, ref int nextSlot ) {
			var modworld = this.MyMod.GetModWorld<CapitalismWorld>();
			if( modworld.ID.Length == 0 ) {
				return;
			}

			var vendor = this.GetOrCreateVendor( modworld.ID, npc_type );
			if( vendor == null ) {
				ErrorLogger.Log( "UpdateGivenShop - No such vendor of type " + npc_type );
				return;
			}

			vendor.UpdateShop( this.MyMod, shop );
		}

		public bool InfuriateVendor( int npc_type ) {
			var modworld = this.MyMod.GetModWorld<CapitalismWorld>();
			if( modworld.ID.Length == 0 ) {
				ErrorLogger.Log( "InfuriateVendor - No world id set." );
				return false;
			}

			var vendor = this.GetOrCreateVendor( modworld.ID, npc_type );
			if( vendor == null ) {
				ErrorLogger.Log( "InfuriateVendor - No such vendor of type " + npc_type );
				return false;
			}

			vendor.Infuriate( this.MyMod );
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

		public void BoughtFrom( Player player, NPC npc, Item item, long _ ) {
			var modworld = this.MyMod.GetModWorld<CapitalismWorld>();
			if( modworld.ID.Length == 0 ) {
				ErrorLogger.Log( "BoughtFrom - No world id." );
				return;
			}

			var vendor = this.GetOrCreateVendor( modworld.ID, npc.type );
			if( vendor != null ) {
				vendor.AddPurchase( this.MyMod, item.type );
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
			var modworld = this.MyMod.GetModWorld<CapitalismWorld>();
			if( modworld.ID == null || modworld.ID.Length == 0 ) {
				ErrorLogger.Log( "DecayAllVendorPrices - No world id." );
				return;
			}
			
			foreach( var kv in this.GetOrCreateWorldVendors(modworld.ID) ) {
				if( kv.Value != null ) {
					kv.Value.DecayPrices( this.MyMod );
				}
			}
		}
	}
}
