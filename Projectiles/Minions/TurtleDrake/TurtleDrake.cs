﻿using DemoMod.Projectiles.Minions.MinonBaseClasses;
using log4net.Repository.Hierarchy;
using log4net.Util;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace DemoMod.Projectiles.Minions.TurtleDrake
{
    public class TurtleDrakeMinionBuff: MinionBuff
    {
        public TurtleDrakeMinionBuff() : base(ProjectileType<TurtleDrakeMinion>(), ProjectileType<TurtleDrakeMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Turtle Drake");
			Description.SetDefault("A possessed dagger will fight for you!");
        }
    }

    public class TurtleDrakeMinionItem: EmpoweredMinionItem<TurtleDrakeMinionBuff, TurtleDrakeMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Turtle Drake Staff");
			Tooltip.SetDefault("Summons a possessed dagger to fight for you!");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
			item.value = Item.buyPrice(0, 30, 0, 0);
			item.rare = ItemRarityID.Blue;
		}
    }


    public class TurtleDrakeMinion : EmpoweredMinion<TurtleDrakeMinionBuff>
    {

        private int framesSinceLastHit;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Turtle Drake");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 6;
		}

		public sealed override void SetDefaults() {
			base.SetDefaults();
			projectile.width = 60;
			projectile.height = 92;
			projectile.tileCollide = false;
            projectile.type = ProjectileType<TurtleDrakeMinion>();
            projectile.ai[0] = 0;
            framesSinceLastHit = 0;
		}

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // TODO: don't count the balloon for collisions
            return base.Colliding(projHitbox, targetHitbox);
        }

        public override Vector2 IdleBehavior()
        {
            base.IdleBehavior();
            Vector2 idlePosition = player.Top;
            idlePosition.X += 48 * -player.direction;
            idlePosition.Y += -32;
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            TeleportToPlayer(vectorToIdlePosition, 2000f);
            return vectorToIdlePosition;
        }

        public override void TargetedMovement(Vector2 vectorToTargetPosition)
        {
            vectorToTargetPosition.Y += -32; // hit with the body instead of the balloon
            if(framesSinceLastHit ++ > 3)
            {
                base.TargetedMovement(vectorToTargetPosition);
            }
            else if(projectile.velocity.Length() < 4)
            {
                projectile.velocity.Normalize();
                projectile.velocity *= 4;
            }
            Lighting.AddLight(projectile.position, Color.Green.ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            base.OnHitNPC(target, damage, knockback, crit);
            framesSinceLastHit = 0;
        }
        protected override int ComputeDamage()
        {
            return 30 + 20 * (int)projectile.minionSlots;
        }

        protected override float ComputeSearchDistance()
        {
            return 600 + 50 * projectile.minionSlots;
        }

        protected override float ComputeInertia()
        {
            return Math.Max(20, 45 - 4 * projectile.minionSlots);
        }

        protected override float ComputeTargetedSpeed()
        {
            return 4 + 3.5f * projectile.minionSlots;
        }

        protected override float ComputeIdleSpeed()
        {
            return ComputeTargetedSpeed() + 3;
        }

        protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
        {
            switch(projectile.minionSlots)
            {
                case 1:
                    minFrame = 0;
                    maxFrame = 2;
                    break;
                case 2:
                    minFrame = 2;
                    maxFrame = 4;
                    break;
                default:
                    minFrame = 4;
                    maxFrame = 6;
                    break;
            }
        }

        public override void Animate(int minFrame = 0, int? maxFrame = null)
        {
            base.Animate(minFrame, maxFrame);

            if(Math.Abs(projectile.velocity.X) > 2)
            {
                projectile.spriteDirection = projectile.velocity.X > 0 ? -1 : 1;
            }
            projectile.rotation = projectile.velocity.X * 0.05f;
        }
    }

}