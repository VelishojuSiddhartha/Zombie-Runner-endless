namespace ZombieRunner.Utilities
{
    /// <summary>Common damage contract for enemies, explosive barrels, and future breakable props.</summary>
    public interface IDamageable
    {
        void TakeDamage(float amount);
    }
}
