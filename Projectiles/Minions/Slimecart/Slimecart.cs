﻿using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.Slimecart
{
	public class SlimecartMinionBuff : MinionBuff
	{
		public SlimecartMinionBuff() : base(ProjectileType<SlimecartMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Slimecart");
			Description.SetDefault("A slime miner will fight for you!");
		}
	}

	public class SlimecartMinionItem : MinionItem<SlimecartMinionBuff, SlimecartMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Slimecart Staff");
			Tooltip.SetDefault("Summons slime miner to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.damage = 10;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 28;
			item.height = 28;
			item.value = Item.buyPrice(0, 0, 5, 0);
			item.rare = ItemRarityID.White;
		}
		public override void AddRecipes()
		{
			foreach(int itemId in new int[] { ItemID.SilverBar, ItemID.TungstenBar})
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ItemID.Minecart, 1);
				recipe.AddIngredient(ItemID.MiningHelmet, 1);
				recipe.AddIngredient(itemId, 12);
				recipe.AddTile(TileID.Anvils);
				recipe.SetResult(this);
				recipe.AddRecipe();
			}
		}
	}

	public class SlimecartMinion : SimpleGroundBasedMinion<SlimecartMinionBuff>, IGroundAwareMinion
	{
		private Color slimeColor;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Slimecart");
			Main.projFrames[projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 32;
			projectile.height = 28;
			drawOffsetX = -2;
			attackFrames = 60;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			maxJumpVelocity = 12;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			float brightness = (lightColor.R + lightColor.G + lightColor.B) / (3f * 255f);
			Color slimeColor = this.slimeColor;
			slimeColor.R = (byte)(slimeColor.R * brightness);
			slimeColor.G = (byte)(slimeColor.G * brightness);
			slimeColor.B = (byte)(slimeColor.B * brightness);
			if(gHelper.isFlying)
			{
				texture = GetTexture(Texture+"_Umbrella");
				spriteBatch.Draw(texture, pos + new Vector2(0, -36) - Main.screenPosition,
					texture.Bounds, lightColor, 0,
					texture.Bounds.Center.ToVector2(), 1, effects, 0);
				texture = GetTexture(Texture+"_UmbrellaGlow");
				spriteBatch.Draw(texture, pos + new Vector2(0, -36) - Main.screenPosition,
					texture.Bounds, slimeColor, 0,
					texture.Bounds.Center.ToVector2(), 1, effects, 0);
			}
			texture = GetTexture(Texture+"_Slime");
			spriteBatch.Draw(texture, pos + new Vector2(0, -14) - Main.screenPosition,
				texture.Bounds, slimeColor, 0,
				texture.Bounds.Center.ToVector2(), 1, effects, 0);
			texture = GetTexture(Texture+"_Hat");
			spriteBatch.Draw(texture, pos + new Vector2(0, -23) - Main.screenPosition,
				texture.Bounds, lightColor, 0,
				texture.Bounds.Center.ToVector2(), 1, effects, 0);
			return true;
		}

		public override void OnSpawn()
		{
			slimeColor = player.GetModPlayer<MinionSpawningItemPlayer>().GetNextColor();
		}
		public override Vector2 IdleBehavior()
		{
			animationFrame++;
			gHelper.SetIsOnGround();
			// the ground-based slime can sometimes bounce its way around 
			// a corner, but the flying version can't
			noLOSPursuitTime = gHelper.isFlying ? 15 : 300;
			List<Projectile> minions = GetActiveMinions();
			int order = minions.IndexOf(projectile);
			Vector2 idlePosition = player.Center;
			idlePosition.X += (40 + order * 38) * -player.direction;
			if (!Collision.CanHitLine(idlePosition, 1, 1, player.Center, 1, 1))
			{
				idlePosition = player.Center;
			}
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}
		
		protected override void DoGroundedMovement(Vector2 vector)
		{
			if(vector.Y < -projectile.height && Math.Abs(vector.X) < startFlyingAtTargetHeight)
			{
				gHelper.DoJump(vector);
			}
			float xInertia = gHelper.stuckInfo.overLedge && !gHelper.didJustLand && Math.Abs(projectile.velocity.X) < 2 ? 1.25f : 8;
			int xMaxSpeed = 8;
			if(vectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				projectile.velocity.X = player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			if(animationFrame - lastHitFrame > 15)
			{
				projectile.velocity.X = (projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
			}
			else
			{
				projectile.velocity.X = Math.Sign(projectile.velocity.X) * xMaxSpeed * 0.75f;
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(gHelper.didJustLand)
			{
				projectile.rotation = 0;
			} else
			{
				projectile.rotation = -projectile.spriteDirection * MathHelper.Pi / 8;
			}
			if(Math.Abs(projectile.velocity.X) < 1)
			{
				return;
			}
			base.Animate(minFrame, maxFrame);
			if(gHelper.didJustLand && Math.Abs(projectile.velocity.X) > 4 && animationFrame % 5 == 0)
			{
				Vector2 pos = projectile.Bottom;
				pos.Y -= 4;
				int idx = Dust.NewDust(pos, 8, 8, 16, -projectile.velocity.X / 2, 0, newColor: Color.Coral);
				Main.dust[idx].scale = .8f;
				Main.dust[idx].alpha = 112;
			}
		}
	}
}
