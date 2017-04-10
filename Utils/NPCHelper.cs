using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;


namespace Utils {
	public static class NPCHelper {
		public static Chest GetShop( int npc_type ) {
			if( Main.instance == null ) {
				ErrorLogger.Log( "No main instance." );
				return null;
			}

			switch( npc_type ) {
			case 17:
				return Main.instance.shop[1];
			case 19:
				return Main.instance.shop[2];
			case 20:
				return Main.instance.shop[3];
			case 38:
				return Main.instance.shop[4];
			case 54:
				return Main.instance.shop[5];
			case 107:
				return Main.instance.shop[6];
			case 108:
				return Main.instance.shop[7];
			case 124:
				return Main.instance.shop[8];
			case 142:
				return Main.instance.shop[9];
			case 160:
				return Main.instance.shop[10];
			case 178:
				return Main.instance.shop[11];
			case 207:
				return Main.instance.shop[12];
			case 208:
				return Main.instance.shop[13];
			case 209:
				return Main.instance.shop[14];
			case 227:
				return Main.instance.shop[15];
			case 228:
				return Main.instance.shop[16];
			case 229:
				return Main.instance.shop[17];
			case 353:
				return Main.instance.shop[18];
			case 368:
				return Main.instance.shop[19];
			case 453:
				return Main.instance.shop[20];
			case 550:
				return Main.instance.shop[21];
			}

			return null;
		}


		public static ISet<int> GetFemaleTownNpcTypes() {
			return new HashSet<int>( new int[] {
				18,    // Nurse
				208,   // Party Girl
				353,   // Hair Stylist
				20,    // Dryad
				124,   // Mechanic
				178   // Steampunker
			} );
		}

		public static ISet<int> GetNonGenderedTownNpcTypes() {
			return new HashSet<int>( new int[] {
				228,   // Witch Doctor
				441,   // Tax Collector
				160,   // Truffle
				209,   // Cyborg
				453	// Skeleton Merchant
			} );
		}


		private static IDictionary<int, int> TypesToWhos = new Dictionary<int, int>();

		public static NPC FindFirstNpcByType( int type ) {
			if( NPCHelper.TypesToWhos.Keys.Contains(type) ) {
				NPC npc = Main.npc[ NPCHelper.TypesToWhos[type] ];
				if( npc != null && npc.active && npc.type == type ) {
					return npc;
				}
			}

			for( int i=0; i<Main.npc.Length; i++ ) {
				NPC npc = Main.npc[i];
				if( npc != null && npc.active && npc.type == type ) {
					NPCHelper.TypesToWhos[type] = i;
					return npc;
				}
			}

			return null;
		}
	}
}
