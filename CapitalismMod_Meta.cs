﻿using HamstarHelpers.Components.Config;
using HamstarHelpers.Helpers.DebugHelpers;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;


namespace Capitalism {
	partial class CapitalismMod : Mod {
		public static string GithubUserName => "hamstar0";
		public static string GithubProjectName => "tml-capitalism-mod";

		public static string ConfigFileRelativePath {
			get { return ConfigurationDataBase.RelativePath + Path.DirectorySeparatorChar + CapitalismConfigData.ConfigFileName; }
		}
		public static void ReloadConfigFromFile() {
			if( Main.netMode != 0 ) {
				throw new Exception( "Cannot reload configs outside of single player." );
			}
			if( CapitalismMod.Instance != null ) {
				if( !CapitalismMod.Instance.ConfigJson.LoadFile() ) {
					CapitalismMod.Instance.ConfigJson.SaveFile();
				}
			}
		}

		public static void ResetConfigFromDefaults() {
			if( Main.netMode != 0 ) {
				throw new Exception( "Cannot reset to default configs outside of single player." );
			}

			var newConfig = new CapitalismConfigData();
			//new_config.SetDefaults();

			CapitalismMod.Instance.ConfigJson.SetData( newConfig );
			CapitalismMod.Instance.ConfigJson.SaveFile();
		}
	}
}
