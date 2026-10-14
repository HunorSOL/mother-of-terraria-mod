using Microsoft.Xna.Framework;
using MotherOfTerraria.Items;
using MotherOfTerraria.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
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
	//  - when it is full the 30 second summon sequence runs (Phase 4, below); at the end the boss spawns (PLACEHOLDER until Phase 5);
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
				Main.NewText("The Basket is already stirring.", 255, 215, 0);
				return;
			}

			// A full Basket starts the summon sequence (again, if an earlier one was interrupted by leaving or dying).
			if (Crystals >= CrystalsNeeded)
			{
				StartSummon();
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
				StartSummon();
			}
		}

		// ---- The summon sequence (Phase 4, owner's script, 2026-10-14) ----
		// Counted from the moment the Basket is full (60 ticks = 1 second). Change the numbers to retime it.
		private const int Seconds = 60;
		private const int MiracleAt = 10 * Seconds;     // chat: "A heavenly miracle is upon us"
		private const int ShakeAt = 15 * Seconds;       // the screen shakes
		private const int ShakeAgainAt = 22 * Seconds;  // ... again
		private const int ShakeLastAt = 28 * Seconds;   // ... and once more, until the boss spawns
		private const int ShakeFor = 2 * Seconds;       // each shake lasts 2 seconds
		private const int ProtectAt = 20 * Seconds;     // chat: "The mother of this world will protect us all"
		private const int AsSheIsAt = 25 * Seconds;     // chat: "As she is.... The Mother of Terraria"
		private const int BossSpawnAt = 30 * Seconds;   // the boss spawns

		private static readonly Color Gold = new Color(255, 215, 0);

		private int summonTimer;

		// Starts the 30 second sequence. It cannot start twice at once, because FightActive blocks the Basket.
		private void StartSummon()
		{
			FightActive = true;
			summonTimer = 0;
			SoundEngine.PlaySound(SoundID.Item119, Player.Center);
		}

		// Runs every tick while the sequence is on. SINGLEPLAYER-ONLY: it runs on the local player's client.
		private void RunSummon()
		{
			summonTimer++;
			switch (summonTimer)
			{
				case MiracleAt:
					Main.NewText("A heavenly miracle is upon us", Gold);
					break;
				case ShakeAt:
				case ShakeAgainAt:
				case ShakeLastAt:
					// One camera "punch" that rattles the screen for 2 seconds.
					Main.instance.CameraModifiers.Add(new PunchCameraModifier(Player.Center, Vector2.UnitY, 7f, 14f, ShakeFor, -1f, "MotherOfTerraria:BasketShake"));
					SoundEngine.PlaySound(SoundID.Item119, Player.Center);
					break;
				case ProtectAt:
					Main.NewText("The mother of this world will protect us all", Gold);
					break;
				case AsSheIsAt:
					Main.NewText("As she is.... The Mother of Terraria", Gold);
					break;
				case BossSpawnAt:
					SpawnBoss();
					break;
			}
		}

		// PLACEHOLDER: the boss does not exist yet. Phase 5 spawns the Mother of Terraria here (on the server, when
		// multiplayer arrives) and keeps FightActive true until she is killed or vanishes because the player died
		// (then EndFight below empties the Basket). For now the sequence just ends and the crystals STAY in the Basket,
		// so it can be run again by right-clicking the full Basket.
		private void SpawnBoss()
		{
			Main.NewText("(Placeholder: the Mother of Terraria spawns here. The boss is built in Phase 5.)", Gold);
			SoundEngine.PlaySound(SoundID.Roar, Player.Center);
			FightActive = false;
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

		// Drives the sequence. Leaving the Heavens or dying cancels a sequence that is still counting down;
		// the crystals stay in the Basket (it only empties when the boss is killed or vanishes because the player died).
		public override void PostUpdate()
		{
			if (!FightActive || Player.whoAmI != Main.myPlayer)
			{
				return;
			}

			if (Player.dead || !SubworldSystem.IsActive<HeavensSubworld>())
			{
				FightActive = false;
				return;
			}

			RunSummon();
		}
	}
}
