using System;
using tainicom.Aether.Physics2D.Dynamics;

namespace MGJ3.Components
{
    public static class CollisionCategories
    {
        public const Category StageBounds = Category.Cat1;
        public const Category Player = Category.Cat2;
        public const Category PlayerBullet = Category.Cat3;
        public const Category Comet = Category.Cat4;
        
        public const Category Enemy = Category.Cat5;
        public const Category EnemyBullet = Category.Cat6;

        // all bonus categories
        public const Category Bonuses = Category.Cat7;
        // all enemy categories
        public const Category Enemies = Comet | EnemyBullet | Enemy;
        // all projectile categories
        public const Category Projectiles = PlayerBullet | EnemyBullet;

    }
}
