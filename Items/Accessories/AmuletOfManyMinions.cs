﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using DemoMod.Projectiles.Minions.PossessedCopperSword;
using DemoMod.Projectiles.Minions.SpiritGun;
using DemoMod.Projectiles.Minions.FlyingSword;
using DemoMod.Projectiles.Minions.VoidKnife;

namespace DemoMod.Items.Accessories
{
    [AutoloadEquip(EquipType.Neck)]
    class AmuletOfManyMinions : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Increases your max number of minions by 2\n" +
                "Greatly improves minion damage");
        }

        public override void SetDefaults()
        {
            item.width = 30;
            item.height = 32;
            item.accessory = true;
            item.value = Item.sellPrice(gold: 5);
            item.rare = ItemRarityID.Expert;
        }

        public override void UpdateEquip(Player player)
        {
            player.minionDamageMult += 0.25f;
            player.maxMinions += 2;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<CopperSwordMinionItem>(), 1);
            recipe.AddIngredient(ItemType<SpiritGunMinionItem>(), 1);
            recipe.AddIngredient(ItemType<FlyingSwordMinionItem>(), 1);
            recipe.AddIngredient(ItemType<VoidKnifeMinionItem>(), 1);
            recipe.AddIngredient(ItemType<CharmOfManyMinions>(), 1);
            recipe.AddIngredient(ItemType<CharmOfMightyMinions>(), 1);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

    }
}
