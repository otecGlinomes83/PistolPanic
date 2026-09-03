namespace PistolPanic.Core
{
    public sealed class FireSystem
    {
        public void Tick(float fixedDelta, AmmoSystem ammoSystem, AmmoState ammo, WeaponParams weaponParams)
        {
            ammoSystem.Tick(fixedDelta, ammo, weaponParams);
        }
    }
}
