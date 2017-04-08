using System.Collections.Generic;
using Terraria;

namespace Utils {
	public static class ItemHelper {
		public static IDictionary<int, KeyValuePair<int, int>> FilterByTypes(
				IDictionary<int, KeyValuePair<int, int>> entries,
				ISet<int> types ) {
			IDictionary<int, KeyValuePair<int, int>> found = new Dictionary<int, KeyValuePair<int, int>>();

			foreach( var where_item in entries ) {
				int where = where_item.Key;
				int i_type = where_item.Value.Key;

				if( types.Contains(i_type) ) {
					found[where] = where_item.Value;
				}
			}

			return found;
		}


		private static IDictionary<long, ISet<int>> SellItems = new Dictionary<long, ISet<int>>(); 
		
		public static ISet<int> FindItemsByValue( long sell_value, bool include_coins=false ) {
			if( !ItemHelper.SellItems.Keys.Contains(sell_value) ) {
				ItemHelper.SellItems[sell_value] = new HashSet<int>();
			} else {
				return ItemHelper.SellItems[sell_value];
			}

			for( int i = 0; i < Main.itemName.Length; i++ ) {   // Main.itemTexture?
				if( !include_coins && i == 71 ) { i = 75; }

				Item item = new Item();
				item.SetDefaults( i );
				if( item.value <= 0 ) { continue; }

				if( sell_value % item.value == 0 ) {
					ItemHelper.SellItems[sell_value].Add( i );

					//long sub_val = sell_value / item.value;
					//if( sub_val != 1 && sub_val != sell_value ) {
					//	ItemHelper.SellItems[ item.value ] = ItemHelper.FindItemsByValue( sub_val );
					//}
				}
			}

			return ItemHelper.SellItems[sell_value];
		}
	}
}
