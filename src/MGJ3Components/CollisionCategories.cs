using System;
using tainicom.Aether.Physics2D.Dynamics;

namespace MGJ3.Components
{
    public static class CollisionCategories
    {
        public const Category StageBounds = Category.Cat1;
        public const Category Player = Category.Cat2;
        public const Category Comet = Category.Cat3;
        public const Category Bullet = Category.Cat4;
        public const Category Ship = Category.Cat5;

        // all enemy categories
        public const Category Enemies = Comet | Bullet | Ship;
        // all bonus categories
        public const Category Bonuses = (Category)0;

    }
}
