using System.Collections.Generic;
using Terraria;


namespace Utils {
	public static class PlayerHelper {
		public static long CountMoney( Player player ) {
			bool _;
			long inv_count = Terraria.Utils.CoinsCount( out _, player.inventory, new int[] {58, 57, 56, 55, 54} );
			long bank_count = Terraria.Utils.CoinsCount( out _, player.bank.item, new int[0] );
			long bank2_count = Terraria.Utils.CoinsCount( out _, player.bank2.item, new int[0] );
			long bank3_count = Terraria.Utils.CoinsCount( out _, player.bank3.item, new int[0] );
			return Terraria.Utils.CoinsCombineStacks( out _, new long[] {inv_count, bank_count, bank2_count, bank3_count} );
		}


		public static ISet<int> FindPossiblePurchaseTypes( Player player, long spent ) {
			ISet<int> possible_purchases = new HashSet<int>();

			if( Main.npcShop <= 0 || Main.npcShop > Main.instance.shop.Length ) {
				return possible_purchases;
			}
			Item[] shop_items = Main.instance.shop[Main.npcShop].item;

			for( int i = 0; i < shop_items.Length; i++ ) {
				Item shop_item = shop_items[i];
				if( shop_item == null || shop_item.IsAir ) { continue; }

				if( shop_item.value == spent ) {
					// If shop item type occurs more than once, skip
					int j;
					for( j = 0; j < i; j++ ) {
						if( shop_items[j].type == shop_item.type ) {
							break;
						}
					}
					if( j != i ) { continue; }

					possible_purchases.Add( shop_item.type );
				}
			}

			return possible_purchases;
		}


		public static IDictionary<int, KeyValuePair<int, int>> FindInventoryChanges( Player player,
				KeyValuePair<int, int> prev_mouse_info,
				IDictionary<int, KeyValuePair<int, int>> prev_inv ) {
			IDictionary<int, KeyValuePair<int, int>> changes = new Dictionary<int, KeyValuePair<int, int>>();
			int len = player.inventory.Length;

			for( int i = 0; i < len; i++ ) {
				Item item = player.inventory[i];

				if( prev_inv[i].Key != 0 && item == null ) {
					changes[i] = new KeyValuePair<int, int>( 0, 0 );
				} else if( prev_inv[i].Key != item.type || prev_inv[i].Value != item.stack ) {
					changes[i] = new KeyValuePair<int, int>( item.type, item.stack );
				}
			}

			if( prev_mouse_info.Key != 0 && Main.mouseItem == null ) {
				changes[-1] = new KeyValuePair<int, int>( 0, 0 );
			} else if( prev_mouse_info.Key != Main.mouseItem.type || prev_mouse_info.Value != Main.mouseItem.stack ) {
				changes[-1] = new KeyValuePair<int, int>( Main.mouseItem.type, Main.mouseItem.stack );
			}

			return changes;
		}


		public static IDictionary<int, Item> FindChangesOf( IDictionary<int, Item> changes_at, ISet<int> types ) {
			IDictionary<int, Item> found = new Dictionary<int, Item>();

			foreach( var where_item in changes_at ) {
				int where = where_item.Key;
				Item item = where_item.Value;

				if( types.Contains( item.type ) ) {
					found[where] = item;
				}
			}

			return found;
		}
	}
}
