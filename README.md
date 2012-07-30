Dredmor-XML-Validator
=====================

XML validation tools for mods for the game Dungeons of Dredmor. 

You can read more here:
http://community.gaslampgames.com/threads/mod-xml-validator.3390/

Changelog:

1.1.1.0

* Made monster dash spells optional.

1.1.0.0:

* Added ecrustDB element to the DredmorSchema.xsd file.
* Altered item element so the type attribute supports values 7 and 8 for daggers and polearms. Now validating the thrown weapon "thrown" attribute as an image *or* sprite. Removed validation of certain toolkit button images because they aren't used anymore and were raising false errors.
* Altered monster element to support new elements: sight, dash, primaryBuff and new attributes: diggleHell, horde, named, maxSpawns. "drop" now supports type in addition to name.
* Altered spell element to support type targetFloorItem and attributes: noAnimation. New child effects of type randomUncurse, moveRandomCurse, grabItem, transformMonster, manipulateObject, randomizePotion, findRecipe, removeMonster, dropItem, ascendDungeonLevel. The requirements element now supports new attribute: zorkmids, and mp is now optional. The buff element now supports elements: payback, zorkmidAbsorption, and attributes: insufficientFunds, zorkmidUpkeep.
* Altered abilities to support new element zorkmidbuff.
* Added support for the new "dash" element for monsters. Validating the hitSpell and missSpell as spell resources.
* Added support for validating the resources and element counts in the encrustDB file.
* Created a new project to generate the words.txt file used for case-insensitive XSD validation. This was previously generated manually.

1.0.3.0:

* Negative values are now allowed in damageBuff.
* consumptionBuff is now allowed for triggered effects other than in buffs.
* XML sprites (and SPR files) are now allowed for trap origins.