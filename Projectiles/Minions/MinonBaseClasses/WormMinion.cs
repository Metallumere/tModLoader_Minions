﻿using AmuletOfManyMinions.Core.Minions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses
{
	/// <summary>
	/// Uses ai[1] for idle movement cycling
	/// </summary>
	public abstract class WormMinion<T> : EmpoweredMinion<T> where T : ModBuff
	{
		private float[] backingArray;
		public CircularLengthQueue PositionLog = null;
		public int framesSinceLastHit = 0;
		private SpriteBatch spriteBatch;
		private Texture2D texture;
		protected Color lightColor;
		protected virtual int cooldownAfterHitFrames => 16;

		protected virtual float baseDamageRatio => 0.67f;
		protected virtual float damageGrowthRatio => 0.33f;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 8;
			projectile.height = 8;
			backingArray = new float[512];
			CircularVectorQueue.Initialize(backingArray);
			PositionLog = new CircularLengthQueue(backingArray, queueSize: 255, maxLength: 1200)
			{
				mod = mod
			};
		}

		protected virtual SpriteEffects GetEffects(float angle)
		{
			SpriteEffects effects = SpriteEffects.FlipHorizontally;
			angle = (angle + 2 * (float)Math.PI) % (2 * (float)Math.PI); // get to (0, 2PI) range
			if (angle > Math.PI / 2 && angle < 3 * Math.PI / 2)
			{
				effects |= SpriteEffects.FlipVertically;
			}
			return effects;

		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			texture = Main.projectileTexture[projectile.type];
			this.spriteBatch = spriteBatch;
			this.lightColor = lightColor;

			DrawTail();
			DrawBody();
			DrawHead();

			return false;
		}

		protected abstract void DrawTail();
		protected abstract void DrawBody();
		protected abstract void DrawHead();
		protected void AddSprite(float dist, Rectangle bounds, Color c = default)
		{
			Vector2 origin = new Vector2(bounds.Width / 2f, bounds.Height / 2f);
			Vector2 angle = new Vector2();
			Vector2 pos = PositionLog.PositionAlongPath(dist, ref angle);
			float r = angle.ToRotation();
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, c == default ? lightColor : c, r,
				origin, 1, GetEffects(r), 0);
		}


		protected int GetSegmentCount()
		{
			return EmpowerCount;
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			projectile.ai[1] = (projectile.ai[1] + 1) % 240;
			int radius = player.velocity.Length() < 4 ? 48 + 2 * EmpowerCount: 48;
			float yRadius = player.velocity.Length() < 4 ? 8 + 0.5f * EmpowerCount: 8;
			Vector2 idlePosition = player.Top;
			idlePosition.X += radius * (float)Math.Cos(Math.PI * projectile.ai[1] / 60);
			idlePosition.Y += -20 + EmpowerCount + yRadius * (float)Math.Sin(Math.PI * projectile.ai[1] / 60);
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		public override void OnHitTarget(NPC target)
		{
			framesSinceLastHit = 0;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			float inertia = ComputeInertia();
			float speed = ComputeTargetedSpeed();
			vectorToTargetPosition.SafeNormalize();
			vectorToTargetPosition *= speed;
			framesSinceLastHit++;
			if (framesSinceLastHit < cooldownAfterHitFrames && framesSinceLastHit > cooldownAfterHitFrames / 2)
			{
				// start turning so we don't double directly back
				Vector2 turnVelocity = new Vector2(-projectile.velocity.Y, projectile.velocity.X) / 8;
				turnVelocity *= Math.Sign(projectile.velocity.X);
				projectile.velocity += turnVelocity;
			}
			else if (framesSinceLastHit++ > cooldownAfterHitFrames)
			{
				projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			}
			else
			{
				projectile.velocity.SafeNormalize();
				projectile.velocity *= speed; // kick it away from enemies that it's just hit
			}
		}

		protected override int ComputeDamage()
		{
			return (int)(baseDamage * baseDamageRatio + baseDamage * damageGrowthRatio * GetSegmentCount());
		}

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
		{
			minFrame = 0;
			maxFrame = 0;
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			PositionLog.AddPosition(projectile.position);
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
		}
	}
}
