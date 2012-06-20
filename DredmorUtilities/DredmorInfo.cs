using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DredmorUtilities
{
	/// <summary>
	/// Contains informational data about the game and what files to load for particular purposes.
	/// </summary>
	public static class DredmorInfo
	{
		/// <summary>
		/// XML files to read for loading resources into a manifest.
		/// </summary>
		public static readonly string[] XmlFilesToLoadResources = 
		{
			"craftdb.xml",
			"itemdb.xml",
			"mondb.xml",
			"soundfx.xml",
			"spelldb.xml",
		};

		/// <summary>
		/// XML files to be validated by the validators.
		/// </summary>
		public static readonly string[] XmlFilesToValidate = 
		{
			"craftdb.xml",
			"itemdb.xml",
			"mondb.xml",
			"soundfx.xml",
			"spelldb.xml",
			"skilldb.xml",
			"rooms.xml",
			"mod.xml"
		};

		/// <summary>
		/// All 'DB' type XML files.
		/// </summary>
		public static readonly string[] AllContentXmlFiles = 
		{
			"branchdb.xml",
			"craftdb.xml",
			"itemdb.xml",
			"mantemplatedb.xml",
			"manual.xml",
			"mondb.xml",
			"quests.xml",
			"rooms.xml",
			"scrolldb.xml",
			"skilldb.xml",
			"soundfx.xml",
			"speech.xml",
			"spelldb.xml",
			"text.xml",
			"tutorial.xml",
			"tweakdb.xml",
			"mod.xml"
		};

		/// <summary>
		/// All currently supported triggered effect element names in lowercase.
		/// </summary>
		public static readonly string[] TriggeredEffectNames = 
		{
			"triggeroncast",
			"triggerondodge",
			"blockbuff",
			"boozebuff",
			"consumeBuff",
			"counterbuff",
			"criticalbuff",
			"crossbowshotbuff",
			"dodgebuff",
			"fireCrossbow",
			"foodbuff",
			"playerhiteffectbuff",
			"targethiteffectbuff",
			"targetkillbuff",
			"thrownbuff"
		};
	}
}
