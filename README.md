Dredmor-XML-Validator
=====================

XML validation tools for mods for the game Dungeons of Dredmor. 

You can read more here:
http://community.gaslampgames.com/threads/mod-xml-validator.3390/

Changelog:

1.1.3.5

* I forgot to regenerate the words file, so I did that. One new word was added. (This supports case-insensitive XML validation.)

1.1.3.4

* Added hitsNeededToBreak to customBreakable
* Spell effect "turns" is no a signed byte instead of unsigned (allows you to use -1)
* mineTimer is now an unsigned int instead of unsigned byte to allow larger values.
* beammissile spells can now use a PNG as their animation.

1.1.3.3
	
* Changed the ModValidator and ContentDirectory class to support file streams and not just paths to files. This supports using the libraries on the web.

1.1.3.2

Many XSD fixes contributed by Null for advanced XML feature that not even Gaslamp Games is using!

* trapOrigin can now be "floor".
* Lots of values changed from bytes to shorts to accomodate larger values.
* Added "elements" for rooms: clock, magicBlocker.
* Added room element types: wizardControls, wizardGraffiti, wizardKey, wizardPedestal, wizardPortal.
* Added attributes for traps: can_push, id.
* Added room sccript condition types: entered, exited, event, isAlive, started, stopped
* Added room script condition attributes: started, stopped, eventType
* Added room script action_type: set.
* Added room script action create_type: clock, customEngraving, trap.
* Added room script action strribute: set_type.
* Added spell type: target_zombie.
* Added spell effect type: moveCurses, resetCooldowns.

1.1.2.3

* Added charge (spell charging) for monsters.
* Made chance and interruptable optional for dash since they are optional for charge, and I assume these attributes have a default value there, too.

1.1.2.2

* Added resetRandomCooldown spell effect type.

1.1.2.1

* Added "percentage" to spell effects which now works tha same as "percent".

1.1.2.0

* Added validation for unstableEffect spells.
* Removed default value for path in DredmorXmlValidator.
* Added spell effect: swapWithMonster
* Added buffTag attribute to spell effects.
* Added consumeItemType: potion
* In a previous version, a beep was added at the completion of validation for the Core Validator.

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