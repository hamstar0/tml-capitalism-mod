using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace Utils {
	public static class Debug {
		public static int DEBUGMODE = 0;	// 1: Info, 2: Reset
		public static Dictionary<string, string> Display = new Dictionary<string, string>();


		public static void PrintToBatch( SpriteBatch sb ) {
			int i = 0;

			foreach( string key in Debug.Display.Keys.ToList() ) {
				string msg = key + ":  " + Debug.Display[key];
				sb.DrawString( Main.fontMouseText, msg, new Vector2( 8, (Main.screenHeight - 32) - (i * 24) ), Color.White );

				Debug.Display[key] = "";
				i++;
			}
		}
	}
}
