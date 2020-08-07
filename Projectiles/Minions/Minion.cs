using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using DemoMod.Dusts;

namespace DemoMod.Projectiles.Minions
{
	public abstract class Minion<T>  : ModProjectile where T: ModBuff
	{
        public readonly float PI = (float)Math.PI;

        public Player player;

		protected int? targetNPCIndex;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }
        public override void AI() {
			player = Main.player[projectile.owner];
			CheckActive();
			Behavior();
		}

        public void CheckActive() {
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(BuffType<T>());
			}
			if (player.HasBuff(BuffType<T>())) {
				projectile.timeLeft = 2;
			}
		}
		public Vector2? PlayerTargetPosition(float maxRange, Vector2? centeredOn = null, float noLOSRange = 0)
        {
			Vector2 center = centeredOn ?? projectile.Center;
			if(player.HasMinionAttackTargetNPC)
            {
				NPC npc = Main.npc[player.MinionAttackTargetNPC];
				float distance = Vector2.Distance(npc.Center, center);
				if(distance < noLOSRange || (distance < maxRange && 
					Collision.CanHitLine(projectile.Center, 1, 1, npc.position, npc.width, npc.height)))
                {
					targetNPCIndex = player.MinionAttackTargetNPC;
					return npc.Center;
                }
            } 
			return null;
        }

		public Vector2? PlayerAnyTargetPosition(float maxRange, Vector2? centeredOn = null)
        {
			Vector2 center = centeredOn ?? projectile.Center;
			if(player.HasMinionAttackTargetNPC)
            {
				NPC npc = Main.npc[player.MinionAttackTargetNPC];
				float distance = Vector2.Distance(npc.Center, center);
                bool lineOfSight =Collision.CanHitLine(center, 1, 1, npc.Center, 1, 1); 
				if(distance < maxRange && lineOfSight)
                {
					targetNPCIndex = player.MinionAttackTargetNPC;
					return npc.Center;
                }
            }
			return null;
        }

		public Vector2? ClosestEnemyInRange(float maxRange, Vector2? centeredOn = null, float noLOSRange = 0)
        {
			Vector2 center = centeredOn ?? projectile.Center;
			Vector2 targetCenter = projectile.position;
			bool foundTarget = false;
			for(int i = 0; i < Main.maxNPCs; i++)
            {
				NPC npc = Main.npc[i];
				if(!npc.CanBeChasedBy())
                {
					continue;
                }
                float between = Vector2.Distance(npc.Center, center);
                bool closest = Vector2.Distance(center, targetCenter) > between;
				// don't let a minion infinitely chain attacks off progressively further enemies
                bool inRange = Vector2.Distance(npc.Center, player.Center) < maxRange;
                bool inNoLOSRange = Vector2.Distance(npc.Center, player.Center) < noLOSRange;
                bool lineOfSight =Collision.CanHitLine(projectile.Center, 1, 1, npc.position, npc.width, npc.height); 
				if((inNoLOSRange || (lineOfSight && inRange)) && (closest || !foundTarget))
                {
					targetNPCIndex = i;
					targetCenter = npc.Center;
					foundTarget = true;
                }
            }
			if(foundTarget)
            {
				return targetCenter;
            } else
            {
				return BeaconPosition(center, maxRange, noLOSRange);
            } 
		}

		public Vector2? BeaconPosition(Vector2 center, float maxRange, float noLOSRange = 0)
        {
			// should automatically fall through to here if can't hit target
			if (player.ownedProjectileCounts[ProjectileType<MinionWaypoint>()] == 0)
			{
				return null;
			}
			Vector2? waypointCenter = null;
            foreach (Projectile p in Main.projectile)
            {
                if(p.type == ProjectileType<MinionWaypoint>() && p.active && p.owner == Main.myPlayer)
                {
                    Vector2 target = p.position;
                    float distance = Vector2.Distance(target, center);
                    if(distance < noLOSRange || (distance < maxRange && 
                        Collision.CanHitLine(projectile.Center, 1, 1, target, 1, 1)))
                    {
						waypointCenter = target;
						break;
                    }
                }
            }
			// try again with the beacon position as the central search point
			if (waypointCenter is Vector2 wCenter && AnyEnemyInRange(maxRange, wCenter) is Vector2 anyTarget)
            {
				DrawDirectionDust(wCenter, anyTarget);
				return wCenter;
            }
			else
            {
				return null;
            }
        }

		private int directionFrame = 0;
		protected void DrawDirectionDust(Vector2 waypointCenter, Vector2 anyTarget)
        {
			if((directionFrame++)%30 != 0)
            {
				return;
            }
			int lineLength = 64;
			Vector2 fromVector = projectile.Center - waypointCenter;
			Vector2 toVector = anyTarget - waypointCenter;
			fromVector.Normalize();
			toVector.Normalize();
			for(int i = 12; i < lineLength; i += 2) 
            {
				float scale = 1.5f - 0.015f * i;
				Dust.NewDust(waypointCenter + fromVector * i, 1, 1, DustType<MinionWaypointDust>(), newColor: new Color(0.5f, 1, 0.5f), Scale: scale);
				Dust.NewDust(waypointCenter + toVector * i, 1, 1, DustType<MinionWaypointDust>(), newColor: new Color(0.5f, 1, 0.5f), Scale: scale);

            }

        }

		public Vector2? AnyEnemyInRange(float maxRange, Vector2? centeredOn = null)
        {
			Vector2 center = centeredOn ?? projectile.Center;
			for(int i = 0; i < Main.maxNPCs; i++)
            {
				NPC npc = Main.npc[i];
				if(!npc.CanBeChasedBy())
                {
					continue;
                }
				// 
                bool inRange = Vector2.Distance(center, npc.Center) < maxRange;
                bool lineOfSight =Collision.CanHitLine(center, 1, 1, npc.Center, 1, 1); 
				if(lineOfSight && inRange)
                {
					return npc.Center;
                }
            }
			return null;
        }

		public abstract void Behavior();
	}
}