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
	// Remembers how many crystals this player has put into the Heavenly Basket, and runs the summon fight's start.
	// A ModPlayer is data and code attached to each player; it is saved with the character.
	//
	// The rules (owner, 2026-10-14):
	//  - the Basket needs 30 crystals;
	//  - crystals put in STAY in the Basket (leaving the Heavens or quitting the game does not empty it);
	//  - when it is full the fight starts (PLACEHOLDER for now: Phase 4 adds the real summon sequence, Phase 5 the boss);
	//  - the Basket empties ONLY when the boss is killed, or when the boss disappears because the player died.
	//    TODO (Phase 5, when the boss is alive and fightable): call EndFight(true) when it is killed and
	//    EndFight(false) when it vanishes because the player died. Until then NOTHING empties the Basket.
	// SINGLEPLAYER-ONLY: the Basket's fill is per player and handled on the local client. For multiplayer it must
	// move to the server (shared or per-player), with the boss spawn done server-side (see Phase 8).
	public class HeavenlyBasketPlayer : ModPlayer
	{
		public const int CrystalsNeeded = 30; // owner: was 50, changed to 30 on 2026-10-14

		public int Crystals;
		public bool FightActive; // not saved: it only lasts while the fight is running in this session

		public override void SaveData(TagCompound tag)
		{
			tag["basketCrystals"] = Crystals;
		}

		public override void LoadData(TagCompound tag)
		{
			Crystals = tag.GetInt("basketCrystals");
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

			if (Crystals >= CrystalsNeeded)
			{
				Main.NewText($"The Heavenly Basket is full ({Crystals}/{CrystalsNeeded}).", 255, 215, 0);
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
			Main.NewText("The Heavenly Basket overflows with light... (placeholder: the Mother of Terraria will appear here.)", 255, 215, 0);
			SoundEngine.PlaySound(SoundID.Item119, Player.Center);
		}

		// The boss code will call this when the real fight ends (see the TODO at the top of this class).
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

		// Leaving the Heavens ends the running fight in this session but keeps the crystals in the Basket.
		public override void PostUpdate()
		{
			if (FightActive && Player.whoAmI == Main.myPlayer && !SubworldSystem.IsActive<HeavensSubworld>())
			{
				FightActive = false;
			}
		}
	}
}
