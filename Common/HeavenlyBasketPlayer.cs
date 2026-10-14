using MotherOfTerraria.Items;
using MotherOfTerraria.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MotherOfTerraria.Common
{
	// Remembers how many crystals this player has put into the Heavenly Basket, and runs the summon fight's
	// start and end. A ModPlayer is data and code attached to each player; it is saved with the character.
	//
	// The rules (owner, 2026-10-14):
	//  - the Basket needs 50 crystals;
	//  - crystals put in stay in the Basket (even after leaving the Heavens) until it is full;
	//  - when it is full the fight starts (PLACEHOLDER for now: Phase 4 adds the real summon sequence, Phase 5 the boss);
	//  - the crystals are used up when the fight ends. A loss (dying, or leaving the Heavens mid-fight, or quitting
	//    the game mid-fight) means starting again from 0, and so does a win.
	// SINGLEPLAYER-ONLY: the Basket's fill is per player and handled on the local client. For multiplayer it must
	// move to the server (shared or per-player), with the boss spawn done server-side (see Phase 8).
	public class HeavenlyBasketPlayer : ModPlayer
	{
		public const int CrystalsNeeded = 50;

		// PLACEHOLDER fight length: with no boss yet, the fight "is won" after this many ticks (60 seconds).
		private const int PlaceholderFightTicks = 60 * 60;

		public int Crystals;
		public bool FightActive;
		private int fightTimer;

		public override void SaveData(TagCompound tag)
		{
			tag["basketCrystals"] = Crystals;
			tag["basketFight"] = FightActive;
		}

		public override void LoadData(TagCompound tag)
		{
			Crystals = tag.GetInt("basketCrystals");
			// A fight that was still running when the game closed counts as lost.
			if (tag.GetBool("basketFight"))
			{
				Crystals = 0;
			}
			FightActive = false;
		}

		// Called by the Basket tile when the player right-clicks it.
		public void TryDeposit()
		{
			if (FightActive)
			{
				Main.NewText("The Basket is already stirring. Finish the fight first.", 255, 215, 0);
				return;
			}

			int type = ModContent.ItemType<HeavensCrystal>();
			int wanted = CrystalsNeeded - Crystals;
			int given = 0;
			for (int i = 0; i < Player.inventory.Length && given < wanted; i++)
			{
				Item item = Player.inventory[i];
				if (item.type == type && item.stack > 0)
				{
					int take = System.Math.Min(item.stack, wanted - given);
					item.stack -= take;
					given += take;
					if (item.stack <= 0)
					{
						item.TurnToAir();
					}
				}
			}

			if (given == 0)
			{
				Main.NewText($"The Heavenly Basket holds {Crystals}/{CrystalsNeeded} crystals. You have none to give.", 255, 215, 0);
				return;
			}

			Crystals += given;
			SoundEngine.PlaySound(SoundID.Item4, Player.Center);
			Main.NewText($"You place {given} crystal{(given == 1 ? "" : "s")} in the Basket: {Crystals}/{CrystalsNeeded}.", 170, 220, 255);

			if (Crystals >= CrystalsNeeded)
			{
				StartFight();
			}
		}

		// PLACEHOLDER summon: Phase 4 replaces this with the real intro and Phase 5 with the boss spawn.
		// Only runs once per attempt, because the Basket refuses crystals while FightActive.
		private void StartFight()
		{
			FightActive = true;
			fightTimer = 0;
			Main.NewText("The Heavenly Basket overflows with light... (placeholder: the Mother of Terraria will appear here.)", 255, 215, 0);
			SoundEngine.PlaySound(SoundID.Item119, Player.Center);
		}

		// The boss code will call this when the real fight ends.
		public void EndFight(bool won)
		{
			if (!FightActive)
			{
				return;
			}
			FightActive = false;
			Crystals = 0;
			Main.NewText(won ? "The fight is won. The Basket is empty again." : "The fight is lost. The crystals are gone; the Basket is empty.", 255, 215, 0);
		}

		public override void PostUpdate()
		{
			if (!FightActive || Player.whoAmI != Main.myPlayer)
			{
				return;
			}

			// Dying or leaving the Heavens mid-fight is a loss.
			if (Player.dead || !SubworldSystem.IsActive<HeavensSubworld>())
			{
				EndFight(false);
				return;
			}

			// PLACEHOLDER: no boss yet, so the "fight" simply ends in a win after a minute.
			fightTimer++;
			if (fightTimer >= PlaceholderFightTicks)
			{
				EndFight(true);
			}
		}
	}
}
